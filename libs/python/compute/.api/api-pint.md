# [PY_COMPUTE_API_PINT]

`pint` supplies the physical-unit registry, dimensional quantities, and unit conversion for the compute units rail. The package owner attaches a unit and dimensionality to every array-admission and study-result claim through a single shared `UnitRegistry`; it never re-implements unit arithmetic pint owns. Member spellings are uncaptured pending a reflectable install (see UN_REFLECTED).

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pint`
- package: `pint`
- import: `pint`
- owner: `compute`
- rail: units
- installed: ABSENT on cp315 (requires-python `>=3.15`, no cp315 wheel; sdist build blocked by manifest gaps 1+2)
- capability: physical-quantity arithmetic — unit registry, dimensional analysis, conversion, and NumPy-array unit wrapping

## [2]-[CAPTURE]

[PUBLIC_TYPE_SCOPE]: unit owners
- rail: units

| [INDEX] | [SYMBOL]                   | [PACKAGE_ROLE]   | [CAPABILITY]                    |
| :-----: | :------------------------- | :--------------- | :------------------------------ |
|   [1]   | `pint.UnitRegistry`        | registry root    | shared unit/dimension registry  |
|   [2]   | `pint.Quantity`            | quantity carrier | magnitude-plus-unit value       |
|   [3]   | `pint.Unit`                | unit atom        | a single registered unit        |
|   [4]   | `pint.DimensionalityError` | dimension fault  | mismatched-dimension error type |

[ENTRYPOINTS]:
- UN_REFLECTED: exact method spellings (`UnitRegistry.__call__`, `Quantity.to`, `Quantity.to_base_units`, `Quantity.magnitude`, `Quantity.dimensionality`) and verified signatures require a reflectable install to capture; type names above are stable package facts, not reflected members.

[IMPLEMENTATION_LAW]:
- registry: one shared `UnitRegistry` instance owns the unit vocabulary; conversion and dimensionality flow from it.
- claim: each array-admission and study result carries a `Quantity` unit claim; a dimensionality mismatch surfaces a `DimensionalityError` on the boundary rail.
- boundary: unit claims are offline study evidence; product unit policy stays in C# owners after graduation.

## [3]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `pint`
- Owns: physical-unit registry, dimensional quantities, and unit conversion for the units rail
- Accept: a unit-bearing array or study claim built from the shared `UnitRegistry`
- Reject: stringly-typed unit labels; hand-rolled conversion factors; wrapper-renames of `Quantity` arithmetic
