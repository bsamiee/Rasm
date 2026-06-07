# [CSHARP_TESTING]

This folder routes C# proof-tool choices. Package versions and graph state live in [build and packages](../platform/build-and-packages.md); shared testkit behavior lives with the test source owners.

## [1][CHOOSE]

This table is a lookup by proof question.

| [INDEX] | [QUESTION]                                   | [READ]                                    |
| :-----: | :------------------------------------------- | :---------------------------------------- |
|   [1]   | Which proof rail fits this behavior?         | [reference](reference.md)                 |
|   [2]   | Which managed law or property shape fits?    | [managed laws](managed-laws.md)           |
|   [3]   | Which evidence rail proves visited behavior? | [evidence rails](evidence-rails.md)       |
|   [4]   | Which specialized rail owns a boundary?      | [specialized rails](specialized-rails.md) |

## [2][BOUNDARIES]

Static managed specs prove pure contracts, guards, deterministic algorithms, generated metadata, typed receipts, rail categories, and pre-native rejection. Runtime scenario rails prove host-native documents, UI state, bridge state, app-bundle loading, viewport behavior, and command interaction.

Coverage, mutation, snapshots, architecture tests, benchmarks, and fuzzers are separate proof rails. Do not use one rail as evidence for another.
