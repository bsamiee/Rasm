# [SPECIALIZED_RAILS]

Architecture tests, benchmarks, and fuzz harnesses are specialized proof rails. Keep them outside ordinary managed unit proof unless the owning test surface explicitly selects them.

## [01]-[ARCHUNITNET]

Use `TngTech.ArchUnitNET.xUnitV3` for compiled assembly boundary laws that the local analyzer does not own.

[FITS]:
- Assembly and package dependency direction.
- Stable namespace slice cycle checks.
- Cross-layer admission rules where downstream types must implement a marker interface or be registered in a dispatch surface.
- High-level boundary ownership that cannot be expressed as a semantic analyzer rule.

[BOUNDARIES]:
- Keep expression style, helper sprawl, branching, LanguageExt rules, and Thinktecture shape rules in the analyzer rail.
- Architecture tests work on compiled assemblies; they are not a substitute for source-level diagnostics.
- Use namespace-slice cycle detection only for stable namespaces.

[ADMISSION_COVERAGE]:
- Rule: when closed-world dispatch must recognize an open downstream type set, assert every admitted type reaches dispatch through a marker interface or registry entry.
- Rail: `tests/csharp/_architecture`.

## [02]-[BENCHMARKDOTNET]

Use BenchmarkDotNet only in benchmark projects. Benchmarks are executable measurement rails, not unit specs and not part of the default test run.

[FITS]:
- Pure managed numeric kernels.
- Generated dispatch and projection hot paths.
- Parsing and normalization work where input size is part of the contract.
- Allocation-sensitive transforms.

[CONFIG_POSTURE]:
- Use: `ManualConfig` as the benchmark project's central configuration.
- Use: named release jobs for repeatability.
- Use: `MemoryDiagnoser` when allocation pressure is part of the claim.
- Export: machine-readable output under the project artifact root.
- Gate: keep execution and JIT validators enabled.
- Selection: use `BenchmarkSwitcher` without extra scripts.
- Rail: `tests/csharp/_benchmarks`.

Reject: host-native documents, UI surfaces, viewport state, UI threads, bridge endpoints, and runtime scenarios in this rail.

## [03]-[SHARPFUZZ]

Use SharpFuzz for instrumented parser, decoder, and token-surface fuzzing. Keep it outside the normal managed test gate.

[FITS]:
- Pure managed parsers.
- Token readers and command-token surfaces.
- Import decoders and resilient binary readers.
- Grammar crash safety where generated samples miss malformed edges.

[SURFACE]:
- `Fuzzer.OutOfProcess.Run(Action<string>, int)`: string grammar and token harnesses with explicit input length bounds.
- `Fuzzer.OutOfProcess.Run(Action<Stream>)`: binary or stream decoders when string normalization is wrong.
- `Fuzzer.LibFuzzer.Run`: span-based harnesses when native setup exists.
- `RunAndIgnoreExceptions`: explicitly non-crashing parsers only.

[ARTIFACTS]:
- Corpus and crash artifacts belong under the project artifact root.
- Instrumentation CLI usage appears only when the fuzz rail becomes first-class for a concrete harness.
- Rail: `tests/csharp/_fuzz`.

Reject: host-native documents, UI surfaces, viewport state, app-bundle loading, or bridge runtime behavior.
