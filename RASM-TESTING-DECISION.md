# RASM-TESTING-DECISION

Binding blueprint for the polyglot testing-infrastructure campaign. Build legs execute this document exactly; deviations return as residuals, never silent improvisation. The repo is planning-first: infrastructure lands complete and proven now, content arrives later. Every lane must hold a failing-state proof — a gate nobody can run is deleted, not kept as aspiration.

## [01]-[ARCHITECTURE]

Folder law for `tests/` (all languages, one scheme):

```
tests/
├── README.md                  # cross-language management law (leg 7)
├── ast-grep/                  # structural rule corpus (pass/fail fixtures)
├── contracts/                 # cross-language frozen corpus: wire bytes + canonical JSON per seam/message;
│                              # C# is sole producer, Python/TS round-trip read-only (leg 7 stands up README + layout)
├── csharp/
│   ├── README.md              # C# test-authoring law
│   ├── _architecture/         # boundary + infra-primitive laws (proves both kits)
│   ├── _benchmarks/           # BDN switcher + regression gate verb
│   ├── _scenariokit/          # host-aware scenario SDK (Rasm.ScenarioKit)
│   ├── _testkit/              # host-free adversarial law substrate (Rasm.TestKit)
│   ├── libs/                  # per-package AssayTestShell shells (content later)
│   ├── scenarios/             # scenario content home (Rasm.Scenarios) + _references/
│   └── tools/                 # infra-testing suites (cs-analyzer, rhino-bridge Contract/Supervisor)
├── python/
│   ├── README.md              # Python test-authoring law
│   ├── _testkit/              # project-agnostic kit (spec/strategies/seams/env/bench/policy)
│   ├── libs/                  # per-package suites mirroring libs/python (conftest shells now, specs at buildout)
│   └── tools/                 # assay + py_analyzer suites
└── typescript/
    ├── README.md              # TS test-authoring law
    ├── .api/                  # dev-tool API catalog tier: vitest family, fast-check, testcontainers,
    │                          # pglite, playwright, stryker, DOM environments, k6 types
    ├── _architecture/         # gauge suites: edge-ledger import, subpath-purity, admission audits
    │                          # (home documented now, suites land at TS buildout)
    ├── _testkit/              # shared TS kit: a workspace package (corpus readers, law combinators,
    │                          # Schema-derived arbitraries, harness layers, e2e drivers)
    └── e2e/                   # playwright home (stood up at TS buildout, documented now)

Polyglot kit law: shared test logic lives in ONE per-language kit under `tests/<language>/_testkit` — `tests/csharp/_testkit` (+ `_scenariokit` for the host-aware scenario SDK), `tests/python/_testkit`, `tests/typescript/_testkit` (a pnpm workspace package; colocated specs import it through the workspace graph). Test kits never live under `libs/` — libs is the production plane. Per-package suite homes mirror `libs/<language>` where the ecosystem separates tests from source (C# shells, Python `tests/python/libs/<package>`); TS unit specs colocate beside source — spec placement is language-idiomatic, kit placement is not. The `libs/typescript` `proof` folder is DISSOLVED (executed, not deferred to buildout): corpus bytes and producer/consumer law live in `tests/contracts/`; TS corpus readers, law combinators, Schema-derived arbitraries, and harness layers land in `tests/typescript/_testkit`; the gauge audits (edge-ledger, subpath-purity, admission) land in `tests/typescript/_architecture`; e2e drivers in `tests/typescript/e2e`; mutation thresholds stay data in `.config/stryker.config.json`; `tests/typescript/.api/` is the dev-tool API catalog tier; the `@rasm/ts` exports map ships no test-infra subpath. Nothing cross-language lives in any single language's tree: the neutral seams are `tests/contracts/`, the shared PG container image, proto descriptors, and assay.
```

- Casing: lowercase tier/grouping dirs, `_`-prefixed kit dirs, PascalCase project dirs only where the dir IS a C# project (`Rasm.*`, `Contract`, `Supervisor`, `Meta`). No PascalCase content folders. TS unit specs colocate beside source per vitest idiom; `tests/typescript/` owns only kit, e2e, and cross-cutting suites — the law doc states this split explicitly.
- Artifact law: every tool writes under `.artifacts/<language>/<tool>/` (reports, coverage, stryker, benchmarks, trx, hypothesis observability) or `.cache/<tool>/` (temp/work dirs). Zero root litter; `.gitignore` stops absorbing `coverage/`, `test-results/`, `mutants/`, `.stryker-tmp/`, root `dist/` once routing lands.
- Tool-admission litter rule: a leg that adds or reconfigures any tool must prove its caches and outputs land under `.cache/` or `.artifacts/` before finishing — routed through the tool's own documented configuration (config-file setting first, CLI flag second), never wrapper scripts or conftest shims. Gate: after the leg's checks run, `git status` plus a root listing shows zero new root entries.
- Test lanes (orthogonal to language): `unit` (in-process, deterministic time, no sockets), `property`, `integration` (real process/IO boundary — containers, subprocess, loopback gRPC; the word is reserved for this), `scenario` (live-host evidence via rhino-bridge), `benchmark` (separate session), `mutation` (assay-gated).

## [02]-[CSHARP_FINALIZE]

Packages (`Directory.Packages.props` Test Stack group + `Directory.Build.props` wiring):
- ADD `Microsoft.Testing.Extensions.Retry` 2.2.3 (all `IsTestProject`, PrivateAssets=all) — MTP-native flake rail.
- REPLACE `Microsoft.Testing.Platform.Extensions.TrxReport.Abstractions` transitive floor usage with full `Microsoft.Testing.Extensions.TrxReport` 2.2.3 on `IsTestProject`; TRX output routes to `.artifacts/csharp/trx/` via MTP `--report-trx` defaults documented in the C# README.
- ADD `Verify.DiffPlex` 3.3.0 wherever `Verify.XunitV3` is referenced (cs-analyzer tests today). `Verify.ImageMagick` deferred until render-capture snapshots exist — recorded here, not installed.
- KEEP xunit.v3 3.2.2 + mtp-v2; collapse to plain `xunit.v3` at v4 GA (watch item). KEEP CsCheck, ArchUnitNET, coverlet.MTP, BenchmarkDotNet as-is.

Stryker.NET (`.config/stryker-config.json`, the one config for solution mode; Stryker.NET auto-discovers only in cwd, which is forgone deliberately — assay owns every mutation invocation and passes `--config-file .config/stryker-config.json`):
- `"test-runner": "mtp"`, `"solution": "Workspace.slnx"`, `"output": ".artifacts/csharp/stryker"`, `"with-baseline"` + Disk provider, thresholds high 90 / low 85 / break 80, reporters json + html (schema JSON feeds the unified gate). Assay's mutation route points at this config; no `StrykerOutput/` can ever appear at root. Watch items: Stryker.NET #3516 (per-test coverage under MTP) and #3655 (bail).
- Vacuity guard: with lib shells empty, a solution mutation run is UNSUPPORTED in assay (zero-target), never a green pass.

Benchmarks: `_benchmarks` already carries the gate verb (Pass|TooNoisy|Breach, exit 0/1/2). JSON exporter stays; CI trend-action wiring is out of scope until CI files exist (watch item).

testing-cs skill repair (`.claude/skills/testing-cs/`): delete the 9 dead `docs/stacks/csharp/testing-libs/*` references; delete the phantom `_tooling`/`_fuzz` rails (SharpFuzz stays unadmitted — no pin, no project, no prescription); refresh the testkit file table to the rebuilt set (Spec, Approx, Gens, Numeric, Laws, Seams, Manifests + ScenarioKit sibling); add the xunit v3 3.2.x surfaces the doctrine lags (TheoryDataRow per-row metadata, Assert.Skip*, Explicit, TestPipelineStartup, MatrixTheoryData) and the CsCheck classify/ChiSquared/Faster/model-SampleParallel surfaces the rebuilt Spec now wraps.

## [03]-[PYTHON_PLATFORM]

`pyproject.toml` deltas:
- Bump/keep current: pytest 9.1.x, hypothesis 6.155.x, inline-snapshot, dirty-equals, mutmut 3.6.x, pytest-{xdist,randomly,rerunfailures,socket,timeout,deadfixtures,benchmark} at latest.
- coverage 7.15+ / pytest-cov 7.1+: add `[tool.coverage.run] patch = ["subprocess"]`; DELETE `.config/coverage-subprocess.ini` (its mirror invariant dies). `.config/coverage-mutmut.ini` folds into pyproject via `[tool.coverage.paths]` + env override if provable in the leg; if mutmut's absolute-path keying genuinely requires the separate file, it survives with a one-line comment and the stale "98% gate" comment corrected.
- Mutation targeting: replace the 4 hardcoded `--deselect` test node-ids with a `subprocess` pytest marker + `-m "not subprocess"` in the mutmut args; mark the 4 tests.
- ADD `import-linter` + `grimp` (dev group): one `[tool.importlinter]` contract set encoding the post-rebuild assay layering (model → diagnostics/settings/store → routing/exec → rails → registry) and reserved contracts for `libs/python` strata; wired as an assay static-route check for Python.
- Hypothesis: observability callbacks write to `.artifacts/python/hypothesis/`; profiles unchanged. HypoFuzz rejected (non-OSI license). pytest-run-parallel deferred (no free-threaded lane planned).

## [04]-[ASSAY_REBUILD]

Execute the audited migration order; the operator stays usable at every step. Target module set (owners, approximate LOC):

```
model.py        wire ADT + status algebra (absorbs status.py); sheds all foreign parsing   (~620)
diagnostics.py  one wire-converter owner block per foreign diagnostic family, keyed by
                Tool.parser — argv sniffing dies; absorbs catalog decoders + SARIF folds    (~480)
catalog.py      the ONE total tool table: every spawn surface incl. provision/probe rows;
                typed splice slots; ad-hoc Tool() minting banned via py_analyzer rule       (~340)
settings.py     AssaySettings + Local/Ssh/Offload value objects                             (~450)
store.py        ArtifactStore + ArtifactScope + zstd history + retention                    (~420)
routing.py      change routing + closure + place/expand; deferred-import cycle dies         (~300)
exec.py         argv composition from catalog templates + local run + telemetry + retry;
                exposes the Executor port                                                   (~620)
remote.py       Ssh transport, manifests, artifact pull, remote prune                       (~480)
govern.py       leases + dotnet slots + concurrency + fan scheduling                        (~380)
aspect.py       weave; checked layer exported publicly                                      (~185)
registry.py     Bind rows + rail weave + emit/persist + Cyclopts; usage strings DERIVED
                from params metadata (_CLAIM_SLOTS/_VERB_SLOTS/_API_ARITY die)              (~420)
oracle.py       api boundary owner: Oracle protocol + host-bundle/nuget/pydist/tsdecl
                adapters + version-probed ilspy port + consumer-TFM ranking + typed cache   (~620)
rails/*         api shrinks to verb dispatch over oracle (~420); health.py absorbs
                registry self-test/probes/orphan hygiene (~260); others surgical
```

Steps, in order: (1) Executor port + public-contract test migration — rails receive `Executor` protocol, tests drive `main()`/`build_app` asserting decoded Envelopes; kills the 262-site monkeypatch estate first. (2) `diagnostics.py` extraction + `Tool.parser` key; model absorbs status. (3) Catalog totalization — all 5 ad-hoc Tool minting sites and all 16 command-surgery sites die; splice slots replace argv string editing (`-p:CspSarifDir=` write/re-parse becomes a typed field). (4) Engine split into exec/remote/govern (move, not rewrite). (5) `oracle.py` full rebuild — TFM policy row closes the standing `[API_TFM_RESOLUTION]` breakage; typed cache entries carry producer version + fingerprint. (6) Registry slimming (health rail out, private imports gone). (7) README rewritten contract-only (scope, first command, output contract, environment; command tables deleted — Cyclopts help + self-test are the source).

Preserve verbatim: the Envelope/one-write wire contract, status algebra, catalog-as-data axes, routing closure + AssayHostBound/AssayTestShell markers, lease/slot machinery, zstd history store, SARIF folding + defect-preserving caps, remote offload, aspect weave, channel discipline, stream spill/stall verdicts, automation unions. `py_analyzer` survives as a separate package; its one inverted import (`assay.rails.code.ts_language`) gets a neutral owner.

## [05]-[PYTHON_TEST_ESTATE]

After step (1) of [04] lands, collapse 14,895 → ~6,500 LOC with zero behavioral coverage lost:
- `_testkit/laws.py` gate rework: delete imperative `register_law`/`register_laws`; coverage derives from `@spec(subject)` decorations + one declarative `COVERS` tuple per module; value-only symbols auto-exempt by predicate. The ~600 LOC write-only ledger dies corpus-wide.
- Matrix folding as default idiom (provision fault table, docs promotion rows, strict-gate trilogy, params families, projector quartets). Tautology purge (frozen-raises, StrEnum roundtrips, isinstance-on-literal, `__all__` mirrors, encode==encode, meta-tests). Private-seam diet where a public driver exists; genuinely white-box seams (stall verdicts, crash recovery) stay.
- Keep the load-bearing survivor list verbatim (SARIF fold block, engine boundary suite, package commit-sentinel, api spill/cache replay, provision sanitizer egress, static phase ordering, mutation-gate argv laws, code prefilter laws, settings remote-env boundary + RBSM, registry census, main channel separation, py_analyzer exemplar, testkit env/policy gates).
- Fill named gaps: `resolve_languages`, status-join tie law; burn the py_analyzer 15-symbol blanket exemption down to per-symbol law-or-permanent-exempt.
- Delete the single external inline-snapshot dir (subsumed); keep inline-snapshot for genuine wire goldens.
- Hypothesis storage litter: a root `.hypothesis/` coexists with the routed `.cache/hypothesis/` — some lane (mutmut children, subprocess tests, or profile-less invocation) falls back to hypothesis's cwd default. Diagnose the producer, then route the hypothesis home/storage directory at the ONE bootstrap point every lane crosses (testkit level, via hypothesis's own configuration surface — verified against current hypothesis docs, never env plumbing per lane), delete the root dir, prove non-reappearance across unit + subprocess + mutation lanes, and drop the `.hypothesis/` gitignore row once impossible.
- Stryker litter proof: after wiring the mutation rows' `--config-file .config/stryker-config.json` / `--configFile .config/stryker.config.json` (+ Stryker.NET `--output .artifacts/csharp/stryker`, TRX `--report-trx --results-directory .artifacts/csharp/trx/<project>` per the C# leg residuals), compose the mutation argv for both languages and prove zero root litter; delete the stray root `.stryker-tmp/`. Also emit `mutation-testing-report-schema` JSON from mutmut via the small shim ([07]).

## [06]-[TYPESCRIPT_PLATFORM]

- Output routing: `vitest.config.ts` reportsDirectory/outputFile/screenshotDirectory/tracesDir → `.artifacts/typescript/{coverage,test-results}`; `nx.json` test/e2e outputs follow; playwright `outputDir` likewise. Remove the now-dead root-litter `.gitignore` rows.
- StrykerJS: one config at `.config/stryker.config.json` (vitest runner, temp under `.cache/stryker/`, reporters json+html into `.artifacts/typescript/stryker`, incremental ON in CI with the incremental file uncommitted per StrykerJS #6004); invocations carry `--configFile .config/stryker.config.json` — assay's mutation rows for both languages carry their config-file flags (wired in the assay legs). Per-package configs arrive with TS buildout. Browser-mode suites never mutated (unsupported).
- Root vite self-build dies: the "library" build of `vite.factory.ts` into root `dist/` is removed; `dist/` deleted; `vite.config.ts` remains the dev/typecheck anchor executing the factory without a root build artifact.
- pnpm hygiene: delete the inert 64-entry `minimumReleaseAgeExclude` (or activate `minimumReleaseAge` deliberately — activation is the recommended posture, exclusions re-derived then); delete the shadowing `package.json#workspaces` field; collapse the triple-listed `libs/typescript` packages entries to the correct pair.
- `@effect/vitest` 0.29.x declares peer `vitest ^3.2` against installed 4.1.x: record the resolution (pnpm override or peer-rule) explicitly instead of relying on silent tolerance; tests are written v4-forward (`it.effect`/`it.live`/`it.layer`/`it.effect.prop`; never `it.scoped`/TestServices).
- `@nxlv/python` removed if present; `@nx/dotnet` kept (graph/affected/local cache) with C# outputs excluded from any remote cache (nx #35934). tsconfig.base `types` injection and `rootDir` reconsidered at TS buildout, not now.
- Ignore-set unification: one canonical scratch-dir vocabulary applied to biome/ruff/ty/mypy/tsconfig/vite-watch/.gitignore in the same leg (add `mutants`, `.stryker-tmp`, `.approval_tests_temp` uniformly, or delete the dirs' possibility entirely via routing).

## [07]-[POLYGLOT_SEAMS]

- Unified mutation gate: assay's kill-floor is the single authority; Stryker.NET and StrykerJS emit `mutation-testing-report-schema` JSON natively into `.artifacts/`; one small mutmut→schema shim (files[].mutants[]{id,mutatorName,status,location}) closes the Python gap and one `mutation-testing-elements` report renders all three languages.
- Coverage aggregation: cobertura (C#) + lcov (Python/TS) under `.artifacts/`; no invented merged format.
- Contract seams: `buf breaking` (FILE category, against main) becomes the required proto gate the day the first `.proto` lands; `tests/contracts/` is the conformance-style corpus home, subdivided by seam/message — C# is sole producer, Python/TS decode→re-encode round-trip via Verify / inline-snapshot / `toMatchFileSnapshot`. Future contract assets (descriptor-set snapshots, exported schemas) land as peer folders when they become real, never reserved in advance. Both are stood up as law + folder now, exercised when content exists.
- Deliberate rejections (do not relitigate): Pact (pact-net stale, no plugin), Specmatic/Microcks (org-scale misfit), TUnit (watch, not adopt), FsCheck, syrupy, @fast-check/vitest (loses TestClock), @playwright/experimental-ct (abandoned), nox/tox (single-interpreter uv), moon/Bazel/Pants (no C# parity below ~100 targets), mise-as-task-owner (would add a fourth spelling of toolchain facts; global.json/uv/pnpm remain the pin owners), HypoFuzz (license).

## [08]-[DOCS]

Four durable agent-facing docs, declarative, zero provenance, conforming to `docs/standards/formatting.md` + `docs/standards/information-structure.md` + `docs/standards/style-guide.md` (heading idiom, container chooser, table rubrics, agent-prose law):
- `tests/README.md` — management law: folder scheme + casing, lane taxonomy, artifact/cache routing table, extension protocol (where a new suite/kit/fixture/corpus goes), the scenario pipeline map (content → closure → bridge → evidence), mutation/coverage gate ownership, contracts corpus law, and the tooling awareness map (root manifests, `.config/`, assay rails and the obligation to update assay's own tests when the operator changes).
- `tests/csharp/README.md`, `tests/python/README.md`, `tests/typescript/README.md` — per-language test-authoring law at docs/stacks density: oracle grades, law-matrix idioms over the actual kit surfaces, generator law, snapshot law, lane markers, density caps, banned shapes (existence tests, mirror tests, speculative states, per-function spam), with real kit-typed examples.

Doc law: no fragile enumerations (file counts, folder counts, LOC figures, version literals) — name owners and routes, never censuses that drift. The rebuild posture is sealed into every doc: anything testing-related — folder architecture, kit files, external libs, tooling config — is rebuilt ground-up whenever a denser shape exists; no workarounds, aliasing, band-aids, or backwards compatibility; breaking old tests is never a reason to preserve chaff.

Review gate: after authoring, two sequential cold-pass agents — critique (mechanical standards-conformance audit: heading idiom, containers, markers, agent-prose law, zero meta commentary, zero fragile enumerations, fix in place) then redteam (hostile re-read: attacks signal density, altitude, completeness of the tooling awareness map, the rebuild-aggression framing, and everything the critique pass fixed; fixes in place). No qualifiers or hedging survive either pass.

## [09]-[LEGS]

| Leg | Scope | Depends on |
|---|---|---|
| L1 | [02] C# finalize + skill repair | wave E landed |
| L2 | [03] Python platform | — |
| L3 | [06] TypeScript platform | — |
| L4 | [04] steps 1-2 (Executor port + test re-coupling + diagnostics) | L2 |
| L5 | [04] steps 3-6 (catalog, engine split, oracle, registry) | L4 |
| L6 | [05] test-estate collapse + [04] step 7 (README/config fold) | L5 |
| L7 | [07] seams (stryker schema shim, contracts skeleton) + [08] docs | L1-L3 |
| LT | Terminal verify: clean build + locked restore + assay static/test/self-test + full pytest + biome/tsc + `fd` litter sweep; `.vscode/settings.json` trued to the final tooling state (drop exclusion rows for now-impossible litter dirs, keep `dist`/`bin`/`obj`/`build`, drop `TestResults` only if TRX routing is centrally owned, nest `stryker-config.json`; Python settings untouched) | all |

L1/L2/L3 run parallel (disjoint files). Every leg's gates: language-native build/test green, assay rails green for touched routes, zero new root litter, zero dead config left behind.

## [10]-[CAPSTONE]

Runs after every build leg and the bridge/proof waves land; three strands, then the terminal sweep.

1. Contracts grounding: reader agents sweep the full `libs/` corpus and every root brief/DECISION to extract the committed cross-language seam facts, then build the non-speculative maximum in `tests/contracts/` — the corpus registry/manifest contract, the seam ledger for the goldens the planning corpus already names (`MATERIAL_LAYER_GOLDEN`, `CLASH_GOLDEN`, `CANONICAL_BYTE_IDENTITY`, HLC two-half vectors, BimWire golden bytes, the `FileDescriptorSet` drift gate), and consumer round-trip contracts. The C#-sole-producer law holds absolutely: no byte is pre-minted by a non-owner; structure exists only where a named seam grounds it.
2. Kit capability ultra-stacking: each language kit gains the full universal capability surface its libs/ demand matrix grounds — property/adversarial/metamorphic rails, regression proofing, seam/mocking substrate, benchmarking, e2e (TypeScript fully via playwright + browser mode; Python where the planned surface justifies it) — external testing packages mined to full depth per their `.api` catalogs, shared logic only, never per-project scaffolding, zero table-stakes code. The TS kit builds at the rebuilt `docs/stacks/typescript` bar after the doctrine lands.
3. Terminal hardening series: three sequential passes (initial rebuild-grade, critique, redteam — the `libs/.planning/campaign-method.md` role law and aggression bar) over ALL tooling infrastructure — root config files, `.config/`, `tests/`, `tools/` — fixing low-quality patterns, collapsing LOC through density (never capability loss), killing dead ends, enforcing `docs/stacks/<language>` adherence, and updating the `tests/` durable docs where reality moved. Every pass writes in place; the redteam pass is a cold re-read.

Live acceptance audit (after the terminal sweep — the campaign's closing gate): a truthful full-surface exercise of the assay operator under real usage, no mocks, no shortcuts. (a) Every verb of every rail runs live: `static` (per-language scopes AND `--all` with the full C# solution build), `test run|list|coverage` per language, `code` search/query, `docs`, `package`, `provision` (read-only probes), `self-test`, `delta` over the history store. (b) `api` proven against ALL FOUR artifact sources — host DLL, NuGet cache, Python dist, node_modules `.d.ts` — resolve + surface + member queries, no errors, cache entries typed and replayed. (c) Adversarial truth checks: seed one deliberate defect per language (a scratch file with a real violation), prove `static` FAILS with correctly folded diagnostics and honest exit codes, then remove the seeds — a gate that cannot be seen to fail is not accepted. (d) Bridge live: `bridge build` emits real closures; empty-corpus short-circuit proven; the Gate harness runs its stand-in rows and, when the host is free, its live lane against a real Rhino launch — the supervisor lifecycle (launch, handshake, quit ladder) exercised for real. (e) Hygiene invariants: `.cache/`/`.artifacts/` size and entry deltas recorded before/after (bounded growth, no unbounded cache litter), zero new root entries, retention pruning observed working. Every failure found is fixed before the campaign closes; the audit report is the campaign's final artifact.
