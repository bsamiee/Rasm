# [PY_COMPUTE_API_PINT]

`pint` supplies the physical-unit registry, dimensional quantities, and unit conversion for the compute units rail. The package owner attaches a unit and dimensionality to every array-admission and study-result claim through a single shared `UnitRegistry`, converts with `Quantity.to`, and raises `DimensionalityError` on a dimension mismatch; it never re-implements unit arithmetic the package owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pint`
- package: `pint`
- import: `pint`
- owner: `compute`
- rail: units
- capability: physical-quantity arithmetic — unit registry, dimensional analysis, conversion, measurement uncertainty, NumPy-array unit wrapping, and decorator-driven function signature checking

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: registry and quantity owners
- rail: units

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY]    | [CAPABILITY]                             |
| :-----: | :------------- | :--------------- | :--------------------------------------- |
|  [01]   | `UnitRegistry` | registry root    | shared unit and dimension vocabulary     |
|  [02]   | `Quantity`     | quantity carrier | magnitude-plus-unit value with NumPy ops |
|  [03]   | `Unit`         | unit atom        | a single registered unit                 |
|  [04]   | `Measurement`  | uncertain value  | magnitude with an error term             |
|  [05]   | `Context`      | transform set    | named cross-dimension conversion rules   |

[PUBLIC_TYPE_SCOPE]: error and warning rails
- rail: units

| [INDEX] | [SYMBOL]                       | [BASE]           | [FAULT]                              |
| :-----: | :----------------------------- | :--------------- | :----------------------------------- |
|  [01]   | `PintError`                    | `Exception`      | root of every pint failure           |
|  [02]   | `UndefinedUnitError`           | `AttributeError` | unit name not in the registry        |
|  [03]   | `DimensionalityError`          | `PintTypeError`  | incompatible-dimension conversion    |
|  [04]   | `OffsetUnitCalculusError`      | `PintTypeError`  | invalid offset-unit (temperature) op |
|  [05]   | `LogarithmicUnitCalculusError` | `PintTypeError`  | invalid logarithmic-unit op          |
|  [06]   | `RedefinitionError`            | `ValueError`     | duplicate unit definition            |
|  [07]   | `DefinitionSyntaxError`        | `ValueError`     | malformed definition string          |
|  [08]   | `UnitStrippedWarning`          | `UserWarning`    | magnitude used with the unit dropped |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registry construction and parsing
- rail: units

| [INDEX] | [SURFACE]                                             | [ENTRY_FAMILY]    | [RESULT]                          |
| :-----: | :---------------------------------------------------- | :---------------- | :-------------------------------- |
|  [01]   | `UnitRegistry()`                                      | registry build    | a fresh unit registry             |
|  [02]   | `UnitRegistry.__call__(input_string, case_sensitive)` | parse             | a `Quantity` from a string        |
|  [03]   | `UnitRegistry.parse_expression(input_string)`         | parse             | a `Quantity` from a string        |
|  [04]   | `UnitRegistry.parse_units(input_string, as_delta)`    | parse             | a `Unit` from a string            |
|  [05]   | `UnitRegistry.Quantity(value, units)`                 | construct         | a magnitude-plus-unit value       |
|  [06]   | `UnitRegistry.Unit(units)`                            | construct         | a unit atom                       |
|  [07]   | `UnitRegistry.Measurement(value, error, units)`       | construct         | a value with uncertainty          |
|  [08]   | `UnitRegistry.define(definition)`                     | mutate            | registers a new unit definition   |
|  [09]   | `UnitRegistry.convert(value, src, dst, inplace)`      | convert           | scalar magnitude in `dst` units   |
|  [10]   | `UnitRegistry.get_dimensionality(input_units)`        | introspect        | a `UnitsContainer` of dimensions  |
|  [11]   | `UnitRegistry.wraps(ret, args, strict)`               | decorator factory | wraps a function with unit specs  |
|  [12]   | `UnitRegistry.check(*args)`                           | decorator factory | asserts argument dimensionalities |

[ENTRYPOINT_SCOPE]: quantity conversion and projection
- rail: units

| [INDEX] | [SURFACE]                              | [ENTRY_FAMILY]   | [RESULT]                            |
| :-----: | :------------------------------------- | :--------------- | :---------------------------------- |
|  [01]   | `Quantity.to(other, *contexts)`        | convert          | a new `Quantity` in target units    |
|  [02]   | `Quantity.ito(other, *contexts)`       | convert in place | mutates to target units             |
|  [03]   | `Quantity.to_base_units()`             | convert          | a `Quantity` in registry base units |
|  [04]   | `Quantity.to_root_units()`             | convert          | a `Quantity` in root units          |
|  [05]   | `Quantity.to_reduced_units()`          | convert          | a `Quantity` with reduced units     |
|  [06]   | `Quantity.to_compact(unit)`            | convert          | a `Quantity` at a readable prefix   |
|  [07]   | `Quantity.m_as(units)`                 | project          | bare magnitude in target units      |
|  [08]   | `Quantity.magnitude` \| `Quantity.m`   | project          | the bare magnitude                  |
|  [09]   | `Quantity.units` \| `Quantity.u`       | project          | the `Unit` of the quantity          |
|  [10]   | `Quantity.dimensionality`              | introspect       | a `UnitsContainer` of dimensions    |
|  [11]   | `Quantity.check(dimension)`            | predicate        | dimensionality match boolean        |
|  [12]   | `Quantity.plus_minus(error, relative)` | construct        | a `Measurement` from the quantity   |
|  [13]   | `Quantity.from_sequence(seq, units)`   | construct        | a `Quantity` array from a sequence  |

[ENTRYPOINT_SCOPE]: module-level application registry and formatting
- rail: units

| [INDEX] | [SURFACE]                            | [ENTRY_FAMILY]  | [RESULT]                            |
| :-----: | :----------------------------------- | :-------------- | :---------------------------------- |
|  [01]   | `get_application_registry()`         | shared registry | the process-wide `UnitRegistry`     |
|  [02]   | `set_application_registry(registry)` | shared registry | installs a process-wide registry    |
|  [03]   | `register_unit_format(name)`         | formatting      | registers a custom format spec      |
|  [04]   | `formatter(numerator, denominator)`  | formatting      | a formatted unit string             |
|  [05]   | `pi_theorem(quantities, registry)`   | analysis        | dimensionless groups via Buckingham |

## [04]-[IMPLEMENTATION_LAW]

[UNIT_TOPOLOGY]:
- registry: one shared `UnitRegistry` owns the vocabulary; conversion, dimensionality, and parsing all flow from that instance.
- application registry: `get_application_registry`/`set_application_registry` share one registry across modules so `Quantity` instances stay compatible.
- parsing: `UnitRegistry.__call__` and `parse_expression` build a `Quantity` from a string; `parse_units` builds a bare `Unit`.
- conversion: `Quantity.to` returns a new value, `Quantity.ito` mutates in place; `to_base_units`/`to_root_units` normalize to registry base.
- projection: `Quantity.magnitude`/`m` strips the unit; `m_as` strips after converting; a bare-magnitude operation on a unit-bearing array raises `UnitStrippedWarning`.
- dimensionality: `Quantity.check` and `UnitRegistry.get_dimensionality` resolve dimensional compatibility before arithmetic.
- decorators: `UnitRegistry.wraps` and `UnitRegistry.check` enforce argument and return units at function boundaries.
- offset units: temperature and other offset units route through `OffsetUnitCalculusError`; logarithmic units route through `LogarithmicUnitCalculusError`.

[LOCAL_ADMISSION]:
- One shared `UnitRegistry` (via `get_application_registry`) carries the unit vocabulary for the rail; per-claim registries are rejected because their `Quantity` types do not interoperate.
- Each array-admission and study result carries a `Quantity` unit claim; a dimensionality mismatch surfaces a `DimensionalityError` on the boundary rail.
- Unit conversion uses `Quantity.to`/`to_base_units`; hand-rolled conversion factors are rejected.
- Unit claims are offline study evidence; product unit policy stays in C# owners after graduation.

[RAIL_LAW]:
- Package: `pint`
- Owns: physical-unit registry, dimensional quantities, unit conversion, and signature-level unit checking for the units rail
- Accept: a unit-bearing `Quantity` built from the shared `UnitRegistry`, converted via `Quantity.to`, with a captured dimensionality claim
- Reject: stringly-typed unit labels; hand-rolled conversion factors; per-claim registries; wrapper-renames of `Quantity` arithmetic
