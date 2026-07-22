---
include:
  - "tests/csharp/**"
---

# [CSHARP_TEST_LAW]

`tests/csharp/README.md` is the C#-tree authoring law; these are its review teeth.

- Rail and carrier outcomes prove through the kit gates — `Spec.Succ`/`Spec.Fail`/`Spec.FailCategory` and kin; raw carrier-flag inspection (`.IsSucc`), truthiness, and failure-message substring matching are findings. `Spec.FailCategory` asserts the closed-family case-type name — message text riding a failure label is presentation, never the contract.
- Every `Law<T>` registration carries a `RefutingWitness` and closes over its equality policy; `Spec.Hold` refuses an empty law table, and a shrunk counterexample pins as a seed-keyed `Spec.Replay` regression, never a hand-copied literal case.
- Numeric proof rides `Spec.Equal` under an explicit `Tolerance` regime and `Metric` row (`SignAmbiguous`, `Periodic`, ulps budget); `Numeric` oracles return values for the gate to decide. Time rides the `Timeline` clock over `FakeTimeProvider` — `Thread.Sleep`/`Task.Delay` in a spec body is the named defect.
- CsCheck generators filter with `Gen.Where(predicate).Select(transform)` — a throwing `Select` breaks shrinking and is a finding.
- Coverage attribution rides `[Law(typeof(Subject), "name")]`, and exemptions derive from production `[CspExempt]`/`[CspScope]` sites; a parallel exemption catalog authored in the test tree is a finding.
- Every ArchUnit rule is vacuously true over an empty type set: a boundary suite without its `NonVacuous` guard first, and any kit fold accepting an empty table, are findings.
- Wire proof rides `Spec.RoundtripBytes` against the generated `JsonTypeInfo` contract with byte identity; process-boundary output decodes through `NdjsonOracle`, never stdout string-scraping.
- A `[RhinoScenario]` recording only `Note` observations with no asserted `Require`/`Expect`/`Certify` fact proves nothing the supervisor can fail; reference candidates promote by human rename to `<method>.reference.json`, and a verify over an unpromoted corpus degrades, never fabricates a pass.
- `Regression.RegistryParity` locks benchmarks to `BenchCase` rows; `TooNoisy` never folds into pass, and an ungateable case is a visible verdict, never silence.
- Verify snapshots register once per assembly via `[ModuleInitializer]`; `VerifyChecks`/`DanglingSnapshots` own hygiene, and a re-accepted `.verified.txt` without producer evidence is a finding.
- Suites mirror `libs/csharp` as `tests/csharp/libs/<Package>/<Source>.spec.cs`; a per-package suite shell is provisioned before bodies land, so a new spec is that shell's first inhabitant, never a new parallel home.
