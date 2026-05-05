# [H1][TESTING_PHILOSOPHY]
>**Dictum:** *Good tests preserve confidence while implementation changes freely.*

<br>

Testing policy treats `tests/` as the only author-owned root. Language, package, and app topology may change; test discovery remains stable.

---
## [1][THESIS]
>**Dictum:** *Contract evidence beats implementation confirmation.*

<br>

| [INDEX] | [PILLAR]              | [MEANING]                                                                              |
| :-----: | --------------------- | -------------------------------------------------------------------------------------- |
|   [1]   | **Algebraic laws**    | Verify identities, inverses, round-trips, equivalence, and invariants.                 |
|   [2]   | **External oracles**  | Compare against standards, protocol vectors, fixtures, or independent implementations. |
|   [3]   | **Generated inputs**  | Explore boundary space beyond hand-written examples.                                   |
|   [4]   | **Mutation pressure** | Detect tests that mirror source logic instead of killing behavioral mutants.           |
|   [5]   | **Human review**      | Validate domain meaning, generator quality, and oracle independence.                   |

---
## [2][ROOT_TOPOLOGY]
>**Dictum:** *Test layout mirrors evidence type, not production folder layout.*

<br>

```text
tests/
|-- unit/
|-- integration/
|-- system/
|-- e2e/
`-- fixtures/
```

`tests/` is canonical. Create missing child directories only when a real spec or fixture needs them.

[CRITICAL]:
- [NEVER] duplicate package, app, or source directory trees under production roots.
- [NEVER] document concrete spec files that do not exist.
- [NEVER] make current package layout part of testing policy.

---
## [3][QUALITY_THRESHOLDS]
>**Dictum:** *Thresholds constrain suites without hard-coding stale project facts.*

<br>

| [INDEX] | [METRIC]            | [POLICY]                                                                         |
| :-----: | ------------------- | -------------------------------------------------------------------------------- |
|   [1]   | **Coverage**        | Enforce per changed file or owned project through configured runner.             |
|   [2]   | **Mutation score**  | Enforce where mutation tooling exists; investigate surviving behavioral mutants. |
|   [3]   | **Spec size**       | Keep specs dense enough for review; split only by public contract boundary.      |
|   [4]   | **Generated cases** | Set run counts high enough to shrink failures reproducibly.                      |

[IMPORTANT]:
1. [ALWAYS] **Configured truth:** Tool configuration owns exact numeric thresholds.
2. [ALWAYS] **Local evidence:** Documentation states policy; commands and config state current numbers.

---
## [4][DEFENSE_PIPELINE]
>**Dictum:** *Independent evidence layers expose circular tests.*

<br>

| [INDEX] | [LAYER]      | [CATCHES]                                                 |
| :-----: | ------------ | --------------------------------------------------------- |
|   [1]   | **Laws**     | Missing invariants and invalid transformations.           |
|   [2]   | **Oracles**  | Divergence from standards or independent implementations. |
|   [3]   | **Mutation** | Assertions that mirror implementation internals.          |
|   [4]   | **Coverage** | Untested public branches and failure rails.               |
|   [5]   | **Review**   | Vacuous properties, weak generators, and stale fixtures.  |

---
## [5][REFERENCES]
>**Dictum:** *Cross-references split why, what, how, and enforcement.*

<br>

- [->laws.md](laws.md) -- Law catalog and oracle selection.
- [->patterns.md](patterns.md) -- Universal implementation patterns.
- [->standards.md](standards.md) -- Structure and root policy.
- [->tooling.md](tooling.md) -- Tool routing and command discovery.
- [->guardrails.md](guardrails.md) -- Review and quality gates.
