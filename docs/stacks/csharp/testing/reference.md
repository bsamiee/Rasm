# [CSHARP_TESTING_REFERENCE]

Use this chooser when a failure mode could fit more than one C# proof tool. Classify behavior before choosing a package or assertion style.

## [1][RAIL_CHOOSER]

Static managed law:
    Choose: [managed laws](managed-laws.md).
    Use when: a value oracle runs without host state.
    Reject: runtime success claims.

Property or model-based law:
    Choose: [managed laws](managed-laws.md).
    Use when: input space, shrinking, model comparison, or concurrency history is the contract.
    Reject: large xUnit row tables for broad axes.

Coverage:
    Choose: [evidence rails](evidence-rails.md).
    Use when: the question is whether managed code is visited.
    Reject: artificial tests written only to raise a percentage.

Mutation:
    Choose: [evidence rails](evidence-rails.md).
    Use when: visited behavior needs a killing oracle.
    Reject: assertions that encode the mutant's output.

Snapshot:
    Choose: [evidence rails](evidence-rails.md).
    Use when: the deterministic artifact text is the contract.
    Reject: floating numeric output, random samples, and host stdout with runtime noise.

Architecture:
    Choose: [specialized rails](specialized-rails.md).
    Use when: the invariant is a compiled assembly dependency, namespace cycle, or boundary reachability law.
    Reject: source-style rules that the analyzer owns.

Benchmark:
    Choose: [specialized rails](specialized-rails.md).
    Use when: durable speed, allocation, or asymptotic claims need release-mode measurement.
    Reject: correctness proof through timing.

Fuzz:
    Choose: [specialized rails](specialized-rails.md).
    Use when: parser, decoder, token, or grammar crash resilience is the target.
    Reject: host-native UI or document behavior.

## [2][CLASSIFICATION]

Static or runtime:
    Static-managed behavior has a value oracle that can run without host state.
    Runtime behavior needs a live host, document, app bundle, UI thread, bridge process, viewport, or native loader.
    Static tests can assert guards, receipts, categories, adapters, and pure projections. Runtime scenarios own native success.

Snapshot or law:
    Snapshots fit analyzer diagnostics, generated manifests, normalized evidence JSON, and public catalogs where the artifact is the contract.
    Algebraic behavior, numeric tolerance, random samples, current implementation output, and host stdout are law-shaped or scenario-shaped.

Coverage or mutation:
    Coverage finds unvisited managed lines.
    Mutation checks whether visited behavior has a killing oracle.
    Classify first, then add the missing law, strengthen the scenario, document the equivalent mutant, or fix production code.

Benchmark or statistical property:
    CsCheck performance comparison fits local algorithm selection over the same generated domain.
    BenchmarkDotNet owns durable speed or allocation claims.
    Runtime-host performance stays outside the managed benchmark rail unless a source-owned runtime measurement exists.
