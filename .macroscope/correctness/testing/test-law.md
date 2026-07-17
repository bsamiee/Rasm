---
include:
  - "tests/**"
---

# [TEST_LAW]

`tests/README.md` is the test law — judge against it, never generic coverage convention. Everything under `tests/` exists to falsify production behavior with an independent oracle; a test confirming current output confirms nothing.

## [01]-[PROOF_BAR]

- An oracle predicts behavior from an independent source — closed-form math, conservation, fixture geometry, category contract, runtime observation, documented external behavior; a shape-only inspection of self-constructed values stands alone nowhere.
- Banned shapes are findings on sight: existence tests (the compiler or importer already proves it), mirror tests asserting a constructed value's own fields or re-implementing the production algorithm as its own oracle, specs proving only that their own doubles were called, speculative-state tests over states the production surface cannot construct, and per-function spam where a generated domain covers the family.
- Every law family registers a refuting witness the law must fail on; a witness the law survives, or one sitting trivially outside the domain, exposes a tautology — and that registration gap is itself the finding.
- A failing law is evidence: the demanded repair targets the production owner, never a dilution of the law into shape-only proof.
- Rail and carrier outcomes prove through the kit gates (`Spec.Succ`/`Spec.Fail`, `assert_ok`/`assert_error`, typed error-tag assertion); raw carrier-flag inspection, truthiness, `isinstance` dances on the carrier, and failure-message substring matching are findings, and process-boundary output decodes through `NdjsonOracle`, never stdout string-scraping.
- Numeric equality proves under an explicit `Tolerance` regime plus a named metric row (sign-ambiguous, periodic, ulps budget), never a hand-rolled either-or assertion; NaN admits nothing, and an oracle returns a value for a spec gate to decide rather than asserting mid-flight.
- Time-dependent behavior takes an injected clock advanced deterministically (`Timeline`, `autojump_backend`, `TestClock.adjust` with fork-advance-join); a unit-lane spec that sleeps or reads the wall clock is the named defect.
- Graph under test is the production composition with edges re-provided — a substitute is a `Layer` against the same Tag with interior edges real; a proof against a hand-assembled parallel graph proves only the parallel graph.
- A capability blocked on an unbuilt producer is a named skip carrying its activation condition; a simulacrum flow, an always-passing stub, or a vacuous green over an absent producer is a finding.
- `integration` is reserved for the real process or IO boundary; a test running in-process with doubles is a unit test no matter how many owners it spans, and mislabeling it hides missing boundary proof.

## [02]-[KITS_AND_GENERATORS]

- Kits live in exactly one per-language `_testkit` (plus `_scenariokit` for host-aware C#); a spec-local assertion helper, tolerance constant, generator, fixture, or double shadowing an existing kit owner is a finding — the owning kit file extends, never duplicates beside a spec.
- Kit import rides the exports-map or project-graph surface (sanctioned, with magic values in specs); a relative-path reach-around into kit internals is a finding.
- Every kit capability lands with its falsification twin proving it can fail; a kit capability without one is unproven.
- Generated inputs draw from the kit's magnitude-stratified, construction-invariant bands; a spec-local scalar generator resampling only the tame band is a coverage illusion, and failure lanes inject the typed fault union, never dispatch on message substrings. CsCheck generators filter with `Gen.Where(predicate).Select(transform)` — a throwing `Select` breaks shrinking.
- Every registered SUT public symbol carries a law or a per-symbol exemption with recorded justification; a blanket package exemption is a finding.
- A snapshot captures only what an independent producer emits; snapshotting a value the test itself constructed, reflexively re-accepting a `.verified`/`.received` diff instead of reading it as producer evidence, and a golden whose owning spec no longer exists are findings.

## [03]-[ESTATE_RULES]

- TypeScript unit specs colocate beside source — a unit spec under `tests/typescript/`, or shared harness beside source, is misfiled; C# and Python suites mirror the production tree (`tests/csharp/libs/<Package>/<Source>.spec.cs`, `tests/python/libs/<package>/`), and e2e/load specs carry the disjoint `*.pw.ts`/`*.k6.ts` suffixes so no runner sweeps another's estate.
- Tier and grouping directories are lowercase with `_`-prefixed kit directories; PascalCase begins only at a C# project boundary, and each `tests/csharp` project carries exactly one routing classifier, adding only its own suite-owned harness packages.
- Nothing cross-language lives inside a single language's tree; `tests/contracts/`, `tests/containers.json`, proto descriptors, and the assay operator are the only neutral seams, and a container image pinned anywhere but `tests/containers.json` is a finding.
- `tests/contracts/` assets are C#-produced only: a Python or TypeScript diff authoring a corpus asset, a fabricated byte set for a `DESIGN-PIN` entry, or a manifest entry leading its producer is a finding; graduation to `REAL` lands on the producer page and flips the manifest entry in the same change.
- assay is the single mutation and coverage gate authority; thresholds and kill-floors live in owning configs, never in docs or specs, and zero mutant discovery is a failed rail, never a green pass. `subprocess`-marked tests and browser-mode suites stay out of the mutation lane.
- Marker taxonomy is closed and declared in the owning config; `network` and `property` auto-apply from socket-fixture and property-engine membership, so an undeclared or hand-applied marker is a finding.
- Benchmarks run only in the separate measurement session, and a gated case is a single registry row with absolute budget and dispersion ceiling; a benchmark without its row, or a phantom row, is a finding.
- A `[RhinoScenario]` recording only `Note` observations with no asserted `Require`/`Expect`/`Certify` fact proves nothing; reference candidates promote by human rename to `<method>.reference.json`, and a verify over an unpromoted corpus degrades, never fabricates a pass.
- Every tool routes reports under `.artifacts/` and work state under `.cache/<tool>/` through the tool's own documented configuration, never a wrapper script; a new repo-root entry without its `tests/python/_testkit/test_policy.py` allowlist row or matching allow-pattern in the same change is a finding.
- Every `tools/` operator moves in the same change as its suite under `tests/<lang>/tools/<tool>`; a bridge protocol change lands with its Contract/Supervisor suite change or does not land.
