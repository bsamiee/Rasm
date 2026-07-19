# [RASM_FABRICATION_API_UNITSNET]

Full surface and stacking: `libs/csharp/.api/api-unitsnet.md` (shared-tier canonical owner).

## [01]-[RATIFIED]

[PARSE_SURFACE]:
Each quantity exposes `Parse(string, IFormatProvider?)` and `TryParse(string?, IFormatProvider?, out T)`. `Parse` throws on an unparseable or ambiguous unit, while each `PhysicsQuantity` row binds the corresponding `TryParse` delegate, fixes `CultureInfo.InvariantCulture`, projects its canonical machining scalar, and lowers `false` through one `Fin<double>` admission rail.

[SCALAR_INGRESS]:
A quantity is constructed through its `From<Unit>(QuantityValue)` factory and projected through its unit-named accessor. `PhysicsQuantity.Feed`, `Spindle`, `Length`, `Pressure`, `Power`, and `Temperature` bind `Speed`, `RotationalSpeed`, `Length`, `Pressure`, `Power`, and `Temperature` to millimetres per minute, revolutions per minute, millimetres, bars, watts, and degrees Celsius. `Duration.TryParse` plus `Duration.Seconds` owns textual dwell admission. `PhysicsAdmission.Quantity` carries only the resulting canonical `double`.

[ENERGY_COMPOSITION]:
`Power.FromWatts(value) * Duration.FromSeconds(seconds)` returns `Energy`; `Energy.FromJoules(value)` admits known work, and `Energy.Joules` projects the canonical scalar without local unit arithmetic.

[RAIL_LAW]:
- Package: `UnitsNet`
- Owns: `PhysicsQuantity` textual admission, canonical machining scalar projection, and typed energy composition
- Accept: unit-bearing text through an invariant `TryParse` delegate, scalar factories, unit-named accessors, and quantity operators
- Reject: quantity values escaping `PhysicsAdmission.Quantity`, local conversion factors, throwing `Parse` admission, and culture-default parsing
