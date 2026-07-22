---
include:
  - "tests/python/**"
---

# [PYTHON_TEST_LAW]

`tests/python/README.md` is the Python-tree authoring law; these are its review teeth.

- `@spec(subject)` is the one registration surface — double-decoration is a hard failure, never a silent stack; module-level `COVERS` is the only other coverage source, and `assert_law_coverage` censuses only fully-collected packages, so a targeted run never reports false gaps.
- Exemptions ride `auto_exempt` predicates or per-symbol `register_sut(..., exempt=...)` with recorded justification; a blanket package exemption is a finding.
- Rail outcomes prove through `assert_ok`/`assert_error`/`assert_error_status`/`assert_some`/`assert_none` — truthiness on the carrier is a finding. `isinstance` narrowing a wire union to its arm is the sanctioned discriminated-union assertion; `isinstance` on the carrier itself is the banned dance — do not flag the former.
- A tool-output passthrough claim may assert a message substring only beside its `assert_error_status` identity check; a message substring as the sole failure proof is a finding.
- Tolerance rides `close`/`assert_close` — payload-recursive over `Result`/`Option`/`Block`, arrays, quantities, and structs, naming the first diverging path; a spec-local tolerance constant is a finding.
- Deadline, retry, and drain laws run in wall-microseconds under `autojump_backend`; a real-clock sleep in a unit body is a finding — but `time.sleep` inside a spawned subprocess argv proving a `TIMEOUT` verdict, and `anyio.sleep(0.0)` scheduler yields, are the sanctioned forms, never real-time waits.
- Matrix folds (`validity_matrix`, `projection_matrix`, `support_matrix`) refuse empty row sets and report per-row through `subtests`; a fact-per-case function family where a fold owns the rows is a finding.
- Stateful subjects prove through `model_based` under the `rasm-stateful` profile; the mutation lane runs derandomized and database-free (`rasm-mutation`), and `subprocess`-marked tests stay out of mutation because children execute the unmutated tree.
- `resolve(subject)` samples defaulted-field absence so the `UNSET`/omitted wire lane generates; a hand-rolled strategy for a subject the resolver owns is a finding.
- Markers are closed and declared in `pyproject.toml`; `network` and `property` auto-apply, so a hand-applied auto-marker is a finding.
