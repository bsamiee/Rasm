# [PY_COMPUTE_API_PINT]

`pint` supplies the physical-unit registry, dimensional quantities, measurement uncertainty, and unit conversion for the compute units rail. One shared `UnitRegistry` owns the vocabulary; every array-admission and study-result claim carries a `Quantity` (magnitude-plus-unit), converts with `Quantity.to`, and surfaces a `DimensionalityError` on a dimension mismatch. The magnitude is any NumPy-array-protocol object, so a `Quantity` wraps the same array the compute array rail folds — `pint` threads units through `numpy` ufuncs (via `__array_ufunc__`/`__array_function__`) rather than re-implementing array arithmetic, and feeds a captured dimensionality claim into the `arviz`/`pandera` study receipt. It never re-implements the unit arithmetic the package owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pint`
- package: `pint`
- import: `pint`
- owner: `compute`
- rail: units
- capability: physical-quantity arithmetic over a shared `UnitRegistry` — string parsing, dimensional analysis, base/root/compact reduction, cross-dimension `Context` transforms, `Measurement` error propagation, NumPy-array unit wrapping through the array protocol, and `wraps`/`check` decorator-driven signature unit enforcement

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: registry, quantity, and context owners
- rail: units

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]     | [CAPABILITY]                                                                                 |
| :-----: | :-------------------- | :---------------- | :------------------------------------------------------------------------------------------- |
|  [01]   | `UnitRegistry`        | registry root     | shared unit and dimension vocabulary; the `Quantity`/`Unit`/`Measurement` factory and parser |
|  [02]   | `ApplicationRegistry` | process registry  | the proxy returned by `get_application_registry`; routes to the installed registry           |
|  [03]   | `LazyRegistry`        | deferred registry | default application registry that defers definition loading until first use                  |
|  [04]   | `Quantity`            | quantity carrier  | magnitude-plus-unit value; magnitude is any array-protocol object, NumPy ufuncs unit-thread  |
|  [05]   | `Unit`                | unit atom         | a single registered unit with prefix/symbol resolution                                       |
|  [06]   | `Measurement`         | uncertain value   | magnitude with a standard-error term; arithmetic propagates uncertainty                      |
|  [07]   | `Context`             | transform set     | named cross-dimension conversion rules (e.g. spectroscopy wavelength↔energy)                 |
|  [08]   | `Group`               | unit grouping     | a named subset of the registry's units for selective compatibility queries                   |

[PUBLIC_TYPE_SCOPE]: error and warning rails
- rail: units

| [INDEX] | [SYMBOL]                       | [BASE]                        | [FAULT]                                             |
| :-----: | :----------------------------- | :---------------------------- | :-------------------------------------------------- |
|  [01]   | `PintError`                    | `Exception`                   | root of every pint failure                          |
|  [02]   | `PintTypeError`                | `PintError`, `TypeError`      | typed base for dimension/offset/log calculus faults |
|  [03]   | `UndefinedUnitError`           | `PintError`, `AttributeError` | unit name not in the registry                       |
|  [04]   | `DimensionalityError`          | `PintTypeError`               | incompatible-dimension conversion or operation      |
|  [05]   | `OffsetUnitCalculusError`      | `PintTypeError`               | invalid offset-unit (temperature) multiply/divide   |
|  [06]   | `LogarithmicUnitCalculusError` | `PintTypeError`               | invalid logarithmic-unit (dB/decade) operation      |
|  [07]   | `DefinitionError`              | `PintError`, `ValueError`     | base for malformed-definition faults                |
|  [08]   | `DefinitionSyntaxError`        | `DefinitionError`             | malformed definition string                         |
|  [09]   | `RedefinitionError`            | `DefinitionError`             | duplicate unit definition                           |
|  [10]   | `UnitStrippedWarning`          | `UserWarning`                 | magnitude consumed with the unit silently dropped   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registry construction, parsing, and definition
- rail: units
- Every surface is a `UnitRegistry` member; the constructor keywords are in note [01].

| [INDEX] | [SURFACE]                                                    | [ENTRY_FAMILY]    | [RESULT]                                             |
| :-----: | :----------------------------------------------------------- | :---------------- | :--------------------------------------------------- |
|  [01]   | `UnitRegistry(...)`                                          | registry build    | a configured unit registry                           |
|  [02]   | `.__call__(input_string, case_sensitive=, **values)`         | parse             | a `Quantity` from a string                           |
|  [03]   | `.parse_expression(input_string, case_sensitive=, **values)` | parse             | a `Quantity` from a string (the `__call__` body)     |
|  [04]   | `.parse_units(input_string, as_delta=, case_sensitive=)`     | parse             | a bare `Unit` from a string                          |
|  [05]   | `.parse_pattern(input_string, pattern, many=)`               | parse             | quantities from a formatted-string pattern           |
|  [06]   | `.Quantity(value, units=)`                                   | construct         | magnitude-plus-unit bound to this registry           |
|  [07]   | `.Unit(units)`                                               | construct         | a unit atom bound to this registry                   |
|  [08]   | `.Measurement(value, error, units=)`                         | construct         | a value with a standard-error term                   |
|  [09]   | `.define(definition)`                                        | mutate            | registers a new unit/prefix/dimension definition     |
|  [10]   | `.load_definitions(file)`                                    | mutate            | loads a unit-definition file into the registry       |
|  [11]   | `.convert(value, src, dst, inplace=False)`                   | convert           | scalar/array magnitude expressed in `dst` units      |
|  [12]   | `.get_base_units(input_units)`                               | introspect        | `(scale, UnitsContainer)` reduction to base units    |
|  [13]   | `.get_dimensionality(input_units)`                           | introspect        | a `UnitsContainer` of base dimensions                |
|  [14]   | `.wraps(ret, args, strict=True)`                             | decorator factory | enforces argument/return units at a boundary         |
|  [15]   | `.check(*args)`                                              | decorator factory | asserts argument dimensionalities at a boundary      |
|  [16]   | `.context(*names, **kwargs)`                                 | context           | activates a `Context` for cross-dimension conversion |
|  [17]   | `.enable_contexts` / `.disable_contexts` / `.add_context`    | context           | enable, disable, and register contexts               |

- [01]-[UNITREGISTRY]: `filename=`, `force_ndarray=`, `force_ndarray_like=`, `autoconvert_offset_to_baseunit=`, `system=`, `auto_reduce_dimensions=`, `on_redefinition=`.

[ENTRYPOINT_SCOPE]: quantity conversion, projection, and compatibility
- rail: units

| [INDEX] | [SURFACE]                                        | [ENTRY_FAMILY]   | [RESULT]                                                |
| :-----: | :----------------------------------------------- | :--------------- | :------------------------------------------------------ |
|  [01]   | `Quantity.to(other, *contexts, **ctx_kwargs)`    | convert          | a new `Quantity` in target units (context-aware)        |
|  [02]   | `Quantity.ito(other, *contexts, **ctx_kwargs)`   | convert in place | mutates magnitude/units to the target                   |
|  [03]   | `Quantity.to_base_units()` / `ito_base_units()`  | convert          | a `Quantity` in registry base units                     |
|  [04]   | `Quantity.to_root_units()` / `ito_root_units()`  | convert          | a `Quantity` in root units                              |
|  [05]   | `Quantity.to_reduced_units()`                    | convert          | a `Quantity` with repeated units cancelled              |
|  [06]   | `Quantity.to_compact(unit=None)`                 | convert          | a `Quantity` at the most readable prefix                |
|  [07]   | `Quantity.m_as(units)`                           | project          | bare magnitude after converting to `units`              |
|  [08]   | `Quantity.magnitude` \| `Quantity.m`             | project          | the bare magnitude (array-protocol object)              |
|  [09]   | `Quantity.units` \| `Quantity.u`                 | project          | the `Unit` of the quantity                              |
|  [10]   | `Quantity.dimensionality`                        | introspect       | a `UnitsContainer` of base dimensions                   |
|  [11]   | `Quantity.check(dimension)`                      | predicate        | dimensionality-match boolean                            |
|  [12]   | `Quantity.is_compatible_with(other, *contexts)`  | predicate        | convertibility boolean (context-aware)                  |
|  [13]   | `Quantity.compatible_units(*contexts)`           | introspect       | the frozenset of convertible units                      |
|  [14]   | `Quantity.plus_minus(error, relative=False)`     | construct        | a `Measurement` from the quantity                       |
|  [15]   | `Quantity.from_sequence(seq, units=None)`        | construct        | a `Quantity` array from a sequence                      |
|  [16]   | `Quantity.to_tuple()` / `Quantity.from_tuple(t)` | (de)serialize    | a `(magnitude, units-tuple)` round-trip for wire/pickle |

[ENTRYPOINT_SCOPE]: module-level application registry, formatting, and analysis
- rail: units

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY]  | [RESULT]                                                                |
| :-----: | :------------------------------------------- | :-------------- | :---------------------------------------------------------------------- |
|  [01]   | `get_application_registry()`                 | shared registry | the process-wide `ApplicationRegistry` proxy                            |
|  [02]   | `set_application_registry(registry)`         | shared registry | installs a process-wide registry                                        |
|  [03]   | `register_unit_format(name)`                 | formatting      | decorator registering a custom unit-format spec                         |
|  [04]   | `formatter(...)`                             | formatting      | a formatted unit string (deprecated module fn; prefer `ureg.formatter`) |
|  [05]   | `UnitRegistry.formatter.default_format`      | formatting      | the registry-owned default format spec                                  |
|  [06]   | `UnitRegistry.setup_matplotlib(enable=True)` | formatting      | registers the unit-aware matplotlib axis support                        |
|  [07]   | `pi_theorem(quantities, registry=None)`      | analysis        | dimensionless Buckingham-π groups                                       |

## [04]-[IMPLEMENTATION_LAW]

[UNIT_TOPOLOGY]:
- registry: one shared `UnitRegistry` owns the vocabulary; parsing, dimensionality, base reduction, and `Quantity`/`Unit`/`Measurement` construction all flow from that instance, so quantities from one registry never interoperate with another.
- application registry: `get_application_registry`/`set_application_registry` install a single `ApplicationRegistry` proxy across modules; the default `LazyRegistry` defers definition loading until first use.
- parsing: `__call__`/`parse_expression` build a `Quantity` from a string, `parse_units` a bare `Unit`, and `parse_pattern` extracts quantities from a formatted record line.
- conversion: `to` returns a new value, `ito` mutates in place; `to_base_units`/`to_root_units`/`to_reduced_units` normalize, `to_compact` picks a readable prefix; cross-dimension conversion requires a `Context` passed to `to`/`is_compatible_with` or activated via `context`/`enable_contexts`.
- projection: `magnitude`/`m` strips the unit, `m_as` strips after converting; consuming the magnitude through a bare NumPy operation that drops the unit raises `UnitStrippedWarning`.
- dimensionality: `check`, `is_compatible_with`, and `get_dimensionality` resolve compatibility before arithmetic; an incompatible operation raises `DimensionalityError`.
- offset/log units: temperature and other offset units route `OffsetUnitCalculusError`; dB/decade logarithmic units route `LogarithmicUnitCalculusError`; both descend from `PintTypeError`.

[INTEGRATION_LAW]:
- array rail: a `Quantity` magnitude is any array-protocol object; constructing the registry with `force_ndarray_like=True` makes every magnitude a NumPy/array-API container, and `numpy` ufuncs/`__array_function__` thread units through the same array the compute array rail folds — never unwrap-to-magnitude, operate, re-wrap.
- uncertainty rail: `Quantity.plus_minus`/`UnitRegistry.Measurement` produce a `Measurement` whose arithmetic propagates standard error; this is the unit-bearing mirror of the `uncertainties` rail, so error-bearing study magnitudes carry both unit and uncertainty in one carrier rather than two parallel objects.
- study receipt: the captured `dimensionality` (a `UnitsContainer`) plus the base-unit `convert` factor are the unit claim attached to a `pandera`/`arviz` study result; a dimensionality mismatch is the boundary rail's reject signal.
- boundary enforcement: `wraps`/`check` decorate the offline study function so argument and return units are asserted at the Python boundary; the magnitude crossing into a numerical kernel is always base-unit-normalized via `m_as`/`to_base_units` so the kernel sees raw arrays.

[LOCAL_ADMISSION]:
- one shared `UnitRegistry` (via `get_application_registry`) carries the rail's vocabulary; per-claim registries are rejected because their `Quantity` types do not interoperate.
- each array-admission and study result carries a `Quantity` unit claim; a dimensionality mismatch surfaces a `DimensionalityError` at the boundary rail.
- conversion uses `to`/`to_base_units`/`convert`; hand-rolled conversion factors and stringly-typed unit labels are rejected.
- error-bearing magnitudes use `Measurement`/`plus_minus` rather than a parallel uncertainty record.
- unit claims are offline study evidence; product unit policy stays in C# owners after graduation.

[RAIL_LAW]:
- Package: `pint`
- Owns: the physical-unit registry, dimensional quantities, measurement uncertainty, cross-dimension contexts, NumPy-array unit threading, and signature-level unit checking for the units rail
- Accept: a unit-bearing `Quantity` (or `Measurement`) built from the shared `UnitRegistry`, converted via `to`/`to_base_units`, with a captured `dimensionality` claim, decorated boundaries via `wraps`/`check`
- Reject: stringly-typed unit labels; hand-rolled conversion factors; per-claim registries; a parallel uncertainty object beside the unit; wrapper-renames of `Quantity` arithmetic
