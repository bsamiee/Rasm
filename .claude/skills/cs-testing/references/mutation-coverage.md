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

- Use Stryker only after it discovers non-zero tests.
- Target 95% for eligible managed code, never for Rhino/GH runtime rails.
- Classify survivors as missing oracle, equivalent mutant, bridge-owned path, or product bug.
- Improve existing laws before adding branch-by-branch tests.
- Use the lowest CLI-accepted thresholds for diagnostics before enforcing thresholds; `dotnet-stryker 4.14.2` rejects `--break-at 0`.
