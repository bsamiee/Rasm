export const meta = {
  name: 'ascend-tests-infra',
  description: 'Three-pass ascension (initial/critique/redteam) per scope over the entire test infrastructure, then one estate smoothing pass and a terminal gate-verify. TS is primary pressure; py/cs estates are the floor all three rise from.',
  whenToUse: 'RASM-TESTING-DECISION.md [11] — run once after residual clearance; ephemeral, deleted after landing.',
  phases: [
    { title: 'Initial', detail: 'per-scope hostile rebuild-grade pass; 3 language lanes concurrent, scopes sequential within a lane' },
    { title: 'Critique', detail: 'per-scope mechanical doctrinal-conformance + capability-completeness audit, fix in place' },
    { title: 'Redteam', detail: 'per-scope terminal adversarial pass with full cold re-review, fix in place' },
    { title: 'Smooth', detail: 'ONE full-estate smoothing pass: cross-language consistency, doc trues, residual resolution' },
    { title: 'Gate', detail: 'terminal gate-verify: every language gate run live, failures fixed in place' },
  ],
}

// --- [CONSTANTS] -------------------------------------------------------------------------

const STALL = 300000

// --- [INPUTS] ----------------------------------------------------------------------------

// args: optional lane filter — 'ts' | ['ts','py'] | {lanes:[...]}; default ALL three lanes.
const lanesIn = Array.isArray(args) ? args
  : (args && typeof args === 'object' && Array.isArray(args.lanes)) ? args.lanes
  : (typeof args === 'string' && args.trim()) ? [args.trim()]
  : ['ts', 'py', 'cs']
const RUN = new Set(lanesIn.filter((l) => l === 'ts' || l === 'py' || l === 'cs'))

// --- [MODELS] ----------------------------------------------------------------------------

const RESIDUAL = { type: 'array', items: { type: 'object', additionalProperties: false, required: ['files', 'claim'], properties: { files: { type: 'array', items: { type: 'string' } }, claim: { type: 'string' } } } }
const FIXLOG = { type: 'object', additionalProperties: false, required: ['files', 'verdict', 'gates', 'summary'], properties: {
  files: { type: 'array', items: { type: 'string' } }, verdict: { type: 'string', enum: ['rebuilt', 'refined', 'clean'] },
  gates: { type: 'string' }, collapsed: { type: 'string' }, extended: { type: 'string' }, residual_high: RESIDUAL, summary: { type: 'string' } } }
const GATE_SCHEMA = { type: 'object', additionalProperties: false, required: ['allGreen', 'gates', 'summary'], properties: {
  allGreen: { type: 'boolean' }, gates: { type: 'array', items: { type: 'object', additionalProperties: false, required: ['gate', 'status'], properties: { gate: { type: 'string' }, status: { type: 'string', enum: ['green', 'fixed-then-green', 'fenced-red', 'red'] }, note: { type: 'string' } } } },
  fixedFiles: { type: 'array', items: { type: 'string' } }, residual_high: RESIDUAL, summary: { type: 'string' } } }

// --- [DOCTRINE] --------------------------------------------------------------------------

const FENCES =
  'FENCES (absolute): NEVER modify anything under libs/ (read-only ground), docs/ (read-only law), tools/ (read-only), .claude/, ' +
  'RASM-*.md briefs, tests/contracts/ (smoothing owns its docs; corpus bytes are C#-sole-producer), or another lane\'s manifests. The ' +
  'worktree carries a CONCURRENT session\'s uncommitted edits (libs/ .md files and briefs) — never modify, stage, revert, or commit them. ' +
  'DO NOT git add/commit — the orchestrator commits by pathspec. Zero root litter: every tool routes caches/outputs under .cache/ or ' +
  '.artifacts/ through its own documented config; NO new repo-root file may be created by a lane pass — a genuinely needed root file is a ' +
  'residual_high for the smoothing pass (which coordinates the tests/python/_testkit/test_policy.py allowlist row in the same change). ' +
  'Shared-file ownership: tests/README.md and tests/contracts/*.md belong to the SMOOTH pass only — a lane pass reports doc drift there as ' +
  'residual_high; each lane trues ONLY its own language README law doc. Safe-by-default bounds are LAW: never loosen a workers cap, ' +
  'timeout, mutate scope, output route, or explicit testDir/testMatch; strengthening is welcome, loosening is a defect.'
const ADVERSARIAL =
  'ADVERSARIAL STANCE — you are HOSTILE: assume the existing infrastructure is NAIVE, SHALLOW, or ILLUSORY until it survives an aggressive ' +
  'attack; the burden of proof is ON THE CODE. "Mature", "recently rebuilt", "already strong", and a prior clean verdict are REJECTED ' +
  'self-assessments — much of this estate is days old and dense-looking, and dense confident code is the PRIME suspect for hollowness. ' +
  'NAIVETY is a defect on TWO orthogonal axes: COVERAGE — the owner models a thin slice of its concept (a kit combinator family with three ' +
  'members where the testing domain carries fifteen; a harness row set covering one topology where the estate needs five); APPROACH — ' +
  'enumerated hardcoded instances where a parameterized, algorithmic owner should GENERATE the space (a fixed roster of fixtures, rows, or ' +
  'arms is seed DATA feeding one generator over named parameters, never the mechanism). ILLUSION is the primary target: a name, signature, ' +
  'or doc that PROMISES capability the body does not implement; a gauge that can never fail; a law without a falsification witness; a ' +
  'config knob that does nothing; a catalog member cited but unverifiable (a phantom — delete it). Every enumerated signal list here is a ' +
  'FLOOR you hunt past, never the complete set. Where the strongest form is genuinely present, prove it by finding nothing — never invent ' +
  'churn to look busy.'
const TESTLAW =
  'TEST-ESTATE LAW (universal): every capability ships with a falsification proof — a gate nobody can see fail is deleted, not kept; ' +
  'witness-mandatory laws, honest typed absence (Unsupported/Awaiting/Blocked) over vacuous green, explicitly-skipped suites naming their ' +
  'activation condition over silent skips. Foresight without spam: capability lands where the libs/ planning corpus grounds real demand — ' +
  'assume 5x future demand on shared infra, never speculative structure with no named consumer. No coupling: kits stay project-agnostic and ' +
  'runner-honest; no exotic pattern that buys a runtime limitation (mutation-runner compatibility, sharding, resume, CI portability all ' +
  'preserved). External packages are mined to FULL catalog depth from official sources (Context7 + installed types/dists — never memory); a ' +
  'NEW package/tooling admission is justified by a capability gap, lands in the owning central manifest at newest stable, carries its .api ' +
  'catalog (real content when this scope composes it), and updates the owning README manifest per the admission law. Comments and prose: ' +
  'agent-first, 1-2 lines, docs/standards/{style-guide,formatting,information-structure}.md law, section separators per the CLAUDE.md [09] ' +
  'language overlay, zero provenance, zero fragile enumerations in .md files. Rebuild over patch: anything testing-related is rebuilt ' +
  'ground-up the moment a denser shape exists; breaking existing specs is never a reason to preserve chaff; capability is never deleted to ' +
  'satisfy a density signal.'

const LANE = {
  ts: {
    name: 'TypeScript',
    readLaw: 'READ FULLY BEFORE EDITING: (1) docs/stacks/typescript/ in FULL — all 12 pages (README, language, values, shapes, computation, ' +
      'derivation, surfaces-and-dispatch, services-and-layers, rails-and-effects, streams, concurrency, boundaries) — the binding production ' +
      'standard for every line of kit/e2e/config source; test-plane adjustments only where tests/typescript/README.md sanctions them, same ' +
      'doctrinal principles. (2) tests/typescript/README.md (binding law of the tree) + tests/README.md (cross-language law, read-only). ' +
      '(3) libs/typescript/.planning/ in FULL as demand signal for kit and e2e capability — every durable page; SKIP task-log and ideas ' +
      'pages; a page absent at run time is simply absent. (4) Every tests/typescript/.api/ catalog the scope composes plus the ' +
      'libs/typescript/.api Effect-substrate catalogs it rides. Effect idioms: it.effect/it.live/it.layer/it.effect.prop; it.scoped and ' +
      'TestServices reach-ins are banned; Schema authority — types derive from schemas, never parallel-declared.',
    verify: 'the published types in node_modules (uv run python -m tools.assay api over node_modules declarations for novel members) + ' +
      'Context7 for current official docs — never memory',
    manifests: 'pnpm-workspace.yaml (catalog, peer rules) + root package.json + pnpm-lock.yaml — THIS lane owns them; catalog: refs only, ' +
      'catalogMode strict',
    gates: 'END WITH GATES GREEN and report them verbatim in `gates`: pnpm install (when manifests changed); biome clean over ' +
      'tests/typescript and every touched root config; tsc + tsgo dual-floor typecheck green; pnpm exec vitest run green (plus ' +
      'RASM_TESTKIT_CONTAINERS=1 lane when harness rows changed); pnpm exec playwright test green when the e2e estate or root playwright ' +
      'config changed; uv run pytest tests/python/_testkit/test_policy.py -q green; git status delta = intended paths only.',
  },
  py: {
    name: 'Python',
    readLaw: 'READ FULLY BEFORE EDITING: (1) docs/stacks/python/ in FULL, atlas order — README then language, shapes, iteration, ' +
      'surfaces-and-dispatch, rails-and-effects, concurrency, boundaries, algorithms, system-apis, runtime — the binding standard for every ' +
      'kit line. (2) tests/python/README.md (binding law of the tree) + tests/README.md (cross-language law, read-only). (3) The pyproject ' +
      'test-plane tables ([tool.pytest.ini_options], [tool.coverage.*], [tool.mutmut], [tool.importlinter], hypothesis profiles in the kit).',
    verify: 'uv run python -m tools.assay api resolve <pkg> over the installed dist + Context7 for current official docs — never memory',
    manifests: 'pyproject.toml + uv.lock — THIS lane owns them; lean unpinned names, newest stable, constraints only on resolver evidence',
    gates: 'END WITH GATES GREEN and report them verbatim in `gates`: uv run pytest tests/python -q FULL suite green (the law-coverage gate ' +
      'is only valid full-session — never gate on a subset run); uv run python -m tools.assay static green over touched python paths ' +
      '(ruff/ty/mypy/lint-imports); uv run python -m tools.assay self-test green when kit seams changed; git status delta = intended paths only.',
  },
  cs: {
    name: 'C#',
    readLaw: 'READ FULLY BEFORE EDITING: (1) docs/stacks/csharp/ in FULL — README, language, shapes, surfaces-and-dispatch, ' +
      'rails-and-effects, boundaries, algorithms, system-apis — the binding standard for every kit line. (2) tests/csharp/README.md ' +
      '(binding law of the tree) + tests/README.md (cross-language law, read-only). (3) .claude/skills/testing-cs/SKILL.md + its references/ ' +
      '(the route-owned C# test standard: density axes, oracles-laws, mutation-coverage, testkit, bridge-runtime) — read-only law. ' +
      '(4) The libs/csharp/.api substrate catalogs the kits compose (CsCheck, xunit v3, Verify, BenchmarkDotNet, ArchUnitNET rows).',
    verify: 'uv run python -m tools.assay api (restored-assembly member truth; verified-local wins) + the nuget MCP for feed truth + ' +
      'Context7 — never memory',
    manifests: 'Directory.Packages.props (Test Stack group) + Directory.Build.props test wiring + packages.lock.json — THIS lane owns the ' +
      'test-stack rows; hand-edit grouped props, never dotnet add; confirm with dotnet restore',
    gates: 'END WITH GATES GREEN and report them verbatim in `gates`: dotnet build green on touched projects; positional ' +
      'dotnet test <csproj> (NEVER --project) green on every touched test project — the SnapshotHygiene walk stays behind --explicit; ' +
      'dotnet format clean over touched files; git status delta = intended paths only. KNOWN FENCED RED: ' +
      'AssemblyBoundaryLaws.WorkspaceSolutionMatchesDiskAndCarriesTheScenarioHome fails on the concurrent session\'s ' +
      'Rasm.Grasshopper slnx/disk divergence — do NOT fix it by editing Workspace.slnx, libs/, or deleting/weakening the law; treat that ' +
      'ONE law as expected-red and carry it as residual_high for the terminal sweep.',
  },
}

const SCOPES = [
  { key: 'ts-kit', lane: 'ts',
    ground: 'tests/typescript/_testkit/** (every src module, every falsification spec, package.json, tsconfig.json)',
    spec: 'The shared kit @rasm/ts-testkit — corpus readers (manifest-driven, typed absence), witness-mandatory law combinators + tautology ' +
      'audit, Schema-derived arbitraries (field-absence/UNSET lane, distinct-payload law), harness Layers (pglite fast lane, testcontainers ' +
      'pg + S3 rows behind ONE filesystem algebra reading tests/containers.json, loopback capsule, Ryuk on), bench autosave -> sustained ' +
      'regression fold, gauge engines (snapshot hygiene, import-graph verdicts), the e2e substrate (hermetic page corpus, k6 owner). ATTACK ' +
      'IT: deepen every owner to the full demand the libs/typescript/.planning corpus grounds — fast-check 4.x advanced surfaces (the ' +
      'scheduler for race/interleaving laws, model-based commands, statistics/bias policies, requiredKeys record lanes), @effect/vitest full ' +
      'depth per its tests-tier catalog, testcontainers 12 depth (wait strategies, exec, networks, compose rows where grounded), pglite ' +
      'extension lanes, vitest 4 advanced config/API surfaces. Every new capability is a case/row/field on an existing owner with its own ' +
      'falsification spec; kit stays playwright-free and @rasm/*-free; exports-map subpaths only, no barrel.' },
  { key: 'ts-e2e', lane: 'ts',
    ground: 'playwright.config.ts (repo root) + tests/typescript/e2e/** + tests/typescript/_architecture/**',
    spec: 'The e2e platform and the architecture gauge home. ATTACK: the one test.extend fixture tower (hermetic, pausedClock, cohort, ' +
      'webauthn, caps/option fixtures) — deepen per the scenario-class matrix (visual + aria snapshot policies, routeWebSocket + HAR lanes, ' +
      'multi-context CRDT cohort choreography, WebGL/WebGPU viewer lane, storageState auth topology readiness); decide the goldens ' +
      '{platform} snapshotPathTemplate question deliberately (macOS-minted today; a Linux CI lane needs per-platform goldens or the token); ' +
      'BLOCKED classes stay typed capability + named-activation skips, zero simulacra; k6 typed scripts + driver depth. _architecture: ' +
      'manifest-level admission gauges runnable now, self-activating source gauges (edge-ledger vs the permitted-edge table, subpath ' +
      'purity, migrator ban) with honest activation tripwires. Root config: bounds are law (explicit testDir/testMatch *.pw.ts, workers ' +
      'cap, timeout hierarchy, routed outputs, forbidOnly/failOnFlakyTests CI rows); verify every knob against current official docs.' },
  { key: 'ts-tooling', lane: 'ts',
    ground: 'vitest.config.ts + vite.config.ts + vite.factory.ts + stryker.config.json + biome.json (test-plane rules) + nx.json (test ' +
      'targets/outputs) + tsconfig.base.json and the estate tsconfigs + the pnpm-workspace peer/catalog rows the runner stack rides',
    spec: 'Root TS tooling at current-docs depth: verify EVERY config knob against the installed major via Context7/official docs — kill ' +
      'dead knobs, adopt stronger current surfaces (vitest 4 projects/pool/benchmark rows, StrykerJS 9 incremental/vitest-runner rows, biome ' +
      '2.5 rule surfaces, nx 23 target defaults/outputs), keep artifact routing (.artifacts/typescript, .cache/*) and safe bounds intact. ' +
      'LEDGER ITEM (close it here): assay\'s TS static route compiles dirty test-estate files under root tsconfig.base.json with ' +
      '--isolatedDeclarations — structurally incompatible with the kit/spec Effect idiom (Data.TaggedError extends-expressions, inferred ' +
      'spec exports). Fix CONFIG-SIDE: a per-project tsconfig route / estate carve-out so estate files typecheck under their own project ' +
      'config while libs/ keeps isolatedDeclarations law; if an assay-side change is genuinely required, report it as residual_high ' +
      '(tools/ is fenced). Prove the fix: touch an estate file, run uv run python -m tools.assay static over it, show the TS lane green.' },
  { key: 'py-kit', lane: 'py',
    ground: 'tests/python/_testkit/** (spec, strategies, seams, env, bench, laws, runtime, test_policy + kit self-tests) + the pyproject ' +
      'test-plane tables',
    spec: 'The Python kit is the FLOOR the TS kit rose from — now raise it back past the TS bar. ATTACK: hypothesis advanced surfaces ' +
      '(stateful RuleBasedStateMachine lanes where a stateful subject exists, targeted PBT, observability wiring already routed to ' +
      '.artifacts), pytest 9 depth, inline-snapshot/dirty-equals depth, the seams/env capsules (grpc loopback, moto ObjectStore row — ' +
      'align its algebra with the TS filesystem-algebra shape where honest, wire the tests/containers.json neutral pin ONLY where a real ' +
      'py container consumer exists, never speculative). LEDGER ITEM (close it here): the law-coverage gate (test_law_coverage_gate) is ' +
      'valid only full-session — targeted subsets report false gaps because COVERS ledgers aggregate across modules; make it subset-aware ' +
      'or honestly self-skipping under partial collection, with a falsification proof. Policy gates (root allowlist, litter) stay exact.' },
  { key: 'py-estate', lane: 'py',
    ground: 'tests/python/tools/** (assay + py_analyzer suites) + tests/python/libs/** conftest shells',
    spec: 'BOUNDED infra-quality pass over the freshly collapsed estate — NOT a rewrite: the load-bearing survivor set is law (SARIF fold ' +
      'block, engine boundary suite, package commit-sentinel, api spill/cache replay, provision sanitizer egress, static phase ordering, ' +
      'mutation-gate argv laws, code prefilter laws, settings remote-env boundary, registry census, main channel separation, py_analyzer ' +
      'exemplar, testkit env/policy gates). DO: absorb repeated local scaffolding into the kit where a kit owner now carries it; upgrade ' +
      'suites to leverage new kit capability (matrix folds, strategies, seams — the user names Python first for this); kill any surviving ' +
      'tautology or naive pattern; keep private-seam tests only where no public driver exists. Coverage held: the instrumented-coverage ' +
      'bar and mutation kill-floor never regress.' },
  { key: 'cs-kits', lane: 'cs',
    ground: 'tests/csharp/_testkit/** (Rasm.TestKit: Spec, Approx, Gens, Numeric, Laws, Seams, Manifests) + tests/csharp/_scenariokit/** ' +
      '(Rasm.ScenarioKit)',
    spec: 'The C# kits are the parity bar — push them past it. ATTACK: CsCheck depth (Faster relative-perf laws, model-based ' +
      'SampleParallel/concurrency laws, classify/ChiSquared distribution proofs) woven into Spec witness-mandatory rows; xunit v3 3.2.x ' +
      'surfaces (TheoryDataRow per-row metadata, MatrixTheoryData, Assert.Skip*, Explicit, TestPipelineStartup) where they densify; ' +
      'Verify + DiffPlex snapshot law depth; Gens adversarial bands vs the full numeric/geometry domain the libs planning corpus grounds; ' +
      'ScenarioKit evidence/FactKey depth for the bridge lane. Host-free law holds absolutely for Rasm.TestKit; every kit capability ' +
      'carries a falsification spec; one generated owner per concern, no parallel helper spam.' },
  { key: 'cs-estate', lane: 'cs',
    ground: 'tests/csharp/_architecture/** + tests/csharp/_benchmarks/** + tests/csharp/tools/** + stryker-config.json + the ' +
      'Directory.Build.props/Directory.Packages.props test-stack wiring',
    spec: 'The C# proof estate: _architecture gauge depth (boundary/strata laws, infra-primitive laws, snapshot hygiene behind --explicit), ' +
      '_benchmarks BDN switcher + Pass/TooNoisy/Breach gate depth (bounded warmup/iteration caps preserved), the rhino-bridge ' +
      'Contract/Supervisor infra suites (wire-stable, evidence-typed), Stryker.NET root config truth against current docs (mtp runner, ' +
      'concurrency 4, output routing, thresholds; the solution-mode vacuity guard). The libs/ test shells stay SHELL-lane (AssayTestShell) ' +
      'and empty — do not seed content. The known fenced boundary-law red is the concurrent session\'s divergence: carry it, never fix it ' +
      'from this run.' },
]

// --- [OPERATIONS] ------------------------------------------------------------------------

const PRE = (scope) => {
  const L = LANE[scope.lane]
  return 'Rasm monorepo — the tests-infra ASCENSION campaign (RASM-TESTING-DECISION.md [11]; read that section first as binding scope). ' +
    'This estate was rebuilt recently to a high bar — that bar is the FLOOR, not the ceiling. ' + L.name + ' lane. SCOPE GROUND (your ' +
    'write surface, plus ' + L.manifests + ' and your own language README law doc): ' + scope.ground + '.\n\n' + L.readLaw + '\nVerify ' +
    'every external member via ' + L.verify + '.\n\n' + ADVERSARIAL + '\n\n' + TESTLAW + '\n\n' + FENCES + '\n\nWRITE-FULLY MANDATE: every ' +
    'fix you identify you MAKE NOW via Edit/Write — the fix-log you return REPORTS edits already made, never a to-do list or a ' +
    'would/should hedge; genuine cross-scope items go to residual_high {files, claim} (a residual spanning two files names BOTH).'
}
const initialPrompt = (scope) => PRE(scope) + '\n\nTASK: INITIAL REBUILD-GRADE PASS over the scope. DISBELIEVE the estate: hunt both ' +
  'naivety axes and every illusion; rebuild ground-up where a denser or more capable shape exists, refine where genuinely strong. ' +
  'SPEC (the floor of your attention, never its ceiling):\n' + scope.spec + '\n\nMANDATES: ultra-stack every admitted package to full ' +
  'catalog/official depth; close capability gaps the demand corpus grounds by growing EXISTING owners (a case/row/field/operation/policy ' +
  'value, each citing a package member, domain demand, or named future consumer); admit a NEW package/tool only for a real capability gap ' +
  '(newest stable, central manifest, catalog + README per the admission law); add new files/modules only where OWNERSHIP demands a new ' +
  'owner, never to shrink an existing one; every new capability lands with its falsification proof. ' + LANE[scope.lane].gates +
  '\nReturn the fix-log.'
const critiquePrompt = (scope, init) => PRE(scope) + '\n\nTASK: HOSTILE MECHANICAL CRITIQUE + FIX IN PLACE over the scope, cold. You are ' +
  'an ULTRA-HARSH, UNAGREEABLE auditor: assume a violation exists in every file until proven otherwise; the initial pass just rewrote ' +
  'this — its output is the PRIME suspect. Run these as a FLOOR and hunt past them: (1) COLLAPSE_SCAN — sibling prefix/suffix names, ' +
  'same-rail-different-arity, get/get-many families, literal-differing functions, bool-selected bodies, one-call hops, parallel dispatch ' +
  'arms, field-sharing types, 3+ sibling constants, package-renaming wrappers, recurring wrapper stacks: collapse each into the owning ' +
  'polymorphic surface IN PLACE. (2) OWNER correctness per shape (the language doctrine owner-chooser). (3) KNOB_TEST per parameter/config ' +
  'row — a knob whose value the input already encodes dies; a dead config knob dies. (4) Rails/idiom law — typed error rails, no ' +
  'exception/throw control flow in domain logic, exhaustive dispatch, the language modernity bar. (5) CAPABILITY-COMPLETENESS + ILLUSION — ' +
  'verify every body implements what its name/doc promises; every falsification spec can actually fail (mentally seed the defect); every ' +
  'gauge has a reachable red; both naivety axes attacked per owner; delete speculative/padding additions. (6) File-organization + section ' +
  'law + comment/prose hygiene. SPEC:\n' + scope.spec + '\nINITIAL PASS RESULT (verdict + residuals only — re-derive everything yourself):\n' +
  JSON.stringify((init && { verdict: init.verdict, residual_high: init.residual_high || [] }) || {}, null, 1) + '\n' +
  LANE[scope.lane].gates + '\nCarry forward unresolved residuals, add your own. Return the fix-log.'
const redteamPrompt = (scope, crit) => PRE(scope) + '\n\nTASK: TERMINAL ADVERSARIAL RED-TEAM + FIX IN PLACE over the scope — the LAST and ' +
  'MOST AGGRESSIVE pass; assume the initial and critique missed things. (A) COUNTERFACTUAL on every core owner: is it categorically the ' +
  'strongest form the doctrine admits, or does a denser owner, a derived table, a parameterized generator over an enumerated roster, or a ' +
  'DEEPER admitted-package primitive collapse it? If a fundamentally stronger design exists, rebuild to it now. (B) DIFF-OF-THE-NEXT-DEMAND: ' +
  'the next scenario class / harness row / law family / config target lands as ONE case/row/policy value with every consumer untouched or ' +
  'loudly broken at type-check; reshape any multi-site growth axis. (C) LONG-TAIL attack: empty/singular/plural/stream, malformed input, ' +
  'concurrency/cancellation, partial failure, version skew, CI-vs-local, cold-vs-warm cache — the infra must hold or fail honestly. ' +
  '(D) BOUNDARY integrity: kit vs suite vs config ownership clean; nothing coupled to a sibling interior; runner discovery sets disjoint; ' +
  'safe bounds intact. (E) PHANTOMS + SPRAWL: a member cited but unverifiable dies; hand-rolled re-derivations of admitted-package ' +
  'capability collapse to package depth. (F) FULL COLD RE-REVIEW of every critique dimension with fresh hostile eyes. The scope must end ' +
  'objectively denser and MORE CAPABLE than critique left it — or prove the strongest form by finding nothing. SPEC:\n' + scope.spec +
  '\nCRITIQUE RESULT (verdict + residuals only):\n' +
  JSON.stringify((crit && { verdict: crit.verdict, residual_high: crit.residual_high || [] }) || {}, null, 1) + '\n' +
  LANE[scope.lane].gates + '\nCarry forward unresolved residuals, add your own. Return the fix-log.'
const smoothPrompt = (residuals) => 'Rasm monorepo — the tests-infra ASCENSION campaign, SMOOTHING pass (RASM-TESTING-DECISION.md [11] ' +
  'tail). Three language lanes just ran initial/critique/redteam over the whole test estate. Your surface: the ENTIRE tests/ tree ' +
  '(all languages), tests/README.md and tests/contracts/*.md (you are their owner this run), the per-language README law docs, and the ' +
  'root test tooling configs. Read tests/README.md + all four language/contracts law docs FULLY, then sweep the estate for CROSS-LANGUAGE ' +
  'smoothing: one consistent naming/section/prose voice per docs/standards law; the law docs trued to post-ascension reality (zero fragile ' +
  'enumerations, zero drift); artifact/cache routing vocabulary consistent; the three kits aligned-never-coupled (a capability one kit ' +
  'carries that a sibling estate needs is a named seam or an honest gap in the sibling README, never silent divergence); dead references, ' +
  'stale invocation spellings, and leftover scaffolding dropped. RESIDUALS from the lanes (resolve each in place where real, drop with a ' +
  'one-line reason where nonsense — you are the arbiter; genuinely out-of-scope items — tools/ changes, the fenced slnx divergence — stay ' +
  'residual_high for the terminal sweep):\n' + JSON.stringify(residuals, null, 1) + '\n' + FENCES + '\nGates: biome + tsc/tsgo + vitest ' +
  'run green if TS files touched; uv run pytest tests/python -q green if python touched; positional dotnet test on touched cs projects; ' +
  'uv run python -m tools.assay docs green (the .md estate); root allowlist pytest green. WRITE-FULLY; return the fix-log.'
const gatePrompt = (residuals) => 'Rasm monorepo — the tests-infra ASCENSION campaign, TERMINAL GATE-VERIFY (the last pass; no further ' +
  'spawning). Run EVERY gate live, in this order, and FIX any failure in place (write, never defer), re-running until green: ' +
  '(1) pnpm install — clean, lockfile settled. (2) pnpm exec biome check tests/typescript + touched root configs — clean. (3) tsc + tsgo ' +
  'dual-floor typecheck over the TS estate — green. (4) pnpm exec vitest run — green; then RASM_TESTKIT_CONTAINERS=1 pnpm exec vitest run ' +
  '— container lanes green (colima/docker available). (5) pnpm exec playwright test — self-proof suites green, skips named. ' +
  '(6) uv run pytest tests/python -q — FULL suite green. (7) uv run python -m tools.assay static over tests/ + touched configs — green; ' +
  'uv run python -m tools.assay self-test — healthy; uv run python -m tools.assay docs — green. (8) dotnet build then positional dotnet ' +
  'test on tests/csharp/_testkit, _scenariokit (spec projects where present), _architecture, and tests/csharp/tools projects — green ' +
  'EXCEPT the known fenced red: AssemblyBoundaryLaws.WorkspaceSolutionMatchesDiskAndCarriesTheScenarioHome (the concurrent session\'s ' +
  'Rasm.Grasshopper slnx/disk divergence) — report it fenced-red, do NOT fix it by editing Workspace.slnx/libs/ or weakening the law. ' +
  '(9) uv run pytest tests/python/_testkit/test_policy.py -q — root allowlist green; then an fd-based litter sweep: zero unexpected root ' +
  'entries, zero stray tool output outside .cache/.artifacts. ' + FENCES + '\nOPEN RESIDUALS handed to the terminal sweep (do not fix ' +
  'tools/ or libs/ items; verify each is still accurately stated, drop solved ones):\n' + JSON.stringify(residuals, null, 1) +
  '\nReturn allGreen, the per-gate ledger (green | fixed-then-green | fenced-red | red — a red you could not fix must carry its exact ' +
  'failure note), fixedFiles, surviving residual_high, and a terse summary.'

// --- [COMPOSITION] -----------------------------------------------------------------------

if (!RUN.size) { log('No valid lanes — pass nothing (all lanes) or any of ts|py|cs.'); return { lanes: [], scopes: 0 } }
const laneScopes = (k) => SCOPES.filter((s) => s.lane === k)
log('Ascension: lanes [' + [...RUN].join(', ') + '] -> ' + [...RUN].reduce((n, k) => n + laneScopes(k).length, 0) +
  ' scopes x 3 passes, then smooth + gate')

// --- [LANES]
const laneRuns = (await parallel([...RUN].map((k) => async () => {
  const out = []
  for (const scope of laneScopes(k)) {
    const init = await agent(initialPrompt(scope), { label: 'init:' + scope.key, phase: 'Initial', model: 'fable', effort: 'max', schema: FIXLOG, stallMs: STALL })
    const crit = await agent(critiquePrompt(scope, init), { label: 'critique:' + scope.key, phase: 'Critique', model: 'fable', effort: 'xhigh', schema: FIXLOG, stallMs: STALL })
    const red = await agent(redteamPrompt(scope, crit), { label: 'redteam:' + scope.key, phase: 'Redteam', model: 'fable', effort: 'max', schema: FIXLOG, stallMs: STALL })
    out.push({ scope: scope.key, init, crit, red })
  }
  return out
}))).filter(Boolean).flat()

// --- [SMOOTH]
phase('Smooth')
const seen = new Set()
const residuals = []
for (const r of laneRuns) for (const k of ['init', 'crit', 'red']) {
  const x = r[k]
  if (x && x.residual_high) for (const res of x.residual_high) {
    const key = (res.files || []).slice().sort().join(',') + '|' + res.claim
    if (!seen.has(key)) { seen.add(key); residuals.push(res) }
  }
}
log('Lanes done: ' + laneRuns.length + ' scopes through 3 passes; ' + residuals.length + ' unique residuals -> smoothing')
const smooth = await agent(smoothPrompt(residuals), { label: 'smooth:estate', phase: 'Smooth', model: 'fable', effort: 'max', schema: FIXLOG, stallMs: STALL })
const openAfterSmooth = (smooth && smooth.residual_high) || residuals

// --- [GATE]
phase('Gate')
const gate = await agent(gatePrompt(openAfterSmooth), { label: 'gate:terminal', phase: 'Gate', model: 'fable', effort: 'xhigh', schema: GATE_SCHEMA, stallMs: STALL })

return {
  lanes: [...RUN], scopes: laneRuns.map((r) => r.scope),
  verdicts: laneRuns.map((r) => ({ scope: r.scope, init: r.init && r.init.verdict, crit: r.crit && r.crit.verdict, red: r.red && r.red.verdict })),
  smoothed: (smooth && smooth.files) || [], allGreen: !!(gate && gate.allGreen), gates: (gate && gate.gates) || [],
  residual_high: (gate && gate.residual_high) || openAfterSmooth,
}
