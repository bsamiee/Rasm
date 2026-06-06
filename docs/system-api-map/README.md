# [SYSTEM_API_MAP]

Scope: BCL, shared-framework surfaces, package state, host references, build metadata. Cross-stack owner order: `../usage.md` §1. Product libraries: `../external-libs/README.md`. Test tools: `../testing-libs/xunit/api.md`. C# 14 language: `../external-libs/csharp/language.md`.

## [1][FILES]

| [INDEX] | [FILE]            | [QUESTION]                                                                        |
| :-----: | ----------------- | --------------------------------------------------------------------------------- |
|   [1]   | `bcl.md`          | Which BCL/shared-framework API owns a primitive concern?                          |
|   [2]   | `packages.md`     | Which package state permits adoption or rejection?                                |
|   [3]   | `replacements.md` | Which owner replaces repeated local machinery?                                    |
|   [4]   | `meta.md`         | Which C# build/meta file owns language, analyzer, host wiring, and global usings? |

## [2][ROUTING]

| [INDEX] | [QUESTION]                                       | [READ]                       |
| :-----: | ------------------------------------------------ | ---------------------------- |
|   [1]   | Cross-stack owner precedence                     | `../usage.md` §1             |
|   [2]   | Proof hierarchy for API claims                   | `../usage.md` §5             |
|   [3]   | Build props, analyzers, host refs, global usings | `meta.md`                    |
|   [4]   | Package graph state / adoption                   | `packages.md`                |
|   [5]   | BCL API ownership                                | `bcl.md`                     |
|   [6]   | Replace local machinery                          | `replacements.md`            |
|   [7]   | Product library APIs                             | `../external-libs/README.md` |
|   [8]   | Test-tool APIs                                   | `../testing-libs/README.md`  |
|   [9]   | Host composition packages                        | `../host-libraries.md` §8    |

## [3][ADOPTION]

- Distinguish in-box BCL from platform packages that still need an explicit `PackageReference` on first consumer (`packages.md` §1).
- Keep unadopted packages out of active guidance until a measured consumer lands.
- Use local XML or decompile evidence for RhinoWIP/GH2 API claims in public docs.
- Reconcile package graph edits with `packages.md` and affected `docs/external-libs/**/api.md` guidance.

## [4][TEST_CONSUMERS]

- Route serializers, fuzz parsers, bridge probes, host loaders, filesystem evidence, and capture code through `bcl.md` and `replacements.md`.
- Route xUnit, CsCheck, Verify, Stryker, ArchUnitNET, BenchmarkDotNet, and SharpFuzz APIs through `../testing-libs/`.
- Treat `.verify.csx` scenarios as staged runtime source. Compile-time global usings in `meta.md` do not define scenario runtime usings.
- Keep `ConcurrentDictionary`, `Interlocked`, `Directory`, `Path`, `Console.WriteLine`, and `System.Drawing` inside explicit test, tool, or bridge boundary adapters.
