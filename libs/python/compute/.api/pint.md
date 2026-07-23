# [PY_COMPUTE_API_PINT]

`pint` owns the physical-unit registry, dimensional quantities, measurement uncertainty, and unit conversion for the compute units rail. One shared `UnitRegistry` mints the vocabulary, and every array-admission and study-result claim rides as a `Quantity` whose magnitude is any array-protocol object, so units thread through `numpy` ufuncs over the same array the compute array rail folds. A dimensional claim captured off the `Quantity` feeds the study receipt, and a dimension mismatch is the boundary reject signal.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pint`
- package: `pint`
- module: `pint`
- owner: `compute`
- rail: units
- capability: physical-quantity arithmetic over a shared `UnitRegistry` — dimensional analysis, base/root/compact reduction, cross-dimension `Context` transforms, `Measurement` uncertainty, NumPy-array unit threading, and `wraps`/`check` signature enforcement

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: registry, quantity, and context owners

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

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]                 | [CAPABILITY]                                        |
| :-----: | :----------------------------- | :---------------------------- | :-------------------------------------------------- |
|  [01]   | `PintError`                    | `Exception`                   | root of every pint failure                          |
|  [02]   | `PintTypeError`                | `PintError`, `TypeError`      | typed base for dimension/offset/log calculus faults |
|  [03]   | `UndefinedUnitError`           | `PintError`, `AttributeError` | unit name not in the registry                       |
|  [04]   | `DimensionalityError`          | `PintTypeError`               | incompatible-dimension conversion or operation      |
|  [05]   | `OffsetUnitCalculusError`      | `PintTypeError`               | invalid offset-unit (temperature) multiply/divide   |
|  [06]   | `LogarithmicUnitCalculusError` | `PintTypeError`               | invalid logarithmic-unit (dB/decade) operation      |
|  [07]   | `DefinitionError`              | `PintError`, `ValueError`     | malformed unit-definition fault                     |
|  [08]   | `DefinitionSyntaxError`        | `PintError`, `ValueError`     | malformed definition string                         |
|  [09]   | `RedefinitionError`            | `PintError`, `ValueError`     | duplicate unit definition                           |
|  [10]   | `UnitStrippedWarning`          | `UserWarning`                 | magnitude consumed with the unit silently dropped   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registry construction, parsing, and definition — every surface is a `UnitRegistry` member; constructor keywords are in note [01].

| [INDEX] | [SURFACE]                                                    | [SHAPE]  | [CAPABILITY]                                      |
| :-----: | :----------------------------------------------------------- | :------- | :------------------------------------------------ |
|  [01]   | `UnitRegistry(...)`                                          | ctor     | a configured unit registry                        |
|  [02]   | `.__call__(input_string, case_sensitive=, **values)`         | instance | a `Quantity` from a string                        |
|  [03]   | `.parse_expression(input_string, case_sensitive=, **values)` | instance | the `__call__` body: a `Quantity` from a string   |
|  [04]   | `.parse_units(input_string, as_delta=, case_sensitive=)`     | instance | a bare `Unit` from a string                       |
|  [05]   | `.parse_pattern(input_string, pattern, many=)`               | instance | quantities from a formatted-string pattern        |
|  [06]   | `.Quantity(value, units=)`                                   | factory  | magnitude-plus-unit bound to this registry        |
|  [07]   | `.Unit(units)`                                               | factory  | a unit atom bound to this registry                |
|  [08]   | `.Measurement(value, error, units=)`                         | factory  | a value with a standard-error term                |
|  [09]   | `.define(definition)`                                        | instance | registers a unit/prefix/dimension definition      |
|  [10]   | `.load_definitions(file)`                                    | instance | loads a unit-definition file                      |
|  [11]   | `.convert(value, src, dst, inplace=False)`                   | instance | scalar/array magnitude expressed in `dst` units   |
|  [12]   | `.get_base_units(input_units)`                               | instance | `(scale, UnitsContainer)` base reduction          |
|  [13]   | `.get_dimensionality(input_units)`                           | instance | a `UnitsContainer` of base dimensions             |
|  [14]   | `.wraps(ret, args, strict=True)`                             | factory  | decorator enforcing argument/return units         |
|  [15]   | `.check(*args)`                                              | factory  | decorator asserting argument dimensionalities     |
|  [16]   | `.context(*names, **kwargs)`                                 | instance | activates a `Context` for cross-dimension convert |
|  [17]   | `.enable_contexts` / `.disable_contexts` / `.add_context`    | instance | enable, disable, and register contexts            |

- [01]-[UNITREGISTRY]: `filename=`, `force_ndarray=`, `force_ndarray_like=`, `autoconvert_offset_to_baseunit=`, `system=`, `auto_reduce_dimensions=`, `on_redefinition=`.

[ENTRYPOINT_SCOPE]: quantity conversion, projection, and compatibility

| [INDEX] | [SURFACE]                                        | [SHAPE]  | [CAPABILITY]                                            |
| :-----: | :----------------------------------------------- | :------- | :------------------------------------------------------ |
|  [01]   | `Quantity.to(other, *contexts, **ctx_kwargs)`    | instance | a new `Quantity` in target units (context-aware)        |
|  [02]   | `Quantity.ito(other, *contexts, **ctx_kwargs)`   | instance | mutates magnitude/units to the target                   |
|  [03]   | `Quantity.to_base_units()` / `ito_base_units()`  | instance | a `Quantity` in registry base units                     |
|  [04]   | `Quantity.to_root_units()` / `ito_root_units()`  | instance | a `Quantity` in root units                              |
|  [05]   | `Quantity.to_reduced_units()`                    | instance | a `Quantity` with repeated units cancelled              |
|  [06]   | `Quantity.to_compact(unit=None)`                 | instance | a `Quantity` at the most readable prefix                |
|  [07]   | `Quantity.m_as(units)`                           | instance | bare magnitude after converting to `units`              |
|  [08]   | `Quantity.magnitude` \| `Quantity.m`             | property | the bare magnitude (array-protocol object)              |
|  [09]   | `Quantity.units` \| `Quantity.u`                 | property | the `Unit` of the quantity                              |
|  [10]   | `Quantity.dimensionality`                        | property | a `UnitsContainer` of base dimensions                   |
|  [11]   | `Quantity.check(dimension)`                      | instance | dimensionality-match boolean                            |
|  [12]   | `Quantity.is_compatible_with(other, *contexts)`  | instance | convertibility boolean (context-aware)                  |
|  [13]   | `Quantity.compatible_units(*contexts)`           | instance | the frozenset of convertible units                      |
|  [14]   | `Quantity.plus_minus(error, relative=False)`     | instance | a `Measurement` from the quantity                       |
|  [15]   | `Quantity.from_sequence(seq, units=None)`        | factory  | a `Quantity` array from a sequence                      |
|  [16]   | `Quantity.to_tuple()` / `Quantity.from_tuple(t)` | instance | a `(magnitude, units-tuple)` round-trip for wire/pickle |

[ENTRYPOINT_SCOPE]: module-level application registry, formatting, and analysis

| [INDEX] | [SURFACE]                                    | [SHAPE]  | [CAPABILITY]                                    |
| :-----: | :------------------------------------------- | :------- | :---------------------------------------------- |
|  [01]   | `get_application_registry()`                 | static   | the process-wide `ApplicationRegistry` proxy    |
|  [02]   | `set_application_registry(registry)`         | static   | installs a process-wide registry                |
|  [03]   | `register_unit_format(name)`                 | factory  | decorator registering a custom unit-format spec |
|  [04]   | `UnitRegistry.formatter.default_format`      | property | the registry-owned default format spec          |
|  [05]   | `UnitRegistry.setup_matplotlib(enable=True)` | instance | registers unit-aware matplotlib axis support    |
|  [06]   | `pi_theorem(quantities, registry=None)`      | static   | dimensionless Buckingham-π groups               |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- registry: one shared `UnitRegistry` owns the vocabulary, so quantities minted by different registries never interoperate; the default application registry is a `LazyRegistry` deferring definition loading to first use.
- conversion: the `to` family returns a new `Quantity`, the `ito` family mutates in place; cross-dimension conversion requires a `Context`, passed to `to`/`is_compatible_with` or activated via `context`/`enable_contexts`.
- faults: an incompatible-dimension operation raises `DimensionalityError`, a magnitude consumed through a unit-dropping NumPy operation raises `UnitStrippedWarning`, and offset (temperature) and logarithmic (dB/decade) units raise `OffsetUnitCalculusError`/`LogarithmicUnitCalculusError`, both under `PintTypeError`.

[STACKING]:
- `numpy` (`.api/numpy.md`, substrate tier): a `Quantity` magnitude is the same array the array rail folds; `force_ndarray_like=True` coerces every magnitude to a NumPy/array-API container, and `numpy` ufuncs/`__array_function__` thread units through it in place of unwrap-operate-rewrap.
- `uncertainties` (`.api/uncertainties.md`): `Quantity.plus_minus`/`UnitRegistry.Measurement` build a `Measurement` carrying magnitude-plus-error, the unit-bearing mirror of the uncertainty rail's correlation graph, so a study magnitude carries unit and uncertainty in one carrier.
- `arviz` (`.api/arviz.md`) / `pandera` (`libs/python/data/.api/pandera.md`): the captured `dimensionality` (`UnitsContainer`) and base-unit `convert` factor are the unit claim on a study receipt, and a dimensionality mismatch is the boundary rail's reject signal.
- `compute` units rail: `wraps`/`check` decorate the offline study function to assert argument and return units at the Python boundary, and a magnitude crossing into a numerical kernel is base-unit-normalized via `m_as`/`to_base_units` so the kernel sees raw arrays.

[LOCAL_ADMISSION]:
- `get_application_registry` serves the rail's one shared registry; per-claim registries are rejected.
- each array-admission and study result carries a `Quantity` unit claim, converted via `to`/`to_base_units`/`convert`; error-bearing magnitudes use `Measurement`/`plus_minus`.
- unit claims are offline study evidence; product unit policy stays in the C# owners after graduation.

[RAIL_LAW]:
- Package: `pint`
- Owns: the physical-unit registry, dimensional quantities, measurement uncertainty, cross-dimension contexts, NumPy-array unit threading, and signature-level unit checking for the units rail
- Accept: a unit-bearing `Quantity` or `Measurement` from the shared `UnitRegistry`, converted via `to`/`to_base_units`, carrying a captured `dimensionality` claim, with boundaries decorated by `wraps`/`check`
- Reject: stringly-typed unit labels; hand-rolled conversion factors; per-claim registries; a parallel uncertainty object beside the unit; wrapper-renames of `Quantity` arithmetic
