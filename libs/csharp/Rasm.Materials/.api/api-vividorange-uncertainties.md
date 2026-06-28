# [RASM_MATERIALS_API_VIVIDORANGE_UNCERTAINTIES]

`VividOrange.Uncertainties` supplies measured-value uncertainty arithmetic for the Materials `Properties/` engineering-property rows: four uncertainty MODELS (absolute `3.14 ± 0.01`, relative `±2%`, interval `[lo, hi]`, normal `mean ± k·sd`) over four NUMERIC carriers — a `double` scalar (`Uncertainty`), a generic `INumber<T>` scalar (`Scalar.UncertaintyScalar<T>`), a `decimal` (`Decimal.*`), and — via `VividOrange.Uncertainties.Quantities` — a `UnitsNet` `IQuantity` (`UncertaintyQuantity<T>`). A property value is admitted fluently (`5.0.WithRelativeUncertainty(0.02)`, `pressure.WithAbsoluteUncertainty(...)`), propagated through one shared bound-corner algebra (`+`/`-`/`*`/`/`, unit-checked for quantities), and carried with its value+unit+uncertainty TOGETHER through the Properties and Profiles surfaces instead of an ad hoc tolerance field. `VividOrange.IUncertainties` is the interface floor; the package composes the SAME in-folder `UnitsNet` `5.75.0` quantity owner (`api-unitsnet.md`) the section-property and material-grade rows use. Its `ITaxonomySerializable` is the NEWER `VividOrange.Taxonomy.Serialization` contract — DISTINCT from the `0.1.0` `VividOrange.Serialization.ITaxonomySerializable` the Sections/Materials/Profiles packages ride (see [04]-[SERIALIZATION_BOUNDARY]); the Materials owner reads the typed value and serializes through the canonical Rasm snapshot codec, never assuming one shared VividOrange serializer.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `VividOrange.Uncertainties`
- package: `VividOrange.Uncertainties`
- version: `0.2.0`
- license: MIT (`licenses.nuget.org/MIT` — MagmaWorks / VividOrange taxonomy; `github.com/VividOrange/Uncertainties`)
- assembly: `VividOrange.Uncertainties`
- namespace: `VividOrange.Uncertainties` (the `double` family + `UncertaintyModel`), `.Scalar` (the `INumber<T>` family), `.Decimal` (the `decimal` family), `.Utility` (admission + operators)
- dependency: `VividOrange.IUncertainties` `0.2.0` (the interface floor)
- target frameworks: `net8.0`, `net7.0`, `net6.0`, `net48`
- asset: runtime library, pure-managed AnyCPU, NO native RID asset. The `net10.0` consumer binds the highest asset `lib/net8.0`.
- rail: properties

[PACKAGE_SURFACE]: `VividOrange.Uncertainties.Quantities`
- package: `VividOrange.Uncertainties.Quantities`
- version: `0.2.0`
- license: MIT (`licenses.nuget.org/MIT`)
- assembly: `VividOrange.Uncertainties.Quantities`
- namespace: `VividOrange.Uncertainties` (`UncertaintyQuantity<T>`), `.Quantities` (the four `*UncertaintyQuantity<TQuantity>` kinds), `.Quantities.Utility` (factory + operators)
- dependencies: `VividOrange.IUncertainties` `0.2.0`; `UnitsNet` `5.75.0` (the shared in-folder quantity floor, `api-unitsnet.md`)
- target frameworks: `net8.0`, `net7.0`, `net6.0`, `net48`
- asset: runtime library, pure-managed AnyCPU; `net10.0` binds `lib/net8.0`
- rail: properties (UnitsNet quantities)

[PACKAGE_SURFACE]: `VividOrange.IUncertainties`
- package: `VividOrange.IUncertainties`
- version: `0.2.0`
- license: MIT (`licenses.nuget.org/MIT`; `github.com/VividOrange/Taxonomy`)
- assembly: `VividOrange.IUncertainties`
- namespace: `VividOrange.Uncertainties` (the interface set — the package id is `I…`, the namespace is NOT)
- dependency: `VividOrange.Taxonomy.ISerialization` `0.2.0` (the `ITaxonomySerializable` floor; transitive-only)
- target frameworks: `net8.0`, `net7.0`, `net6.0`, `net48`
- asset: runtime library, pure-managed AnyCPU; `net10.0` binds `lib/net8.0`
- rail: properties (contract floor)
- ABI floor: a `0.2.0` PRE-1.0 contract — the `IUncertainty<T>` member set may break across a minor bump.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: interface floor (`VividOrange.IUncertainties`, namespace `VividOrange.Uncertainties`)
- rail: properties

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY] | [CAPABILITY]                                                                              |
| :-----: | :---------------------------------- | :------------ | :--------------------------------------------------------------------------------------- |
|  [01]   | `IUncertainty<T>`                   | contract      | `: ITaxonomySerializable` — `CentralValue`/`LowerBound`/`UpperBound` (all `T`); `PropagateBinary(IUncertainty<T> other, Func<T,T,T> op)`; `PropagateUnary(Func<double,double> op)` |
|  [02]   | `IAbsoluteUncertainty<T>`           | contract      | `: IUncertainty<T>` + `T AbsoluteUncertaintyValue`                                         |
|  [03]   | `IRelativeUncertainty<T>`           | contract      | `: IUncertainty<T>` + `double RelativeUncertaintyValue`                                    |
|  [04]   | `IIntervalUncertainty<T>`           | contract      | `: IUncertainty<T>` — bounds-only (`LowerBound`/`UpperBound` are the carrier)              |
|  [05]   | `INormalDistributionUncertainty<T>` | contract      | `: IUncertainty<T>` + `T StandardDeviation` + `double CoverageFactor`                      |

[PUBLIC_TYPE_SCOPE]: `double` scalar family (`VividOrange.Uncertainties`)
- rail: properties

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [CAPABILITY]                                                                                  |
| :-----: | :-------------------------------- | :------------ | :------------------------------------------------------------------------------------------- |
|  [01]   | `Uncertainty`                     | wrapper       | wraps `IUncertainty<double> UncertaintyModel`; operators `+`/`-`/`*`(double)/`/`(double); implicit-from / explicit-to the four kinds; `From{Absolute,Relative,Interval,NormalDistribution}Uncertainty` factories |
|  [02]   | `AbsoluteUncertainty`             | model         | `(centralValue, uncertainty)` — `[cv-u, cv+u]`                                                |
|  [03]   | `RelativeUncertainty`             | model         | `(centralValue, relativeUncertainty)` — `[cv·(1-r), cv·(1+r)]`                                |
|  [04]   | `IntervalUncertainty`             | model         | `(centralValue, boundStart, boundEnd)` — explicit `[lo, hi]` (auto-ordered)                   |
|  [05]   | `NormalDistributionUncertainty`   | model         | `(mean, standardDeviation, coverageFactor = 3.0)` — `[μ-k·σ, μ+k·σ]`                          |
|  [06]   | `UncertaintyModel`                | enum          | `Interval` / `Relative` / `Absolute` / `NormalDistribution` — the kind discriminant            |

[PUBLIC_TYPE_SCOPE]: generic `INumber<T>` scalar family (`.Scalar`) and `decimal` family (`.Decimal`)
- rail: properties

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY] | [CAPABILITY]                                                                  |
| :-----: | :---------------------------------------- | :------------ | :--------------------------------------------------------------------------- |
|  [01]   | `Scalar.UncertaintyScalar<T>`             | wrapper       | `where T : INumber<T>` — the generic-numeric wrapper (the `double`/`decimal` peers' generic form) |
|  [02]   | `Scalar.{Absolute,Relative,Interval,NormalDistribution}UncertaintyScalar<T>` | model | the four `INumber<T>` models                                  |
|  [03]   | `Decimal.{Absolute,Relative,Interval,NormalDistribution}UncertaintyDecimal` | model | the four `decimal` models (financial/precision cost rows)        |

[PUBLIC_TYPE_SCOPE]: `UnitsNet` quantity family (`VividOrange.Uncertainties.Quantities`)
- rail: properties

| [INDEX] | [SYMBOL]                                       | [TYPE_FAMILY] | [CAPABILITY]                                                                                 |
| :-----: | :--------------------------------------------- | :------------ | :----------------------------------------------------------------------------------------- |
|  [01]   | `UncertaintyQuantity<T>`                       | wrapper       | `where T : IQuantity` (namespace `VividOrange.Uncertainties`) — value+unit+uncertainty together; implicit-from / explicit-to the four kinds; `From…` factories |
|  [02]   | `Quantities.AbsoluteUncertaintyQuantity<TQuantity>` | model   | `(centralValue, uncertainty)` — bounds via `IQuantity.Subtract`/`Add`                        |
|  [03]   | `Quantities.RelativeUncertaintyQuantity<TQuantity>` | model   | `(centralValue, Ratio|double uncertainty)` — bounds via `Multiply(1 ± r)`                    |
|  [04]   | `Quantities.IntervalUncertaintyQuantity<TQuantity>` | model   | `(centralValue, lowerBound, upperBound)` — auto-ordered quantity bounds                      |
|  [05]   | `Quantities.NormalDistributionUncertaintyQuantity<TQuantity>` | model | `(mean, standardDeviation, coverageFactor = 3.0)` — RSS combination on binary propagate |

[PUBLIC_TYPE_SCOPE]: admission + arithmetic (`.Utility`, `.Quantities.Utility`)
- rail: properties

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY]   | [CAPABILITY]                                                                       |
| :-----: | :---------------------------------------- | :-------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `Utility.DoubleExtensions`                | static (fluent) | `(this double).With{Absolute,Relative,Interval,NormalDistribution}Uncertainty(...)` -> a `double` model |
|  [02]   | `Utility.GenericINumberExtensions`        | static (fluent) | `(this T).WithX<T>(...) where T : INumber<T>` -> a `Scalar.*UncertaintyScalar<T>`   |
|  [03]   | `Utility.DecimalExtensions`               | static (fluent) | `(this decimal).WithX(...)` -> a `Decimal.*UncertaintyDecimal`                      |
|  [04]   | `Utility.UncertaintyOperators`            | static (math)   | `Add`/`Subtract`/`Multiply`(double)/`Divide`(double) over `IUncertainty<double>`    |
|  [05]   | `Utility.UncertaintyScalarOperators`      | static (math)   | `Add`/`Subtract`/`Multiply`(double)/`Divide`(double) over `UncertaintyScalar<T>` (`T : INumber<T>`) |
|  [06]   | `Quantities.Utility.UncertaintyQuantityFactory` | static (fluent) | `(this TQuantity).WithX<TQuantity>(...) where TQuantity : IQuantity` -> a `*UncertaintyQuantity<TQuantity>` |
|  [07]   | `Quantities.Utility.UncertaintyQuantityOperators` | static (math) | `Add`/`Subtract` (unit-checked, throws `"Incompatible units."`) / `Multiply`(double) / `Divide`(double) over `UncertaintyQuantity<T>` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: fluent admission — attach uncertainty to a value
- rail: properties
- composition law: the `.WithXUncertainty` extension is the primary entry; it returns the typed model the operators and `IUncertainty<T>` bounds read. A `UnitsNet` quantity (`Pressure`/`Density`/…) admits via `UncertaintyQuantityFactory`; a raw `double`/`decimal`/`INumber<T>` via the matching `.Utility` class.

| [INDEX] | [SURFACE]                                          | [CALL_SHAPE]                                                                                     | [CAPABILITY]                                  |
| :-----: | :------------------------------------------------- | :--------------------------------------------------------------------------------------------- | :------------------------------------------- |
|  [01]   | `(double).WithRelativeUncertainty`                 | `RelativeUncertainty WithRelativeUncertainty(this double value, double relativeUncertainty)`     | `value ± r·value`                             |
|  [02]   | `(double).WithAbsoluteUncertainty`                 | `AbsoluteUncertainty WithAbsoluteUncertainty(this double value, double absoluteUncertainty)`     | `value ± u`                                   |
|  [03]   | `(double).WithIntervalUncertainty`                 | `IntervalUncertainty WithIntervalUncertainty(this double value, double lowerBound, double upperBound)` | explicit `[lo, hi]`                     |
|  [04]   | `(double).WithNormalDistributionUncertainty`       | `NormalDistributionUncertainty WithNormalDistributionUncertainty(this double value, double standardDeviation, double coverageFactor = 3.0)` | `μ ± k·σ` |
|  [05]   | `(TQuantity).WithRelativeUncertainty`              | `RelativeUncertaintyQuantity<TQuantity> WithRelativeUncertainty<TQuantity>(this TQuantity quantity, Ratio|double relativeUncertainty) where TQuantity : IQuantity` | a `UnitsNet` value `± r%` |
|  [06]   | `(TQuantity).WithAbsoluteUncertainty`              | `AbsoluteUncertaintyQuantity<TQuantity> WithAbsoluteUncertainty<TQuantity>(this TQuantity quantity, TQuantity|double absoluteUncertainty) where TQuantity : IQuantity` | a `UnitsNet` value `± u` (same-unit or double) |
|  [07]   | `(T).WithAbsoluteUncertainty<T>`                   | `AbsoluteUncertaintyScalar<T> WithAbsoluteUncertainty<T>(this T value, T absoluteUncertainty) where T : INumber<T>` | generic-numeric `value ± u`        |

[ENTRYPOINT_SCOPE]: wrapper construction + factories
- rail: properties

| [INDEX] | [SURFACE]                                       | [CALL_SHAPE]                                                                                  | [CAPABILITY]                                 |
| :-----: | :---------------------------------------------- | :------------------------------------------------------------------------------------------ | :------------------------------------------ |
|  [01]   | `new UncertaintyQuantity<T>(cv, uncertainty)`   | `UncertaintyQuantity(T centralValue, T absoluteUncertainty)`                                  | absolute quantity wrapper                    |
|  [02]   | `new UncertaintyQuantity<T>(cv, Ratio|double)`  | `UncertaintyQuantity(T centralValue, Ratio relativeUncertainty)` / `(T, double)`             | relative quantity wrapper (ctor discriminates by arg type) |
|  [03]   | `new UncertaintyQuantity<T>(cv, lo, hi)`        | `UncertaintyQuantity(T centralValue, T lowerBound, T upperBound)`                             | interval quantity wrapper                    |
|  [04]   | `new UncertaintyQuantity<T>(cv, sd, k)`         | `UncertaintyQuantity(T centralValue, T standardDeviation, double coverageFactor = 3.0)`       | normal quantity wrapper                      |
|  [05]   | `UncertaintyQuantity<T>.FromNormalDistributionUncertainty` | `static UncertaintyQuantity<T> FromNormalDistributionUncertainty(T cv, T sd, double coverageFactor = 3.0)` | named-factory mirror of the ctors |
|  [06]   | `Uncertainty.FromIntervalUncertainty`           | `static Uncertainty FromIntervalUncertainty(double cv, double lo, double hi)`                 | `double` named factory (the four `From…` mirror the ctors) |
|  [07]   | `(IntervalUncertaintyQuantity<T>)quantityWrapper` | explicit conversion                                                                          | unwrap to the concrete kind (throws on model mismatch) |

[ENTRYPOINT_SCOPE]: propagation + arithmetic
- rail: properties
- composition law: arithmetic propagates uncertainty through the bound algebra; `Add`/`Subtract` require the SAME model kind (else `InvalidOperationException "Incompatible uncertainty model."`), and the quantity `Add`/`Subtract` additionally require unit compatibility (else `"Incompatible units."`). `Multiply`/`Divide` scale by a `double`.

| [INDEX] | [SURFACE]                                       | [CALL_SHAPE]                                                                                              | [CAPABILITY]                                 |
| :-----: | :---------------------------------------------- | :------------------------------------------------------------------------------------------------------ | :------------------------------------------ |
|  [01]   | `UncertaintyQuantityOperators.Add`             | `UncertaintyQuantity<T> Add<T>(this UncertaintyQuantity<T> a, UncertaintyQuantity<T> b) where T : IQuantity` | unit-checked quantity sum with propagated bounds |
|  [02]   | `UncertaintyQuantityOperators.Multiply`        | `UncertaintyQuantity<T> Multiply<T>(this UncertaintyQuantity<T> a, double factor) where T : IQuantity`    | scale value + uncertainty by a scalar         |
|  [03]   | `UncertaintyOperators.Add`                     | `Uncertainty Add<T>(this IUncertainty<double> a, T b) where T : IUncertainty<double>`                     | `double` model sum                           |
|  [04]   | `UncertaintyScalarOperators.Subtract`          | `IUncertainty<T> Subtract<T>(this UncertaintyScalar<T> a, UncertaintyScalar<T> b) where T : INumber<T>`   | generic-numeric difference                    |
|  [05]   | `a + b` / `a * factor`                          | `static Uncertainty operator +(Uncertainty, Uncertainty)` / `operator *(Uncertainty, double)`            | the `double` wrapper's native operators       |
|  [06]   | `model.PropagateBinary`                         | `IUncertainty<T> PropagateBinary(IUncertainty<T> other, Func<T,T,T> operation)`                           | custom binary op with bound propagation       |
|  [07]   | `model.PropagateUnary`                          | `IUncertainty<T> PropagateUnary(Func<double,double> operation)`                                           | custom unary op (scales uncertainty by `|f(1)|`) |
|  [08]   | `model.CentralValue` / `.LowerBound` / `.UpperBound` | property (`T`)                                                                                      | read the value and the propagated bounds      |

## [04]-[IMPLEMENTATION_LAW]

[PROPAGATION_ALGEBRA]:
- `PropagateBinary` is model-homogeneous: it casts `other` to the SAME kind interface (`IAbsoluteUncertainty<T>`/`IIntervalUncertainty<T>`/`IRelativeUncertainty<T>`/`INormalDistributionUncertainty<T>`) and throws `InvalidOperationException "Incompatible uncertainty model."` on a mismatch — you cannot add an absolute to a normal without lowering one first.
- For the absolute/relative/interval kinds, binary propagation evaluates the operation over the FOUR bound corners (`op(lo_a, lo_b)`, `op(lo_a, hi_b)`, `op(hi_a, lo_b)`, `op(hi_a, hi_b)`) and returns an `IntervalUncertaintyQuantity`/`IntervalUncertainty` of `(op(cv_a, cv_b), min(corners), max(corners))` — exact interval arithmetic, not a linearized error term.
- For the normal kind, binary propagation combines in QUADRATURE (root-sum-square): `σ = sqrt(σ_a² + σ_b²)`, preserving the `CoverageFactor`. `PropagateUnary` applies the operation to the central value and scales the uncertainty by `|operation(1.0)|` (absolute/normal) or maps the bounds (interval).
- The quantity arithmetic works in the central value's unit: bounds and quadrature compute via `IQuantity.As(unit)` / `Quantity.From(value, unit)`, so a `Pressure ± Pressure` stays in the left operand's unit and a unit mismatch is caught by `QuantityInfo.BaseUnitInfo.QuantityName` before the operation.

[SERIALIZATION_BOUNDARY]:
- The uncertainty types implement `VividOrange.Taxonomy.Serialization.ITaxonomySerializable` (from the `0.2.0` `VividOrange.Taxonomy.ISerialization` assembly). This is a DISTINCT interface from `VividOrange.Serialization.ITaxonomySerializable` (the `0.1.0` `VividOrange.ISerialization` assembly) that the Sections/Materials/Profiles packages implement — different namespace, different assembly, different type identity. The 0.1.0 PUBLIC serializer entry — `VividOrange.Serialization.JsonSerializationExtensions.ToJson<T>`/`FromJson<T>`, both `where T : VividOrange.Serialization.ITaxonomySerializable` (defaulting to the internal `TaxonomyJsonSerializer.Settings`: `StringEnumConverter` + `UnitsNetIQuantityJsonConverter`, `TypeNameHandling.Objects`) — CANNOT accept a `0.2.0` uncertainty type at all: it implements the `VividOrange.Taxonomy.Serialization` `ITaxonomySerializable`, so it fails the 0.1.0 generic constraint at COMPILE time, not at round-trip. The two `ITaxonomySerializable` are NOT interchangeable.
- Consequence: a Materials property owner reads the typed `CentralValue`/`LowerBound`/`UpperBound`/uncertainty value off the wrapper and serializes through the canonical Rasm snapshot codec (`Thinktecture.Runtime.Extensions.Json`/`MessagePack`) at the boundary — it never routes an uncertainty value through either VividOrange taxonomy serializer, and never assumes the section/material serializer covers it.

[STACK]:
- units seam: the quantity family wraps `UnitsNet` `IQuantity` (`api-unitsnet.md`) on the SAME `5.75.0` floor the section-property (`api-vividorange-sections-sectionproperties.md`) and material-grade (`api-vividorange-materials.md`) rows use — so a computed `AreaMomentOfInertia`, a measured `Pressure`, and its `±` band are one quantity type; `pressure.WithRelativeUncertainty(0.05)` yields an `UncertaintyQuantity<Pressure>` whose bounds are `Pressure`.
- properties seam: a `MaterialProperty` (mechanical/thermal/acoustic/fire/carbon/cost) carries its measurement band as an `UncertaintyQuantity<T>` instead of a bare scalar + separate tolerance; the `AssemblyProperty` rule-of-mixtures / series-resistance / layered-STC folds propagate the band through `UncertaintyQuantityOperators.Add`/`Multiply`, so a composite assembly's aggregated property reports its propagated uncertainty, not a discarded one.
- model-axis seam: the four MODELS (absolute/relative/interval/normal) × four CARRIERS (`double`/`INumber<T>`/`decimal`/`IQuantity`) collapse to ONE `IUncertainty<T>` propagation contract — a Materials property owner discriminates on the `UncertaintyModel` axis (or the carrier `T`), never a per-property uncertainty type; `decimal` carries cost rows, `IQuantity` the physical properties, `INumber<T>` the dimensionless ratios.

[RAIL_LAW]:
- Packages: `VividOrange.Uncertainties` + `VividOrange.Uncertainties.Quantities` + `VividOrange.IUncertainties` `0.2.0` (MIT, pure-managed AnyCPU, `net10.0` binds `net8.0`, PRE-1.0 contract)
- Owns: the four uncertainty models over four numeric carriers, the fluent `.WithXUncertainty` admission, the bound-corner / quadrature propagation algebra, the unit-checked quantity arithmetic, and the `IUncertainty<T>` floor — all carried with the value through the Materials `Properties/` and `Profiles/` surfaces
- Accept: a measured/declared material property attached to its uncertainty via `.WithXUncertainty`, read as typed `UnitsNet` quantity bounds, propagated through the assembly-property folds; the value serialized via the canonical Rasm codec at the boundary
- Reject: a bare scalar + parallel tolerance field where `UncertaintyQuantity<T>` carries both; a per-property uncertainty type where the model×carrier axis collapses to one contract; mixing uncertainty models in arithmetic without lowering (the propagation throws); assuming the `VividOrange.Serialization` `0.1.0` serializer round-trips these `0.2.0` taxonomy types ([04]-[SERIALIZATION_BOUNDARY])

## [05]-[CATALOGUE_LAW]

[PACKAGE_SCOPE]:
- This page carries `VividOrange.Uncertainties[.Quantities]` + `VividOrange.IUncertainties` API facts only; the `MaterialProperty`/`AssemblyProperty` rows, the model-axis discriminant, and the snapshot codec binding are owned at the Materials `Properties/` design pages.
- Units lane: the quantity carrier composes the in-folder `UnitsNet` `5.75.0` owner (`api-unitsnet.md`); this page never re-documents the `UnitsNet` quantity surface.
- Serialization lane: the `0.2.0` `VividOrange.Taxonomy.Serialization.ITaxonomySerializable` floor is DISTINCT from the `0.1.0` `VividOrange.Serialization` floor the Sections/Materials/Profiles use; the two are not interchangeable, and the Materials owner serializes uncertainty values through the canonical Rasm codec ([04]-[SERIALIZATION_BOUNDARY]).
