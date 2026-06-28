# [RASM_BIM_API_VIVIDORANGE_LOADS]

`VividOrange.Loads` (concrete) over `VividOrange.ILoads` (contract) is the host-neutral
structural-load VALUE taxonomy: the closed family of idealized actions a structural member
carries — point force/moment/displacement, line force, area force, column load, and gravity —
each modeled as an immutable `ILoad` whose components are `UnitsNet` dimensioned quantities,
not bare `double`. Every load is `ILoad : ITaxonomySerializable` exposing a `string Label` and
a single `ILoad Factor(Ratio)` scaling combinator, so a partial-safety-factor or combination
multiplier is applied by `load.Factor(γ)` rather than by hand-multiplying force scalars — the
algebra the `Cases` `ENCombinationFactory`/`Combinations.Utility` fold composes when it factors
a load case into a design combination. The 2D/3D split is interface inheritance: `IPointForce2d`
(`Force X`, `Force Z`) is the in-plane base and `IPointForce` adds `Force Y`, so a planar model
and a spatial model share one load record discriminated by which interface a consumer reads.
This is the load-value counterpart to the `Model/structural#ANALYSIS_MODEL` `LoadGroup` record:
the `AnalysisModel` graph owns load-group topology by GlobalId, while the typed `ILoad` carries
the applied action quantity the Compute solver evaluates. The package STACKS directly on the
`UnitsNet` rail the `.api/api-unitsnet` `QuantitySet` owner already governs — `Force`, `Torque`,
`Pressure`, `ForcePerLength`, `Length`, `Ratio` — so a load quantity coerces to SI-base through
the same `ToUnit(UnitSystem.SI)` path, and it is QUANTITY-ISOMORPHIC to the SAF load model
(`.api/api-structuralanalysisformat`): `PointForce`↔`ExcelStructuralPointAction<Force>`,
`LineForce`↔`ExcelStructuralCurveAction<ForcePerLength>`, `AreaForce`↔`ExcelStructuralSurfaceAction`,
`PointMoment`↔`ExcelStructuralPointMoment`, `PointDisplacement`↔`ExcelStructuralPointSupportDeformation`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `VividOrange.Loads`
- package: `VividOrange.Loads` (concrete implementations), `VividOrange.ILoads` (contracts + enum)
- version: `0.1.0`
- license: MIT
- assembly: `VividOrange.Loads`, `VividOrange.ILoads`
- namespace: `VividOrange.Loads` (both contract and impl share the one namespace)
- asset: multi-target `net48`/`net6.0`/`net7.0`/`net8.0`/`netstandard2.0`; the `net10.0` consumer binds `lib/net8.0` (the highest .NET asset, identical public surface across the .NET TFMs)
- asset: pure-managed AnyCPU IL-only assemblies; no native binaries, no satellite resources; ALC-safe inside the in-Rhino plugin assembly
- dependency: `VividOrange.ILoads` → `UnitsNet` (the quantity payload) + `VividOrange.ISerialization` (the `ITaxonomySerializable` marker); `VividOrange.Loads` → `VividOrange.ILoads`
- rail: structural-load

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: load contract family (`VividOrange.ILoads`)
- rail: structural-load

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]              | [RAIL]                                                    |
| :-----: | :--------------------- | :------------------------- | :------------------------------------------------------- |
|  [01]   | `ILoad`                | base load contract         | `ITaxonomySerializable` + `Label` + `Factor(Ratio)`      |
|  [02]   | `IPointForce2d`        | planar point force         | `Force X`, `Force Z`                                      |
|  [03]   | `IPointForce`          | spatial point force        | `: IPointForce2d` + `Force Y`                             |
|  [04]   | `IPointMoment2d`       | planar point moment        | `Torque Yy`, `Torque Zz`                                 |
|  [05]   | `IPointMoment`         | spatial point moment       | `: IPointMoment2d` + `Torque Xx`                         |
|  [06]   | `IPointDisplacement2d` | planar prescribed disp.    | `Length X`, `Length Z`                                   |
|  [07]   | `IPointDisplacement`   | spatial prescribed disp.   | `: IPointDisplacement2d` + `Length Y`                    |
|  [08]   | `ILineForce2d`         | planar line force          | `ForcePerLength X`, `ForcePerLength Z`, `LoadApplication` |
|  [09]   | `ILineForce`           | spatial line force         | `: ILineForce2d` + `ForcePerLength Y`                    |
|  [10]   | `IAreaForce`           | surface pressure           | `Pressure X/Y/Z` + `LoadApplication Application`         |
|  [11]   | `IColumnLoad`          | column head/base action    | `Force Force` + `IPointMoment2d TopMoment`/`BottomMoment` |
|  [12]   | `IGravity2d`           | planar gravity multiplier  | `Ratio X`, `Ratio Z`                                     |
|  [13]   | `IGravity`             | spatial gravity multiplier | `: IGravity2d` + `Ratio Y`                               |
|  [14]   | `LoadApplication`      | projection-frame enum      | `Local` / `Global` / `Projected`                         |

[PUBLIC_TYPE_SCOPE]: concrete load family (`VividOrange.Loads`)
- rail: structural-load

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]            | [RAIL]                                            |
| :-----: | :--------------------- | :----------------------- | :----------------------------------------------- |
|  [01]   | `PointForce2d`/`PointForce`             | point force      | `Force` components; `implicit operator` from `Force` |
|  [02]   | `PointMoment2d`/`PointMoment`           | point moment     | `Torque` components                              |
|  [03]   | `PointDisplacement2d`/`PointDisplacement` | prescribed disp. | `Length` components                              |
|  [04]   | `LineForce2d`/`LineForce`               | line force       | `ForcePerLength` components + `Application`       |
|  [05]   | `AreaForce`                             | surface pressure | `Pressure` components + `Application`             |
|  [06]   | `ColumnLoad`                            | column action    | `Force` axial + top/bottom `IPointMoment2d`       |
|  [07]   | `Gravity2d`/`Gravity`                   | gravity field    | `Ratio` g-multiplier components                  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: typed construction and scaling
- rail: structural-load

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY]      | [RAIL]                                                  |
| :-----: | :---------------------------------------------- | :------------------ | :----------------------------------------------------- |
|  [01]   | `new PointForce(Force x, Force y, Force z)`      | quantity ctor       | spatial point force from typed `Force` components       |
|  [02]   | `new LineForce(...)` / `new AreaForce(...)`      | quantity ctor       | line/area action from `ForcePerLength`/`Pressure`       |
|  [03]   | `new Gravity(Ratio z)` / `new Gravity()`        | quantity ctor       | gravity field as a `Ratio` g-multiplier                |
|  [04]   | `implicit operator PointForce(Force f)`         | scalar lift         | a bare `Force` lifts to a Z-axis `PointForce`           |
|  [05]   | `ILoad.Factor(Ratio factor)`                    | scaling combinator  | returns a new `ILoad` with every component × `factor.DecimalFractions`, preserving `Label` |
|  [06]   | `ILoad.Label { get; }`                          | identity read       | the human-facing load label carried across `Factor`    |
|  [07]   | `PointForce.X`/`.Y`/`.Z` (and family component reads) | component read | typed `UnitsNet` quantity per axis                     |

## [04]-[IMPLEMENTATION_LAW]

[LOAD_TOPOLOGY]:
- namespace: `VividOrange.Loads` (one namespace spans the `ILoads` contract assembly and the `Loads` impl assembly)
- base contract: `ILoad : ITaxonomySerializable` — `string Label { get; }`, `ILoad Factor(Ratio factor)`
- component quantities (all `UnitsNet` `readonly struct`): `Force` (point force, column axial), `Torque` (point moment), `Length` (prescribed displacement), `ForcePerLength` (line force), `Pressure` (area force), `Ratio` (gravity multiplier + the `Factor` argument)
- axis convention: 2D interfaces carry the in-plane `X`/`Z` (forces, displacements) or `Yy`/`Zz` (moments) pair; the 3D interface derives the 2D interface and adds the out-of-plane component (`Y`, or `Xx` for moment)
- projection frame: `LoadApplication` (`Local`/`Global`/`Projected`) on `ILineForce`/`IAreaForce` selects the coordinate frame the distributed action resolves against
- scaling: `Factor(Ratio)` rebuilds the concrete load with each component multiplied by `factor.DecimalFractions`; gravity and force loads alike round-trip through this one combinator

[LOCAL_ADMISSION]:
- the `Model/structural#ANALYSIS_MODEL` `LoadGroup` record owns load-group TOPOLOGY (GlobalId, `StructuralLoadKind`, the loaded-item GlobalId sequence); the typed `ILoad` carries the applied action VALUE — the group references items, the `ILoad` carries the force quantity, and the two never merge into one stringly-typed record
- a load quantity is always a `UnitsNet` struct, never a bare `double` + unit string; coercion to the persisted SI-base form is the `.api/api-unitsnet` `ToUnit(UnitSystem.SI)` path
- combination factoring is `load.Factor(γ)` (or the `Cases` `Combinations.Utility.FactorLoads` fold), never an ad-hoc `force * gamma` over a raw scalar — `Factor` preserves the `Label` and the concrete type so the factored result is still a typed `ILoad`
- the concrete `PointForce`/`LineForce`/… classes are MUTABLE settable-property carriers (`{ get; set; }`); treat them as boundary ingest DTOs and project onto the immutable Bim record set, never as the in-graph authority

[STACKING]:
- with `UnitsNet` (`.api/api-unitsnet`): every load component IS a `UnitsNet` typed quantity, so a `PointForce` reduction over an element set is `UnitMath.Sum(forces, ForceUnit.Newton)` and a unit-checked SI coercion is the shared `ToUnit(UnitSystem.SI)` — the same rail the `QuantitySet` `MeasureValue` carrier governs; the `Factor(Ratio)` argument is the `UnitsNet` `Ratio` whose `.DecimalFractions` is the dimensionless multiplier
- with `VividOrange.ISerialization`: `ILoad : ITaxonomySerializable` is the empty marker the VividOrange serializer keys on; a load round-trips through the same taxonomy-serialization rail as `VividOrange.Cases`/`VividOrange.Countries`, so a single serialization seam covers the whole structural taxonomy
- with `VividOrange.Cases` (`.api/api-vividorange-cases`): `ILoadCase.Loads` is an `IList<ILoad>` and `ILoadCombination.GetFactoredLoads()` returns `IList<ILoad>` — the load value taxonomy is the payload the Eurocode case/combination engine factors; `Combinations.Utility.FactorLoads<T>(γ, cases)` and `ENCombinationFactory.CreateStrGeoSetB` fold `ILoad.Factor` across a case set
- with `Thinktecture.Runtime.Extensions` (`.api/api-thinktecture-json`): the canonical in-graph discriminant is the `Model/structural` `StructuralLoadKind` `[SmartEnum<string>]`; the VividOrange concrete load type and `LoadApplication` enum are the boundary vocabulary mapped onto the Bim record at ingest, never re-exported as the canonical shape
- with `StructuralAnalysisFormat` (`.api/api-structuralanalysisformat`): the SAF load model is QUANTITY-ISOMORPHIC — `ExcelStructuralPointAction : ExcelStructuralPointLoadBase<Force, …>` ↔ `PointForce`, `ExcelStructuralCurveAction` (`ForcePerLength`) ↔ `LineForce`, `ExcelStructuralSurfaceAction` ↔ `AreaForce`; the SAF↔VividOrange round-trip is a per-component `UnitsNet` map, never a scalar reinterpretation, so a model authored against the Eurocode taxonomy exports to the SAF XLSX wire with no unit loss
- with `LanguageExt.Core`: an out-of-range or unparseable foreign load quantity at the SAF/IFC ingest boundary lowers onto `Model/faults#FAULT_BAND` `BimFault.CodecReject`/`Fin<T>` through `.ToError()`, never a thrown exception inside the load fold

[RAIL_LAW]:
- Package: `VividOrange.Loads` over `VividOrange.ILoads`
- Owns: the typed structural-load value taxonomy and the `Factor(Ratio)` scaling combinator
- Accept: load components carried as `UnitsNet` quantities; combination scaling through `ILoad.Factor`; serialization through `ITaxonomySerializable`
- Reject: bare-`double` load components, hand-multiplied partial-factor scaling, a per-load-type parallel record family on the Bim graph (the canonical owner is `Model/structural` `LoadGroup` + `StructuralLoadKind`), a SAF round-trip that reinterprets a quantity scalar instead of mapping the typed unit
