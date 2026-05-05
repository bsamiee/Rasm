# [H1][TESTING_PATTERNS]
>**Dictum:** *Patterns translate testing laws into repeatable spec structure.*

<br>

Use these patterns across languages and runners. Keep implementation details in `tests/`; keep policy language universal.

---
## [1][SPEC_SHAPE]
>**Dictum:** *Spec shape follows public contract evidence.*

<br>

| [INDEX] | [PATTERN]           | [STRUCTURE]                                                                |
| :-----: | ------------------- | -------------------------------------------------------------------------- |
|   [1]   | **Contract spec**   | Public operation -> generated inputs -> observable output or failure rail. |
|   [2]   | **Workflow spec**   | User journey -> boundary setup -> externally visible state transition.     |
|   [3]   | **Oracle spec**     | Input vector -> independent authority -> output comparison.                |
|   [4]   | **Regression spec** | Observed failure -> minimal reproduction -> fixed contract.                |
|   [5]   | **Model spec**      | Command sequence -> independent model -> invariant after each step.        |

---
## [2][DATA_PATTERNS]
>**Dictum:** *Input construction determines defect discovery quality.*

<br>

| [INDEX] | [PATTERN]              | [GUIDANCE]                                                           |
| :-----: | ---------------------- | -------------------------------------------------------------------- |
|   [1]   | **Bounded generators** | Generate valid and invalid inputs inside domain limits.              |
|   [2]   | **Case tables**        | Use small matrices for finite policy combinations.                   |
|   [3]   | **Fixture vectors**    | Store normative examples under `tests/fixtures/`.                    |
|   [4]   | **Adversarial sets**   | Include malformed, duplicate, unauthorized, and maximal inputs.      |
|   [5]   | **Clock control**      | Replace wall-clock time with configured deterministic time controls. |

---
## [3][ORACLE_PATTERNS]
>**Dictum:** *Expected behavior comes from authority outside source code.*

<br>

| [INDEX] | [PATTERN]                  | [USE_WHEN]                                                  |
| :-----: | -------------------------- | ----------------------------------------------------------- |
|   [1]   | **Law oracle**             | Algebraic property describes valid behavior for all inputs. |
|   [2]   | **Reference oracle**       | Independent library or service owns mature behavior.        |
|   [3]   | **Standard oracle**        | Specification document defines exact expected output.       |
|   [4]   | **Human-approved fixture** | Domain owner approves canonical examples.                   |
|   [5]   | **State model**            | Smaller independent model captures legal transitions.       |

---
## [4][ANTI_PATTERNS]
>**Dictum:** *Named failures make review corrections deterministic.*

<br>

| [INDEX] | [ANTI_PATTERN]            | [REPLACEMENT]                                    |
| :-----: | ------------------------- | ------------------------------------------------ |
|   [1]   | **Source-adjacent specs** | Move spec under `tests/` matching evidence type. |
|   [2]   | **Implementation mirror** | Assert law, oracle, or public contract.          |
|   [3]   | **Magic fixture**         | Name provenance and expected contract.           |
|   [4]   | **Branching expectation** | Encode table rows or generator preconditions.    |
|   [5]   | **Snapshot authority**    | Use snapshot only with named review reason.      |
|   [6]   | **Private API test**      | Compose through public API surface.              |
|   [7]   | **Wall-clock flake**      | Use deterministic clock or scheduler controls.   |

---
## [5][VALIDATION]
>**Dictum:** *Pattern use is complete only when evidence stays independent.*

<br>

[VERIFY]:
- [ ] Spec follows one pattern from §1.
- [ ] Test data follows one pattern from §2.
- [ ] Expected result follows one oracle pattern from §3.
- [ ] No anti-pattern from §4 remains.
