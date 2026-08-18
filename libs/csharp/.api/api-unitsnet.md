# [RASM_API_UNITSNET]

`UnitsNet` owns strongly typed physical quantities over the `double`-or-`decimal` `QuantityValue` scalar, binding native operator algebra, boundary conversion, registry identity, and unit-system policy across measured inputs and receipts. Each generated quantity struct fixes one physical concern to its `QuantityInfo.Name` and SI base unit, and feeds the units rail every Compute and materials boundary canonicalizes through.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `UnitsNet`
- package: `UnitsNet` (MIT-0)
- assembly: `UnitsNet`
- namespace: `UnitsNet`, `UnitsNet.Units`, `UnitsNet.GenericMath`
- asset: managed runtime library with localized abbreviation satellite assemblies
- rail: units

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: quantity contracts and scalar carriers

`IQuantity` boxes projection and conversion; its generic forms progressively bind unit, backing scalar, and self type.

| [INDEX] | [SYMBOL]                                    | [TYPE_FAMILY] | [CAPABILITY]                      |
| :-----: | :------------------------------------------ | :------------ | :-------------------------------- |
|  [01]   | `IQuantity : IFormattable`                  | interface     | boxed projection and conversion   |
|  [02]   | `IQuantity<TUnit>`                          | interface     | typed-unit projection             |
|  [03]   | `IQuantity<TSelf, TUnit, TValue>`           | interface     | self-typed quantity contract      |
|  [04]   | `IValueQuantity<TValue>`                    | interface     | backing-scalar projection         |
|  [05]   | `IArithmeticQuantity<TSelf, TUnit, TValue>` | interface     | native arithmetic contract        |
|  [06]   | `QuantityValue`                             | struct        | `double`/`decimal` scalar carrier |
|  [07]   | `BaseDimensions`                            | class         | seven-axis SI exponent signature  |
|  [08]   | `BaseUnits`                                 | class         | nullable SI unit policy           |

[PUBLIC_TYPE_SCOPE]: admitted quantity families

Each family is a `readonly struct` with native operators, keyed by its `QuantityInfo.Name` to an SI `BaseUnit` and a parallel `<Quantity>Unit` enum in `UnitsNet.Units`; a family enters the corpus when a boundary consumer admits it. `TemperatureDelta` carries the affine difference between two `Temperature` values.

| [INDEX] | [SYMBOL]                  | [BASE_UNIT]                    | [CAPABILITY]              |
| :-----: | :------------------------ | :----------------------------- | :------------------------ |
|  [01]   | `Length`                  | `Meter`                        | linear dimension          |
|  [02]   | `Area`                    | `SquareMeter`                  | planar measure            |
|  [03]   | `Volume`                  | `CubicMeter`                   | spatial measure           |
|  [04]   | `Mass`                    | `Kilogram`                     | inertial measure          |
|  [05]   | `Duration`                | `Second`                       | physical time             |
|  [06]   | `Speed`                   | `MeterPerSecond`               | linear rate               |
|  [07]   | `Acceleration`            | `MeterPerSecondSquared`        | speed derivative          |
|  [08]   | `Force`                   | `Newton`                       | mass-acceleration product |
|  [09]   | `Pressure`                | `Pascal`                       | force-area quotient       |
|  [10]   | `Energy`                  | `Joule`                        | power-duration product    |
|  [11]   | `Power`                   | `Watt`                         | energy-duration quotient  |
|  [12]   | `Temperature`             | `Kelvin`                       | affine temperature        |
|  [13]   | `TemperatureDelta`        | `Kelvin`                       | affine temperature step   |
|  [14]   | `Angle`                   | `Degree`                       | plane rotation            |
|  [15]   | `Torque`                  | `NewtonMeter`                  | force-length moment       |
|  [16]   | `Ratio`                   | `DecimalFraction`              | dimensionless ratio       |
|  [17]   | `Density`                 | `KilogramPerCubicMeter`        | volumetric mass           |
|  [18]   | `AreaMomentOfInertia`     | `MeterToTheFourth`             | section second moment     |
|  [19]   | `ThermalConductivity`     | `WattPerMeterKelvin`           | material heat conduction  |
|  [20]   | `ThermalResistance`       | `SquareMeterKelvinPerKilowatt` | thermal insulance         |
|  [21]   | `HeatTransferCoefficient` | `WattPerSquareMeterKelvin`     | thermal transmittance     |
|  [22]   | `SpecificEntropy`         | `JoulePerKilogramKelvin`       | mass-specific entropy     |
|  [23]   | `Frequency`               | `Hertz`                        | cyclic rate               |
|  [24]   | `VolumeFlow`              | `CubicMeterPerSecond`          | volumetric flow rate      |
|  [25]   | `MassFlow`                | `GramPerSecond`                | mass flow rate            |
|  [26]   | `RotationalSpeed`         | `RadianPerSecond`              | angular velocity          |
|  [27]   | `Level`                   | `Decibel`                      | logarithmic level         |
|  [28]   | `Illuminance`             | `Lux`                          | incident luminous flux    |
|  [29]   | `Irradiance`              | `WattPerSquareMeter`           | radiant flux density      |
|  [30]   | `Irradiation`             | `JoulePerSquareMeter`          | radiant exposure          |
|  [31]   | `Luminance`               | `CandelaPerSquareMeter`        | directional surface light |
|  [32]   | `LuminousFlux`            | `Lumen`                        | perceived light power     |
|  [33]   | `LuminousIntensity`       | `Candela`                      | directional luminous flux |
|  [34]   | `LinearDensity`           | `KilogramPerMeter`             | mass per unit length      |
|  [35]   | `VolumePerLength`         | `CubicMeterPerMeter`           | volume per unit length    |
|  [36]   | `RelativeHumidity`        | `Percent`                      | moisture ratio            |
|  [37]   | `ElectricResistivity`     | `OhmMeter`                     | volume resistivity        |
|  [38]   | `ElectricField`           | `VoltPerMeter`                 | electric field strength   |

[PUBLIC_TYPE_SCOPE]: parsing, conversion, metadata, and registration

`DefaultUnitAttribute`, `ConvertToUnitAttribute`, and `DisplayAsUnitAttribute` are consumer-applied metadata read only by `QuantityTypeConverter`; the generated quantity structs emit none, so attribute reflection over them resolves to nothing.

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [CAPABILITY]                    |
| :-----: | :---------------------------- | :------------ | :------------------------------ |
|  [01]   | `Quantity`                    | class         | boxed resolution registry       |
|  [02]   | `QuantityInfo`                | class         | family metadata                 |
|  [03]   | `UnitInfo`                    | class         | unit metadata                   |
|  [04]   | `UnitConverter`               | class         | dynamic conversion registry     |
|  [05]   | `ConversionFunction`          | delegate      | cached conversion               |
|  [06]   | `UnitParser`                  | class         | unit-enum parsing               |
|  [07]   | `QuantityParser`              | class         | boxed-quantity parsing          |
|  [08]   | `QuantityFormatter`           | class         | typed quantity rendering        |
|  [09]   | `UnitAbbreviationsCache`      | class         | culture-keyed abbreviations     |
|  [10]   | `UnitMath`                    | class         | typed quantity aggregation      |
|  [11]   | `UnitSystem`                  | class         | base-unit policy                |
|  [12]   | `UnitsNetSetup`               | class         | configured service root         |
|  [13]   | `ComparisonType`              | enum          | relative or absolute error mode |
|  [14]   | `Comparison`                  | class         | scalar tolerance comparison     |
|  [15]   | `QuantityTypeConverter`       | class         | attribute-driven type converter |
|  [16]   | `DefaultUnitAttribute`        | class         | default-unit declaration        |
|  [17]   | `ConvertToUnitAttribute`      | class         | conversion-unit declaration     |
|  [18]   | `DisplayAsUnitAttribute`      | class         | display-unit declaration        |
|  [19]   | `UnitsNetException`           | class         | package failure root            |
|  [20]   | `UnitNotFoundException`       | class         | unresolved unit failure         |
|  [21]   | `AmbiguousUnitParseException` | class         | ambiguous-abbreviation failure  |

`ConversionFunction` carries two shapes — `public delegate IQuantity ConversionFunction(IQuantity)` and `public delegate TQuantity ConversionFunction<TQuantity>(TQuantity) where TQuantity : IQuantity` — and BOTH `GetConversionFunction<TQuantity>` and `TryGetConversionFunction<TQuantity>` yield the NON-generic typeless form, so a typed hot path re-narrows the boxed `IQuantity` result itself; a `Func<TQuantity, TQuantity>` method-group conversion over either getter does not compile.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: boxed quantity projection

| [INDEX] | [SURFACE]                                          | [SHAPE]  | [CAPABILITY]              |
| :-----: | :------------------------------------------------- | :------- | :------------------------ |
|  [01]   | `IQuantity.QuantityInfo`                           | property | family metadata           |
|  [02]   | `IQuantity.Unit`                                   | property | constructed unit          |
|  [03]   | `IQuantity.Value`                                  | property | constructed scalar        |
|  [04]   | `IQuantity.Dimensions`                             | property | `BaseDimensions` read     |
|  [05]   | `IQuantity.As(Enum)`                               | instance | scalar unit projection    |
|  [06]   | `IQuantity.As(UnitSystem)`                         | instance | scalar policy projection  |
|  [07]   | `IQuantity.Equals(IQuantity?, IQuantity)`          | instance | tolerance equality        |
|  [08]   | `IQuantity.ToUnit(Enum)`                           | instance | boxed unit reprojection   |
|  [09]   | `IQuantity.ToUnit(UnitSystem)`                     | instance | boxed policy reprojection |
|  [10]   | `IQuantity.ToString(IFormatProvider?)`             | instance | culture-aware rendering   |
|  [11]   | `IQuantity.ToString(string?, IFormatProvider?)`    | instance | format-bearing rendering  |
|  [12]   | `QuantityValue.Type`                               | property | scalar storage kind       |
|  [13]   | `QuantityValue.IsDecimal`                          | property | decimal-kind test         |
|  [14]   | `QuantityValue.Zero`                               | static   | scalar identity           |
|  [15]   | `implicit operator QuantityValue(<numeric-value>)` | operator | numeric admission         |
|  [16]   | `explicit operator double(QuantityValue)`          | operator | double projection         |
|  [17]   | `explicit operator decimal(QuantityValue)`         | operator | decimal projection        |

- Row [11] rides the `IFormattable` base, so a boxed quantity takes a numeric format string without narrowing to its family struct; `QuantityFormatter.Format<TUnit>` is the typed-unit equivalent and needs the narrowed `IQuantity<TUnit>` face.
- Row [04] is `[Obsolete]` on the installed surface — "This property will be removed in the next major release. Consider using QuantityInfo.BaseDimensions instead." — so a dimension read off a boxed quantity spells `q.QuantityInfo.BaseDimensions`, and `IQuantity.Dimensions` stays a read of last resort where no `QuantityInfo` is in hand.

[ENTRYPOINT_SCOPE]: dimensional signature and unit policy

`BaseDimensions` and `BaseUnits` own the physical signature and the SI unit policy; algebra methods and operators derive compound dimensions for family discovery.

| [INDEX] | [SURFACE]                                          | [SHAPE]  | [CAPABILITY]                |
| :-----: | :------------------------------------------------- | :------- | :-------------------------- |
|  [01]   | `BaseDimensions.Length`                            | property | length exponent             |
|  [02]   | `BaseDimensions.Mass`                              | property | mass exponent               |
|  [03]   | `BaseDimensions.Time`                              | property | time exponent               |
|  [04]   | `BaseDimensions.Current`                           | property | current exponent            |
|  [05]   | `BaseDimensions.Temperature`                       | property | temperature exponent        |
|  [06]   | `BaseDimensions.Amount`                            | property | substance exponent          |
|  [07]   | `BaseDimensions.LuminousIntensity`                 | property | luminous-intensity exponent |
|  [08]   | `BaseDimensions.Multiply(BaseDimensions)`          | instance | dimension multiplication    |
|  [09]   | `BaseDimensions.Divide(BaseDimensions)`            | instance | dimension division          |
|  [10]   | `operator *(BaseDimensions, BaseDimensions)`       | operator | multiplication operator     |
|  [11]   | `operator /(BaseDimensions, BaseDimensions)`       | operator | division operator           |
|  [12]   | `BaseDimensions.Dimensionless`                     | static   | dimension identity          |
|  [13]   | `BaseDimensions.IsBaseQuantity()`                  | instance | base-quantity test          |
|  [14]   | `BaseDimensions.IsDerivedQuantity()`               | instance | derived-quantity test       |
|  [15]   | `BaseDimensions.IsDimensionless()`                 | instance | dimensionless test          |
|  [16]   | `BaseUnits.Length` … `BaseUnits.LuminousIntensity` | property | per-axis unit selector      |
|  [17]   | `BaseUnits.IsFullyDefined`                         | property | policy-completeness test    |
|  [18]   | `BaseUnits.IsSubsetOf(BaseUnits)`                  | instance | policy-subset test          |
|  [19]   | `BaseUnits.Undefined`                              | static   | undefined policy            |

[ENTRYPOINT_SCOPE]: typed construction, conversion, and operators

`Length` is the exemplar whose members every generated quantity struct repeats: one `From<Unit>` factory, one `<Unit>` projection property, and `Zero` per unit in `UnitsNet.Units`, with the algebra below.

| [INDEX] | [SURFACE]                                                  | [SHAPE]  | [CAPABILITY]               |
| :-----: | :--------------------------------------------------------- | :------- | :------------------------- |
|  [01]   | `Length.FromMeters(QuantityValue)`                         | factory  | per-unit construction      |
|  [02]   | `Length.From(QuantityValue, LengthUnit)`                   | factory  | enum-keyed construction    |
|  [03]   | `new Length(double, LengthUnit)`                           | ctor     | explicit-unit construction |
|  [04]   | `Length.FromFeetInches(double, double)`                    | factory  | composite construction     |
|  [05]   | `Length.As(LengthUnit) -> double`                          | instance | scalar unit projection     |
|  [06]   | `Length.As(UnitSystem) -> double`                          | instance | scalar policy projection   |
|  [07]   | `Length.Meters -> double`                                  | property | per-unit projection        |
|  [08]   | `Length.ToUnit(LengthUnit)`                                | instance | quantity reprojection      |
|  [09]   | `Length.ToUnit(LengthUnit, UnitConverter)`                 | instance | converter reprojection     |
|  [10]   | `Length.ToUnit(UnitSystem)`                                | instance | policy reprojection        |
|  [11]   | `Length.Inverse() -> ReciprocalLength`                     | instance | reciprocal quantity        |
|  [12]   | `Length.GetAbbreviation(LengthUnit, IFormatProvider?)`     | static   | unit abbreviation          |
|  [13]   | `operator +(Length, Length)`                               | operator | quantity addition          |
|  [14]   | `operator -(Length, Length)`                               | operator | quantity subtraction       |
|  [15]   | `operator -(Length)`                                       | operator | additive inversion         |
|  [16]   | `operator *(Length, double)`                               | operator | scalar multiplication      |
|  [17]   | `operator /(Length, double)`                               | operator | scalar division            |
|  [18]   | `operator /(Length, Length) -> double`                     | operator | quantity ratio             |
|  [19]   | `operator *(Length, Length) -> Area`                       | operator | area derivation            |
|  [20]   | `operator /(Length, Duration) -> Speed`                    | operator | speed derivation           |
|  [21]   | `operator *(Mass, Acceleration) -> Force`                  | operator | force derivation           |
|  [22]   | `operator /(Force, Area) -> Pressure`                      | operator | pressure derivation        |
|  [23]   | `operator *(Power, Duration) -> Energy`                    | operator | energy derivation          |
|  [24]   | `operator *(Force, Length) -> Torque`                      | operator | torque derivation          |
|  [25]   | `operator *(Pressure, Area) -> Force`                      | operator | force from pressure        |
|  [26]   | `operator *(Area, Length) -> Volume`                       | operator | volume derivation          |
|  [27]   | `operator *(Density, Volume) -> Mass`                      | operator | mass derivation            |
|  [28]   | `operator -(Temperature, Temperature) -> TemperatureDelta` | operator | affine difference          |
|  [29]   | `operator +(Temperature, TemperatureDelta) -> Temperature` | operator | affine offset              |
|  [30]   | `operator -(Temperature, TemperatureDelta) -> Temperature` | operator | affine offset              |
|  [31]   | `operator <(Length, Length)`                               | operator | ordered comparison         |
|  [32]   | `operator <=(Length, Length)`                              | operator | ordered comparison         |
|  [33]   | `Length.CompareTo(Length)`                                 | instance | quantity ordering          |
|  [34]   | `Length.Equals(Length, Length)`                            | instance | tolerance equality         |
|  [35]   | `Length.ToString(string?, IFormatProvider?)`               | instance | culture-aware rendering    |
|  [36]   | `Length.Zero`                                              | static   | additive identity          |
|  [37]   | `Length.BaseUnit`                                          | static   | base-unit metadata         |
|  [38]   | `Length.Units`                                             | static   | unit vocabulary            |
|  [39]   | `Length.Info`                                              | static   | quantity metadata          |
|  [40]   | `Length.BaseDimensions`                                    | static   | dimension metadata         |

Equality on a quantity struct is the tolerance form ALONE: `==`, `!=`, `Equals(object?)`, and `Equals(Length other)` all carry `[Obsolete]` on the installed surface, and row [34]'s second argument is a QUANTITY, not a `double` — the tolerance therefore states its own unit and the comparison crosses units by construction. Ordering (`<`, `<=`, `>`, `>=`, `CompareTo`) and `GetHashCode` stay live, so a quantity still keys and sorts; the scalar-tolerance form `Equals(Length, double, ComparisonType)` is obsolete and `ComparisonType` survives only on `Comparison`. `Length.FeetInches` is the inverse of row [04]: it answers a `public sealed class FeetInches` carrying `Feet` truncated from the inch projection and `Inches` as the remainder, both `double`, so a customary split needs no local divmod and the fractional remainder is what an architectural denominator rounds.

Per-unit projection property names PLURALIZE the singular `<Quantity>Unit` enum row, and the plural falls on the leading noun of a compound unit, so a name derived mechanically from the enum misses on every compound. Every cross-quantity operator row above carries its commuted twin on the same declaring struct (decompile-verified for `Pressure*Area`, `Area*Length`, `Density*Volume`). `Angle` repeats the exemplar whole — `Angle.FromDegrees`, `Angle.ToUnit(AngleUnit)`, `AngleUnit.Degree`, the abbreviation render — so the plane-rotation family reads off this table without a second roster. Selectors below are the SI base-unit reads for the families whose fences project a canonical scalar.

[SI_SELECTORS]: `Density.KilogramsPerCubicMeter` `ThermalConductivity.WattsPerMeterKelvin` `SpecificEntropy.JoulesPerKilogramKelvin` `HeatTransferCoefficient.WattsPerSquareMeterKelvin` `LinearDensity.KilogramsPerMeter` `VolumePerLength.CubicMetersPerMeter` `MassFlow.GramsPerSecond`

[ENTRYPOINT_SCOPE]: typed and dynamic parsing

| [INDEX] | [SURFACE]                                                                                   | [SHAPE] | [CAPABILITY]                    |
| :-----: | :------------------------------------------------------------------------------------------ | :------ | :------------------------------ |
|  [01]   | `Length.Parse(string, IFormatProvider?)`                                                    | static  | typed parse                     |
|  [02]   | `Length.TryParse(string?, IFormatProvider?, out Length)`                                    | static  | guarded typed parse             |
|  [03]   | `Length.TryParseUnit(string, IFormatProvider?, out LengthUnit)`                             | static  | unit-token parse                |
|  [04]   | `Quantity.Parse(IFormatProvider?, Type, string)`                                            | static  | boxed parse                     |
|  [05]   | `Quantity.TryParse(IFormatProvider?, Type, string, out IQuantity?)`                         | static  | guarded culture-scoped parse    |
|  [06]   | `Quantity.From(QuantityValue, Enum)`                                                        | factory | enum-keyed construction         |
|  [07]   | `Quantity.From(QuantityValue, string, string)`                                              | factory | name-keyed construction         |
|  [08]   | `Quantity.FromUnitAbbreviation(IFormatProvider?, QuantityValue, string)`                    | factory | abbreviation construction       |
|  [09]   | `Quantity.TryFrom(QuantityValue, Enum?, out IQuantity?)`                                    | static  | guarded enum construction       |
|  [10]   | `Quantity.TryFromUnitAbbreviation(IFormatProvider?, QuantityValue, string, out IQuantity?)` | static  | guarded abbrev construction     |
|  [11]   | `UnitParser.Default.Parse<TUnit>(string, IFormatProvider?)`                                 | static  | unit-enum parse                 |
|  [12]   | `UnitParser.Default.TryParse<TUnit>(string?, IFormatProvider?, out TUnit)`                  | static  | guarded unit-enum parse         |
|  [13]   | `UnitParser.Default.TryParse(string?, Type, IFormatProvider?, out Enum?)`                   | static  | guarded runtime-type unit parse |

[ENTRYPOINT_SCOPE]: conversion, aggregation, metadata, and registration

| [INDEX] | [SURFACE]                                                                                   | [SHAPE]  | [CAPABILITY]                    |
| :-----: | :------------------------------------------------------------------------------------------ | :------- | :------------------------------ |
|  [01]   | `UnitConverter.Convert(QuantityValue, Enum, Enum) -> double`                                | static   | unboxed dynamic conversion      |
|  [02]   | `UnitConverter.TryConvert(QuantityValue, Enum, Enum, out double)`                           | static   | guarded dynamic conversion      |
|  [03]   | `UnitConverter.ConvertByName(QuantityValue, string, string, string) -> double`              | static   | name-keyed conversion           |
|  [04]   | `UnitConverter.TryConvertByName(QuantityValue, string, string, string, out double)`         | static   | guarded name-keyed conversion   |
|  [05]   | `UnitConverter.ConvertByAbbreviation(QuantityValue, string, string, string)`                | static   | abbreviation conversion         |
|  [06]   | `UnitConverter.TryConvertByAbbreviation(QuantityValue, string, string, string, out double)` | static   | guarded abbreviation conversion |
|  [07]   | `UnitConverter.Default.GetConversionFunction<TQuantity>(Enum, Enum)`                        | instance | cached conversion delegate      |
|  [08]   | `UnitConverter.TryGetConversionFunction<TQuantity>(Enum, Enum, out ConversionFunction)`     | instance | guarded cached delegate         |
|  [09]   | `UnitConverter.CreateDefault() -> UnitConverter`                                            | factory  | default converter root          |
|  [10]   | `UnitConverter.SetConversionFunction(Type, Enum, Type, Enum, ConversionFunction)`           | instance | conversion registration         |
|  [11]   | `UnitMath.Sum<TQuantity>(IEnumerable<TQuantity>, Enum)`                                     | fold     | chosen-unit sum                 |
|  [12]   | `UnitMath.Min<TQuantity>(TQuantity, TQuantity)`                                             | fold     | pairwise minimum                |
|  [13]   | `UnitMath.Max<TQuantity>(TQuantity, TQuantity)`                                             | fold     | pairwise maximum                |
|  [14]   | `UnitMath.Average<TQuantity>(IEnumerable<TQuantity>, Enum)`                                 | fold     | chosen-unit average             |
|  [15]   | `UnitMath.Clamp<TQuantity>(TQuantity, TQuantity, TQuantity)`                                | fold     | bounded quantity                |
|  [16]   | `UnitMath.Abs<TQuantity>(TQuantity)`                                                        | fold     | absolute value                  |
|  [17]   | `Quantity.Infos`                                                                            | static   | quantity metadata registry      |
|  [18]   | `Quantity.ByName`                                                                           | static   | name-keyed metadata registry    |
|  [19]   | `Quantity.Names`                                                                            | static   | quantity-name roster            |
|  [20]   | `Quantity.GetUnitInfo(Enum)`                                                                | static   | unit metadata lookup            |
|  [21]   | `Quantity.TryGetUnitInfo(Enum, out UnitInfo?)`                                              | static   | guarded unit metadata lookup    |
|  [22]   | `Quantity.GetQuantitiesWithBaseDimensions(BaseDimensions)`                                  | static   | dimension-based discovery       |
|  [23]   | `Quantity.AddUnitInfo(Enum, UnitInfo)`                                                      | static   | runtime unit registration       |
|  [24]   | `QuantityInfo.BaseDimensions`                                                               | property | physical signature              |
|  [25]   | `QuantityInfo.BaseUnitInfo`                                                                 | property | base-unit projection            |
|  [26]   | `QuantityInfo.UnitInfos`                                                                    | property | unit metadata projection        |
|  [27]   | `QuantityInfo.GetUnitInfoFor(BaseUnits)`                                                    | instance | policy unit lookup              |
|  [28]   | `QuantityInfo.GetUnitInfosFor(BaseUnits)`                                                   | instance | every policy-matching unit      |
|  [29]   | `UnitInfo.Value`                                                                            | property | unit `Enum` token               |
|  [30]   | `UnitInfo.Name`                                                                             | property | singular unit name              |
|  [31]   | `UnitInfo.PluralName`                                                                       | property | plural unit name                |
|  [32]   | `UnitInfo.QuantityName`                                                                     | property | owning quantity name            |
|  [33]   | `UnitInfo.BaseUnits`                                                                        | property | unit SI policy                  |
|  [34]   | `UnitAbbreviationsCache.GetAbbreviations(UnitInfo, IFormatProvider?)`                       | instance | unit alias set                  |
|  [35]   | `UnitAbbreviationsCache.GetDefaultAbbreviation<TUnit>(TUnit, IFormatProvider?)`             | instance | default abbreviation            |
|  [36]   | `UnitAbbreviationsCache.MapUnitToAbbreviation<TUnit>(TUnit, string[])`                      | instance | abbreviation registration       |
|  [37]   | `QuantityFormatter.Format<TUnit>(IQuantity<TUnit>, string?, IFormatProvider?)`              | static   | explicit quantity rendering     |
|  [38]   | `UnitSystem.SI`                                                                             | static   | SI policy                       |
|  [39]   | `new UnitSystem(BaseUnits)`                                                                 | ctor     | custom policy                   |
|  [40]   | `new UnitsNetSetup(ICollection<QuantityInfo>, UnitConverter)`                               | ctor     | configured service root         |
|  [41]   | `UnitsNetSetup.Default`                                                                     | static   | ambient service root            |
|  [42]   | `UnitsNetSetup.UnitConverter`                                                               | property | the root's converter instance   |
|  [43]   | `UnitsNetSetup.UnitAbbreviations`                                                           | property | the root's abbreviation cache   |
|  [44]   | `UnitsNetSetup.UnitParser`                                                                  | property | the root's unit parser          |
|  [45]   | `UnitsNetSetup.QuantityParser`                                                              | property | the root's quantity parser      |
|  [46]   | `QuantityInfo.ValueType`                                                                    | property | quantity struct `Type`          |
|  [47]   | `QuantityInfo.UnitType`                                                                     | property | unit-enum `Type`                |
|  [48]   | `QuantityInfo.Zero`                                                                         | property | family additive identity        |
|  [49]   | `Quantity.FromQuantityInfo(QuantityInfo, QuantityValue)`                                    | factory  | metadata-keyed construction     |
|  [50]   | `UnitAbbreviationsCache.Default`                                                            | static   | ambient abbreviation cache      |
|  [51]   | `UnitAbbreviationsCache.GetDefaultAbbreviation(Type, int, IFormatProvider?)`                | instance | erased default abbreviation     |
|  [52]   | `UnitAbbreviationsCache.CreateDefault()` / `.CreateEmpty()`                                 | factory  | a cache outside the setup root  |
|  [53]   | `UnitAbbreviationsCache.MapUnitToDefaultAbbreviation<TUnit>(TUnit, string)`                 | instance | set the PRIMARY abbreviation    |
|  [54]   | `UnitAbbreviationsCache.GetUnitAbbreviations<TUnit>(TUnit, IFormatProvider?)`               | instance | every alias for one unit        |
|  [55]   | `UnitAbbreviationsCache.GetAllUnitAbbreviationsForQuantity(Type, IFormatProvider?)`         | instance | every alias across a unit enum  |
|  [56]   | `Comparison.EqualsRelative(double, double, double)`                                         | static   | error relative to the reference |
|  [57]   | `Comparison.EqualsAbsolute(double, double, double)`                                         | static   | error as an absolute magnitude  |
|  [58]   | `Comparison.Equals(double, double, double, ComparisonType)`                                 | static   | mode-selected scalar tolerance  |

- Row [26] returns ONE `UnitInfo` and throws where the `BaseUnits` match is ambiguous or absent; row [27] returns the whole `IEnumerable<UnitInfo>` and is the read the boxed `ToUnit(UnitSystem)` runs internally. `UnitInfo<TUnit>` re-declares both as `UnitInfo<TUnit>`-typed, and re-declares `Value` as `TUnit`.
- Rows [42]-[45] are the setup root's own instances, and `UnitParser.Default`, `UnitAbbreviationsCache.Default`, and `UnitConverter.Default` are declared shortcuts FORWARDING to rows [44], [43], and [42] — one object under two spellings, so a surface composing the root reaches each instance there and a static facade beside it renames what it already holds.
- `UnitConverter.Convert`/`TryConvert`/`ConvertByName`/`ConvertByAbbreviation` (rows [01]-[06]) carry NO instance twin on the root's converter, so those stay static reads; `QuantityInfoLookup` is `internal` and reaches no consumer.
- Row [42] is the quantity STRUCT type (`ValueType` is assigned `zero.GetType()` at construction), so it is the argument rows [04], [05], and `Parse(Type, string)` take, while row [43] is the parallel `<Quantity>Unit` enum type; a boxed parse against a family therefore needs no per-family switch.
- [UNITMATH_CONSTRAINTS]: rows [11], [14], and [16] constrain `TQuantity : IQuantity` alone, while rows [12], [13], and [15] constrain `TQuantity : IComparable, IQuantity` — the boxed `IQuantity` face satisfies neither comparison constraint, so `Min`, `Max`, and `Clamp` are unreachable from an erased quantity and a bound over a runtime-selected family projects through `As(unit)`, clamps the scalar, and rebuilds through `Quantity.From(value, unit)`.
- [BASEUNITS_PARTIALITY]: rows [26], [27], [32], [37] resolve through full SEVEN-AXIS `BaseUnits` equality, and the metadata that walk needs is absent from most of the roster — `UnitInfo.BaseUnits` is `BaseUnits.Undefined` on the majority of unit rows, `MassUnit.Kilogram` and every `LinearDensityUnit`/`ThermalResistanceUnit` member included. Consequence: `GetUnitInfosFor(UnitSystem.SI.BaseUnits)` returns EMPTY and `IQuantity.ToUnit(UnitSystem.SI)` throws `ArgumentException("No units were found for the given UnitSystem")` for 69 of the 135 registry quantities (`Mass`, `Density`, `Torque`, `Frequency`, `HeatTransferCoefficient`, `ThermalConductivity`, `LinearDensity`, `ThermalResistance`, `VolumeFlow`, `ElectricResistance` among them), while `Force`, `Pressure`, and `Area` succeed. `BaseUnits.IsSubsetOf` is no remedy — it returns `false` for an `Undefined` receiver against a defined target by construction, and a subset walk resolves only 49 of 135. A canonical-SI resolution therefore ELECTS its target from `QuantityInfo.BaseUnitInfo` (SI-coherent for all but a named handful — `Angle`→`Degree`, `MassFlow`→`GramPerSecond`, `ThermalResistance`→`SquareMeterKelvinPerKilowatt` are the prefixed/convention exceptions) and never walks `BaseUnits`.
- Rows [52]-[55] are the abbreviation registry's own surface: `MapUnitToAbbreviation` APPENDS aliases while `MapUnitToDefaultAbbreviation` decides the one token `GetDefaultAbbreviation` answers with, and `CreateDefault`/`CreateEmpty` mint a cache a custom-unit registration mutates without touching `UnitsNetSetup.Default`. `GetAllUnitAbbreviationsForQuantity` keys on the unit ENUM type, so an intake parser builds its whole alias table from one call per family.
- Rows [56]-[58] are `Comparison`, the only live consumer of `ComparisonType` now that the quantity-level scalar-tolerance overload is obsolete: the quantity face compares through `Equals(TQuantity, TQuantity tolerance)`, and `Comparison` stays the rail for the raw scalars a projection already unwrapped.
- [BASEUNIT_COHERENCE]: `QuantityInfo.BaseUnitInfo` is the DECLARED base, not a guaranteed SI-coherent one. Ten quantities carry no coherent unit anywhere in their roster: the logarithmic `Level`/`AmplitudeRatio`/`PowerRatio`, the volt-ampere-hour `ApparentEnergy`/`ElectricApparentEnergy`/`ReactiveEnergy`/`ElectricReactiveEnergy`, `RelativeHumidity` (`Percent` alone), `FuelEfficiency` (`LiterPer100Kilometers`), and `SpecificFuelConsumption` (`GramPerKiloNewtonSecond`). `UnitInfo.Name` IS `Value.ToString()` for every row, so a name-keyed index and an enum-keyed one join exactly — and it is therefore NOT a display token: a readout resolving a unit label through row [30] prints `Millimeter` where a reader expects `mm`. Row [35] constrains `TUnit : Enum` and cannot bind a value already erased to the `Enum` face, so an elected unit carried as `Enum` reaches its abbreviation through row [47], which is exactly what row [35]'s own body forwards to; a `null` provider on either sends the lookup to `CurrentCulture`, so a deterministic surface passes its own resolved culture.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every quantity is a `readonly struct` over generic-math static-abstract interfaces; native operators preserve quantity identity across same-quantity, scalar, ratio, and cross-quantity algebra, and cross-quantity operators yield typed results at compile time. `QuantityValue` holds an admitted scalar as `double` or `decimal` until construction, `IValueQuantity<TValue>` fixes each family's backing precision, and primitive projection stays a boundary cast.
- `QuantityInfo.Name` is the quantity-type discriminator and `BaseDimensions` is the compatibility predicate, so two same-dimension families stay distinct while `Quantity.GetQuantitiesWithBaseDimensions` discovers derived quantities by signature.
- `Quantity.From(value, quantityName, unitName)` admits a registry-named quantity; a quantity outside the registry composes its `BaseDimensions` and registers through `UnitsNetSetup`, `Quantity.AddUnitInfo`, `UnitConverter.SetConversionFunction`, and `UnitAbbreviationsCache.MapUnitToAbbreviation` rather than a local discriminant.
- `UnitSystem` projects policy through the units `BaseUnits` selects, so it reaches only the quantities whose unit rows declare that metadata — see `[BASEUNITS_PARTIALITY]`; a receipt spanning the whole registry elects per-quantity targets instead of declaring one unit system.

[STACKING]:
- `Rasm.Compute/Symbolic/units` composes the metadata rail: `QuantityInfo.BaseUnitInfo`/`UnitInfos` source each `QuantityFamily` row's canonical and convert-target units, cross-quantity operators derive compound families, `UnitMath` folds a same-family sequence at a chosen unit, and `UnitConverter.TryConvert` guards numeric-only conversion on the typed-error rail.
- `System.Text.Json` / Thinktecture-JSON (`api-thinktecture-json.md`): a measured receipt persists as the `IQuantity.Value` scalar and `IQuantity.Unit` `Enum` token and rehydrates through `Quantity.From(value, unitEnum)`, the dynamic façade owning the decode.
- `Rasm.AppUi/.api/api-reactiveui-avalonia.md`: resolves view-model display through `UnitAbbreviationsCache.GetAbbreviations` and the culture-scoped `Length.ToString`/`Length.TryParse` pair, an `IValueConverter` wrapping `As` and `Parse` binding the XAML seam; `Rasm.AppUi` composes the typed measure surface at `Render/drafting#DIMENSIONING` (`DraftUnits` over `Length`/`Angle` with the `IQuantity`-narrowed locale edge), `Render/reality` (`UnitsNet.Length`/`Seq<UnitsNet.Angle>` receipt evidence), and `Editing/inspector` (the rank-10 `IQuantity` editor gate); `Theme/locale#MEASUREMENT_FORMAT` owns the display edge whole — a `MeasureRole` row names both its metric and its imperial unit token outright and `MeasurePolicy` folds `ToUnit`, `UnitAbbreviationsCache.GetDefaultAbbreviation(Type, int, IFormatProvider?)`, and `Length.FeetInches` into one render, so every dimensioned readout in the package elects through that owner rather than a per-site cache read. `Irradiance`, `Illuminance`, and `RelativeHumidity` ship NO imperial unit member at all, so their readout rows carry the SI token in both postures and `Irradiation` is the one analysis family with a real pair.
- `api-nodatime.md`: `Duration.ToTimeSpan()` and the `explicit operator TimeSpan(Duration)`, `DateTime + Duration`, and `Duration`↔`TimeSpan` comparison operators meet BCL time; the unit-bearing `UnitsNet.Duration` carries measured physical seconds on receipts while a NodaTime `Duration` carries a wall-clock span.
- `Rasm.Compute` admission rail: the erased `IQuantity` face admits every family through one polymorphic entrypoint returning `Fin<UnitEvidence>`, `QuantityFamily` is a `[SmartEnum<string>]` under `StringComparer.OrdinalIgnoreCase` key policy reading `Info.BaseUnitInfo.Value` once at static construction, `DimensionMonomial` is a `[ValueObject]` over the Q⁷ `Seq<ERational>` exponent vector lifting the seven `BaseDimensions` `int` exponents so a symbolic `Powf` arm carries the non-integer exponent UnitsNet cannot, the dimensional proof accumulates every compound mismatch through `Validation<Error, DimensionMonomial>` with `BaseDimensions.Equals` as the leaf predicate, and the AngouriMath bridge matches the proven monomial against `QuantityFamily.Items` (`sqrt` lowering to `Powf(arg, 1/2)`); no UnitsNet type crosses a JSON or proto wire — `UnitEvidence` projects to plain `string`/`double` fields.
- `Rasm.Bim`: `Rasm.Element/Properties/quantity#MEASURE_VALUE` `MeasureValue` (a `sealed record` whose private constructor is reachable only through the `Fin`-returning `Of(double, Enum|string, Op)` factories; its stored `Type`/`Dimension`/`Si`/`CanonicalUnit` is evidence the UnitsNet resolution already derived rather than raw admitted fields, so a `[ComplexValueObject]` graduation would re-validate derived evidence and is refused) and the `PropertyValue.Measure` `[Union]` arm own shape while UnitsNet owns dimension — the persisted scalar is always `ToUnit(UnitSystem.SI)`-coerced before entering the carrier; IFC ingest declares a per-axis BASE-UNIT policy (`IfcUnitAssignment.ScaleSI` per `IfcUnitEnum`), not a per-quantity unit enum, so Bim coerces through its own `UnitScale.Coerce`/`Declare` exact-inverse pair over the federated `Dimension` exponents and admits through `MeasureValue.OfSi` — the `UnitParser`/`Quantity.From` abbreviation path has no Bim call site, and the `[BASEUNITS_PARTIALITY]` registry gap (69 of 135 quantities throw) is why no UnitsNet member can answer the per-axis fold; `Semantics/properties#BASE_QUANTITIES` `QuantityDerivation.Derive` admits through the three-argument `OfSi(QuantityType, Dimension, double)` carrying its QTO identity, with the seam `Multiply`/`WithType` algebra closing each derivation without leaving the dimensioned carrier.
- `Rasm.Fabrication`: each `PhysicsQuantity` row binds its quantity's `TryParse` delegate under `CultureInfo.InvariantCulture` and lowers `false` through one `Fin<double>` admission rail — `Feed`, `Spindle`, `Length`, `Pressure`, `Power`, and `Temperature` binding `Speed`, `RotationalSpeed`, `Length`, `Pressure`, `Power`, and `Temperature` to canonical machining units, `Duration.TryParse` with `Duration.Seconds` owning textual dwell, and `PhysicsAdmission.Quantity` carrying only the resulting canonical `double`; `Power.FromWatts * Duration.FromSeconds -> Energy` composes typed work, and `Mass.FromKilograms`/`Volume.Liters` carry the sustainability evidence scalars `FabricationFact.QualitySeal.Of` folds; the derivation sites compose the typed products in place of literal scale chains — `Pressure * Area -> Force` at the workholding clamp folds, `Area * Length -> Volume` then `Volume * Density -> Mass` at the estimation and audit mass products, `Mass.FromGrams(...).Kilograms` at the magazine slot bound. The thermal-diffusivity quotient (L2T-1) has NO typed route — `ThermalConductivity` exposes scalar operators only, `SpecificEntropy` only `* Mass -> Entropy`, and `KinematicViscosity` shares the dimension under a different physical concern — so `Process/physics` keeps its bare SI quotient with the derived unit ratio stated at the site.
- `Rasm.Materials`: `MaterialUnits` is the one in-folder boundary — `MaterialUnits.Admit(Illuminance.Info, value, unit, …)` gates membership through `q.QuantityInfo.BaseDimensions.Equals(Info.BaseDimensions)`, `UnitConverter.TryConvert` rescales to the family `BaseUnit`, and the boundary returns `Fin<UnitEvidence>` carrying `evidence.CanonicalValue`, the 683 lm/W luminous↔radiometric divide staying the author-kernel's outside UnitsNet conversion; `MaterialUnits.Coerce` targets the thermal `BaseUnit` set with layered-assembly resistance folding through `UnitMath.Sum<T>`, `interchange#MATERIAL_WIRE` carries the SI-base scalar with its unit `Enum` token the TS and Python peers decode, and IFC abbreviations resolve through `UnitParser.Default.Parse(abbr, CultureInfo.InvariantCulture)`.

[LOCAL_ADMISSION]:
- Compute inputs and receipts carry explicit quantity structs wherever units affect meaning, the type binding unit identity to the scalar.
- Unit conversion, culture-aware parse, and format cross through `IFormatProvider` at the boundary rail.
- Collection aggregation folds through `UnitMath` in the selected unit without collapsing quantity type.
- `Quantity` and `IQuantity` are the reflection seam for a runtime-selected family driving diagnostics, support output, and receipt projection.
- Boundary parse, format, and abbreviation lookup pass `CultureInfo.InvariantCulture` explicitly — `UnitAbbreviationsCache` accessors default to `CurrentCulture` on a `null` provider, the internal invariant `FallbackCulture` only the per-unit secondary degrade — so admission is deterministic across ambient culture and loaded satellites.
- Conversion runs exactly once at admission and interior numerics are raw `double`; abbreviation resolution rides `UnitParser.Default.TryParse(string, Type, IFormatProvider, out Enum)`, never a probe-string parse, and `UnitsNetSetup.Default` is the single setup root composed once.
- `SpecificEntropy` carries specific-heat-capacity values at its `JoulePerKilogramKelvin` base, and an affine family (`Temperature`) aggregates at the canonical absolute scale, never across display offsets.

[RAIL_LAW]:
- Package: `UnitsNet`
- Owns: typed quantity algebra, registry identity, boundary conversion, unit-system policy, and typed aggregation.
- Accept: unit-aware inputs and receipts; `QuantityInfo.Name` identity with `BaseDimensions` validation; `UnitConverter` boundary conversion; `UnitMath` aggregation; `IQuantity.Value`/`IQuantity.Unit` wire projection.
- Reject: a quantity `==`/`!=` or single-argument `Equals` comparison where the two-quantity tolerance form is the live one, a hand-rolled unit-conversion table where `UnitConverter` owns the rescale, a per-quantity conversion helper where the struct's `ToUnit`/`As` owns it, a raw `double` carrying its unit in a comment where the quantity struct binds it, a quantity type crossing an interior signature or a wire, an AEC-domain reach down to the Compute units owner — each stratum owns its own unit boundary — and a `GenericMathExtensions`/`DecimalGenericMathExtensions` call (neither exists), `UnitFormatter` (internal; `QuantityFormatter` is the public formatter), or `IDecimalQuantity` (`IValueQuantity<decimal>` is the live face).
