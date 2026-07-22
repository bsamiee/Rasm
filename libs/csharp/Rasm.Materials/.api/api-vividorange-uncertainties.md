# [RASM_MATERIALS_API_VIVIDORANGE_UNCERTAINTIES]

`VividOrange.Uncertainties` owns uncertainty arithmetic for Materials `Properties/`: four models — absolute, relative, interval, normal — over four numeric carriers (`double`, `INumber<T>`, `decimal`, a `UnitsNet` `IQuantity`) collapse to one `IUncertainty<T>` propagation contract. A value admits its uncertainty fluently and crosses Properties and Profiles as value+unit+uncertainty, never a bare scalar beside a tolerance field. `VividOrange.IUncertainties` is the interface floor; the quantity carrier composes the shared `UnitsNet` owner (`libs/csharp/.api/api-unitsnet.md`).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `VividOrange.Uncertainties`
- package: `VividOrange.Uncertainties` (MIT, MagmaWorks / VividOrange taxonomy)
- assembly: `VividOrange.Uncertainties`
- namespace: `VividOrange.Uncertainties`, `double` family + `UncertaintyModel`, `.Scalar` (`INumber<T>` family), `.Decimal` (`decimal` family), `.Utility`
- dependency: `VividOrange.IUncertainties`
- asset: pure-managed AnyCPU runtime, no native RID; `net10.0` binds `lib/net8.0`
- rail: properties

[PACKAGE_SURFACE]: `VividOrange.Uncertainties.Quantities`
- package: `VividOrange.Uncertainties.Quantities` (MIT)
- assembly: `VividOrange.Uncertainties.Quantities`
- namespace: `VividOrange.Uncertainties` (`UncertaintyQuantity<T>`), `.Quantities`, `*UncertaintyQuantity<TQuantity>`, and `.Quantities.Utility`
- dependencies: `VividOrange.IUncertainties`, `UnitsNet` (shared quantity floor, `libs/csharp/.api/api-unitsnet.md`)
- asset: pure-managed AnyCPU runtime; `net10.0` binds `lib/net8.0`
- rail: properties (UnitsNet quantities)

[PACKAGE_SURFACE]: `VividOrange.IUncertainties`
- package: `VividOrange.IUncertainties` (MIT)
- assembly: `VividOrange.IUncertainties`
- namespace: `VividOrange.Uncertainties` (the interface set — the package id carries `I…`, the namespace does not)
- dependency: `VividOrange.Taxonomy.ISerialization` (the `ITaxonomySerializable` floor; transitive-only)
- asset: pure-managed AnyCPU runtime; `net10.0` binds `lib/net8.0`
- rail: properties (contract floor)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: interface floor — `VividOrange.IUncertainties`, namespace `VividOrange.Uncertainties`, one propagation contract and four model-specific carriers.

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY] | [CAPABILITY]                                                            |
| :-----: | :---------------------------------- | :------------ | :---------------------------------------------------------------------- |
|  [01]   | `IUncertainty<T>`                   | interface     | propagation contract; the bound reads, inherits `ITaxonomySerializable` |
|  [02]   | `IAbsoluteUncertainty<T>`           | interface     | adds `T AbsoluteUncertaintyValue`                                       |
|  [03]   | `IRelativeUncertainty<T>`           | interface     | adds `double RelativeUncertaintyValue`                                  |
|  [04]   | `IIntervalUncertainty<T>`           | interface     | carries the `[lo, hi]` bounds directly                                  |
|  [05]   | `INormalDistributionUncertainty<T>` | interface     | adds `T StandardDeviation` and `double CoverageFactor`                  |

[PUBLIC_TYPE_SCOPE]: `double` scalar family — `VividOrange.Uncertainties`, one wrapper, four bound models, and their discriminant.

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [CAPABILITY]                                                                     |
| :-----: | :------------------------------ | :------------ | :------------------------------------------------------------------------------- |
|  [01]   | `Uncertainty`                   | class         | wraps `IUncertainty<double>`; native operators and conversions to the four kinds |
|  [02]   | `AbsoluteUncertainty`           | class         | `(cv, u)` -> `[cv-u, cv+u]`                                                      |
|  [03]   | `RelativeUncertainty`           | class         | `(cv, r)` -> `[cv·(1-r), cv·(1+r)]`                                              |
|  [04]   | `IntervalUncertainty`           | class         | `(cv, boundStart, boundEnd)` -> auto-ordered `[lo, hi]`                          |
|  [05]   | `NormalDistributionUncertainty` | class         | `(mean, sd, k)` -> `[μ-k·σ, μ+k·σ]`                                              |
|  [06]   | `UncertaintyModel`              | enum          | kind discriminant: `Interval`, `Relative`, `Absolute`, `NormalDistribution`      |

[PUBLIC_TYPE_SCOPE]: generic and decimal families — `.Scalar` (`INumber<T>`) and `.Decimal` (`decimal`) mirror the four scalar models; `*` expands to the `Absolute`/`Relative`/`Interval`/`NormalDistribution` prefixes.

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [CAPABILITY]                                        |
| :-----: | :----------------------------- | :------------ | :-------------------------------------------------- |
|  [01]   | `Scalar.UncertaintyScalar<T>`  | class         | generic-numeric wrapper under `where T: INumber<T>` |
|  [02]   | `Scalar.*UncertaintyScalar<T>` | class         | the four models over `INumber<T>` values            |
|  [03]   | `Decimal.*UncertaintyDecimal`  | class         | the four models over `decimal`, carrying cost rows  |

[PUBLIC_TYPE_SCOPE]: quantity family — `VividOrange.Uncertainties.Quantities` binds a `UnitsNet` value+unit+uncertainty; the wrapper is `UncertaintyQuantity<T>` and the four `.Quantities` models are `*UncertaintyQuantity<TQuantity>`.

| [INDEX] | [SYMBOL]                                           | [TYPE_FAMILY] | [CAPABILITY]                                       |
| :-----: | :------------------------------------------------- | :------------ | :------------------------------------------------- |
|  [01]   | `UncertaintyQuantity<T>`                           | class         | value+unit+uncertainty under `where T: IQuantity`  |
|  [02]   | `AbsoluteUncertaintyQuantity<TQuantity>`           | class         | `(cv, u)` bounds via `IQuantity.Subtract`/`Add`    |
|  [03]   | `RelativeUncertaintyQuantity<TQuantity>`           | class         | `(cv, Ratio\|double)` bounds via `Multiply(1 ± r)` |
|  [04]   | `IntervalUncertaintyQuantity<TQuantity>`           | class         | `(cv, lo, hi)` auto-ordered quantity bounds        |
|  [05]   | `NormalDistributionUncertaintyQuantity<TQuantity>` | class         | `(mean, sd, k)` RSS on binary propagation          |

[PUBLIC_TYPE_SCOPE]: static utility — `.Utility` and `.Quantities.Utility` own fluent admission and arithmetic.

| [INDEX] | [SYMBOL]                                          | [TYPE_FAMILY] | [CAPABILITY]                                              |
| :-----: | :------------------------------------------------ | :------------ | :-------------------------------------------------------- |
|  [01]   | `Utility.DoubleExtensions`                        | class         | `(double).WithXUncertainty` admission                     |
|  [02]   | `Utility.GenericINumberExtensions`                | class         | `(T).WithXUncertainty<T>` admission under `INumber<T>`    |
|  [03]   | `Utility.DecimalExtensions`                       | class         | `(decimal).WithXUncertainty` admission                    |
|  [04]   | `Utility.UncertaintyOperators`                    | class         | arithmetic over `IUncertainty<double>`                    |
|  [05]   | `Utility.UncertaintyScalarOperators`              | class         | the same arithmetic over `UncertaintyScalar<T>`           |
|  [06]   | `Quantities.Utility.UncertaintyQuantityFactory`   | class         | `(TQuantity).WithXUncertainty<TQuantity>` admission       |
|  [07]   | `Quantities.Utility.UncertaintyQuantityOperators` | class         | unit-checked `Add`/`Subtract`, scalar `Multiply`/`Divide` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: fluent admission — `.WithXUncertainty` attaches uncertainty to a value and returns the typed model the operators and `IUncertainty<T>` bounds read; a `UnitsNet` quantity admits through `UncertaintyQuantityFactory`, a raw `double`/`decimal`/`INumber<T>` through the matching `.Utility` class.

| [INDEX] | [SURFACE]                                                    | [SHAPE] | [CAPABILITY]                                        |
| :-----: | :----------------------------------------------------------- | :------ | :-------------------------------------------------- |
|  [01]   | `(double).WithRelativeUncertainty(double)`                   | static  | `value ± r·value`                                   |
|  [02]   | `(double).WithAbsoluteUncertainty(double)`                   | static  | `value ± u`                                         |
|  [03]   | `(double).WithIntervalUncertainty(double, double)`           | static  | explicit `[lo, hi]`                                 |
|  [04]   | `(double).WithNormalDistributionUncertainty(double, double)` | static  | `μ ± k·σ`, `k` defaults `3.0`                       |
|  [05]   | `(TQuantity).WithRelativeUncertainty(Ratio\|double)`         | static  | -> `RelativeUncertaintyQuantity<TQuantity>`, `± r%` |
|  [06]   | `(TQuantity).WithAbsoluteUncertainty(TQuantity\|double)`     | static  | -> `AbsoluteUncertaintyQuantity<TQuantity>`, `± u`  |
|  [07]   | `(T).WithAbsoluteUncertainty(T)`                             | static  | -> `AbsoluteUncertaintyScalar<T>`, generic `± u`    |

[ENTRYPOINT_SCOPE]: wrapper construction — `UncertaintyQuantity<T>` ctors discriminate the model by argument shape, and named factories on the wrappers mirror them.

| [INDEX] | [SURFACE]                                                                | [SHAPE]  | [CAPABILITY]                                     |
| :-----: | :----------------------------------------------------------------------- | :------- | :----------------------------------------------- |
|  [01]   | `new UncertaintyQuantity<T>(T, T)`                                       | ctor     | absolute wrapper `(cv, u)`                       |
|  [02]   | `new UncertaintyQuantity<T>(T, Ratio\|double)`                           | ctor     | relative wrapper; the arg type selects the ctor  |
|  [03]   | `new UncertaintyQuantity<T>(T, T, T)`                                    | ctor     | interval wrapper `(cv, lo, hi)`                  |
|  [04]   | `new UncertaintyQuantity<T>(T, T, double)`                               | ctor     | normal wrapper `(cv, sd, k)`, `k` defaults `3.0` |
|  [05]   | `UncertaintyQuantity<T>.FromNormalDistributionUncertainty(T, T, double)` | factory  | named mirror of the normal ctor                  |
|  [06]   | `Uncertainty.FromIntervalUncertainty(double, double, double)`            | factory  | one of the four `double` named factories         |
|  [07]   | `(IntervalUncertaintyQuantity<T>)wrapper`                                | operator | explicit unwrap; throws on model mismatch        |

[ENTRYPOINT_SCOPE]: propagation and arithmetic over `UncertaintyQuantity<T>`, `IUncertainty<double>`, and `UncertaintyScalar<T>` operands — `Add`/`Subtract` require the same model kind (else `InvalidOperationException "Incompatible uncertainty model."`) and, for quantities, unit agreement (else `"Incompatible units."`); `Multiply`/`Divide` scale by a `double`.

| [INDEX] | [SURFACE]                                                       | [SHAPE]  | [CAPABILITY]                                         |
| :-----: | :-------------------------------------------------------------- | :------- | :--------------------------------------------------- |
|  [01]   | `UncertaintyQuantityOperators.Add`                              | static   | quantity + quantity, unit-checked, bounds propagated |
|  [02]   | `UncertaintyQuantityOperators.Multiply(double)`                 | static   | scales value and uncertainty                         |
|  [03]   | `UncertaintyOperators.Add`                                      | static   | `double` model sum                                   |
|  [04]   | `UncertaintyScalarOperators.Subtract`                           | static   | generic-numeric difference                           |
|  [05]   | `Uncertainty + Uncertainty`, `Uncertainty * double`             | operator | native `double` wrapper operators                    |
|  [06]   | `IUncertainty<T>.PropagateBinary(IUncertainty<T>, Func<T,T,T>)` | instance | custom binary op with bound propagation              |
|  [07]   | `IUncertainty<T>.PropagateUnary(Func<double,double>)`           | instance | custom unary op scaling uncertainty by `\|f(1)\|`    |
|  [08]   | `IUncertainty<T>.CentralValue` / `.LowerBound` / `.UpperBound`  | property | the `T` value and propagated bounds                  |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `PropagateBinary` is model-homogeneous: it casts `other` to the same kind interface and throws `InvalidOperationException "Incompatible uncertainty model."` on a mismatch, so an absolute never combines with a normal without lowering one first.
- Absolute, relative, and interval kinds propagate over the four bound corners (`op(lo_a,lo_b)`, `op(lo_a,hi_b)`, `op(hi_a,lo_b)`, `op(hi_a,hi_b)`) and return an interval `(op(cv_a,cv_b), min, max)` — exact interval arithmetic, never a linearized error term; `PropagateUnary` applies the op to the central value and scales uncertainty by `|op|` or maps the bounds.
- Normal-kind binary propagation combines in quadrature: `σ = sqrt(σ_a² + σ_b²)`, preserving `CoverageFactor`.
- Quantity arithmetic works in the central value's unit via `IQuantity.As(unit)` and `Quantity.From(value, unit)`, so `Pressure ± Pressure` stays in the left operand's unit and a unit mismatch is caught by `QuantityInfo.BaseUnitInfo.QuantityName` before the operation.

[STACKING]:
- `api-unitsnet.md`(`libs/csharp/.api/api-unitsnet.md`): the quantity family wraps `UnitsNet` `IQuantity` on the same floor the section-property and material-grade rows use, so a measured `Pressure` and its `±` band are one type — `pressure.WithRelativeUncertainty` yields `UncertaintyQuantity<Pressure>` whose bounds are `Pressure`.
- `api-vividorange-serialization.md`(`.api/api-vividorange-serialization.md`): the uncertainty types carry the `VividOrange.Taxonomy.Serialization` marker, distinct from the `VividOrange.Serialization` marker that catalog's `ToJson<T>`/`FromJson<T>` constrains; the serializer settings stack and the two-assembly marker split are owned there.
- Materials `Properties/`: the shared `Published<T>` ingress wraps `IUncertainty<T>` through `.Quantities.Utility` for a quantity or `.Utility` for a raw scalar, lowering the provider model to the neutral `MeasureBand` seam at the one mint so a property crosses as value+band.
- `Rasm.Compute`: multi-ply rule-of-mixtures, series-resistance, and layered-STC folds propagate bands through `UncertaintyQuantityOperators.Add` and `Multiply`, so an aggregated assembly property reports its propagated uncertainty.

[LOCAL_ADMISSION]:
- A property owner admits a measured or declared value through `.WithXUncertainty`, reads it as typed `UnitsNet` quantity bounds, and propagates it through the assembly-property folds.
- Uncertainty types carry the taxonomy-serialization marker, so `VividOrange.Serialization.ToJson<T>`/`FromJson<T>` reject them at compile time; the property owner reads the typed `CentralValue`/`LowerBound`/`UpperBound` off the wrapper and serializes through the canonical Rasm snapshot codec (`Thinktecture.Runtime.Extensions.Json`/`MessagePack`), never through a VividOrange serializer.

[RAIL_LAW]:
- Package: `VividOrange.Uncertainties`, `VividOrange.Uncertainties.Quantities`, `VividOrange.IUncertainties` (MIT, pure-managed AnyCPU, `net10.0` binds `net8.0`)
- Owns: the four uncertainty models over four numeric carriers, the fluent `.WithXUncertainty` admission, the bound-corner and quadrature propagation algebra, the unit-checked quantity arithmetic, and the `IUncertainty<T>` floor, all carried with the value through the Materials `Properties/` and `Profiles/` surfaces.
- Accept: a material property attached to its uncertainty via `.WithXUncertainty`, read as typed quantity bounds, propagated through the assembly-property folds, and serialized via the canonical Rasm codec at the boundary.
- Reject: a bare scalar beside a parallel tolerance field where `UncertaintyQuantity<T>` carries both; a per-property uncertainty type where the model×carrier axis collapses to one contract; mixing uncertainty models in arithmetic without lowering; routing an uncertainty value through a VividOrange serializer.
