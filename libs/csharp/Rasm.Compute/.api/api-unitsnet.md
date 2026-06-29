# [RASM_COMPUTE_API_UNITSNET]

`UnitsNet` supplies source-generated quantity structs, unit enums, a value
carrier that unifies `double` and `decimal`, culture-scoped parse/format,
generic-math operator quantities, the SI dimension vector with its own algebra,
and a single mutable setup root for conversion/abbreviation policy. It is the
measured-execution boundary owner: every unit-bearing Compute input
canonicalizes through it exactly once, and only raw SI scalars cross interior
signatures or the wire.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `UnitsNet`
- package: `UnitsNet` 5.75.0
- license: MIT-0 (no-attribution; safe to vendor concepts without notice obligations)
- assembly: `UnitsNet`
- namespaces: `UnitsNet` (quantities, statics, metadata), `UnitsNet.Units` (unit enums)
- target: multi-targets `net8.0` / `net9.0` / `netstandard2.0`; the `net10.0` workspace consumer binds `lib/net9.0/UnitsNet.dll` (highest available TFM precedence) — verify members against the `net9.0` asset, never the `netstandard2.0` fallback whose generic-math operator interfaces (`IArithmeticQuantity`, `static abstract Zero`) are absent
- runtime: managed-only, no native dependency; ships satellite `*.resources.dll` for `fr-CA`/`nb-NO`/`ru-RU`/`zh-CN` localized abbreviations
- asset: runtime library
- rail: units

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: quantity contracts (`UnitsNet`)
- rail: units
- note: `IQuantity` is the non-generic erased face every struct exposes; the generic faces tighten the unit enum and value type. The value-type face is covariant (`out TValueType`), so an `IValueQuantity<double>` flows where `IValueQuantity` is wanted. The Compute `Admit` rail consumes the erased `IQuantity` (`QuantityInfo`/`Dimensions`/`Unit`/`As`) so one polymorphic entrypoint admits every family without a per-family signature.

| [INDEX] | [SYMBOL]                                       | [SHAPE / KEY MEMBERS]                                                                                                                                              | [CONSUMER / BOUNDARY NOTE]                                                                                          |
| :-----: | :--------------------------------------------- | :---------------------------------------------------------------------------------------------------------------------------------------------------------------- | :----------------------------------------------------------------------------------------------------------------- |
|  [01]   | `IQuantity`                                    | `QuantityValue Value`; `Enum Unit`; `QuantityInfo QuantityInfo`; `BaseDimensions Dimensions`; `double As(Enum)`; `double As(UnitSystem)`; `IQuantity ToUnit(Enum)`; `IQuantity ToUnit(UnitSystem)`; `bool Equals(IQuantity?, IQuantity tolerance)`; `string ToString(IFormatProvider?)` | erased admission face — `QuantityFamily.Admit(IQuantity, …)` reads `QuantityInfo.Name`/`Dimensions`/`Unit`/`As(canonical)` to mint `UnitEvidence` without a typed cast |
|  [02]   | `IQuantity<TUnitType>`                          | `IQuantity` + `new TUnitType Unit`; `QuantityInfo<TUnitType> QuantityInfo`; `IQuantity<TUnitType> ToUnit(TUnitType)`; `double As(TUnitType)` (`where TUnitType : Enum`) | single-param enum-typed face; the input to `QuantityFormatter.Format<TUnitType>`                                    |
|  [03]   | `IQuantity<TUnitType, out TValueType>`         | `IQuantity<TUnitType>` + `IValueQuantity<TValueType>` + `new TValueType As(TUnitType)` (`TUnitType : Enum`, `TValueType : INumber<TValueType>`)                     | full two-param face that avoids `Enum` casts AND value boxing                                                       |
|  [04]   | `IValueQuantity<out TValueType>`               | `IQuantity` + `new TValueType Value` (`where TValueType : INumber<TValueType>`)                                                                                     | covariant value face; `IValueQuantity<double>` for every Rasm-admitted quantity, `IValueQuantity<decimal>` for the few decimal-backed families |
|  [05]   | `IArithmeticQuantity<TSelf, TUnitType, TValueType>` | `IQuantity<TSelf,TUnitType,TValueType>` + `IComparisonOperators` + `IEqualityOperators` + `IParsable<TSelf>` + `IAdditionOperators`/`ISubtractionOperators`/`IMultiplyOperators<TSelf,TValueType,TSelf>`/`IDivisionOperators<TSelf,TValueType,TSelf>`/`IUnaryNegationOperators` + `IAdditiveIdentity` + `static abstract TSelf Zero` (`TSelf : IArithmeticQuantity<…>`, `TUnitType : Enum`, `TValueType : struct, INumber<TValueType>`) | the generic-math constraint surface: a `where T : IArithmeticQuantity<T,U,V>` generic accepts `+`/`-`/`* scalar`/`/ scalar`/`Zero` over any quantity — the boundary aggregation contract (NOT a `GenericMathExtensions` type, which does not exist in 5.75) |
|  [06]   | `IDecimalQuantity`                             | `[Obsolete("Use the IValueQuantity<decimal> interface.")]` : `IValueQuantity<decimal>`                                                                             | DO NOT consume — superseded; `QuantityValue` now carries `decimal` natively, so the decimal face is `IValueQuantity<decimal>` |
|  [07]   | `QuantityValue`                                | `readonly struct` : `IFormattable`/`IEquatable`/`IComparable`; `enum UnderlyingDataType : byte`; `readonly UnderlyingDataType Type`; `bool IsDecimal`; `static QuantityValue Zero`; implicit from `byte`/`short`/`int`/`long`/`float`/`double`/`decimal`; explicit to `double`/`decimal`; full comparison + unary-negate operators; `ToString(string?, IFormatProvider?)` | the single value carrier unifying `double` and `decimal` with no precision loss; every `From(QuantityValue, unit)` and `UnitConverter.Convert(QuantityValue, …)` takes it — pass a bare `double`/`decimal` and the implicit operator lifts it, so the wire stays scalar |

[PUBLIC_TYPE_SCOPE]: metadata, dimensions, base units (`UnitsNet`)
- rail: units
- note: this tier is the metadata-at-construction source the `QuantityFamily` `[SmartEnum]` reads ONCE per row instead of hand-passing canonical enums. Every `<Quantity>.Info` static is a `QuantityInfo<TUnit>` (the generic subtype); the erased `QuantityInfo` base is what the smart-enum column stores.

| [INDEX] | [SYMBOL]                  | [SHAPE / KEY MEMBERS]                                                                                                                                                                                                            | [CONSUMER / BOUNDARY NOTE]                                                                                                              |
| :-----: | :------------------------ | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :-------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `QuantityInfo`            | `string Name`; `UnitInfo[] UnitInfos`; `UnitInfo BaseUnitInfo`; `IQuantity Zero`; `Type UnitType`; `Type ValueType`; `BaseDimensions BaseDimensions`; `UnitInfo GetUnitInfoFor(BaseUnits)`; `IEnumerable<UnitInfo> GetUnitInfosFor(BaseUnits)` | the row's metadata column: `Canonical = Info.BaseUnitInfo.Value`, `Catalogue() = Items.Map(r => r.Info)`, `Quantity.TryParse(culture, Info.ValueType, …)`, dimensional row `Info.BaseDimensions`, picker targets `Info.UnitInfos` |
|  [02]   | `QuantityInfo<TUnit>`     | `QuantityInfo` + `new UnitInfo<TUnit>[] UnitInfos`; `new UnitInfo<TUnit> BaseUnitInfo`; `new IQuantity<TUnit> Zero`; `new TUnit UnitType` (`where TUnit : Enum`)                                                                     | what `UnitsNet.Length.Info` returns; the strongly-typed `BaseUnitInfo.Value` is a `LengthUnit`, not a boxed `Enum`                       |
|  [03]   | `UnitInfo`                | `Enum Value`; `string Name`; `string PluralName`; `BaseUnits BaseUnits`; `string? QuantityName`                                                                                                                                    | the conversion-target row for the dashboard picker: `UnitMetadata.ConvertTargets(info) => info.UnitInfos.Map(u => u.Value)`              |
|  [04]   | `UnitInfo<TUnit>`         | `UnitInfo` + `new TUnit Value` (`where TUnit : Enum`)                                                                                                                                                                              | typed unit-info element of `QuantityInfo<TUnit>.UnitInfos`                                                                               |
|  [05]   | `BaseDimensions`          | `sealed`; `int Length`/`Mass`/`Time`/`Current`/`Temperature`/`Amount`/`LuminousIntensity` (seven SI exponents); `static BaseDimensions Dimensionless`; `bool IsBaseQuantity()`/`IsDerivedQuantity()`/`IsDimensionless()`; `BaseDimensions Multiply(BaseDimensions)`/`Divide(BaseDimensions)`; `operator *`/`/`/`==`/`!=`; structural `Equals`/`GetHashCode` | the SI proof vocabulary: `UnitAlgebra.Relations` composes `Left.Info.BaseDimensions` via `Multiply`/`Divide` and asserts `Equals(Compound.Info.BaseDimensions)`; `DimensionMonomial.From` lifts the seven `int` exponents to exact `BigRational` so the symbolic `Power` arm carries a half-power root the `int`-exponent `BaseDimensions` cannot |
|  [06]   | `BaseUnits`               | `sealed` : `IEquatable`; nullable `LengthUnit?`/`MassUnit?`/`DurationUnit? Time`/`ElectricCurrentUnit? Current`/`TemperatureUnit?`/`AmountOfSubstanceUnit? Amount`/`LuminousIntensityUnit?`; `bool IsFullyDefined`; `static BaseUnits Undefined`; `bool IsSubsetOf(BaseUnits)`; `operator ==`/`!=` | the base-unit set of a `UnitSystem`; `QuantityInfo.GetUnitInfoFor(BaseUnits)` resolves a family's unit under a custom base-unit policy   |

[PUBLIC_TYPE_SCOPE]: admitted quantity families (`UnitsNet`)
- rail: units
- note: each is a source-generated `readonly partial struct` implementing `IArithmeticQuantity<Self, <Self>Unit, double>` (so it is comparable, parsable, generic-math-capable, and `IConvertible`). Shape is uniform across families: `static QuantityInfo<TUnit> Info`; `static Self Zero`; `Self(double, TUnit)` and `Self(double, UnitSystem)` ctors; `static Self From(QuantityValue, TUnit)` plus one `static Self From<UnitName>(QuantityValue)` per unit; `<UnitName>` read-only accessors; `double As(TUnit)`/`As(UnitSystem)`; `Self ToUnit(TUnit)`/`ToUnit(TUnit, UnitConverter)`/`ToUnit(UnitSystem)`; `static Self Parse(string, IFormatProvider?)`/`TryParse(…)`. Cross-quantity operators encode dimensional algebra at the type level (see `[03]-[ENTRYPOINTS]`).

| [INDEX] | [SYMBOL]       | [CANONICAL (BaseUnitInfo.Value)] | [DISPLAY DEFAULT FOR RASM]   | [CONSUMER NOTE]                                       |
| :-----: | :------------- | :------------------------------- | :--------------------------- | :--------------------------------------------------- |
|  [01]   | `Length`       | `Meter`                          | `Millimeter`                 | `Length * Length -> Area`, `Length / Duration -> Speed`, `Length * Force -> Torque` |
|  [02]   | `Area`         | `SquareMeter`                    | `SquareMeter`                | `Area * Length -> Volume`, `Force / Area -> Pressure` |
|  [03]   | `Volume`       | `CubicMeter`                     | `CubicMeter`                 | `Volume / Area -> Length`                            |
|  [04]   | `Mass`         | `Kilogram`                       | `Kilogram`                   | `Mass / Volume -> Density`, `Mass * Acceleration -> Force` |
|  [05]   | `Duration`     | `Second`                         | `Second`                     | NodaTime owns interior time; row canonicalizes boundary text to seconds only |
|  [06]   | `Speed`        | `MeterPerSecond`                 | `MeterPerSecond`             | feed/surface-speed cut-parameter ingress             |
|  [07]   | `Acceleration` | `MeterPerSecondSquared`          | `MeterPerSecondSquared`      | `Speed / Duration`                                   |
|  [08]   | `Force`        | `Newton`                         | `Newton`                     | `Force / Area -> Pressure`, `Force * Length -> Torque`/`Energy` |
|  [09]   | `Pressure`     | `Pascal`                         | `Kilopascal`                 | `Force / Area`                                       |
|  [10]   | `Energy`       | `Joule`                          | `KilowattHour`               | `Energy / Duration -> Power`; decimal-precision sums |
|  [11]   | `Power`        | `Watt`                           | `Watt`                       | `Energy / Duration`; decimal-precision sums          |
|  [12]   | `Temperature`  | `Kelvin`                         | `DegreeCelsius`              | affine offset units — never sum directly             |
|  [13]   | `Angle`        | `Radian`                         | `Radian`                     | interior angle math stays radians                    |
|  [14]   | `Torque`       | `NewtonMeter`                    | `NewtonMeter`                | `Force * Length` (distinct dimension from Energy)    |
|  [15]   | `Ratio`        | `DecimalFraction`                | `Percent`                    | the dimensionless family the dimensional bridge maps `Dimensionless` to |

[PUBLIC_TYPE_SCOPE]: admitted unit enums (`UnitsNet.Units`)
- rail: units
- note: each `<Quantity>Unit` is a flat `enum`; the canonical member is `Info.BaseUnitInfo.Value`, NOT a hardcoded constant. Resolve display/parse targets through metadata, never a literal. Rasm admits the rows below plus the AEC-successor enums (`DensityUnit`, `AreaMomentOfInertiaUnit`, `HeatTransferCoefficientUnit`, `ThermalResistanceUnit`, `IlluminanceUnit`, `RotationalSpeedUnit`).

| [INDEX] | [SYMBOL]           | [CANONICAL]              | [REPRESENTATIVE DISPLAY]      |
| :-----: | :----------------- | :---------------------- | :---------------------------- |
|  [01]   | `LengthUnit`       | `Meter`                 | `Millimeter`                  |
|  [02]   | `AreaUnit`         | `SquareMeter`           | `SquareMeter`                 |
|  [03]   | `VolumeUnit`       | `CubicMeter`            | `CubicMeter`                  |
|  [04]   | `MassUnit`         | `Kilogram`              | `Kilogram`                    |
|  [05]   | `DurationUnit`     | `Second`                | `Second`                      |
|  [06]   | `SpeedUnit`        | `MeterPerSecond`        | `MeterPerSecond`              |
|  [07]   | `AccelerationUnit` | `MeterPerSecondSquared` | `MeterPerSecondSquared`       |
|  [08]   | `ForceUnit`        | `Newton`                | `Newton`                      |
|  [09]   | `PressureUnit`     | `Pascal`                | `Kilopascal`                  |
|  [10]   | `EnergyUnit`       | `Joule`                 | `KilowattHour`                |
|  [11]   | `PowerUnit`        | `Watt`                  | `Watt`                        |
|  [12]   | `TemperatureUnit`  | `Kelvin`                | `DegreeCelsius`               |
|  [13]   | `AngleUnit`        | `Radian`                | `Degree`                      |
|  [14]   | `TorqueUnit`       | `NewtonMeter`           | `NewtonMeter`                 |
|  [15]   | `RatioUnit`        | `DecimalFraction`       | `Percent`                     |

[PUBLIC_TYPE_SCOPE]: statics, parsing, conversion, policy (`UnitsNet`)
- rail: units

| [INDEX] | [SYMBOL]                 | [SHAPE / KEY MEMBERS]                                                                                                                                                                                                                                                                | [CONSUMER / BOUNDARY NOTE]                                                                                                       |
| :-----: | :----------------------- | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------------------------------------------------------------------------------------------------------------------------ |
|  [01]   | `Quantity`               | `static QuantityInfo[] Infos`; `static string[] Names`; `static IDictionary<string,QuantityInfo> ByName`; `static UnitInfo GetUnitInfo(Enum)`/`TryGetUnitInfo(Enum, out UnitInfo?)`; `static IQuantity From(QuantityValue, Enum)`/`From(QuantityValue, string, string)`; `static IQuantity FromQuantityInfo(QuantityInfo, QuantityValue)`; `static bool TryFrom(double, Enum, out IQuantity?)`/`TryFrom(QuantityValue, Enum?, out IQuantity?)`; `static IQuantity Parse(IFormatProvider?, Type, string)`/`bool TryParse(IFormatProvider?, Type, string, out IQuantity?)`; `static IEnumerable<QuantityInfo> GetQuantitiesWithBaseDimensions(BaseDimensions)` | the dynamic dispatch root: `Admit(string)` -> `Quantity.TryParse(culture, Info.ValueType, text, out parsed)`; `Admit(double, Enum)` -> `Quantity.TryFrom(value, unit, out typed)`; `GetQuantitiesWithBaseDimensions` is an alternative dimensional row-match to the local `DimensionAdmission.Table` |
|  [02]   | `QuantityParser`         | `static QuantityParser Default`; `ctor(UnitAbbreviationsCache?)`; `TQuantity Parse<TQuantity,TUnitType>(string, IFormatProvider?, QuantityFromDelegate<TQuantity,TUnitType>)`; `bool TryParse<TQuantity,TUnitType>(string?, IFormatProvider?, QuantityFromDelegate<…>, out TQuantity)` / `out IQuantity?` (`TQuantity : struct, IQuantity`, `TUnitType : struct, Enum`) | typed value+unit text parse when the quantity type is statically known; pass the family `From` as the `QuantityFromDelegate`     |
|  [03]   | `UnitParser`             | `static UnitParser Default`; `ctor(UnitAbbreviationsCache?)`; `TUnitType Parse<TUnitType>(string, IFormatProvider? = null)`; `Enum Parse(string?, Type, IFormatProvider? = null)`; `bool TryParse<TUnitType>(string?, IFormatProvider?, out TUnitType)`; `bool TryParse(string?, Type, IFormatProvider?, out Enum?)` | abbreviation -> unit enum; `Resolve(unit, policy) => UnitParser.Default.TryParse(unit, Info.UnitType, policy.Culture, out resolved)` replaces the `"1 {unit}"` probe-parse hack |
|  [04]   | `QuantityFormatter`      | `static string Format<TUnitType>(IQuantity<TUnitType>, string)`; `static string Format<TUnitType>(IQuantity<TUnitType>, string?, IFormatProvider?)` (`where TUnitType : Enum`) | culture-scoped render behind a `UnitPolicy` format string; takes the single-param typed face. There is NO public `UnitFormatter` (it is `internal`) — the only public formatter is `QuantityFormatter` |
|  [05]   | `UnitConverter`          | `static UnitConverter Default`; `static double Convert(QuantityValue, Enum, Enum)`; `static bool TryConvert(QuantityValue, Enum, Enum, out double)`; `static double ConvertByName(QuantityValue, string, string, string)`/`bool TryConvertByName(…)`; `static double ConvertByAbbreviation(QuantityValue, string, string, string, string? culture)`/`bool TryConvertByAbbreviation(…)`; `void SetConversionFunction<TQ>(Enum, Enum, ConversionFunction<TQ>)` / `(Type,Enum,Type,Enum,ConversionFunction)`; `ConversionFunction GetConversionFunction(…)`/`bool TryGetConversionFunction(…)`; `static UnitConverter CreateDefault()`; `static void RegisterDefaultConversions(UnitConverter)` | numeric-only conversion that constructs NO `IQuantity`: `UnitAlgebra.Numeric` -> `UnitConverter.TryConvert(value, from, to, out converted)`. The `SetConversionFunction`/`GetConversionFunction` surface registers custom inter-unit functions on a non-default `UnitConverter` for a bespoke conversion not in the default catalog |
|  [06]   | `ConversionFunction`     | `delegate IQuantity ConversionFunction(IQuantity inputValue)`                                                                                                                                                                                                                          | the custom-conversion delegate `UnitConverter.SetConversionFunction` registers; identity is `EchoFunction`                      |
|  [07]   | `QuantityFromDelegate<out TQuantity, in TUnitType>` | `delegate TQuantity (QuantityValue value, TUnitType fromUnit)` (`TQuantity : IQuantity`, `TUnitType : Enum`)                                                                                                                                                  | the factory the typed `QuantityParser.Parse<TQuantity,TUnitType>` invokes; bind `Length.From` etc.                              |
|  [08]   | `UnitAbbreviationsCache` | `static UnitAbbreviationsCache Default`; `static CreateEmpty()`/`CreateDefault()`; `void MapUnitToAbbreviation<TUnitType>(TUnitType, params string[])` / `(…, IFormatProvider?, params string[])`; `void MapUnitToDefaultAbbreviation<TUnitType>(TUnitType, string)`; `string GetDefaultAbbreviation<TUnitType>(TUnitType, IFormatProvider? = null)`; `string[] GetUnitAbbreviations<TUnitType>(TUnitType, IFormatProvider? = null)`; `IReadOnlyList<string> GetAllUnitAbbreviationsForQuantity(Type, IFormatProvider? = null)`; `IReadOnlyList<string> GetAbbreviations(UnitInfo, IFormatProvider? = null)` | abbreviation backing store with invariant-culture fallback; `MapUnitToAbbreviation` registers project-specific aliases (e.g. a domain glyph) on the setup cache once at composition |
|  [09]   | `UnitSystem`             | `sealed` : `IEquatable`; `BaseUnits BaseUnits`; `static UnitSystem SI`; `ctor(BaseUnits)`; `operator ==`/`!=`                                                                                                                                                                          | `UnitPolicy.Baseline => UnitSystem.SI`; the policy row pinning every canonicalization target                                    |
|  [10]   | `UnitsNetSetup`          | `sealed`; `static UnitsNetSetup Default`; `UnitConverter UnitConverter`; `UnitAbbreviationsCache UnitAbbreviations`; `UnitParser UnitParser`; `QuantityParser QuantityParser`; `ctor(ICollection<QuantityInfo>, UnitConverter)`                                                          | the SINGLE setup root composed once at the composition root; `Default` exposes the four sub-services. The `ctor` builds a scoped setup with a custom quantity catalog + converter when the default registry must be replaced — a second ambient setup is rejected |
|  [11]   | `UnitMath`               | `static TQuantity Abs<TQuantity>(this TQuantity)`; `Sum<TQuantity>(this IEnumerable<TQuantity>, Enum)` / `Sum<TSource,TQuantity>(this IEnumerable<TSource>, Func<TSource,TQuantity>, Enum)`; `Min`/`Max<TQuantity>(TQuantity, TQuantity)` and the sequence+selector mirrors; `Average<TQuantity>(this IEnumerable<TQuantity>, Enum)` (+selector); `Clamp<TQuantity>(TQuantity, TQuantity, TQuantity)`; `QuantityValue ToQuantityValue(this double)`/`(this decimal)` (`TQuantity : IQuantity`, `Min`/`Max`/`Clamp` add `IComparable`) | the boundary-side aggregation surface (`this` static class) for `Sum`/`Min`/`Max`/`Average`/`Clamp`/`Abs` over a quantity sequence at a chosen unit, BEFORE re-entry into `Admit`. This is the ONLY generic-math aggregation owner — `GenericMathExtensions`/`DecimalGenericMathExtensions` do not exist in 5.75; for per-element `+`/`-`/`*`/`/`/`Zero` use the `IArithmeticQuantity` operators directly |
|  [12]   | `DefaultUnitAttribute`   | `class : UnitAttributeBase`; `ctor(object? unitType)`                                                                                                                                                                                                                                  | declares a default unit on a user property; the generated quantity structs carry NO such attribute, so reflection over them resolves nothing — Rasm sources canonical/display from `QuantityInfo`, not attributes |
|  [13]   | `ConvertToUnitAttribute` | `class : DefaultUnitAttribute`; `ctor(object? unitType)`                                                                                                                                                                                                                               | property-level conversion-target marker (user model annotation, not on built-in quantities)                                    |
|  [14]   | `DisplayAsUnitAttribute` | `class : DefaultUnitAttribute`; `ctor(object? unitType, string format = "")`                                                                                                                                                                                                           | property-level display marker with optional format (user model annotation)                                                     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: quantity ingress and conversion (typed struct surface)
- rail: units
- note: the polymorphic ingress collapse — text, value+enum, and value+abbreviation all converge on the same `As(canonical)` projection. Conversion runs exactly once at admission; interior numerics are raw `double` owned by Rasm core.

| [INDEX] | [SURFACE]                                  | [CALL_SHAPE]                                                      | [CAPABILITY / STACK]                                                                                                  |
| :-----: | :----------------------------------------- | :--------------------------------------------------------------- | :------------------------------------------------------------------------------------------------------------------ |
|  [01]   | `<Quantity>.From(QuantityValue, TUnit)`    | `Length.From(v, LengthUnit.Millimeter)`                          | strongly-typed factory; the `QuantityValue` implicit operator lifts a bare `double`/`decimal`                       |
|  [02]   | `<Quantity>.From<UnitName>(QuantityValue)` | `Length.FromMillimeters(v)`                                      | per-unit factory (one per enum member) for the common literal case                                                  |
|  [03]   | `<Quantity>.As(TUnit)` / `As(UnitSystem)`  | `q.As(row.Canonical)` / `q.As(UnitSystem.SI)`                    | the canonicalization read that produces the SI scalar `UnitEvidence.CanonicalValue` records                          |
|  [04]   | `<Quantity>.ToUnit(TUnit)` / `(UnitSystem)`| `q.ToUnit(row.Display)`                                          | unit re-projection returning the same quantity type; `ToUnit(TUnit, UnitConverter)` injects a custom converter      |
|  [05]   | `<Quantity>.Parse/TryParse`                | `Length.TryParse(text, culture, out len)`                       | typed parse when the family is known; the dynamic mirror is `Quantity.TryParse`                                     |
|  [06]   | `Quantity.From(QuantityValue, Enum)`       | `Quantity.From(canonical, row.Canonical)`                       | dynamic factory returning `IQuantity`; `Render` uses it then `.ToUnit(target).ToString(format, culture)`            |
|  [07]   | `Quantity.TryFrom(double, Enum, out IQuantity?)` | `Quantity.TryFrom(value, unit, out typed)`                | dynamic value+enum ingress — the `Admit(double, Enum)` arity                                                        |
|  [08]   | `Quantity.FromUnitAbbreviation`            | `Quantity.TryFromUnitAbbreviation(culture, v, abbr, out q)`     | value+abbreviation ingress (culture-scoped overload available)                                                      |
|  [09]   | `IQuantity.Equals(IQuantity?, IQuantity tolerance)` | `a.Equals(b, Length.FromMeters(row.Tolerance))`        | tolerance-aware equivalence feeding the per-row tolerance column                                                   |

[ENTRYPOINT_SCOPE]: cross-quantity operators (type-level dimensional algebra)
- rail: units
- note: each struct overloads `*`/`/` against sibling quantities so a product/quotient yields the correctly-dimensioned result type at compile time. This is the static counterpart to the runtime `BaseDimensions` algebra — the page `[03]-[DIMENSIONAL_LAW]` verifies the SAME relations at composition via metadata, so the operators are the typed fast-path and the relations are the proof.

| [INDEX] | [OPERATOR]                                   | [RESULT TYPE]        | [DIMENSIONAL RELATION]                |
| :-----: | :------------------------------------------- | :------------------- | :------------------------------------ |
|  [01]   | `Length * Length`                            | `Area`               | L · L = L²                            |
|  [02]   | `Area * Length` / `Length * Area`            | `Volume`             | L² · L = L³                           |
|  [03]   | `Length / Duration`                          | `Speed`              | L / T                                 |
|  [04]   | `Speed / Duration`                           | `Acceleration`       | L / T²                                |
|  [05]   | `Mass * Acceleration`                        | `Force`              | M · L / T²                            |
|  [06]   | `Force / Area`                               | `Pressure`           | M / (L · T²)                          |
|  [07]   | `Force * Length` / `Length * Force`          | `Torque`             | M · L² / T²                           |
|  [08]   | `Mass / Volume`                              | `Density`            | M / L³                                |

[ENTRYPOINT_SCOPE]: metadata, dimensions, policy roots
- rail: units

| [INDEX] | [SURFACE]                                  | [CALL_SHAPE]                                                  | [CAPABILITY / STACK]                                                                                          |
| :-----: | :----------------------------------------- | :---------------------------------------------------------- | :----------------------------------------------------------------------------------------------------------- |
|  [01]   | `<Quantity>.Info`                          | `UnitsNet.Length.Info` -> `QuantityInfo<LengthUnit>`         | the metadata column source; `Canonical = Info.BaseUnitInfo.Value`, dimensional row `Info.BaseDimensions`     |
|  [02]   | `QuantityInfo.BaseUnitInfo.Value`          | `info.BaseUnitInfo.Value`                                    | base-unit `Enum` read once per smart-enum row at static construction — replaces the hand-passed canonical arg |
|  [03]   | `QuantityInfo.UnitInfos`                   | `info.UnitInfos.Map(u => u.Value)`                          | dashboard picker conversion targets, zero attribute reflection                                              |
|  [04]   | `QuantityInfo.ValueType` / `.UnitType`     | `Quantity.TryParse(culture, Info.ValueType, text, out q)`   | the CLR value/unit `Type` driving dynamic parse and unit resolution                                         |
|  [05]   | `BaseDimensions.Multiply` / `.Divide`      | `left.Multiply(right)` / `left.Divide(right)`               | the `UnitAlgebra.Relations` compound proof composer                                                         |
|  [06]   | `BaseDimensions.Dimensionless`             | `BaseDimensions.Dimensionless`                              | the zero vector the `Reciprocals` product and `Ratio` row assert against                                    |
|  [07]   | `BaseDimensions.{Length…LuminousIntensity}`| `BigRational.FromInt(dims.Length)`                          | the seven `int` SI exponents `DimensionMonomial.From` lifts to exact rationals                              |
|  [08]   | `UnitConverter.TryConvert`                 | `UnitConverter.TryConvert(v, from, to, out converted)`     | numeric-only convert that builds no `IQuantity` — `UnitAlgebra.Numeric`                                     |
|  [09]   | `UnitParser.Default.TryParse(…, Type, …)`  | `UnitParser.Default.TryParse(unit, Info.UnitType, culture, out e)` | abbreviation resolution; the `Resolve` accessor                                                      |
|  [10]   | `QuantityFormatter.Format<TUnit>`          | `QuantityFormatter.Format(typed, policy.Format, culture)`  | culture-scoped rendered text                                                                                |
|  [11]   | `UnitMath.Sum`/`Min`/`Max`/`Average`/`Clamp`/`Abs` | `seq.Sum(canonical)` / `seq.Average(canonical)`     | boundary aggregation at a chosen unit before re-entering `Admit`                                            |
|  [12]   | `UnitSystem.SI`                            | `UnitSystem.SI`                                            | the `Baseline` policy row                                                                                   |
|  [13]   | `UnitsNetSetup.Default`                    | `UnitsNetSetup.Default`                                    | the single composed setup root; `.UnitConverter`/`.UnitParser`/`.UnitAbbreviations`/`.QuantityParser`      |

## [04]-[INTEGRATION_LAW]

[STACK_THINKTECTURE]:
- `QuantityFamily` is a `[SmartEnum<string>]` (Thinktecture.Runtime.Extensions) whose rows store the erased `QuantityInfo` and read `BaseUnitInfo.Value` once at static construction; the smart-enum key policy `[KeyMemberEqualityComparer]`/`[KeyMemberComparer]` rides `StringComparer.OrdinalIgnoreCase` so the family key is the canonical name.
- `DimensionMonomial` is a `[ValueObject<Arr<BigRational>>]` (Thinktecture) lifting the `BaseDimensions` seven `int` exponents to exact rationals so the symbolic `Power` arm carries a non-integer exponent UnitsNet cannot; structural value equality is generated.
- Serialization stacks onto `Thinktecture.Runtime.Extensions.Json`: NO UnitsNet type crosses a JSON or proto wire — `UnitEvidence` projects to plain `string`/`double` fields, and the Thinktecture JSON context serializes the `[SmartEnum]`/`[ValueObject]` wrappers. Conversion-at-admission is what enforces the UnitsNet-serialization SKIP.

[STACK_LANGUAGEEXT]:
- Every admission returns `Fin<UnitEvidence>` (LanguageExt.Core): `Quantity.TryParse`/`TryFrom`/`UnitParser.TryParse` `bool`+`out` boundary calls lift into the `Fin` rail, and a parse/family/dimension failure mints `ComputeFault` on the 2200 band rather than throwing.
- The dimensional proof stacks onto `Validation<Error, DimensionMonomial>`: the applicative `Apply`/`Traverse` accumulates EVERY compound mismatch across a symbolic tree in one pass — `Error` is the monoidal failure carrier (`ComputeFault` is not its own monoid, so each typed `DimensionMismatch` lifts onto `Error` through its `Expected` base), the UnitsNet `BaseDimensions.Equals` is the leaf predicate, and the LanguageExt applicative is the accumulation algebra.
- Sequence aggregation uses `Seq`/`Arr` carriers; `UnitMetadata.ConvertTargets` maps `info.UnitInfos.ToSeq()` to its `.Value` projection.

[STACK_MATHNET_SYMBOLICS]:
- The `Symbolic/dimensional` arm folds the MathNet.Symbolics F# `Expression` DU onto `DimensionMonomial`, then the `UNITS_BRIDGE` matches the proven monomial against `QuantityFamily.Items` lifted through `DimensionMonomial.From(row.Info.BaseDimensions)` — UnitsNet supplies the named-family target vocabulary, MathNet supplies the symbolic tree, and the bridge is the single seam. `sqrt` lowers to `Power(arg, 1/2)`, which the exact-rational monomial scales — the reason the monomial is `BigRational` and not the `int`-exponent `BaseDimensions`.

[CONVERSION_POLICY]:
- Conversion runs EXACTLY ONCE at admission (`As(canonical)`); a quantity type in an interior signature or on a wire is the seam violation the boundary deletes.
- Numeric-only conversion that crosses no quantity type rides `UnitConverter.TryConvert` (non-throwing, constructs no `IQuantity`).
- Canonical and display units source from `QuantityInfo.BaseUnitInfo.Value` and an explicit per-row display `Enum`; the generated structs emit no `DefaultUnitAttribute`/`DisplayAsUnitAttribute`, so attribute reflection over them resolves nothing.
- Abbreviation resolution runs through `UnitParser.Default.TryParse(string, Type, IFormatProvider, out Enum)`, never a `"1 {unit}"` probe-string parse.
- `UnitsNetSetup.Default` is the single setup root composed once; a second ambient setup is rejected.
- Affine families (`Temperature`) carry an offset — sum/aggregate at the canonical absolute scale, never across display offsets.

[RAIL_LAW]:
- Package: `UnitsNet` 5.75.0 (MIT-0)
- Owns: source-generated quantity/unit algebra, the SI dimension vector, culture-scoped parse/format, and conversion/abbreviation policy
- Accept: measured unit-aware execution input at the boundary, canonicalized once to an SI raw double
- Reject: raw numeric unit comments; a UnitsNet type on an interior signature or a wire; `GenericMathExtensions`/`DecimalGenericMathExtensions`/`UnitFormatter`/`IDecimalQuantity` (the first two do not exist, the third is internal, the fourth is `[Obsolete]`)
