# [H1][MUTATION_COVERAGE]
>**Dictum:** *Scores are gates only after ownership and discovery are true.*

<br>

## [1][COVERAGE]

| [INDEX] | [CLASS] | [ACTION] |
| :-----: | ------- | -------- |
| [1] | Missing static law | Add or strengthen managed xUnit/CsCheck spec. |
| [2] | Bridge-owned runtime | Add or strengthen `*.verify.csx`; do not fake coverage. |
| [3] | Generated code | Exclude centrally by file/attribute. |
| [4] | Defensive unreachable | Document only after source review. |

## [2][MUTATION]

- Use Stryker only through explicit `tools.quality test run --mutation changed|full`; zero discovery fails the rail.
- Target 95% for eligible managed code, never for Rhino/GH runtime rails.
- Classify survivors as missing oracle, equivalent mutant, bridge-owned path, or product bug.
- Improve existing laws before adding branch-by-branch tests.
- Use `--mutation changed` for bounded diagnostics before enforcing full mutation; full thresholds remain `95/90/85`.

---
## [3][THEORY_EXPANSION_AS_STRYKER_ENABLER]
>**Dictum:** *PBT hosts are one mutation target; Theory rows are N.*

<br>

Stryker mutates each test method body and asks "does any test fail?". A `Spec.ForAll(Gen.OneOfConst([A,B,C]), ...)` body is ONE method, ONE mutation target — Stryker kills the mutation if any case catches it, then moves on. Survivors that affect only case B are invisible.

A `[Theory][InlineData(A)][InlineData(B)][InlineData(C)]` (or `[MemberData(nameof(CasesOf))]` populated from `SmartEnum.Items`) becomes THREE separately-tracked entry points. Stryker can now report "survivor in case B" specifically, enabling targeted oracle improvements.

Convert from PBT to Theory when:
- Stryker survivor analysis points at a SmartEnum / Union case the PBT host under-samples.
- The CI mutation budget cannot afford to re-run the full PBT body per mutant (Theory rows are independent and parallelize better).
- Per-case failure-ID matters for CI triage (each row gets its own test ID).

Do NOT convert when:
- Cases share oracle logic and the PBT body is the more honest representation.
- The oracle requires random input variation per case (PBT generators over case-specific inputs).
- Per-case Theory rows would be pure copy-paste of body code.

---
## [4][SURVIVOR_TAXONOMY]

| [INDEX] | [CLASS] | [ACTION] |
| :-----: | ------- | -------- |
| [1] | Missing oracle | Add Grade A/B/B+ oracle (closed-form, smaller model, metamorphic, conditioning-aware). |
| [2] | Equivalent mutant | Document inline; do not weaken oracle. |
| [3] | Bridge-owned path | Add/strengthen `*.verify.csx`; static spec cannot kill it. |
| [4] | Product bug | Fix production code; mutation revealed a real defect. |

Do not chase a survivor by adding an assertion on the mutant's behavior — that is implementation mirror coverage (Grade F per [oracles-laws.md `[4]`](oracles-laws.md)).
