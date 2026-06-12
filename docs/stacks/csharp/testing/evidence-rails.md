# [EVIDENCE_RAILS]

Coverage, mutation, and snapshots are evidence rails over managed tests and deterministic artifacts. They do not replace static laws, runtime scenarios, architecture checks, benchmarks, or fuzz harnesses.

## [1]-[COVERAGE]

[SURFACE]:
- `--coverlet`: command-time coverage opt-in.
- `--coverlet-output-format`: `json` and `cobertura` for machine-readable output.
- `--coverlet-include` and `--coverlet-exclude`: managed assembly selection.
- `--coverlet-exclude-by-file`: generated and obj-path exclusions.
- `--coverlet-exclude-by-attribute`: coverage and generated-code attributes.
- `--coverlet-exclude-assemblies-without-sources`: strict assembly-source handling.

[CLASSIFICATION]:
- Missing static law: add the law to the owning spec.
- Runtime-owned path: strengthen the runtime scenario; coverage cannot prove native behavior.
- Generated source: exclude centrally by file or attribute.
- Defensive unreachable: name the invariant locally; do not force execution through reflection.
- Dead code: remove it.

[BOUNDARIES]:
- Coverage increases through real reachable behavior, not artificial tests.
- Threshold changes never hide runtime-owned paths.
- Private methods stay private; coverage reaches them through public laws.
- Testkit, benchmarks, fuzz harnesses, and runtime scenario scripts are not product coverage targets.

## [2]-[MUTATION]

[LOCAL_RAIL]:
- Tool restore: local .NET tool manifest.
- Pair: one managed product project and one managed test project.
- Runner: Microsoft Testing Platform when the project configures it.
- Output: project artifact root.
- Lock: live mutation lock fails fast; stale unlocked files are reusable.
- Discovery: zero-test discovery fails the rail.

[MODES]:
- `off`: unit-only default.
- `changed`: mutate changed eligible managed files.
- `full`: full managed mutation with strict thresholds.

[SURVIVORS]:
- Missing oracle: add a law that distinguishes the mutant.
- Equivalent mutant: document the equivalence; do not weaken the oracle.
- Runtime-owned path: add or strengthen the runtime scenario.
- Product bug: fix production code.

[THEORY_SPLIT]:
- Rule: Stryker mutates each test method body.
- Use: split into theory rows only when survivor analysis proves a property under-samples a closed case and the split preserves the real oracle.
- Reject: asserting the mutant's behavior.

[BOUNDARIES]:
- Do not mutate host-runtime projects, plugin apps, runtime bridge tools, or scenario scripts.
- Treat selected mutants with non-zero discovery and explicit timeouts as real mutation results.
- Keep configuration in the quality operator while one product/test pair is under mutation.

## [3]-[VERIFY]

Use `Verify.XunitV3` for deterministic artifact contracts. Do not use snapshots for algebraic laws, numeric behavior, host-native truth, random samples, or current implementation output.

[FITS]:
- Analyzer diagnostics whose text is user-facing.
- Generated manifests and normalized package/config reports.
- Public catalogs where ordering and names are contract.
- Normalized runtime evidence JSON after the runtime rail stabilizes.
- Select generated source outputs when generator behavior is part of the package upgrade contract.

[SETTINGS]:
- Global scrubbers: module initializer.
- Per-test settings: only when the artifact owner requires path or name changes.
- Snapshot placement: beside owning specs or under a stable artifact directory.
- Scrub: machine paths, timestamps, generated IDs, runtime versions, and environment-specific noise.
- Review: use the Verify CLI for manual review and acceptance; never auto-accept received files in scripts.

[BOUNDARIES]:
- Floating numeric output uses tolerance law instead of snapshots.
- Runtime stdout with clocks, machine paths, or runtime version strings is scrubbed or excluded.
- Generated random samples stay outside snapshot contracts.
- Snapshots confirm known contracts; they never bless unknown drift.
