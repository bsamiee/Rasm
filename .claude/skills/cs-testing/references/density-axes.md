# [H1][DENSITY_AXES]
>**Dictum:** *Breadth comes from generated axes, not loose test count.*

<br>

## [1][PACKING]

| [INDEX] | [TECHNIQUE] | [USE] |
| :-----: | ----------- | ----- |
| [1] | Packed property body | One `Spec.ForAll` asserts constructor, projection, unsupported output, and invariant rails for the same input. |
| [2] | Case generator | `Gen.OneOfConst` over SmartEnum/union cases; assert key uniqueness and case semantics together. |
| [3] | Product generator | `a.Select(b, c, ...)` for law dimensions such as value/context/output. |
| [4] | Edge-biased generator | Mix normal range with `0`, tolerance, `TwoPI`, `1`, non-finite, and tiny magnitudes. |
| [5] | Metamorphic transform | Generate `(input, transform)` and compare invariant/transformed outputs. |
| [6] | Reference loop | Small independent loop over rows/columns/items inside one generated matrix or sequence. |
| [7] | Receipt table | Generate operation modes and assert typed receipt invariants in one pass. |

---
## [2][COVERAGE_AXES]

Every owning spec should explicitly consider these axes:

| [INDEX] | [AXIS] | [QUESTION] |
| :-----: | ------ | ---------- |
| [1] | Lines/branches | Which source branches are static-managed and reachable? |
| [2] | Public functions | Does every public/internal API surface have at least one real law? |
| [3] | Type cases | Are all `[Union]`/`[SmartEnum]` cases enumerated or generated? |
| [4] | Domain boundaries | Are minima/maxima/tolerances/non-finite/null/empty covered? |
| [5] | Output directions | Are supported and unsupported `TOut` projections exercised? |
| [6] | Failure rails | Are `InvalidInput`, `InvalidResult`, `Unsupported`, and accumulated errors checked by stable shape? |
| [7] | Numeric stability | Are residuals, reconstruction, tolerance scaling, and rank/conditioning tested independently? |
| [8] | Native ownership | Is each native-dependent behavior represented by a bridge scenario? |

---
## [3][LOC_TACTICS]

- Collapse repeated `Fact`s into one generated law when they share setup.
- Use local arrays for case tables; use `Spec.SmartEnumKeysUnique` for distinctness.
- Prefer `Spec.SuccValue` once per fixture object, then generated assertions over that object.
- Keep one-off expected values inline only when they are true mathematical constants.
- Promote only reusable generators and independent oracles to `_testkit`.
- Prefer product generators that vary mode, payload, output kind, and invalid edge together; this catches branch swaps with fewer lines than one fact per mode.
- Use local fixture geometry only when it is the independent model: asymmetric tetrahedra, a unit segment, one triangle, one square, diagonal matrices, and one-point probability plans often expose more bugs than large random fixtures.
- Raise the 175 LOC target only after collapsing repeated setup into arrays, `Spec.Cases`, `Spec.SmartEnumKeysUnique`, `Numeric`, or a two-consumer testkit primitive.
- Keep bridge classification concise in static specs; executable native success belongs in `*.verify.csx`, not in long static workarounds.
