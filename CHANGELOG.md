# Changelog

All notable changes to this workspace are documented here. Format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/); versioning is
release-paced rather than SemVer until the first tagged Yak release.

## [Unreleased]

### Added
- `Aspect<,,>` polymorphic primitive for analysis algebra (Bounds, Locate, Measure,
  Faces, Conformance, Topology, Intersect, Deviation now share one shape).
- `Schedule.upto` upper-time-bound combinator.
- `UniqueCorners`, `WorldCardinalPoints` geometric primitives.
- `GeometryParameterKind` SmartEnum as the single extension point for new geometry
  parameters in GH2 components.
- Three Roslyn analyzer rules in `tools/cs-analyzer`:
  - `CSP0723` `RhinoActiveDocLeak` — flags reads of `RhinoDoc.ActiveDoc` outside
    boundary adapters (FunctionalDiscipline, Error).
  - `CSP0724` `FlagsEnumOveruse` — rejects `[Flags]` enums where a sealed DU or
    `[SmartEnum<T>]` would express the variant set without bitwise arithmetic
    (TypeDiscipline, Error).
  - `CSP0725` `ImperativeAccumulator` — flags mutable accumulator variables
    (`var sum = 0;` then `sum += ...`) in domain transforms; folds, projections,
    and `Aggregate` are the canonical surfaces (FunctionalDiscipline, Error).

### Changed
- Architectural collapse: 9 csproj merged into 6; `Core` and `Analysis` consolidated
  into `Rasm`; `Grasshopper` package renamed to `Rasm.Grasshopper`.
- Folders renamed to assembly-name convention: `libs/csharp/Rasm/` and
  `libs/csharp/Rasm.Grasshopper/`. `tests/rhino/` flattened to mirror the new
  single-assembly library layout. Test mirrors flattened accordingly.
- `GeometryContext` now serves as `Eff` runtime; legacy `AnalysisRuntime` removed.
- Bounds, Location, Measure aspects rebuilt on Thinktecture `[Union]`.
- Mass helper signature converted to a fully reader-monadic form.

### Fixed
- Solver-thread freeze on invalid input. Removing the retry policy from the
  Grasshopper boundary collapsed the worst-case validation latency on invalid-input
  paths from ~3.85 seconds to single-digit milliseconds. The old policy re-ran the
  typed-input rejection three times with backoff inside the solver thread; the
  improvement is user-facing solver responsiveness, not internal cleanup —
  invalid-input components now report immediate red status instead of a multi-
  second UI stall.

### Removed
- `AnalysisRuntime`, `Resilience.cs`, `WithStandardResilience`, `StandardPolicy`,
  `IndexHint` (formerly Scope state, briefly a wrapper, now eliminated),
  `MassFault`, `SemanticFault`, `GeometryResult`, single-call helpers in midpoint /
  topology / mesh paths, and 4 private delegate types replaced by `Func<>`.
- Catalog entries: Polly, FluentValidation, NodaTime, Scrutor (none used; resilience
  is now `Schedule` + `@catchM`, validation is applicative, time is host-supplied,
  DI is runtime-record).
