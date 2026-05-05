# [H1][TESTING_LAWS]
>**Dictum:** *Laws define reusable truth independent from implementation.*

<br>

For implementation patterns see [->patterns.md](patterns.md). For structural standards see [->standards.md](standards.md).

---
## [1][LAW_TAXONOMY]
>**Dictum:** *Named laws give reviewers precise expectations.*

<br>

| [INDEX] | [LAW]             | [FORMULA]                       | [USE_WHEN]                                              |
| :-----: | ----------------- | ------------------------------- | ------------------------------------------------------- |
|   [1]   | **Identity**      | `f(x, x) = neutral`             | Diffing, comparison, merge, equality, normalization.    |
|   [2]   | **Inverse**       | `decode(encode(x)) = x`         | Serialization, codecs, encryption, import/export.       |
|   [3]   | **Idempotence**   | `f(f(x)) = f(x)`                | Formatting, normalization, cache warming, migrations.   |
|   [4]   | **Composition**   | `f(g(x)) = (f . g)(x)`          | Pipelines, patches, validation stages, transformations. |
|   [5]   | **Immutability**  | `f(x)` leaves `x` unchanged     | Pure transforms and persistent data structures.         |
|   [6]   | **Equivalence**   | `ours(x) = oracle(x)`           | Protocols, math, parsers, crypto, standards.            |
|   [7]   | **Reflexivity**   | `compare(x, x) = true`          | Equality, ordering, authorization, membership.          |
|   [8]   | **Symmetry**      | `compare(x, y) = compare(y, x)` | Equality, distance, conflict detection.                 |
|   [9]   | **Associativity** | `(x <> y) <> z = x <> (y <> z)` | Combining, batching, reductions, aggregation.           |
|  [10]   | **Monotonicity**  | `x <= y -> f(x) <= f(y)`        | Pagination, ordering, scoring, throttling.              |

---
## [2][SECURITY_LAWS]
>**Dictum:** *Security properties stay executable and adversarial.*

<br>

| [INDEX] | [LAW]                          | [PROPERTY]                                                                   |
| :-----: | ------------------------------ | ---------------------------------------------------------------------------- |
|   [1]   | **Tenant isolation**           | Cross-tenant input cannot read, mutate, decrypt, or infer other tenant data. |
|   [2]   | **Path confinement**           | User-controlled paths cannot escape configured roots.                        |
|   [3]   | **Prototype safety**           | Object-shaped input cannot mutate prototypes or global state.                |
|   [4]   | **Authorization monotonicity** | Removing permission never increases accessible operations.                   |
|   [5]   | **Redaction stability**        | Sensitive fields remain redacted through serialization and logging.          |

---
## [3][ORACLE_SELECTION]
>**Dictum:** *Expected values come from independent authority.*

<br>

| [INDEX] | [ORACLE]                     | [USE_WHEN]                                                                    |
| :-----: | ---------------------------- | ----------------------------------------------------------------------------- |
|   [1]   | **Standards document**       | Protocols, formats, crypto, compliance, accessibility.                        |
|   [2]   | **Reference implementation** | Mature library exists outside implementation under test.                      |
|   [3]   | **Fixture vector**           | Input/output examples are normative or approved by domain owner.              |
|   [4]   | **Mathematical law**         | Property holds for every valid input independent of implementation.           |
|   [5]   | **State model**              | Workflow state transitions can be represented by a smaller independent model. |

[CRITICAL]:
- [NEVER] compute expected values by calling private production functions.
- [NEVER] copy implementation branching into test assertions.
- [NEVER] accept snapshot output without a named oracle or review reason.

---
## [4][LAW_QUALITY]
>**Dictum:** *A property must fail for plausible bugs.*

<br>

[VERIFY]:
- [ ] Property quantifies over meaningful generated input.
- [ ] Preconditions exclude invalid samples at generator boundary.
- [ ] Failure shrinks to minimal reproducible input.
- [ ] Expected value comes from law, oracle, fixture, or explicit example.
- [ ] Property would fail for at least one realistic implementation defect.
