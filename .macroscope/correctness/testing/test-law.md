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
- `integration` is reserved for the real process or IO boundary; a test running in-process with doubles is a unit test no matter how many owners it spans, and mislabeling it hides missing boundary proof.

## [02]-[ESTATE_RULES]

- Kits live in exactly one per-language `_testkit` (plus `_scenariokit` for host-aware C#); specs importing the private-by-design testkit, and magic values in specs, are sanctioned — never findings.
- assay is the single mutation and coverage gate authority; thresholds and kill-floors live in owning configs, never in docs or specs, and zero mutant discovery is a failed rail, never a green pass.
- Every tool writes reports under `.artifacts/` and work state under `.cache/<tool>/`; a new repo-root entry without its `tests/python/_testkit/test_policy.py` allowlist row in the same change is a finding.
- Every `tools/` operator moves in the same change as its suite under `tests/<lang>/tools/<tool>`; a bridge protocol change lands with its Contract/Supervisor suite change or does not land.
- CsCheck generators filter with `Gen.Where(predicate).Select(transform)` — a throwing `Select` breaks shrinking and is the finding.
- TypeScript unit specs colocate beside source; C# and Python suites mirror the production tree.
