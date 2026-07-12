# [RASM_FABRICATION_API_UNITSNET]

Full surface and stacking: `libs/csharp/.api/api-unitsnet.md` (shared-tier canonical owner).

## [01]-[RATIFIED]

[PARSE_SURFACE]:
Each quantity exposes a culture-aware `Parse(string str)` / `Parse(string str, IFormatProvider? provider)` static facade reading a `"<value> <unit>"` string (`"3000 mm/min"`, `"200 m/min"`, `"12000 rpm"`, `"5 mm"`, `"8 bar"`), and a `TryParse(string? str, out result)` / `TryParse(string? str, IFormatProvider? provider, out result)` non-throwing probe. The `Parse` facade THROWS on an unparseable or ambiguous unit (`UnitsNetSetup.Default.QuantityParser.Parse` raises a parse exception), so the cut-parameter boundary uses `TryParse` and lowers a `false` to `GeometryFault.DegenerateInput` ONCE at admission, the parse exception never escaping the boundary. The provider argument fixes the decimal/group culture; pass an invariant `CultureInfo` so a comma-decimal locale never reinterprets the scalar.

[SCALAR_INGRESS]:
A quantity is constructed from a scalar through the `From<Unit>(QuantityValue value)` static factories — `Speed.FromMillimetersPerMinutes`, `Length.FromMillimeters`, `RotationalSpeed.FromRevolutionsPerMinute`, `Pressure.FromBars` — where `QuantityValue` carries an implicit conversion from `double`, so a raw scalar admits without an explicit wrap. The reverse is the unit-named accessor property (`Speed.MetersPerSecond`, `Length.Meters`, `RotationalSpeed.RevolutionsPerMinute`, `Pressure.Pascals`), each `As(<Unit>)` converting to the requested unit and returning the raw `double` the interior reads; the `BaseUnit` value (`MeterPerSecond`, `Meter`, `RadianPerSecond`, `Pascal`) is the SI canonical scalar. The boundary parses unit-bearing text once at `RemovalParameter.Admit`, reads the SI accessor, and emits the canonical `double`; no `Speed`/`Length`/`RotationalSpeed`/`Pressure` value crosses the boundary into a generator signature (the interior-double law).

[RAIL_LAW]:

- Package: `UnitsNet`
- Owns: culture-aware quantity-text parsing and the unit-converting SI-scalar canonicalization the `RemovalParameter.Admit` boundary reads to ingress feed/speed/depth/spindle/pressure text into raw doubles, EXTENDED with the `Force`/`Power`/`Temperature`/`Angle`/`Torque` quantities the cutting-force/power/thermal/orientation/torque boundary and the `Spec/tolerance` dimensioned rows read
- Accept: a `"<value> <unit>"` string and an invariant `IFormatProvider` at the boundary, the `From<Unit>(QuantityValue)` scalar factories, and the unit-named SI accessor properties emitting the canonical `double`, across the nine consumed quantities
- Reject: any consumed quantity type escaping the boundary into a `Cam`/`RemovalParameter`/`Spec` generator signature (the unit-bearing quantity crosses to a raw `double` at the one boundary, never travels the interior), a hand-rolled unit-conversion factor where the package owns the `As(<Unit>)` accessor, the throwing `Parse` facade at admission where `TryParse` lowers a `false` to `GeometryFault.DegenerateInput`, a culture-default parse where the boundary fixes an invariant provider, and re-documenting the full UnitsNet SI catalogue beyond the nine consumed quantities
