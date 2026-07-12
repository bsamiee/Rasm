# [RASM_MATERIALS_API_VIVIDORANGE_UNCERTAINTIES]

`VividOrange.Uncertainties` supplies measured-value uncertainty arithmetic for the Materials `Properties/` engineering-property rows: four uncertainty MODELS (absolute ±, relative `±2%`, interval `[lo, hi]`, normal `mean ± k·sd`) over four NUMERIC carriers — a `double` scalar (`Uncertainty`), a generic `INumber<T>` scalar (`Scalar.UncertaintyScalar<T>`), a `decimal` (`Decimal.*`), and — via `VividOrange.Uncertainties.Quantities` — a `UnitsNet` `IQuantity` (`UncertaintyQuantity<T>`). A property value is admitted fluently (`5.0.WithRelativeUncertainty`, `pressure.WithAbsoluteUncertainty(...)`), propagated through one shared bound-corner algebra (`+`/`-`/`*`/`/`, unit-checked for quantities), and carried with its value+unit+uncertainty TOGETHER through the Properties and Profiles surfaces instead of an ad hoc tolerance field. `VividOrange.IUncertainties` is the interface floor; the package composes the SAME in-folder `UnitsNet` quantity owner (`api-unitsnet.md`) the section-property and material-grade rows use. Its `ITaxonomySerializable` is the NEWER `VividOrange.Taxonomy.Serialization` contract — DISTINCT from the `VividOrange.Serialization.ITaxonomySerializable` the Sections/Materials/Profiles packages ride (see [04]-[SERIALIZATION_BOUNDARY]); the Materials owner reads the typed value and serializes through the canonical Rasm snapshot codec, never assuming one shared VividOrange serializer.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `VividOrange.Uncertainties`

- package: `VividOrange.Uncertainties`
- license: MIT (`licenses.nuget.org/MIT` — MagmaWorks / VividOrange taxonomy; `github.com/VividOrange/Uncertainties`)
- assembly: `VividOrange.Uncertainties`
- namespace: `VividOrange.Uncertainties` (the `double` family + `UncertaintyModel`), `.Scalar` (the `INumber<T>` family), `.Decimal` (the `decimal` family), `.Utility` (admission + operators)
- dependency: `VividOrange.IUncertainties` (the interface floor)
- target frameworks: `net8.0`, `net7.0`, `net6.0`, `net48`
- asset: runtime library, pure-managed AnyCPU, NO native RID asset. The `net10.0` consumer binds the highest asset `lib/net8.0`.
- rail: properties

[PACKAGE_SURFACE]: `VividOrange.Uncertainties.Quantities`

- package: `VividOrange.Uncertainties.Quantities`
- license: MIT (`licenses.nuget.org/MIT`)
- assembly: `VividOrange.Uncertainties.Quantities`
- namespace: `VividOrange.Uncertainties` (`UncertaintyQuantity<T>`), `.Quantities` (the four `*UncertaintyQuantity<TQuantity>` kinds), `.Quantities.Utility` (factory + operators)
- dependencies: `VividOrange.IUncertainties`; `UnitsNet` (the shared in-folder quantity floor, `api-unitsnet.md`)
- target frameworks: `net8.0`, `net7.0`, `net6.0`, `net48`
- asset: runtime library, pure-managed AnyCPU; `net10.0` binds `lib/net8.0`
- rail: properties (UnitsNet quantities)

[PACKAGE_SURFACE]: `VividOrange.IUncertainties`

- package: `VividOrange.IUncertainties`
- license: MIT (`licenses.nuget.org/MIT`; `github.com/VividOrange/Taxonomy`)
- assembly: `VividOrange.IUncertainties`
- namespace: `VividOrange.Uncertainties` (the interface set — the package id is `I…`, the namespace is NOT)
- dependency: `VividOrange.Taxonomy.ISerialization` (the `ITaxonomySerializable` floor; transitive-only)
- target frameworks: `net8.0`, `net7.0`, `net6.0`, `net48`
- asset: runtime library, pure-managed AnyCPU; `net10.0` binds `lib/net8.0`
- rail: properties (contract floor)
- ABI floor: a PRE-1.0 contract — the `IUncertainty<T>` member set may break across a minor bump.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: interface floor (`VividOrange.IUncertainties`, namespace `VividOrange.Uncertainties`)

- rail: properties

The interface floor mints one propagation contract and four model-specific contracts.

| [INDEX] | [SYMBOL]                            |
| :-----: | :---------------------------------- |
|  [01]   | `IUncertainty<T>`                   |
|  [02]   | `IAbsoluteUncertainty<T>`           |
|  [03]   | `IRelativeUncertainty<T>`           |
|  [04]   | `IIntervalUncertainty<T>`           |
|  [05]   | `INormalDistributionUncertainty<T>` |

[IUNCERTAINTY]:

- inheritance: `ITaxonomySerializable`
- values: `CentralValue`, `LowerBound`, and `UpperBound` (all `T`)
- binary propagation: `PropagateBinary(IUncertainty<T> other, Func<T,T,T> op)`
- unary propagation: `PropagateUnary(Func<double,double> op)`

[IABSOLUTE_UNCERTAINTY]:

- inheritance: `IUncertainty<T>`
- member: `T AbsoluteUncertaintyValue`

[IRELATIVE_UNCERTAINTY]:

- inheritance: `IUncertainty<T>`
- member: `double RelativeUncertaintyValue`

[IINTERVAL_UNCERTAINTY]:

- inheritance: `IUncertainty<T>`
- bounds: `LowerBound` and `UpperBound` are the carrier

[INORMAL_DISTRIBUTION_UNCERTAINTY]:

- inheritance: `IUncertainty<T>`
- members: `T StandardDeviation` and `double CoverageFactor`

[PUBLIC_TYPE_SCOPE]: `double` scalar family (`VividOrange.Uncertainties`)

- rail: properties

The `double` family carries one wrapper, four bound models, and their discriminant.

| [INDEX] | [SYMBOL]                        | [KIND]  |
| :-----: | :------------------------------ | :------ |
|  [01]   | `Uncertainty`                   | wrapper |
|  [02]   | `AbsoluteUncertainty`           | model   |
|  [03]   | `RelativeUncertainty`           | model   |
|  [04]   | `IntervalUncertainty`           | model   |
|  [05]   | `NormalDistributionUncertainty` | model   |
|  [06]   | `UncertaintyModel`              | enum    |

[UNCERTAINTY]:

- shape: wraps `IUncertainty<double> UncertaintyModel`
- operators: `+`, `-`, `*` (`double`), and `/` (`double`)
- conversions: implicit from and explicit to the four kinds
- factories: `From{Absolute,Relative,Interval,NormalDistribution}Uncertainty`

[ABSOLUTE_UNCERTAINTY]: `(centralValue, uncertainty)` produces `[cv-u, cv+u]`.
[RELATIVE_UNCERTAINTY]: `(centralValue, relativeUncertainty)` produces `[cv·(1-r), cv·(1+r)]`.
[INTERVAL_UNCERTAINTY]: `(centralValue, boundStart, boundEnd)` produces the auto-ordered `[lo, hi]` interval.
[NORMAL_DISTRIBUTION_UNCERTAINTY]: `(mean, standardDeviation, coverageFactor =)` produces `[μ-k·σ, μ+k·σ]`.
[UNCERTAINTY_MODEL]: `Interval`, `Relative`, `Absolute`, and `NormalDistribution` form the kind discriminant.

[PUBLIC_TYPE_SCOPE]: generic `INumber<T>` scalar family (`.Scalar`) and `decimal` family (`.Decimal`)

- rail: properties

The generic and decimal families mirror the four scalar uncertainty models.

| [INDEX] | [SYMBOL]                                                                     | [KIND]  |
| :-----: | :--------------------------------------------------------------------------- | :------ |
|  [01]   | `Scalar.UncertaintyScalar<T>`                                                | wrapper |
|  [02]   | `Scalar.{Absolute,Relative,Interval,NormalDistribution}UncertaintyScalar<T>` | model   |
|  [03]   | `Decimal.{Absolute,Relative,Interval,NormalDistribution}UncertaintyDecimal`  | model   |

[UNCERTAINTY_SCALAR]: `Scalar.UncertaintyScalar<T>` is the generic-numeric wrapper under `where T: INumber<T>` and the generic form of the `double` and `decimal` peers.
[UNCERTAINTY_SCALAR_MODELS]: the four `Scalar.*UncertaintyScalar<T>` models carry `INumber<T>` values.
[UNCERTAINTY_DECIMAL_MODELS]: the four `Decimal.*UncertaintyDecimal` models carry financial and precision cost rows.

[PUBLIC_TYPE_SCOPE]: `UnitsNet` quantity family (`VividOrange.Uncertainties.Quantities`)

- rail: properties

The quantity family binds a `UnitsNet` value, unit, and uncertainty under one wrapper and four models.

| [INDEX] | [SYMBOL]                                                      | [KIND]  |
| :-----: | :------------------------------------------------------------ | :------ |
|  [01]   | `UncertaintyQuantity<T>`                                      | wrapper |
|  [02]   | `Quantities.AbsoluteUncertaintyQuantity<TQuantity>`           | model   |
|  [03]   | `Quantities.RelativeUncertaintyQuantity<TQuantity>`           | model   |
|  [04]   | `Quantities.IntervalUncertaintyQuantity<TQuantity>`           | model   |
|  [05]   | `Quantities.NormalDistributionUncertaintyQuantity<TQuantity>` | model   |

[UNCERTAINTY_QUANTITY]:

- constraint: `where T: IQuantity`
- namespace: `VividOrange.Uncertainties`
- shape: carries value, unit, and uncertainty together
- conversions: implicit from and explicit to the four kinds
- factories: `From…`

[ABSOLUTE_UNCERTAINTY_QUANTITY]: `(centralValue, uncertainty)` computes bounds through `IQuantity.Subtract` and `IQuantity.Add`.
[RELATIVE_UNCERTAINTY_QUANTITY]: `(centralValue, Ratio\| double uncertainty)` computes bounds through `Multiply(1 ± r)`.
[INTERVAL_UNCERTAINTY_QUANTITY]: `(centralValue, lowerBound, upperBound)` produces auto-ordered quantity bounds.
[NORMAL_DISTRIBUTION_UNCERTAINTY_QUANTITY]: `(mean, standardDeviation, coverageFactor =)` applies RSS combination during binary propagation.

[PUBLIC_TYPE_SCOPE]: admission + arithmetic (`.Utility`, `.Quantities.Utility`)

- rail: properties

The static utility surfaces own fluent admission and arithmetic.

| [INDEX] | [SYMBOL]                                          | [KIND] |
| :-----: | :------------------------------------------------ | :----- |
|  [01]   | `Utility.DoubleExtensions`                        | fluent |
|  [02]   | `Utility.GenericINumberExtensions`                | fluent |
|  [03]   | `Utility.DecimalExtensions`                       | fluent |
|  [04]   | `Utility.UncertaintyOperators`                    | math   |
|  [05]   | `Utility.UncertaintyScalarOperators`              | math   |
|  [06]   | `Quantities.Utility.UncertaintyQuantityFactory`   | fluent |
|  [07]   | `Quantities.Utility.UncertaintyQuantityOperators` | math   |

[DOUBLE_EXTENSIONS]: `(this double).With{Absolute,Relative,Interval,NormalDistribution}Uncertainty(...)` returns a `double` model.
[GENERIC_INUMBER_EXTENSIONS]: `(this T).WithX<T>(...) where T: INumber<T>` returns a `Scalar.*UncertaintyScalar<T>`.
[DECIMAL_EXTENSIONS]: `(this decimal).WithX(...)` returns a `Decimal.*UncertaintyDecimal`.
[UNCERTAINTY_OPERATORS]: `Add`, `Subtract`, `Multiply` (`double`), and `Divide` (`double`) operate over `IUncertainty<double>`.
[UNCERTAINTY_SCALAR_OPERATORS]: `Add`, `Subtract`, `Multiply` (`double`), and `Divide` (`double`) operate over `UncertaintyScalar<T>` under `T: INumber<T>`.
[UNCERTAINTY_QUANTITY_FACTORY]: `(this TQuantity).WithX<TQuantity>(...) where TQuantity: IQuantity` returns a `*UncertaintyQuantity<TQuantity>`.
[UNCERTAINTY_QUANTITY_OPERATORS]: `Add` and `Subtract` enforce units and throw `"Incompatible units."`; `Multiply` and `Divide` take a `double` over `UncertaintyQuantity<T>`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: fluent admission — attach uncertainty to a value

- rail: properties
- composition law: the `.WithXUncertainty` extension is the primary entry; it returns the typed model the operators and `IUncertainty<T>` bounds read. A `UnitsNet` quantity (`Pressure`/`Density`/…) admits via `UncertaintyQuantityFactory`; a raw `double`/`decimal`/`INumber<T>` via the matching `.Utility` class.

The admission surface dispatches by scalar or quantity carrier.

| [INDEX] | [SURFACE]                                    | [CARRIER]    |
| :-----: | :------------------------------------------- | :----------- |
|  [01]   | `(double).WithRelativeUncertainty`           | `double`     |
|  [02]   | `(double).WithAbsoluteUncertainty`           | `double`     |
|  [03]   | `(double).WithIntervalUncertainty`           | `double`     |
|  [04]   | `(double).WithNormalDistributionUncertainty` | `double`     |
|  [05]   | `(TQuantity).WithRelativeUncertainty`        | `IQuantity`  |
|  [06]   | `(TQuantity).WithAbsoluteUncertainty`        | `IQuantity`  |
|  [07]   | `(T).WithAbsoluteUncertainty<T>`             | `INumber<T>` |

[DOUBLE_RELATIVE_ADMISSION]:

- call: `RelativeUncertainty WithRelativeUncertainty(this double value, double relativeUncertainty)`
- result: `value ± r·value`

[DOUBLE_ABSOLUTE_ADMISSION]:

- call: `AbsoluteUncertainty WithAbsoluteUncertainty(this double value, double absoluteUncertainty)`
- result: `value ± u`

[DOUBLE_INTERVAL_ADMISSION]:

- call: `IntervalUncertainty WithIntervalUncertainty(this double value, double lowerBound, double upperBound)`
- result: explicit `[lo, hi]`

[DOUBLE_NORMAL_ADMISSION]:

- call: `NormalDistributionUncertainty WithNormalDistributionUncertainty(this double value, double standardDeviation, double coverageFactor =)`
- result: `μ ± k·σ`

[QUANTITY_RELATIVE_ADMISSION]:

- call: `RelativeUncertaintyQuantity<TQuantity> WithRelativeUncertainty<TQuantity>(this TQuantity quantity, Ratio\| double relativeUncertainty) where TQuantity: IQuantity`
- result: a `UnitsNet` value `± r%`

[QUANTITY_ABSOLUTE_ADMISSION]:

- call: `AbsoluteUncertaintyQuantity<TQuantity> WithAbsoluteUncertainty<TQuantity>(this TQuantity quantity, TQuantity\| double absoluteUncertainty) where TQuantity: IQuantity`
- result: a `UnitsNet` value `± u` in the same unit or as `double`

[GENERIC_ABSOLUTE_ADMISSION]:

- call: `AbsoluteUncertaintyScalar<T> WithAbsoluteUncertainty<T>(this T value, T absoluteUncertainty) where T: INumber<T>`
- result: generic-numeric `value ± u`

[ENTRYPOINT_SCOPE]: wrapper construction + factories

- rail: properties

Construction discriminates the quantity model by argument shape, while named factories mirror the constructors.

| [INDEX] | [SURFACE]                                                  | [KIND]      |
| :-----: | :--------------------------------------------------------- | :---------- |
|  [01]   | `new UncertaintyQuantity<T>(cv, uncertainty)`              | constructor |
|  [02]   | `new UncertaintyQuantity<T>(cv, Ratio\| double)`           | constructor |
|  [03]   | `new UncertaintyQuantity<T>(cv, lo, hi)`                   | constructor |
|  [04]   | `new UncertaintyQuantity<T>(cv, sd, k)`                    | constructor |
|  [05]   | `UncertaintyQuantity<T>.FromNormalDistributionUncertainty` | factory     |
|  [06]   | `Uncertainty.FromIntervalUncertainty`                      | factory     |
|  [07]   | `(IntervalUncertaintyQuantity<T>)quantityWrapper`          | conversion  |

[ABSOLUTE_QUANTITY_CONSTRUCTOR]:

- call: `UncertaintyQuantity(T centralValue, T absoluteUncertainty)`
- result: absolute quantity wrapper

[RELATIVE_QUANTITY_CONSTRUCTOR]:

- calls: `UncertaintyQuantity(T centralValue, Ratio relativeUncertainty)` and `(T, double)`
- result: relative quantity wrapper; the argument type selects the constructor

[INTERVAL_QUANTITY_CONSTRUCTOR]:

- call: `UncertaintyQuantity(T centralValue, T lowerBound, T upperBound)`
- result: interval quantity wrapper

[NORMAL_QUANTITY_CONSTRUCTOR]:

- call: `UncertaintyQuantity(T centralValue, T standardDeviation, double coverageFactor =)`
- result: normal quantity wrapper

[NORMAL_QUANTITY_FACTORY]:

- call: `static UncertaintyQuantity<T> FromNormalDistributionUncertainty(T cv, T sd, double coverageFactor =)`
- result: named-factory mirror of the constructors

[INTERVAL_DOUBLE_FACTORY]:

- call: `static Uncertainty FromIntervalUncertainty(double cv, double lo, double hi)`
- result: one of the four `double` named factories that mirror the constructors

[INTERVAL_QUANTITY_CONVERSION]: an explicit conversion unwraps the concrete kind and throws on a model mismatch.

[ENTRYPOINT_SCOPE]: propagation + arithmetic

- rail: properties
- composition law: arithmetic propagates uncertainty through the bound algebra; `Add`/`Subtract` require the SAME model kind (else `InvalidOperationException "Incompatible uncertainty model."`), and the quantity `Add`/`Subtract` additionally require unit agreement (else `"Incompatible units."`). `Multiply`/`Divide` scale by a `double`.

The arithmetic surface preserves the carrier while propagating bounds.

| [INDEX] | [SURFACE]                                            | [KIND]   |
| :-----: | :--------------------------------------------------- | :------- |
|  [01]   | `UncertaintyQuantityOperators.Add`                   | quantity |
|  [02]   | `UncertaintyQuantityOperators.Multiply`              | quantity |
|  [03]   | `UncertaintyOperators.Add`                           | `double` |
|  [04]   | `UncertaintyScalarOperators.Subtract`                | generic  |
|  [05]   | `a + b` / `a * factor`                               | wrapper  |
|  [06]   | `model.PropagateBinary`                              | model    |
|  [07]   | `model.PropagateUnary`                               | model    |
|  [08]   | `model.CentralValue` / `.LowerBound` / `.UpperBound` | read     |

[QUANTITY_ADD]:

- call: `UncertaintyQuantity<T> Add<T>(this UncertaintyQuantity<T> a, UncertaintyQuantity<T> b) where T: IQuantity`
- result: unit-checked quantity sum with propagated bounds

[QUANTITY_MULTIPLY]:

- call: `UncertaintyQuantity<T> Multiply<T>(this UncertaintyQuantity<T> a, double factor) where T: IQuantity`
- result: scales value and uncertainty by a scalar

[DOUBLE_ADD]:

- call: `Uncertainty Add<T>(this IUncertainty<double> a, T b) where T: IUncertainty<double>`
- result: `double` model sum

[GENERIC_SUBTRACT]:

- call: `IUncertainty<T> Subtract<T>(this UncertaintyScalar<T> a, UncertaintyScalar<T> b) where T: INumber<T>`
- result: generic-numeric difference

[DOUBLE_WRAPPER_OPERATORS]: `static Uncertainty operator +(Uncertainty, Uncertainty)` and `operator *(Uncertainty, double)` are the `double` wrapper's native operators.

[PROPAGATE_BINARY]:

- call: `IUncertainty<T> PropagateBinary(IUncertainty<T> other, Func<T,T,T> operation)`
- result: custom binary operation with bound propagation

[PROPAGATE_UNARY]:

- call: `IUncertainty<T> PropagateUnary(Func<double,double> operation)`
- result: custom unary operation that scales uncertainty by `\| f(1)\| `

[PROPAGATED_VALUES]: `model.CentralValue`, `.LowerBound`, and `.UpperBound` are `T` properties that expose the value and propagated bounds.

## [04]-[IMPLEMENTATION_LAW]

[PROPAGATION_ALGEBRA]:

- `PropagateBinary` is model-homogeneous: it casts `other` to the SAME kind interface (`IAbsoluteUncertainty<T>`/`IIntervalUncertainty<T>`/`IRelativeUncertainty<T>`/`INormalDistributionUncertainty<T>`) and throws `InvalidOperationException "Incompatible uncertainty model."` on a mismatch — the caller cannot add an absolute to a normal without lowering one first.
- For the absolute/relative/interval kinds, binary propagation evaluates the operation over the FOUR bound corners (`op(lo_a, lo_b)`, `op(lo_a, hi_b)`, `op(hi_a, lo_b)`, `op(hi_a, hi_b)`) and returns an `IntervalUncertaintyQuantity`/`IntervalUncertainty` of `(op(cv_a, cv_b), min(corners), max(corners))` — exact interval arithmetic, not a linearized error term.
- For the normal kind, binary propagation combines in QUADRATURE (root-sum-square): `σ = sqrt(σ_a² + σ_b²)`, preserving the `CoverageFactor`. `PropagateUnary` applies the operation to the central value and scales the uncertainty by `\|operation\|` (absolute/normal) or maps the bounds (interval).
- The quantity arithmetic works in the central value's unit: bounds and quadrature compute via `IQuantity.As(unit)` / `Quantity.From(value, unit)`, so a `Pressure ± Pressure` stays in the left operand's unit and a unit mismatch is caught by `QuantityInfo.BaseUnitInfo.QuantityName` before the operation.

[SERIALIZATION_BOUNDARY]:

- The uncertainty types implement `VividOrange.Taxonomy.Serialization.ITaxonomySerializable` from the `VividOrange.Taxonomy.ISerialization` assembly. The Sections, Materials, and Profiles packages implement the distinct `VividOrange.Serialization.ITaxonomySerializable` from `VividOrange.ISerialization`; the namespace, assembly, and type identity differ.
- `VividOrange.Serialization.JsonSerializationExtensions.ToJson<T>` and `FromJson<T>` require `T: VividOrange.Serialization.ITaxonomySerializable` and default to the internal `TaxonomyJsonSerializer.Settings` carrying `StringEnumConverter`, `UnitsNetIQuantityJsonConverter`, and `TypeNameHandling.Objects`. An uncertainty type implements the taxonomy-serialization interface instead, so the generic constraint rejects it at compile time rather than at round-trip.
- Consequence: a Materials property owner reads the typed `CentralValue`/`LowerBound`/`UpperBound`/uncertainty value off the wrapper and serializes through the canonical Rasm snapshot codec (`Thinktecture.Runtime.Extensions.Json`/`MessagePack`) at the boundary — it never routes an uncertainty value through either VividOrange taxonomy serializer, and never assumes the section/material serializer covers it.

[STACK]:

- units seam: the quantity family wraps `UnitsNet` `IQuantity` (`api-unitsnet.md`) on the SAME floor the section-property (`api-vividorange-sections-sectionproperties.md`) and material-grade (`api-vividorange-materials.md`) rows use — so a computed `AreaMomentOfInertia`, a measured `Pressure`, and its `±` band are one quantity type; `pressure.WithRelativeUncertainty` yields an `UncertaintyQuantity<Pressure>` whose bounds are `Pressure`.
- properties seam: the Materials `Properties/` shared `Published<T>` ingress carrier wraps `IUncertainty<T>` through `.Quantities.Utility.WithRelativeUncertainty` for a `UnitsNet` quantity or the double `.Utility` for a raw scalar. The carrier lowers the provider model to the neutral `MeasureBand` seam at the ONE mint, so a property crosses as value+band rather than a bare scalar and separate tolerance.
- propagation seam: the multi-ply rule-of-mixtures, series-resistance, and layered-STC folds live in `Rasm.Compute` and propagate bands through `UncertaintyQuantityOperators.Add` and `Multiply`. An aggregated assembly property therefore reports its propagated uncertainty.
- model-axis seam: the four MODELS (absolute/relative/interval/normal) × four CARRIERS (`double`/`INumber<T>`/`decimal`/`IQuantity`) collapse to ONE `IUncertainty<T>` propagation contract — a Materials property owner discriminates on the `UncertaintyModel` axis (or the carrier `T`), never a per-property uncertainty type; `decimal` carries cost rows, `IQuantity` the physical properties, `INumber<T>` the dimensionless ratios.

[RAIL_LAW]:

- Packages: `VividOrange.Uncertainties` + `VividOrange.Uncertainties.Quantities` + `VividOrange.IUncertainties` (MIT, pure-managed AnyCPU, `net10.0` binds `net8.0`, PRE-1.0 contract)
- Owns: the four uncertainty models over four numeric carriers, the fluent `.WithXUncertainty` admission, the bound-corner / quadrature propagation algebra, the unit-checked quantity arithmetic, and the `IUncertainty<T>` floor — all carried with the value through the Materials `Properties/` and `Profiles/` surfaces
- Accept: a measured/declared material property attached to its uncertainty via `.WithXUncertainty`, read as typed `UnitsNet` quantity bounds, propagated through the assembly-property folds; the value serialized via the canonical Rasm codec at the boundary
- Reject: a bare scalar + parallel tolerance field where `UncertaintyQuantity<T>` carries both; a per-property uncertainty type where the model×carrier axis collapses to one contract; mixing uncertainty models in arithmetic without lowering (the propagation throws); assuming the `VividOrange.Serialization` serializer round-trips these taxonomy types ([04]-[SERIALIZATION_BOUNDARY])

## [05]-[CATALOGUE_LAW]

[PACKAGE_SCOPE]:

- This page carries `VividOrange.Uncertainties[.Quantities]` + `VividOrange.IUncertainties` API facts only; the shared `Published<T>` ingress carrier, the catalogue rows, the model-axis discriminant, and the snapshot codec binding are owned at the Materials `Properties/` design pages.
- Units lane: the quantity carrier composes the in-folder `UnitsNet` owner (`api-unitsnet.md`); this page never re-documents the `UnitsNet` quantity surface.
- Serialization lane: the `VividOrange.Taxonomy.Serialization.ITaxonomySerializable` floor is DISTINCT from the `VividOrange.Serialization` floor the Sections/Materials/Profiles use; the two are not interchangeable, and the Materials owner serializes uncertainty values through the canonical Rasm codec ([04]-[SERIALIZATION_BOUNDARY]).
