# [H1][TESTKIT]
>**Dictum:** *The testkit is one rail, not a module-specific helper drawer.*

<br>

## [1][FILES]

| [INDEX] | [FILE] | [OWNS] |
| :-----: | ------ | ------ |
| [1] | `Spec.cs` | Law adapters, CsCheck sample policy, `Fin`/`Validation`/`Option` assertions, equality/tolerance assertions. |
| [2] | `Gens.cs` | Reusable primitive, geometry, value-object, context, and rail generators consumed by multiple specs. |
| [3] | `Numeric.cs` | Independent row-major numerical oracles for matrix/vector laws. |
| [4] | `RhinoGeometrySerializer.cs` | xUnit serializer for pure data display. |

---
## [2][SPEC_CONTRACT]

- Use `Spec.ForAll` instead of raw `gen.Sample` so seed/iter/time/thread precedence is consistent.
- Use `Spec.Metamorphic` when expected values come from an independent path.
- Treat `Spec.Metamorphic` as a path/oracle law wrapper; it is not CsCheck `SampleMetamorphic`.
- Use `Spec.Valid` / `Spec.Invalid` for `Validation<Error,T>` and `Spec.Succ` / `Spec.Fail` for `Fin<T>`.
- Use `Spec.EqualWithin`, `NearEqual`, and `SeqEqualWithin` for generated numeric comparisons.
- Use `Numeric.*` for matrix expected values when testing `Matrix`.
- Add model-based, async, snapshot, benchmark, or fuzz adapters only when two concrete consumers exist.

---
## [3][GENERATOR_CONTRACT]

- Value-object generators call production `TryCreate`/`Create`/`AcceptValidated` paths.
- Edge-biased scalar generators include tolerance-adjacent values, not just broad random ranges.
- `Context` generators must produce valid `Context` instances and remain reusable across vector/flow/field/cloud specs.
- Keep module-local generators inside the spec until at least two consumers need the same concept.

---
## [4][SUPPRESSION_POLICY]

- No local `SuppressMessage` attributes for normal xUnit generator classes.
- Keep test classes public for xUnit discovery.
- Keep spec-local generator/static data classes non-public when discovery does not need them.
- Folder-wide analyzer rationale belongs in `.editorconfig`, not in repeated file-local comments.
