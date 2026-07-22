---
include:
  - "tests/typescript/**"
  - "libs/typescript/**/*.spec.ts"
  - "libs/typescript/**/*.test.ts"
---

# [TYPESCRIPT_TEST_LAW]

`tests/typescript/README.md` is the TypeScript-tree authoring law, colocated unit specs under `libs/typescript` included; these are its review teeth.

- `it.effect` owns the unit lane with `TestClock` control, `it.layer` shares expensive layers, and `it.effect.prop` is the one property surface; `it.live` is the sanctioned carve-out for laws about wall-clock behavior. `it.scoped` and direct `TestServices` reach-ins are findings.
- Property engine is the one `effect/FastCheck` re-export; a direct `fast-check` import or manifest admission is a finding — a second engine copy breaks `Arbitrary` class identity across the kit's dispatch.
- `Law.make` demands a refuting foil at construction and registration runs the tautology audit; law failures carry `Data.TaggedError` tags (`LawRefuted`, `LawTautology`), and failure assertions match tags, never message substrings.
- An `any` or unchecked cast in a spec hides exactly the boundary the test exists to prove — a finding; structural equality rides the kit's `addEqualityTesters` boot, never a hand-rolled comparator.
- Every dependency pin in the spec estate resolves through `catalog:` or `workspace:`; an inline version in any test manifest is a finding.
- A unit spec under `tests/typescript/`, or a shared harness beside source, is misfiled; colocated runtime-branch specs hold the 175-LOC density cap, the kit falsification and gauge suites being the declared carve-out.
- Gauge verdicts are red-capable or `Unsupported`, never vacuous green: an audit passing over zero modules, a ledger parse surviving an undeclared endpoint, or a gauge without its synthetic-source falsification block is a finding. Permitted-edge law parses live from the owning ARCHITECTURE page — a transcribed copy of the strata table is a finding.
- `Date.now()` in k6 latency probes, epoch fixtures in playwright clock suites, and the harness's armed e2e pause are the wall-clock subjects themselves — do not flag them; a wall-clock read in a unit-law body is the finding.
- Browser-mode suites are never mutated; screenshot goldens key per-project and per-platform, aria goldens route engine-invariant, and run artifacts never mix with committed goldens.
- Testcontainers ride the Ryuk reaper on by default; a diff setting `TESTCONTAINERS_RYUK_DISABLED` is a finding.
