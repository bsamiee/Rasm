---
include:
  - "tests/**"
  - "libs/typescript/**/*.spec.ts"
  - "libs/typescript/**/*.test.ts"
---

# [TEST_LAW]

`tests/README.md` is the test law — judge against it, never generic coverage convention. Everything under `tests/` exists to falsify production behavior with an independent oracle; a test confirming current output confirms nothing. Language spellings live in the sibling testing files; this file carries only law that bites every tree.

## [01]-[PROOF_BAR]

- An oracle predicts behavior from an independent source — closed-form math, conservation, fixture geometry, category contract, runtime observation, documented external behavior; a shape-only inspection of self-constructed values stands alone nowhere.
- Banned shapes are findings on sight: existence tests (the compiler or importer already proves it), mirror tests asserting a constructed value's own fields or re-implementing the production algorithm as its own oracle, specs proving only that their own doubles were called, speculative-state tests over states the production surface cannot construct, and per-function spam where a generated domain covers the family.
- Every law family registers a refuting witness the law must fail on; a witness the law survives, or one sitting trivially outside the domain, exposes a tautology — and that registration gap is itself the finding.
- A failing law is evidence: the demanded repair targets the production owner, never a dilution of the law into shape-only proof.
- Numeric equality proves under an explicit tolerance regime and a named metric row, never a hand-rolled either-or assertion; NaN admits nothing, and an oracle returns a value for a spec gate to decide rather than asserting mid-flight.
- Time-dependent behavior takes an injected clock advanced deterministically; a unit-lane spec that sleeps or reads the wall clock is the named defect.
- Graph under test is the production composition with edges re-provided; a proof against a hand-assembled parallel graph proves only the parallel graph.
- A capability blocked on an unbuilt producer is a named skip carrying its activation condition; a simulacrum flow, an always-passing stub, or a vacuous green over an absent producer is a finding — every fold, gauge, and law refuses an empty input set as proving nothing.
- `integration` is reserved for the real process or IO boundary; a test running in-process with doubles is a unit test no matter how many owners it spans, and mislabeling it hides missing boundary proof.

## [02]-[KITS]

- Kits live in exactly one per-language `_testkit` (with `_scenariokit` for host-aware C#); a spec-local assertion helper, tolerance constant, generator, fixture, or double shadowing an existing kit owner is a finding — the owning kit file extends, never duplicates beside a spec.
- Kit import rides the exports-map or project-graph surface (sanctioned, with magic values in specs); a relative-path reach-around into kit internals is a finding.
- Every kit capability lands with its falsification twin proving it can fail; a kit capability without one is unproven — and a spec deliberately driving an oracle to failure is that twin, never a defect.
- Generated inputs draw from the kit's magnitude-stratified, construction-invariant bands; a spec-local scalar generator resampling only the tame band is a coverage illusion, and failure lanes inject the typed fault union, never dispatch on message substrings.
- Every registered SUT public symbol carries a law or a per-symbol exemption with recorded justification; a blanket package exemption is a finding.
- A snapshot captures only what an independent producer emits; snapshotting a value the test itself constructed, reflexively re-accepting a `.verified`/`.received` diff instead of reading it as producer evidence, and a golden whose owning spec no longer exists are findings.

## [03]-[ESTATE]

- TypeScript unit specs colocate beside source — a unit spec under `tests/typescript/`, or shared harness beside source, is misfiled; C# and Python suites mirror the production tree (`tests/csharp/libs/<Package>/<Source>.spec.cs`, `tests/python/libs/<package>/`), and e2e/load specs carry the disjoint `*.pw.ts`/`*.k6.ts` suffixes so no runner sweeps another's estate.
- Tier and grouping directories are lowercase with `_`-prefixed kit directories; PascalCase begins only at a C# project boundary, and each `tests/csharp` project carries exactly one routing classifier, adding only its own suite-owned harness packages.
- Nothing cross-language lives inside a single language's tree; `tests/contracts/`, `tests/containers.json`, proto descriptors, and the assay operator are the only neutral seams, and a container image pinned anywhere but `tests/containers.json` is a finding.
- `tests/contracts/` assets are C#-produced only: a Python or TypeScript diff authoring a corpus asset, a fabricated byte set for a `DESIGN-PIN` entry, or a manifest entry leading its producer is a finding; graduation to `REAL` lands on the producer page and flips the manifest entry in the same change.
- assay is the single mutation and coverage gate authority; thresholds and kill-floors live in owning configs, never in docs or specs, and zero mutant discovery is a failed rail, never a green pass.
- Marker taxonomy is closed and declared in the owning config; an undeclared or hand-applied auto-marker is a finding.
- Benchmarks run only in the separate measurement session, and a gated case is a single registry row with absolute budget and dispersion ceiling; a benchmark without its row, or a phantom row, is a finding.
- Every tool routes reports under `.artifacts/` and work state under `.cache/<tool>/` through the tool's own documented configuration, never a wrapper script; a new repo-root entry without its `tests/python/_testkit/test_policy.py` allowlist row in the same change is a finding.
- Every `tools/` operator moves in the same change as its suite under `tests/<lang>/tools/<tool>`.

## [04]-[RULINGS]

- Estate and language-tree `RULINGS.md` registries (`tests/RULINGS.md`, `tests/<language>/RULINGS.md`) are settled testing law with the force of the owning README: a finding contradicting a ruling row is void with the row as refutation, and a diff re-litigating one — re-admitting a rejected test-stack package, re-homing a ruled kit owner, re-adding a retired structure — is a finding citing the row verbatim.
- A diff exposing a homeless testing discriminant — two lawful twin surfaces whose separating rule lives nowhere durable — earns the new ruling row at the narrowest owning tier: the language tree for a tree-scoped decision, the estate root for a cross-tree one.
