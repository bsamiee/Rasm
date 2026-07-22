# [RASM_API_UNITSNET]

`UnitsNet` owns strongly typed physical quantities over the `double`-or-`decimal` `QuantityValue` scalar, binding native operator algebra, boundary conversion, registry identity, and unit-system policy across measured inputs and receipts. Each generated quantity struct fixes one physical concern to its `QuantityInfo.Name` and SI base unit, and feeds the units rail every Compute and materials boundary canonicalizes through.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `UnitsNet`
- package: `UnitsNet` (MIT-0, © Andreas Gullberg Larsen)
- assembly: `UnitsNet`
- namespace: `UnitsNet`, `UnitsNet.Units`, `UnitsNet.GenericMath`
- asset: managed runtime library with localized abbreviation satellite assemblies
- rail: units

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: quantity contracts and scalar carriers

`IQuantity` boxes projection and conversion; its generic forms progressively bind unit, backing scalar, and self type.

| [INDEX] | [SYMBOL]                                    | [TYPE_FAMILY] | [CAPABILITY]                      |
| :-----: | :------------------------------------------ | :------------ | :-------------------------------- |
|  [01]   | `IQuantity`                                 | interface     | boxed projection and conversion   |
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
|  [25]   | `RotationalSpeed`         | `RadianPerSecond`              | angular velocity          |
|  [26]   | `Level`                   | `Decibel`                      | logarithmic level         |
|  [27]   | `Illuminance`             | `Lux`                          | incident luminous flux    |
|  [28]   | `Irradiance`              | `WattPerSquareMeter`           | radiant flux density      |
|  [29]   | `Luminance`               | `CandelaPerSquareMeter`        | directional surface light |
|  [30]   | `LuminousFlux`            | `Lumen`                        | perceived light power     |
|  [31]   | `LuminousIntensity`       | `Candela`                      | directional luminous flux |

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
|  [13]   | `ComparisonType`              | enum          | tolerance comparison mode       |
|  [14]   | `QuantityTypeConverter`       | class         | attribute-driven type converter |
|  [15]   | `DefaultUnitAttribute`        | class         | default-unit declaration        |
|  [16]   | `ConvertToUnitAttribute`      | class         | conversion-unit declaration     |
|  [17]   | `DisplayAsUnitAttribute`      | class         | display-unit declaration        |
|  [18]   | `UnitsNetException`           | class         | package failure root            |
|  [19]   | `UnitNotFoundException`       | class         | unresolved unit failure         |
|  [20]   | `AmbiguousUnitParseException` | class         | ambiguous-abbreviation failure  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: boxed quantity projection

| [INDEX] | [SURFACE]                                          | [SHAPE]  | [CAPABILITY]              |
| :-----: | :------------------------------------------------- | :------- | :------------------------ |
|  [01]   | `IQuantity.Dimensions`                             | property | physical signature        |
|  [02]   | `IQuantity.QuantityInfo`                           | property | family metadata           |
|  [03]   | `IQuantity.Unit`                                   | property | constructed unit          |
|  [04]   | `IQuantity.Value`                                  | property | constructed scalar        |
|  [05]   | `IQuantity.As(Enum)`                               | instance | scalar unit projection    |
|  [06]   | `IQuantity.As(UnitSystem)`                         | instance | scalar policy projection  |
|  [07]   | `IQuantity.Equals(IQuantity?, IQuantity)`          | instance | tolerance equality        |
|  [08]   | `IQuantity.ToUnit(Enum)`                           | instance | boxed unit reprojection   |
|  [09]   | `IQuantity.ToUnit(UnitSystem)`                     | instance | boxed policy reprojection |
|  [10]   | `IQuantity.ToString(IFormatProvider?)`             | instance | culture-aware rendering   |
|  [11]   | `QuantityValue.Type`                               | property | scalar storage kind       |
|  [12]   | `QuantityValue.IsDecimal`                          | property | decimal-kind test         |
|  [13]   | `QuantityValue.Zero`                               | static   | scalar identity           |
|  [14]   | `implicit operator QuantityValue(<numeric-value>)` | operator | numeric admission         |
|  [15]   | `explicit operator double(QuantityValue)`          | operator | double projection         |
|  [16]   | `explicit operator decimal(QuantityValue)`         | operator | decimal projection        |

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
|  [25]   | `operator -(Temperature, Temperature) -> TemperatureDelta` | operator | affine difference          |
|  [26]   | `operator +(Temperature, TemperatureDelta) -> Temperature` | operator | affine offset              |
|  [27]   | `operator -(Temperature, TemperatureDelta) -> Temperature` | operator | affine offset              |
|  [28]   | `operator <(Length, Length)`                               | operator | ordered comparison         |
|  [29]   | `operator ==(Length, Length)`                              | operator | strict equality            |
|  [30]   | `Length.CompareTo(Length)`                                 | instance | quantity ordering          |
|  [31]   | `Length.Equals(Length, double, ComparisonType)`            | instance | tolerance equality         |
|  [32]   | `Length.ToString(string?, IFormatProvider?)`               | instance | culture-aware rendering    |
|  [33]   | `Length.Zero`                                              | static   | additive identity          |
|  [34]   | `Length.BaseUnit`                                          | static   | base-unit metadata         |
|  [35]   | `Length.Units`                                             | static   | unit vocabulary            |
|  [36]   | `Length.Info`                                              | static   | quantity metadata          |
|  [37]   | `Length.BaseDimensions`                                    | static   | dimension metadata         |

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
|  [08]   | `UnitConverter.CreateDefault() -> UnitConverter`                                            | factory  | default converter root          |
|  [09]   | `UnitConverter.SetConversionFunction(Type, Enum, Type, Enum, ConversionFunction)`           | instance | conversion registration         |
|  [10]   | `UnitMath.Sum<TQuantity>(IEnumerable<TQuantity>, Enum)`                                     | fold     | chosen-unit sum                 |
|  [11]   | `UnitMath.Min<TQuantity>(IEnumerable<TQuantity>, Enum)`                                     | fold     | chosen-unit minimum             |
|  [12]   | `UnitMath.Max<TQuantity>(IEnumerable<TQuantity>, Enum)`                                     | fold     | chosen-unit maximum             |
|  [13]   | `UnitMath.Average<TQuantity>(IEnumerable<TQuantity>, Enum)`                                 | fold     | chosen-unit average             |
|  [14]   | `UnitMath.Clamp<TQuantity>(TQuantity, TQuantity, TQuantity)`                                | fold     | bounded quantity                |
|  [15]   | `UnitMath.Abs<TQuantity>(TQuantity)`                                                        | fold     | absolute value                  |
|  [16]   | `Quantity.Infos`                                                                            | static   | quantity metadata registry      |
|  [17]   | `Quantity.ByName`                                                                           | static   | name-keyed metadata registry    |
|  [18]   | `Quantity.Names`                                                                            | static   | quantity-name roster            |
|  [19]   | `Quantity.GetUnitInfo(Enum)`                                                                | static   | unit metadata lookup            |
|  [20]   | `Quantity.TryGetUnitInfo(Enum, out UnitInfo?)`                                              | static   | guarded unit metadata lookup    |
|  [21]   | `Quantity.GetQuantitiesWithBaseDimensions(BaseDimensions)`                                  | static   | dimension-based discovery       |
|  [22]   | `Quantity.AddUnitInfo(Enum, UnitInfo)`                                                      | static   | runtime unit registration       |
|  [23]   | `QuantityInfo.BaseUnitInfo`                                                                 | property | base-unit projection            |
|  [24]   | `QuantityInfo.UnitInfos`                                                                    | property | unit metadata projection        |
|  [25]   | `QuantityInfo.GetUnitInfoFor(BaseUnits)`                                                    | instance | policy unit lookup              |
|  [26]   | `UnitInfo.Name`                                                                             | property | singular unit name              |
|  [27]   | `UnitInfo.PluralName`                                                                       | property | plural unit name                |
|  [28]   | `UnitInfo.QuantityName`                                                                     | property | owning quantity name            |
|  [29]   | `UnitInfo.BaseUnits`                                                                        | property | unit SI policy                  |
|  [30]   | `UnitAbbreviationsCache.GetAbbreviations(UnitInfo, IFormatProvider?)`                       | instance | unit alias set                  |
|  [31]   | `UnitAbbreviationsCache.GetDefaultAbbreviation<TUnit>(TUnit, IFormatProvider?)`             | instance | default abbreviation            |
|  [32]   | `UnitAbbreviationsCache.MapUnitToAbbreviation<TUnit>(TUnit, string[])`                      | instance | abbreviation registration       |
|  [33]   | `QuantityFormatter.Format<TUnit>(IQuantity<TUnit>, string?, IFormatProvider?)`              | static   | explicit quantity rendering     |
|  [34]   | `UnitSystem.SI`                                                                             | static   | SI policy                       |
|  [35]   | `new UnitSystem(BaseUnits)`                                                                 | ctor     | custom policy                   |
|  [36]   | `new UnitsNetSetup(ICollection<QuantityInfo>, UnitConverter)`                               | ctor     | configured service root         |
|  [37]   | `UnitsNetSetup.Default`                                                                     | static   | ambient service root            |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every quantity is a `readonly struct` over generic-math static-abstract interfaces; native operators preserve quantity identity across same-quantity, scalar, ratio, and cross-quantity algebra, and cross-quantity operators yield typed results at compile time. `QuantityValue` holds an admitted scalar as `double` or `decimal` until construction, `IValueQuantity<TValue>` fixes each family's backing precision, and primitive projection stays a boundary cast.
- `QuantityInfo.Name` is the quantity-type discriminator and `BaseDimensions` is the compatibility predicate, so two same-dimension families stay distinct while `Quantity.GetQuantitiesWithBaseDimensions` discovers derived quantities by signature.
- `Quantity.From(value, quantityName, unitName)` admits a registry-named quantity; a quantity outside the registry composes its `BaseDimensions` and registers through `UnitsNetSetup`, `Quantity.AddUnitInfo`, `UnitConverter.SetConversionFunction`, and `UnitAbbreviationsCache.MapUnitToAbbreviation` rather than a local discriminant.
- `UnitSystem` projects policy through the units `BaseUnits` selects, so a receipt declares one unit system for every quantity.

[STACKING]:
- `Rasm.Compute/Symbolic/units` composes the metadata rail: `QuantityInfo.BaseUnitInfo`/`UnitInfos` source each `QuantityFamily` row's canonical and convert-target units, cross-quantity operators derive compound families, `UnitMath` folds a same-family sequence at a chosen unit, and `UnitConverter.TryConvert` guards numeric-only conversion on the typed-error rail.
- `System.Text.Json` / Thinktecture-JSON (`api-thinktecture-json.md`): a measured receipt persists as the `IQuantity.Value` scalar and `IQuantity.Unit` `Enum` token and rehydrates through `Quantity.From(value, unitEnum)`, the dynamic façade owning the decode.
- `Rasm.AppUi/.api/api-reactiveui-avalonia.md`: a view-model resolves display through `UnitAbbreviationsCache.GetAbbreviations` and the culture-scoped `Length.ToString`/`Length.TryParse` pair, an `IValueConverter` wrapping `As` and `Parse` binding the XAML seam.
- `api-nodatime.md`: `Duration.ToTimeSpan()` and the `explicit operator TimeSpan(Duration)`, `DateTime + Duration`, and `Duration`↔`TimeSpan` comparison operators meet BCL time; the unit-bearing `UnitsNet.Duration` carries measured physical seconds on receipts while a NodaTime `Duration` carries a wall-clock span.

[LOCAL_ADMISSION]:
- Compute inputs and receipts carry explicit quantity structs wherever units affect meaning, the type binding unit identity to the scalar.
- Unit conversion, culture-aware parse, and format cross through `IFormatProvider` at the boundary rail.
- Collection aggregation folds through `UnitMath` in the selected unit without collapsing quantity type.
- `Quantity` and `IQuantity` are the reflection seam for a runtime-selected family driving diagnostics, support output, and receipt projection.

[RAIL_LAW]:
- Package: `UnitsNet`
- Owns: typed quantity algebra, registry identity, boundary conversion, unit-system policy, and typed aggregation.
- Accept: unit-aware inputs and receipts; `QuantityInfo.Name` identity with `BaseDimensions` validation; `UnitConverter` boundary conversion; `UnitMath` aggregation; `IQuantity.Value`/`IQuantity.Unit` wire projection.
- Reject: a hand-rolled unit-conversion table where `UnitConverter` owns the rescale, a per-quantity conversion helper where the struct's `ToUnit`/`As` owns it, and a raw `double` carrying its unit in a comment where the quantity struct binds it.
