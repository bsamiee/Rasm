# [H1][TESTING_GUARDRAILS]
>**Dictum:** *Guardrails preserve independent evidence as suites evolve.*

<br>

Guardrails combine static checks, configured quality gates, mutation testing, and review. Each layer catches a different failure mode.

---
## [1][IMPLEMENTATION_CONFIRMING]
>**Dictum:** *Tests verify contracts, not algorithms.*

<br>

Implementation-confirming tests fail when internals change while behavior remains correct. Replace them with laws, oracles, or public-contract examples.

| [INDEX] | [SIGNAL]                           | [FIX]                                                           |
| :-----: | ---------------------------------- | --------------------------------------------------------------- |
|   [1]   | **Private structure assertion**    | Assert public output, event, state transition, or failure rail. |
|   [2]   | **Mirrored branching**             | Encode cases as data or property preconditions.                 |
|   [3]   | **Intermediate state expectation** | Assert final invariant or externally visible side effect.       |
|   [4]   | **Refactor-sensitive failure**     | Reframe around public contract.                                 |
|   [5]   | **Low mutation score**             | Add law, oracle vector, or independent model.                   |

---
## [2][QUALITY_GATES]
>**Dictum:** *Configured gates turn policy into executable feedback.*

<br>

| [INDEX] | [GATE]              | [BLOCKS]                                                                   |
| :-----: | ------------------- | -------------------------------------------------------------------------- |
|   [1]   | **Root placement**  | Specs outside `tests/`.                                                    |
|   [2]   | **Coverage**        | Untested changed behavior and missing failure rails.                       |
|   [3]   | **Mutation**        | Circular assertions and weak oracles.                                      |
|   [4]   | **Static analysis** | Forbidden types, unchecked exceptions, brittle time, and mutable fixtures. |
|   [5]   | **Review**          | Vacuous laws, weak generators, and stale fixtures.                         |

[CRITICAL]:
- [NEVER] weaken configured thresholds to land a test change.
- [NEVER] replace failing contract tests with implementation-specific assertions.
- [NEVER] skip review for generated or property-based tests.

---
## [3][REVIEW_CHECKLIST]
>**Dictum:** *Human review validates semantic strength beyond syntax.*

<br>

[VERIFY]:
- [ ] Spec lives under `tests/`.
- [ ] Test name states public contract or workflow.
- [ ] Generator covers valid, invalid, boundary, and adversarial inputs.
- [ ] Oracle is independent from source implementation.
- [ ] Regression tests link to observed failure or issue.
- [ ] Assertions avoid private structure and intermediate implementation state.

---
## [4][FAILURE_RESPONSE]
>**Dictum:** *Failed gates improve evidence rather than reduce ambition.*

<br>

| [INDEX] | [FAILURE]            | [RESPONSE]                                                                 |
| :-----: | -------------------- | -------------------------------------------------------------------------- |
|   [1]   | **Coverage gap**     | Add missing public branch, error rail, or boundary case.                   |
|   [2]   | **Surviving mutant** | Strengthen oracle or add law that detects changed behavior.                |
|   [3]   | **Flaky test**       | Remove wall-clock time, shared mutable state, and external nondeterminism. |
|   [4]   | **Weak generator**   | Broaden input domain and encode preconditions explicitly.                  |
|   [5]   | **Stale fixture**    | Regenerate from authoritative source and document provenance.              |
