# [TESTS_BRIDGE_01_CONTEXT]

## [TRANSCRIPT]

Role: context / Tests and Bridge Context agent.

Scope: read-only context pass over `tests/csharp/**`, `tools/rhino-bridge/**`, and the `docs/standards` rules needed to recommend future `AGENTS.md` overlay changes. The only write is this temporary report.

Loaded governing sources:
- `CLAUDE.md:14-30` routes `.spec.cs` and `.verify.csx` through `testing-cs`, and Markdown through `docs/standards`.
- `CLAUDE.md:116-132` separates static analysis, unit tests, and Rhino runtime verification; `bridge verify` is the host/runtime rail.
- `docs/standards/README.md:68-79` routes shared documentation rules and `AGENTS.md` standards.
- `docs/standards/AGENTS.md:13-18` requires `CLAUDE.md`, root `AGENTS.md`, and `docs/standards/README.md` before standards edits, with wider reads for `AGENTS.md` work.
- `docs/standards/agents-md.md:1-4` defines `AGENTS.md` as a scoped behavioral overlay, not README, command catalog, roadmap, validation ladder, or research summary.
- `docs/standards/agents-md.md:30-41` requires produced overlays to carry scope, read behavior, future-standard posture, owner contract, route-away rules, rejections, and local stop behavior.
- `docs/standards/proof.md:18-27` ranks repository truth and executed local verification over prose.
- `docs/standards/explanation/test-strategy.md:174-176` says static/build, unit/property, mutation, fuzz, benchmark, snapshot/visual, and host/runtime scenario rails must stay distinct.

Loaded testing and bridge sources:
- Runtime skill `testing-cs` says tests are adversarial laws with independent oracles, and bridge scenarios own native Rhino/GH runtime behavior (`/Users/bardiasamiee/.codex/skills/testing-cs/SKILL.md:11-17`).
- `tests/csharp/AGENTS.md:1-4` scopes the C# test-tree overlay and delegates universal policy to root and `testing-cs`.
- `tests/csharp/libs/AGENTS.md:1-4` scopes source-mirrored library spec deltas.
- `tools/rhino-bridge/AGENTS.md:1-4` scopes bridge operator deltas and routes architecture, command catalog, output contract, env vars, and failure reading to `tools/rhino-bridge/README.md`.
- `tools/rhino-bridge/README.md:1-18` defines the bridge as real RhinoWIP/RhinoCode evidence, not a synthetic unit-test framework.

Folder discovery:
- `fd '^(AGENTS|README)\.md$' tests/csharp tools/rhino-bridge docs/standards -H` found `docs/standards/AGENTS.md`, `docs/standards/README.md`, `tests/csharp/AGENTS.md`, `tests/csharp/libs/AGENTS.md`, `tools/rhino-bridge/AGENTS.md`, and `tools/rhino-bridge/README.md`.
- No `tests/csharp/README.md` or `tests/csharp/libs/README.md` exists in the discovered set.
- `fd '\.(spec\.cs|verify\.csx)$' tests/csharp/libs` found 49 non-scenario library specs and 21 bridge scenarios.
- Project distribution: `Rasm=40`, `Rasm.Rhino=21`, `Rasm.Grasshopper=7`, `Rasm.Materials=2` across specs plus scenarios.
- `_testkit` owners found: `Approx.cs`, `Fixtures.cs`, `Gens.cs`, `HostBundle.cs`, `Numeric.cs`, `Scenarios/Capture.cs`, `Scenarios/DocumentScope.cs`, `Scenarios/Probe.cs`, `Serializer.cs`, and `Spec.cs`.
- Other rail folders found: `_architecture/AssemblyBoundaries.spec.cs`, `_tooling/TestingRail.spec.cs`, `_benchmarks/Program.cs`, and `_fuzz/Program.cs`.

Representative files read for patterns:
- `tests/csharp/_testkit/Spec.cs`
- `tests/csharp/_testkit/Gens.cs`
- `tests/csharp/_testkit/Scenarios/DocumentScope.cs`
- `tests/csharp/_testkit/Scenarios/Probe.cs`
- `tests/csharp/libs/Rasm/Analysis/Bounds.spec.cs`
- `tests/csharp/libs/Rasm/Vectors/Matrix.spec.cs`
- `tests/csharp/libs/Rasm/Analysis/scenarios/analysis-native-rail.verify.csx`
- `tests/csharp/libs/Rasm/Vectors/scenarios/vectors-cloud.verify.csx`
- `tests/csharp/libs/Rasm.Rhino/Blocks/scenarios/blocks-core-rail.verify.csx`
- `tests/csharp/libs/Rasm.Grasshopper/UI/scenarios/gh-ui-motion-layout.verify.csx`

Search checks:
- Searched for bridge-deferred markers, fake/static-host language, scenario reference directives, direct rail peeking, helpers, `Skip`, `#r`, `#load`, `Scenario.Run`, `facts.Add`, and bridge marker usage across `tests/csharp/**` and bridge docs.
- The scenario search found `Scenario.Run` and `facts.Add` broadly across scenario files, and found no scenario-authored `#r` or `#load` hits in the sampled command output.
- The direct rail-peeking search found some `.IsSome`/`.IsNone` assertions in specs, but no `.IsSucc` or `.IsFail` primary-proof pattern in the sampled result set.

No quality gates were run. This is a read-only context report, not an implementation or proof pass.

## [FOLDER_CONTEXT]

`tests/csharp/AGENTS.md` already states the core test contract:
- Build adversarial laws, not confirmation checks (`tests/csharp/AGENTS.md:18-23`).
- Expected values must come from independent math, smaller models, metamorphic relations, fixture geometry, documented runtime behavior, or bridge evidence (`tests/csharp/AGENTS.md:23-24`).
- Shared capabilities extend `_testkit`; architecture, tooling, benchmark, fuzz, library spec, and bridge scenario rails route to their owners (`tests/csharp/AGENTS.md:25-34`).
- Oracle rules prefer `Spec.ForAll`, `Spec.Metamorphic`, and `Spec.Regression`, explicit seeds only for preserved regressions, real oracles over shape checks, distinct payloads, and `Spec` rail helpers (`tests/csharp/AGENTS.md:35-43`).
- Rejections ban implementation mirrors, primary `.IsSucc`/`.IsFail`/`.IsSome`/`.IsNone`, shape-only constructor proof, local suppressions, transient generated output, and scenario `#r`/`#load`/absolute build paths (`tests/csharp/AGENTS.md:44-52`).
- Stop rules route host assembly identity, staged-reference, and LanguageExt bootstrap failures to bridge setup before weakening tests or scenarios (`tests/csharp/AGENTS.md:53-55`).

`tests/csharp/libs/AGENTS.md` adds library-spec-specific density and ownership:
- Specs mirror source ownership; add a spec only when no truthful owner exists, and keep module-local generators local until multiple specs need the same shape (`tests/csharp/libs/AGENTS.md:14-20`).
- Shape-only tests should become law matrices varying construction, projection, unsupported output, failure category, and independent oracle (`tests/csharp/libs/AGENTS.md:21-28`).
- Dense case families should use generated sweeps, union sweeps, reflected `TheoryData`, product generators, or dispatch tables before repeated facts (`tests/csharp/libs/AGENTS.md:30-37`).
- Native Rhino/GH2 behavior belongs in mirrored source-path scenarios; static specs may classify it but must not pretend to execute native runtime paths (`tests/csharp/libs/AGENTS.md:38-44`).
- Rejections ban one-spec helper files, circular implementation snapshots, broad specs, low-value coverage kept for counts, and public default-struct gaps without invalid-default laws (`tests/csharp/libs/AGENTS.md:45-53`).

`tools/rhino-bridge/AGENTS.md` and `README.md` define the bridge boundary:
- Invoke bridge commands only when static managed gates cannot answer runtime host behavior (`tools/rhino-bridge/AGENTS.md:12-20`).
- Scenarios are source-only; bridge owns reference projection, host assemblies, and injected usings (`tools/rhino-bridge/AGENTS.md:22-28`).
- Scenario authors use the testkit scenario harness and bridge markers; no legacy key-value parsing (`tools/rhino-bridge/AGENTS.md:29-34`).
- Artifacts are ephemeral evidence under the bridge artifact route and must not be persisted as durable documentation truth (`tools/rhino-bridge/AGENTS.md:35-38`).
- Bridge README maps library scenarios under `tests/csharp/libs/<Project>/<MirrorPath>/scenarios/` to `libs/csharp/<Project>/<Project>.csproj` without manifests or catalogs (`tools/rhino-bridge/README.md:90-99`).
- Bridge verify writes aggregate JSON and scenario evidence under `.artifacts/rhino/verify/<run-id>` (`tools/rhino-bridge/README.md:66-71`, `tools/rhino-bridge/README.md:163-170`).
- The bridge marker contract says `Scenario.Run` emits one `facts={json}` plain line and one `rasm.rhino-bridge.evidence=facts={json}` marker, and agents should parse the prefixed evidence marker (`tools/rhino-bridge/README.md:213-235`).
- Host references resolve from installed RhinoWIP app-bundle evidence; scenario references are client-applied and shadow-copied; GH2 scenarios must drive host state through `Rasm.Grasshopper.UI` wrappers, not raw GH2 types (`tools/rhino-bridge/README.md:237-255`).
- Bridge failure reading separates build, resolve, connect, execute diagnostics, package collision, unsupported source check, and apphost failure (`tools/rhino-bridge/README.md:256-269`).

Real test patterns match the stated doctrine:
- `_testkit/Spec.cs` centralizes `ForAll`, algebraic/metamorphic laws, rail assertions, category assertions, cases, output dispatch, projection matrices, validity matrices, support matrices, model-based checks, and boundary adapters (`tests/csharp/_testkit/Spec.cs:13-24`, `tests/csharp/_testkit/Spec.cs:48-59`, `tests/csharp/_testkit/Spec.cs:67-91`, `tests/csharp/_testkit/Spec.cs:120-131`, `tests/csharp/_testkit/Spec.cs:203-265`, `tests/csharp/_testkit/Spec.cs:289-312`).
- `_testkit/Gens.cs` supplies edge-biased numeric and domain generators, uses option-routed generation to preserve shrinking, and keeps mesh fixtures as pure data to avoid native `Mesh` handle ownership in generators (`tests/csharp/_testkit/Gens.cs:12-45`, `tests/csharp/_testkit/Gens.cs:150-170`).
- `Bounds.spec.cs` explicitly marks native evaluation as bridge-deferred while static tests own catalog, factories, dispatch, and pre-native rejection rails (`tests/csharp/libs/Rasm/Analysis/Bounds.spec.cs:8-17`, `tests/csharp/libs/Rasm/Analysis/Bounds.spec.cs:55-123`, `tests/csharp/libs/Rasm/Analysis/Bounds.spec.cs:152-167`).
- `Matrix.spec.cs` uses independent numeric products, residual checks, normal equations, reconstruction laws, eigenpair residuals, and receipt metadata instead of output mirroring (`tests/csharp/libs/Rasm/Vectors/Matrix.spec.cs:55-71`, `tests/csharp/libs/Rasm/Vectors/Matrix.spec.cs:83-115`, `tests/csharp/libs/Rasm/Vectors/Matrix.spec.cs:232-316`).
- `analysis-native-rail.verify.csx` uses real Rhino `Curve` instances and checks native length, midpoint, boundary validity, and facts through `Scenario.Run` (`tests/csharp/libs/Rasm/Analysis/scenarios/analysis-native-rail.verify.csx:13-36`).
- `vectors-cloud.verify.csx` proves native cloud, Rhino point-cloud, hull, curvature, and receipt behavior with explicit runtime facts (`tests/csharp/libs/Rasm/Vectors/scenarios/vectors-cloud.verify.csx:15-48`, `tests/csharp/libs/Rasm/Vectors/scenarios/vectors-cloud.verify.csx:51-117`, `tests/csharp/libs/Rasm/Vectors/scenarios/vectors-cloud.verify.csx:120-181`).
- `blocks-core-rail.verify.csx` uses real `RhinoDoc`, `InstanceDefinitions`, `RhinoBlocks.Live`, and facts for native index/member evidence (`tests/csharp/libs/Rasm.Rhino/Blocks/scenarios/blocks-core-rail.verify.csx:8-38`).
- `gh-ui-motion-layout.verify.csx` documents why live GH2 layout proof belongs in the bridge, drives only `Rasm.Grasshopper.UI` wrappers, asserts committed object positions and snap seams, and records detailed facts (`tests/csharp/libs/Rasm.Grasshopper/UI/scenarios/gh-ui-motion-layout.verify.csx:5-13`, `tests/csharp/libs/Rasm.Grasshopper/UI/scenarios/gh-ui-motion-layout.verify.csx:48-78`, `tests/csharp/libs/Rasm.Grasshopper/UI/scenarios/gh-ui-motion-layout.verify.csx:90-117`).

## [FINDINGS]

1. The test overlays already contain the core doctrine, but `agents-md.md` does not require a test-specific owner contract for test-tree overlays.

Evidence: `tests/csharp/AGENTS.md:18-24` and `tests/csharp/libs/AGENTS.md:21-28` are strong. `docs/standards/agents-md.md:30-41` defines generic overlay functions but does not name a conditional test-proof function for folders that own tests. A future `AGENTS.md` author could satisfy the generic contract while omitting adversarial-oracle, bridge-boundary, mutation, and helper-promotion rules.

Risk: weak local overlays can regress to "run test command X" guidance instead of preventing confirmation checks, fake native proof, or helper proliferation.

2. The bridge docs and test overlays create a useful but fragile distinction: bridge is not a unit-test framework, yet test-owned `*.verify.csx` files are executable runtime laws.

Evidence: `tools/rhino-bridge/README.md:3-18` says the bridge validates real project files, source files, assemblies, and scripts, and not artificial unit-test suites. `tools/rhino-bridge/README.md:96-99` still defines library scenarios under `tests/csharp/libs/.../scenarios`. `tests/csharp/libs/AGENTS.md:38-44` says runtime gaps become executable scenario work, not skipped xUnit or shape-only assertions.

Risk: a future overlay rewrite could over-read "not a unit-test framework" as "do not write bridge scenarios," or over-read "scenario proof" as license for synthetic probes. The universal rule should name both halves: no artificial bridge tests, but real host behavior must be proven by source-owned scenarios.

3. Static-vs-bridge classification is strong in examples, but it is not yet a universal `AGENTS.md` production rule.

Evidence: `Bounds.spec.cs:8-9`, `Conformance.spec.cs:9`, `Curves.spec.cs:9`, `Measure.spec.cs:9`, `Mesh.spec.cs:9`, and many other specs use `BRIDGE-DEFERRED` comments. `tests/csharp/libs/AGENTS.md:40-43` says static specs may classify bridge-owned behavior and must not fake native runtime paths.

Risk: agents can add static tests that call native-looking APIs, use Rhino handles, or assert host behavior without first classifying the behavior. The overlay should require classification before assertions: static-managed, bridge-owned runtime, architecture/tooling/fuzz/benchmark, or unsupported proof gap.

4. `_testkit` is correctly acting as a high-density law surface, but the universal overlay standard does not yet say how test helpers earn promotion.

Evidence: `tests/csharp/AGENTS.md:27` and `tests/csharp/libs/AGENTS.md:19` require shared law/generator/oracle capability to serve multiple specs. `_testkit/Spec.cs:203-265` shows promoted reusable projection, output, validity, and support matrices. `_testkit/Gens.cs:35-45` and `_testkit/Gens.cs:150-170` show generator and pure-fixture capability that prevents repeated brittle setup.

Risk: future `AGENTS.md` files may ban helpers as a slogan but fail to name the replacement route: keep spec-local data local, and promote only universal law/oracle/generator/scenario capability with multiple real consumers.

5. Existing specs still expose a residual direct rail-peeking hazard that should be codified more precisely.

Evidence: `tests/csharp/AGENTS.md:42-48` rejects `.IsSucc`, `.IsFail`, `.IsSome`, and `.IsNone` as primary proof. The search found direct `.IsSome`/`.IsNone` checks in several specs, for example `tests/csharp/libs/Rasm.Grasshopper/UI/Motion.spec.cs:162-175`, `tests/csharp/libs/Rasm/Vectors/Field.spec.cs:206`, and `tests/csharp/libs/Rasm/Vectors/Sample.spec.cs:236-237`.

Risk: some direct option-state checks may be supplemental and legitimate, but the current wording leaves review friction. A universal rule should say raw rail state is allowed only as a secondary invariant after a value/category/oracle assertion, or when the subject under test is the rail-state policy itself. Otherwise use `Spec.Some`, `Spec.None`, `Spec.Succ`, `Spec.FailCategory`, `Spec.Valid`, or `Spec.Invalid`.

6. Bridge evidence format is now structured, but future overlays need to prevent legacy stdout parsing and durable artifact-path claims.

Evidence: `tools/rhino-bridge/README.md:226-235` says `Scenario.Run` emits one `facts={json}` plain line plus one prefixed evidence marker, and agents parse the prefixed marker. `tools/rhino-bridge/AGENTS.md:31-38` says use fact bags/markers and treat artifacts as run-local evidence. `_testkit/Scenarios/Probe.cs:47-68` implements `Scenario.Run` with `BridgeMarker.EmitFacts`.

Risk: weak bridge instructions may parse human-readable output or persist `.artifacts/rhino/verify/...` paths as durable truth. That creates fake proof that cannot refresh cleanly.

7. The current folder inventory supports high-density tests and real bridge proof, not minimal smoke checks.

Evidence: 49 library specs and 21 scenarios are present; `Matrix.spec.cs:83-115` packs solve, inverse, pseudoinverse, rank, least-squares receipt, and normal-equation oracles; `vectors-cloud.verify.csx:51-117` packs duplicate point behavior, native KNN, admission receipts, normal orientation, radius-limited search, and curvature classifications.

Risk: future overlay language that optimizes for "small tests" alone would be wrong here. The standard should prefer dense, owner-local laws where one generated or fixed fixture attacks multiple failure modes, while still rejecting broad owner drift.

## [RECOMMENDED_OVERLAY_CHANGES]

Recommended change for `docs/standards/agents-md.md`:
- Add a conditional produced function for test-owning overlays: when a folder owns tests, specs, scenarios, testkit, fuzz, benchmark, snapshot, mutation, or architecture test rails, the overlay must include a `TEST_CONTRACT` or equivalent slot. That slot must name the local proof rail, oracle requirement, native/runtime boundary, helper-promotion route, and stop rule.
- Add a route-away rule: command syntax and runner details belong to tool READMEs or `docs/testing-libs`; `AGENTS.md` names only local selector, owner, rejection, and stop behavior.
- Add a rejection pattern for test overlays: no confirmation checks, no implementation mirrors, no fake host/runtime proof, no skipped xUnit as a bridge gap, no single-use helper files, no coverage-count preservation.

Recommended change for root `AGENTS.md` test routing:
- Preserve the current read-order routes, but add a universal test-edit trigger: before adding or changing a test assertion, classify the behavior as static-managed law, bridge-owned runtime scenario, architecture/tooling/fuzz/benchmark, or proof gap.
- State that bridge-owned behavior must become source-owned `*.verify.csx` scenario work under `tests/csharp/libs/<Project>/<MirrorPath>/scenarios/`, not a static fake, skipped unit test, mock host, or documentation-only caveat.
- State that direct rail state checks are supplemental unless the tested contract is rail state itself; primary proof uses `Spec` helpers or an independent oracle.

Recommended change for `tests/csharp/AGENTS.md`:
- Add `.verify.csx` explicitly to the `[REQUIRED]` line, not only `.spec.cs`, so local text mirrors `CLAUDE.md:20-22`.
- Add a classification requirement before editing assertions: read the production owner and decide static-managed vs bridge-owned before choosing xUnit or scenario.
- Add a mutation-survivor triage rule from the skill: classify survivors as missing oracle, equivalent mutant, bridge-owned path, or product bug before editing tests.
- Add a narrow rule for raw rail peeking: `IsSome`/`IsNone`/`IsSucc`/`IsFail` may appear only as secondary invariant or rail-policy subject; otherwise use `Spec` helpers.

Recommended change for `tests/csharp/libs/AGENTS.md`:
- Add a trigger: when a spec has `BRIDGE-DEFERRED` or touches Rhino/GH2 host behavior, read sibling `scenarios/` and update or add source-owned scenario proof for runtime gaps.
- Strengthen "Pair each bridge scenario with an owning source file" into an action rule: scenario proof must name or be invokable with the owning source/project through bridge check or verify routing; do not add manifests or scenario catalogs.
- Add a density standard: prefer one owner-local law matrix or generated case sweep that attacks construction, projection, unsupported outputs, failure categories, receipts, and independent oracle over repeated narrow facts.

Recommended change for `tools/rhino-bridge/AGENTS.md`:
- Clarify the README tension: the bridge is not a synthetic unit-test framework, but real host behavior in library code is proven by source-owned `*.verify.csx` runtime laws under `tests/csharp/libs`.
- Add a stop rule for fake proof: if a proposed bridge scenario has no real production source, host API, assembly freshness, document/canvas, or native geometry fact to prove, reject it as an artificial probe.
- Add a marker rule: scenario evidence must go through `Scenario.Run` and `FactBag`; parse `BridgeMarker.Evidence("facts", ...)`, not raw human-readable lines or durable artifact paths.

## [UNIVERSAL_PATTERNS_TO_CODIFY]

- Test overlays must prevent weak tests, not merely list test commands.
- Every test assertion starts with behavior classification: static-managed, bridge-owned runtime, architecture/tooling/fuzz/benchmark, mutation, or proof gap.
- Static specs prove pure managed behavior: constructors, value objects, smart enums, unions, deterministic algorithms, failure categories, projection dispatch, metadata generated from source, and pre-native guards.
- Bridge scenarios prove real host behavior: RhinoCommon/GH2 runtime APIs, native geometry validity, document/canvas/UI state, viewport/capture behavior, assembly freshness, host resolver behavior, and native topology.
- No static fake of host runtime: no mocks, headless substitutes, Docker/VM Rhino substitutes, Rhino.Inside replacement claims, skipped xUnit gaps, or shape-only host assertions.
- Bridge scenarios are source-only runtime laws: no scenario-authored `#r`, `#load`, absolute build-output paths, manifests, catalogs, or per-scenario path maps.
- Scenario evidence is structured: use `Scenario.Run`, `FactBag`, and bridge evidence markers; parse JSON facts through `BridgeMarker.Scan`, not legacy per-fact stdout.
- Oracles must be independent: closed-form math, smaller model, metamorphic relation, fixture geometry, documented host behavior, bridge observation, receipt invariant, or failure category.
- Confirmation checks are rejected: no expected value copied from the implementation path, no asserting an object against fields constructed inside the same test unless it guards an invariant/failure rail, and no coverage-count preservation.
- Dense laws beat fact sprawl: one generated domain should attack multiple dimensions when the owner exposes them.
- Helper promotion must be earned: keep one-spec data and generators local; promote only universal law/oracle/generator/serializer/scenario capability consumed by multiple specs.
- Direct monadic rail peeking is not primary proof: use `Spec.Succ`, `Spec.FailCategory`, `Spec.Valid`, `Spec.Invalid`, `Spec.Some`, and `Spec.None` unless the rail state is itself the contract.
- Mutation survivors are triage input, not a blind request for more assertions: classify as missing oracle, equivalent mutant, bridge-owned path, or product bug.
- Runtime artifacts are ephemeral proof receipts: report the command, scenario, status, and fact keys; do not preserve run-local `.artifacts` paths as durable documentation truth.
- `AGENTS.md` files should route command syntax to tool READMEs and `docs/testing-libs`; overlays own local action constraints, boundaries, and stop rules.

## [CONFIDENCE]

High for folder context and current patterns. I read the governing overlays, bridge README, docs standards for `AGENTS.md`, proof, and test strategy, inventoried the test/scenario tree, and sampled representative static specs, testkit surfaces, and bridge scenarios with line references.

Medium for completeness of weak-test detection. I searched for common hazards (`IsSome`, `IsNone`, `#r`, `#load`, helper names, skips, bridge markers, facts), but I did not review all 49 specs and 21 scenarios line by line.

No runtime confidence is claimed. I did not run `static`, `test`, `bridge`, mutation, fuzz, benchmark, or docs validation gates because the task was a read-only _reports/report pass.
