# [CSHARP_TESTING]

This folder routes C# proof-tool choices. Package versions and graph state live in the central package manifest; shared testkit behavior lives with the test source owners.

## [1]-[CHOOSE]

This table routes the first proof question. Classify ambiguous behavior in the rail chooser before adding a package, assertion style, or scenario.

| [INDEX] | [QUESTION]                                   | [READ]                                    |
| :-----: | :------------------------------------------- | :---------------------------------------- |
|   [1]   | Which managed law or property shape fits?    | [managed laws](managed-laws.md)           |
|   [2]   | Which evidence rail proves visited behavior? | [evidence rails](evidence-rails.md)       |
|   [3]   | Which specialized rail owns a boundary?      | [specialized rails](specialized-rails.md) |

## [2]-[RAIL_CHOOSER]

[STATIC_MANAGED_LAW]:
- Choose: [managed laws](managed-laws.md).
- Use when: a value oracle runs without host state.
- Reject: runtime success claims.

[PROPERTY_OR_MODEL_BASED_LAW]:
- Choose: [managed laws](managed-laws.md).
- Use when: input space, shrinking, model comparison, or concurrency history is the contract.
- Reject: large xUnit row tables for broad axes.

[COVERAGE]:
- Choose: [evidence rails](evidence-rails.md).
- Use when: the question is whether managed code is visited.
- Reject: artificial tests written only to raise a percentage.

[MUTATION]:
- Choose: [evidence rails](evidence-rails.md).
- Use when: visited behavior needs a killing oracle.
- Reject: assertions that encode the mutant's output.

[SNAPSHOT]:
- Choose: [evidence rails](evidence-rails.md).
- Use when: the deterministic artifact text is the contract.
- Reject: floating numeric output, random samples, and host stdout with runtime noise.

[ARCHITECTURE]:
- Choose: [specialized rails](specialized-rails.md).
- Use when: the invariant is a compiled assembly dependency, namespace cycle, or boundary reachability law.
- Reject: source-style rules that the analyzer owns.

[BENCHMARK]:
- Choose: [specialized rails](specialized-rails.md).
- Use when: durable speed, allocation, or asymptotic claims need release-mode measurement.
- Reject: correctness proof through timing.

[FUZZ]:
- Choose: [specialized rails](specialized-rails.md).
- Use when: parser, decoder, token, or grammar crash resilience is the target.
- Reject: host-native UI or document behavior.

## [3]-[CLASSIFICATION]

[STATIC_OR_RUNTIME]:
- Static-managed behavior has a value oracle that can run without host state.
- Runtime behavior needs a live host, document, app bundle, UI thread, bridge process, viewport, or native loader.
- Static tests can assert guards, receipts, categories, adapters, and pure projections. Runtime scenarios own native success.

[SNAPSHOT_OR_LAW]:
- Snapshots fit analyzer diagnostics, generated manifests, normalized evidence JSON, and public catalogs where the artifact is the contract.
- Algebraic behavior, numeric tolerance, random samples, current implementation output, and host stdout are law-shaped or scenario-shaped.

[COVERAGE_OR_MUTATION]:
- Coverage finds unvisited managed lines.
- Mutation checks whether visited behavior has a killing oracle.
- Classify first, then add the missing law, strengthen the scenario, document the equivalent mutant, or fix production code.

[BENCHMARK_OR_STATISTICAL_PROPERTY]:
- CsCheck performance comparison fits local algorithm selection over the same generated domain.
- BenchmarkDotNet owns durable speed or allocation claims.
- Runtime-host performance stays outside the managed benchmark rail unless a source-owned runtime measurement exists.

## [4]-[BOUNDARIES]

Static managed specs prove pure contracts, guards, deterministic algorithms, generated metadata, typed receipts, rail categories, and pre-native rejection. Runtime scenario rails prove host-native documents, UI state, bridge state, app-bundle loading, viewport behavior, and command interaction.

Coverage, mutation, snapshots, architecture tests, benchmarks, and fuzzers are separate proof rails. Do not use one rail as evidence for another.
