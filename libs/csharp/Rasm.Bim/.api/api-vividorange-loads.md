# [RASM_BIM_API_VIVIDORANGE_LOADS]

`VividOrange.Loads` over the `VividOrange.ILoads` contract mints the host-neutral structural-load value taxonomy: the closed family of idealized member actions — point force, moment, displacement, line force, area force, column load, gravity — each an immutable `ILoad` whose components are `UnitsNet` dimensioned quantities rather than bare `double`. `ILoad.Factor(Ratio)` is the one scaling combinator every partial-safety and combination multiplier folds through, so factoring rebuilds the typed load and preserves its `Label`, never hand-multiplying a force scalar.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `VividOrange.Loads`
- package: `VividOrange.Loads` (impl) over `VividOrange.ILoads` (contracts + enum) (MIT)
- assembly: `VividOrange.Loads`, `VividOrange.ILoads`
- namespace: `VividOrange.Loads` (one namespace spans both the contract and impl assemblies)
- asset: pure-managed AnyCPU IL binding the `net8.0` lib asset; no native binaries, ALC-safe inside the in-Rhino plugin
- depends: `VividOrange.ILoads` → `UnitsNet` (quantity payload) + `VividOrange.ISerialization` (`ITaxonomySerializable`); `VividOrange.Loads` → `VividOrange.ILoads`
- rail: structural-load

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: load contract family (`VividOrange.ILoads`); the 3D interface derives its 2D base and adds the out-of-plane component

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                                              |
| :-----: | :--------------------- | :------------ | :-------------------------------------------------------- |
|  [01]   | `ILoad`                | interface     | `: ITaxonomySerializable` + `Label` + `Factor(Ratio)`     |
|  [02]   | `IPointForce2d`        | interface     | `Force X`, `Force Z`                                      |
|  [03]   | `IPointForce`          | interface     | `: IPointForce2d` + `Force Y`                             |
|  [04]   | `IPointMoment2d`       | interface     | `Torque Yy`, `Torque Zz`                                  |
|  [05]   | `IPointMoment`         | interface     | `: IPointMoment2d` + `Torque Xx`                          |
|  [06]   | `IPointDisplacement2d` | interface     | `Length X`, `Length Z`                                    |
|  [07]   | `IPointDisplacement`   | interface     | `: IPointDisplacement2d` + `Length Y`                     |
|  [08]   | `ILineForce2d`         | interface     | `ForcePerLength X`, `ForcePerLength Z`, `LoadApplication` |
|  [09]   | `ILineForce`           | interface     | `: ILineForce2d` + `ForcePerLength Y`                     |
|  [10]   | `IAreaForce`           | interface     | `Pressure X/Y/Z` + `LoadApplication Application`          |
|  [11]   | `IColumnLoad`          | interface     | `Force Force` + `IPointMoment2d TopMoment`/`BottomMoment` |
|  [12]   | `IGravity2d`           | interface     | `Ratio X`, `Ratio Z`                                      |
|  [13]   | `IGravity`             | interface     | `: IGravity2d` + `Ratio Y`                                |
|  [14]   | `LoadApplication`      | enum          | `Local` / `Global` / `Projected`                          |

[PUBLIC_TYPE_SCOPE]: concrete load family (`VividOrange.Loads`); each 2D/3D pair shares one settable-property class

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY] | [CAPABILITY]                                  |
| :-----: | :---------------------------------------- | :------------ | :-------------------------------------------- |
|  [01]   | `PointForce2d`/`PointForce`               | class         | `Force` components + `implicit operator` lift |
|  [02]   | `PointMoment2d`/`PointMoment`             | class         | `Torque` components                           |
|  [03]   | `PointDisplacement2d`/`PointDisplacement` | class         | `Length` components                           |
|  [04]   | `LineForce2d`/`LineForce`                 | class         | `ForcePerLength` components + `Application`   |
|  [05]   | `AreaForce`                               | class         | `Pressure` components + `Application`         |
|  [06]   | `ColumnLoad`                              | class         | `Force` axial + top/bottom `IPointMoment2d`   |
|  [07]   | `Gravity2d`/`Gravity`                     | class         | `Ratio` g-multiplier components               |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: typed construction and scaling

| [INDEX] | [SURFACE]                             | [SHAPE]  | [CAPABILITY]                                       |
| :-----: | :------------------------------------ | :------- | :------------------------------------------------- |
|  [01]   | `PointForce(Force, Force, Force)`     | ctor     | spatial point force from typed `Force` components  |
|  [02]   | `LineForce(...)` / `AreaForce(...)`   | ctor     | line/area action from `ForcePerLength`/`Pressure`  |
|  [03]   | `Gravity(Ratio)` / `Gravity()`        | ctor     | gravity field as a `Ratio` g-multiplier            |
|  [04]   | `implicit operator PointForce(Force)` | operator | a bare `Force` lifts to a Z-axis `PointForce`      |
|  [05]   | `ILoad.Factor(Ratio) -> ILoad`        | instance | rebuilt load, each component × `.DecimalFractions` |
|  [06]   | `ILoad.Label -> string`               | property | human-facing label carried across `Factor`         |
|  [07]   | `PointForce.X`/`.Y`/`.Z`              | property | typed `UnitsNet` quantity per axis                 |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- base contract: `ILoad: ITaxonomySerializable` with `string Label { get; }` and `ILoad Factor(Ratio)`; every load folds through this one surface
- projection frame: `LoadApplication` (`Local`/`Global`/`Projected`) on `ILineForce`/`IAreaForce` selects the coordinate frame the distributed action resolves against
- scaling: `Factor(Ratio)` rebuilds the concrete load with each component multiplied by `factor.DecimalFractions`; gravity and force loads alike round-trip through this one combinator

[STACKING]:
- `UnitsNet` (`libs/csharp/.api/api-unitsnet.md`): every load component IS a `UnitsNet` typed quantity, so a `PointForce` reduction is `UnitMath.Sum(forces, ForceUnit.Newton)` and SI coercion is the shared `ToUnit(UnitSystem.SI)`; the `Factor(Ratio)` argument is the `Ratio` whose `.DecimalFractions` is the dimensionless multiplier
- `VividOrange.ISerialization`: `ILoad: ITaxonomySerializable` keys the VividOrange serializer, so a load round-trips through the one taxonomy-serialization rail the whole structural taxonomy shares
- `VividOrange.Cases` (`.api/api-vividorange-cases`): `ILoadCase.Loads` is `IList<ILoad>` and `ILoadCombination.GetFactoredLoads()` returns `IList<ILoad>`; the load taxonomy is the payload `Combinations.Utility.FactorLoads<T>` and `ENCombinationFactory.CreateStrGeoSetB` fold `ILoad.Factor` across
- `Thinktecture.Runtime.Extensions` (`libs/csharp/.api/api-thinktecture-json.md`): the canonical in-graph discriminant is `Model/structural` `StructuralLoadKind` `[SmartEnum<string>]`; the concrete load type and `LoadApplication` enum are boundary vocabulary mapped onto the Bim record at ingest, never re-exported as the canonical shape
- `StructuralAnalysisFormat` (`.api/api-structuralanalysisformat`): the SAF load model is quantity-isomorphic — `ExcelStructuralPointAction<Force>` ↔ `PointForce`, `ExcelStructuralCurveAction<ForcePerLength>` ↔ `LineForce`, `ExcelStructuralSurfaceAction` ↔ `AreaForce`, `ExcelStructuralPointMoment` ↔ `PointMoment`, `ExcelStructuralPointSupportDeformation` ↔ `PointDisplacement`; the round-trip is a per-component `UnitsNet` map, so a Eurocode-authored model exports to the SAF XLSX wire with no unit loss
- `LanguageExt.Core`: an out-of-range or unparseable foreign load quantity at the SAF/IFC ingest boundary lowers onto `Model/faults#FAULT_BAND` `BimFault.CodecReject`/`Fin<T>` through `.ToError()`, never a thrown exception inside the load fold

[LOCAL_ADMISSION]:
- `Model/structural#ANALYSIS_MODEL` `LoadGroup` owns load-group topology by GlobalId while the typed `ILoad` carries the applied action quantity; the group references items, the load carries the force, and the two never merge into one stringly-typed record
- a load quantity is always a `UnitsNet` struct; SI-base coercion is the `libs/csharp/.api/api-unitsnet.md` `ToUnit(UnitSystem.SI)` path
- `PointForce`/`LineForce`/… concrete classes are mutable settable-property carriers; ingest them as boundary DTOs and project onto the immutable Bim record set, never as in-graph authority

[RAIL_LAW]:
- Package: `VividOrange.Loads` over `VividOrange.ILoads`
- Owns: the typed structural-load value taxonomy and the `Factor(Ratio)` scaling combinator
- Accept: load components as `UnitsNet` quantities; combination scaling through `ILoad.Factor`; serialization through `ITaxonomySerializable`
- Reject: bare-`double` components, hand-multiplied partial-factor scaling, a per-load-type parallel record family on the Bim graph (the owner is `Model/structural` `LoadGroup` + `StructuralLoadKind`), a SAF round-trip reinterpreting a scalar instead of mapping the typed unit
