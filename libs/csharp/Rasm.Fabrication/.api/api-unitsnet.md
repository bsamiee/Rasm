# [RASM_FABRICATION_API_UNITSNET]

`UnitsNet` is a pure-managed AnyCPU IL-only quantity library providing strongly-typed physical quantities with culture-aware text parsing and unit-converting accessors; the fabrication folder consumes only the parse-and-canonicalize surface to admit unit-bearing text into the raw SI doubles the interiors operate on. The consumed slice is a THIN OVERLAY of the SI catalogue: the cut-parameter quartet `Speed`/`Length`/`RotationalSpeed`/`Pressure` (`Process/physics#CUT_PARAMETER`), EXTENDED with `Force`/`Power`/`Temperature`/`Angle`/`Torque` for the cutting-force/spindle-power/thermal/orientation/tightening-torque boundary and the `Spec/tolerance` dimensioned rows. The primary entry points are the static `<Quantity>.Parse(str, provider)` / `TryParse(str, provider, out result)` text-ingress facades and the unit-named SI accessor properties (`Speed.MetersPerSecond`, `Length.Meters`, `RotationalSpeed.RevolutionsPerMinute`, `Pressure.Pascals`, `Force.Newtons`, `Power.Watts`, `Temperature.DegreesCelsius`, `Angle.Degrees`, `Torque.NewtonMeters`) that emit the canonical raw `double`. A quantity is constructed from a scalar through the `From<Unit>(QuantityValue)` static factories (`QuantityValue` carries an implicit conversion from `double`), so a feed in `mm/min`, a surface speed in `m/min`, a spindle in `rpm`, a depth in `mm`, an assist pressure in `bar`, a cutting force in `N`, a spindle power in `W`, a temperature in `°C`, an orientation angle in `deg`, and a clamp torque in `N·m` admit once at their boundary and leave as raw SI doubles. No native asset and no RID burden — the package is managed IL (`lib/net9.0/UnitsNet.dll` plus localized satellite resource assemblies), ALC-safe, with no native runtime dependency.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `UnitsNet`
- package: `UnitsNet`
- version: `5.75.0` (centrally pinned)
- license: `MIT`
- assembly: `UnitsNet`
- namespace: `UnitsNet`
- asset: pure-managed AnyCPU IL (no native asset, no RID burden, ALC-safe); multi-targeted `net9.0`/`net8.0`/`netstandard2.0` with localized satellite resource assemblies — the `net10.0` consumer binds the highest compatible `lib/net9.0/UnitsNet.dll` (no `net10.0`-specific asset)
- rail: fabrication

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: cut-parameter quantity structs
- rail: fabrication

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]    | [CAPABILITY]                                                       |
| :-----: | :---------------- | :--------------- | :---------------------------------------------------------------- |
|  [01]   | `Speed`           | quantity struct  | feed and surface-speed text — `mm/min`, `m/min`, `m/s` ingress    |
|  [02]   | `Length`          | quantity struct  | depth and offset text — `mm`, `m` ingress                         |
|  [03]   | `RotationalSpeed` | quantity struct  | spindle text — `rpm`, `rad/s` ingress                             |
|  [04]   | `Pressure`        | quantity struct  | assist-gas/jet pressure text — `bar`, `Pa` ingress               |
|  [05]   | `Force`           | quantity struct  | cutting-force text — `N`, `lbf` ingress (Kienzle `kc`·chip area) |
|  [06]   | `Power`           | quantity struct  | spindle-power text — `W`, `kW` ingress                          |
|  [07]   | `Temperature`     | quantity struct  | cut/interface temperature text — `°C`, `K` ingress             |
|  [08]   | `Angle`           | quantity struct  | orientation/lead/taper angle text — `deg`, `rad` ingress       |
|  [09]   | `Torque`          | quantity struct  | clamp/tightening torque text — `N·m`, `lbf·ft` ingress         |
|  [10]   | `QuantityValue`   | scalar carrier   | the factory argument carrying an implicit conversion from `double` |

[PUBLIC_TYPE_SCOPE]: unit-enum axes selecting the parsed and emitted unit
- rail: fabrication

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                                                |
| :-----: | :-------------------- | :------------ | :--------------------------------------------------------- |
|  [01]   | `SpeedUnit`           | unit enum     | `MeterPerSecond`/`MeterPerMinute`/`MillimeterPerMinute`     |
|  [02]   | `LengthUnit`          | unit enum     | `Meter`/`Millimeter`                                        |
|  [03]   | `RotationalSpeedUnit` | unit enum     | `RevolutionPerMinute`/`RadianPerSecond`                    |
|  [04]   | `PressureUnit`        | unit enum     | `Bar`/`Pascal`                                             |
|  [05]   | `ForceUnit`           | unit enum     | `Newton`/`PoundForce`                                      |
|  [06]   | `PowerUnit`           | unit enum     | `Watt`/`Kilowatt`                                          |
|  [07]   | `TemperatureUnit`     | unit enum     | `DegreeCelsius`/`Kelvin`                                   |
|  [08]   | `AngleUnit`           | unit enum     | `Degree`/`Radian`                                          |
|  [09]   | `TorqueUnit`          | unit enum     | `NewtonMeter`/`PoundForceFoot`                             |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: text ingress — `<Quantity>.Parse` / `TryParse` facades
- rail: fabrication

| [INDEX] | [SURFACE]                                                                   | [ENTRY_FAMILY] | [CAPABILITY]                                                            |
| :-----: | :-------------------------------------------------------------------------- | :------------- | :--------------------------------------------------------------------- |
|  [01]   | `Speed.Parse(string str, IFormatProvider? provider)`                        | static parse   | parse a `"<value> <unit>"` speed string; THROWS on unparseable input   |
|  [02]   | `Speed.TryParse(string? str, IFormatProvider? provider, out Speed result)`  | static parse   | the non-throwing speed parse probe (false on unparseable input)        |
|  [03]   | `Length.Parse(string, IFormatProvider?)` / `TryParse(...)`                  | static parse   | parse/probe a length string                                            |
|  [04]   | `RotationalSpeed.Parse(string, IFormatProvider?)` / `TryParse(...)`         | static parse   | parse/probe a rotational-speed string                                  |
|  [05]   | `Pressure.Parse(string, IFormatProvider?)` / `TryParse(...)`                | static parse   | parse/probe a pressure string                                          |

[ENTRYPOINT_SCOPE]: scalar construction — `From<Unit>` static factories
- rail: fabrication

| [INDEX] | [SURFACE]                                                | [ENTRY_FAMILY]  | [CAPABILITY]                                  |
| :-----: | :------------------------------------------------------- | :-------------- | :-------------------------------------------- |
|  [01]   | `Speed.FromMillimetersPerMinutes(QuantityValue)`         | static factory  | mint a speed from a `mm/min` scalar           |
|  [02]   | `Speed.FromMetersPerSecond(QuantityValue)`               | static factory  | mint a speed from a `m/s` scalar              |
|  [03]   | `Length.FromMillimeters(QuantityValue)` / `FromMeters`   | static factory  | mint a length from a `mm`/`m` scalar          |
|  [04]   | `RotationalSpeed.FromRevolutionsPerMinute(QuantityValue)`| static factory  | mint a spindle speed from an `rpm` scalar     |
|  [05]   | `Pressure.FromBars(QuantityValue)` / `FromPascals`       | static factory  | mint a pressure from a `bar`/`Pa` scalar      |
|  [06]   | `Force.FromNewtons(QuantityValue)`                       | static factory  | mint a cutting force from an `N` scalar        |
|  [07]   | `Power.FromWatts(QuantityValue)` / `FromKilowatts`       | static factory  | mint a spindle power from a `W`/`kW` scalar    |
|  [08]   | `Temperature.FromDegreesCelsius(QuantityValue)` / `FromKelvins` | static factory | mint a temperature from a `°C`/`K` scalar  |
|  [09]   | `Angle.FromDegrees(QuantityValue)` / `FromRadians`       | static factory  | mint an angle from a `deg`/`rad` scalar        |
|  [10]   | `Torque.FromNewtonMeters(QuantityValue)`                 | static factory  | mint a torque from an `N·m` scalar             |

[ENTRYPOINT_SCOPE]: SI canonicalization — unit-named accessor properties emitting the raw `double`
- rail: fabrication

| [INDEX] | [SURFACE]                              | [ENTRY_FAMILY]  | [CAPABILITY]                                            |
| :-----: | :------------------------------------- | :-------------- | :----------------------------------------------------- |
|  [01]   | `Speed.MetersPerSecond`                | accessor → `double` | the SI canonical speed scalar (the `BaseUnit` value)   |
|  [02]   | `Speed.MetersPerMinutes`               | accessor → `double` | the surface-speed scalar in `m/min`                    |
|  [03]   | `Speed.MillimetersPerMinutes`          | accessor → `double` | the feed scalar in `mm/min`                            |
|  [04]   | `Length.Meters` / `Length.Millimeters` | accessor → `double` | the SI length scalar / the `mm` scalar                 |
|  [05]   | `RotationalSpeed.RevolutionsPerMinute` | accessor → `double` | the spindle scalar in `rpm`                            |
|  [06]   | `Pressure.Bars` / `Pressure.Pascals`   | accessor → `double` | the pressure scalar in `bar` / the SI `Pa` scalar      |
|  [07]   | `Force.Newtons`                        | accessor → `double` | the SI cutting-force scalar in `N`                     |
|  [08]   | `Power.Watts` / `Power.Kilowatts`      | accessor → `double` | the spindle-power scalar in `W` / `kW`                 |
|  [09]   | `Temperature.DegreesCelsius` / `Kelvins` | accessor → `double` | the temperature scalar in `°C` / the SI `K` scalar   |
|  [10]   | `Angle.Degrees` / `Angle.Radians`      | accessor → `double` | the angle scalar in `deg` / the SI `rad` scalar        |
|  [11]   | `Torque.NewtonMeters`                  | accessor → `double` | the SI torque scalar in `N·m`                          |

## [04]-[RATIFIED]

- [PARSE_SURFACE] Each quantity exposes a culture-aware `Parse(string str)` / `Parse(string str, IFormatProvider? provider)` static facade reading a `"<value> <unit>"` string (`"3000 mm/min"`, `"200 m/min"`, `"12000 rpm"`, `"5 mm"`, `"8 bar"`), and a `TryParse(string? str, out result)` / `TryParse(string? str, IFormatProvider? provider, out result)` non-throwing probe. The `Parse` facade THROWS on an unparseable or ambiguous unit (`UnitsNetSetup.Default.QuantityParser.Parse` raises a parse exception), so the cut-parameter boundary uses `TryParse` and lowers a `false` to `GeometryFault.DegenerateInput` ONCE at admission, the parse exception never escaping the boundary. The provider argument fixes the decimal/group culture; pass an invariant `CultureInfo` so a comma-decimal locale never reinterprets the scalar.
- [SCALAR_INGRESS] A quantity is constructed from a scalar through the `From<Unit>(QuantityValue value)` static factories — `Speed.FromMillimetersPerMinutes`, `Length.FromMillimeters`, `RotationalSpeed.FromRevolutionsPerMinute`, `Pressure.FromBars` — where `QuantityValue` carries an implicit conversion from `double`, so a raw scalar admits without an explicit wrap. The reverse is the unit-named accessor property (`Speed.MetersPerSecond`, `Length.Meters`, `RotationalSpeed.RevolutionsPerMinute`, `Pressure.Pascals`), each `As(<Unit>)` converting to the requested unit and returning the raw `double` the interior reads; the `BaseUnit` value (`MeterPerSecond`, `Meter`, `RadianPerSecond`, `Pascal`) is the SI canonical scalar. The boundary parses unit-bearing text once at `RemovalParameter.Admit`, reads the SI accessor, and emits the canonical `double`; no `Speed`/`Length`/`RotationalSpeed`/`Pressure` value crosses the boundary into a generator signature (the interior-double law).

[RAIL_LAW]:
- Package: `UnitsNet`
- Owns: culture-aware quantity-text parsing and the unit-converting SI-scalar canonicalization the `Process/physics#CUT_PARAMETER` `RemovalParameter.Admit` boundary reads to ingress feed/speed/depth/spindle/pressure text into raw doubles, EXTENDED with the `Force`/`Power`/`Temperature`/`Angle`/`Torque` quantities the cutting-force/power/thermal/orientation/torque boundary and the `Spec/tolerance` dimensioned rows read
- Accept: a `"<value> <unit>"` string and an invariant `IFormatProvider` at the boundary, the `From<Unit>(QuantityValue)` scalar factories, and the unit-named SI accessor properties emitting the canonical `double`, across the nine consumed quantities
- Reject: any consumed quantity type escaping the boundary into a `Cam`/`RemovalParameter`/`Spec` generator signature (the unit-bearing quantity crosses to a raw `double` at the one boundary, never travels the interior), a hand-rolled unit-conversion factor where the package owns the `As(<Unit>)` accessor, the throwing `Parse` facade at admission where `TryParse` lowers a `false` to `GeometryFault.DegenerateInput`, a culture-default parse where the boundary fixes an invariant provider, and re-documenting the full UnitsNet SI catalogue beyond the nine consumed quantities
