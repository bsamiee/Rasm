# [RASM_FABRICATION_API_UNITSNET]

`UnitsNet` is a pure-managed AnyCPU IL-only quantity library providing strongly-typed physical quantities (`Speed`, `Length`, `RotationalSpeed`, `Pressure`, and the rest of the SI/imperial catalogue) with culture-aware text parsing and unit-converting accessors; the fabrication folder consumes only the parse-and-canonicalize surface to admit unit-bearing cutting-data text into the raw SI doubles the `Process/physics#CUT_PARAMETER` interior operates on. The primary entry points are the static `<Quantity>.Parse(str, provider)` / `TryParse(str, provider, out result)` text-ingress facades and the unit-named SI accessor properties (`Speed.MetersPerSecond`, `Length.Meters`, `RotationalSpeed.RevolutionsPerMinute`, `Pressure.Pascals`) that emit the canonical raw `double`. A quantity is constructed from a scalar through the `From<Unit>(QuantityValue)` static factories (`QuantityValue` carries an implicit conversion from `double`), so a feed in `mm/min`, a surface speed in `m/min`, a spindle in `rpm`, a depth in `mm`, and an assist pressure in `bar` admit once at the cut-parameter boundary and leave as raw SI doubles. No native asset and no RID burden — the package is managed IL (`lib/net9.0/UnitsNet.dll` plus localized satellite resource assemblies), ALC-safe, with no native runtime dependency.

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
|  [05]   | `QuantityValue`   | scalar carrier   | the factory argument carrying an implicit conversion from `double` |

[PUBLIC_TYPE_SCOPE]: unit-enum axes selecting the parsed and emitted unit
- rail: fabrication

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                                                |
| :-----: | :-------------------- | :------------ | :--------------------------------------------------------- |
|  [01]   | `SpeedUnit`           | unit enum     | `MeterPerSecond`/`MeterPerMinute`/`MillimeterPerMinute`     |
|  [02]   | `LengthUnit`          | unit enum     | `Meter`/`Millimeter`                                        |
|  [03]   | `RotationalSpeedUnit` | unit enum     | `RevolutionPerMinute`/`RadianPerSecond`                    |
|  [04]   | `PressureUnit`        | unit enum     | `Bar`/`Pascal`                                             |

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

## [04]-[RATIFIED]

- [PARSE_SURFACE] Each quantity exposes a culture-aware `Parse(string str)` / `Parse(string str, IFormatProvider? provider)` static facade reading a `"<value> <unit>"` string (`"3000 mm/min"`, `"200 m/min"`, `"12000 rpm"`, `"5 mm"`, `"8 bar"`), and a `TryParse(string? str, out result)` / `TryParse(string? str, IFormatProvider? provider, out result)` non-throwing probe. The `Parse` facade THROWS on an unparseable or ambiguous unit (`UnitsNetSetup.Default.QuantityParser.Parse` raises a parse exception), so the cut-parameter boundary uses `TryParse` and lowers a `false` to `GeometryFault.DegenerateInput` ONCE at admission, the parse exception never escaping the boundary. The provider argument fixes the decimal/group culture; pass an invariant `CultureInfo` so a comma-decimal locale never reinterprets the scalar.
- [SCALAR_INGRESS] A quantity is constructed from a scalar through the `From<Unit>(QuantityValue value)` static factories — `Speed.FromMillimetersPerMinutes`, `Length.FromMillimeters`, `RotationalSpeed.FromRevolutionsPerMinute`, `Pressure.FromBars` — where `QuantityValue` carries an implicit conversion from `double`, so a raw scalar admits without an explicit wrap. The reverse is the unit-named accessor property (`Speed.MetersPerSecond`, `Length.Meters`, `RotationalSpeed.RevolutionsPerMinute`, `Pressure.Pascals`), each `As(<Unit>)` converting to the requested unit and returning the raw `double` the interior reads; the `BaseUnit` value (`MeterPerSecond`, `Meter`, `RadianPerSecond`, `Pascal`) is the SI canonical scalar. The boundary parses unit-bearing text once at `RemovalParameter.Admit`, reads the SI accessor, and emits the canonical `double`; no `Speed`/`Length`/`RotationalSpeed`/`Pressure` value crosses the boundary into a generator signature (the interior-double law).

[RAIL_LAW]:
- Package: `UnitsNet`
- Owns: culture-aware quantity-text parsing and the unit-converting SI-scalar canonicalization the `Process/physics#CUT_PARAMETER` `RemovalParameter.Admit` boundary reads to ingress feed/speed/depth/spindle/pressure text into raw doubles
- Accept: a `"<value> <unit>"` string and an invariant `IFormatProvider` at the cut-parameter boundary, the `From<Unit>(QuantityValue)` scalar factories, and the unit-named SI accessor properties emitting the canonical `double`
- Reject: a `Speed`/`Length`/`RotationalSpeed`/`Pressure` quantity type escaping the boundary into a `Cam`/`StraightSkeleton`/`RemovalParameter` generator signature (the unit-bearing quantity crosses to a raw `double` at the one boundary, never travels the interior), a hand-rolled unit-conversion factor where the package owns the `As(<Unit>)` accessor, the throwing `Parse` facade at admission where `TryParse` lowers a `false` to `GeometryFault.DegenerateInput`, and a culture-default parse where the boundary fixes an invariant provider
