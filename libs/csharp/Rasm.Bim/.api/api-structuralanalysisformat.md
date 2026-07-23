# [RASM_BIM_API_STRUCTURALANALYSISFORMAT]

`StructuralAnalysisFormat` owns the SAF XLSX exchange codec — the buildingSMART/IDEA StatiCa schema (`saf.guide`) round-tripping a structural-analysis model between tools. It is the exchange wire over the `Model/structural#STRUCTURAL_PROJECTION` seam payloads, never the canonical model: the seam `ElementGraph` holds the in-memory authority, `ExcelModel` folds those payloads to and from `.xlsx`. Every dimensioned field is a `UnitsNet` quantity struct, so the SAF object model is quantity-isomorphic to `VividOrange.Loads`, its combination enums the image of the `VividOrange.Cases` factory set.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `StructuralAnalysisFormat`
- package: `StructuralAnalysisFormat` (Apache-2.0)
- assembly: `SAF.DataAccess.Contracts` (services), `SAF.DataAccess.Models` (object model), `SAF.DataAccess.Excel` (XLSX wrapper), `SAF.DataAccess.Implementation` (service impls), `SAF.DataAccess.Validation` (FluentValidation rules), `SAF.Infrastructure` (events/config/bootstrap); `ref/` ships `Contracts`/`Models`/`Infrastructure` as the compile surface
- namespace: `SAF.DataAccess.Contracts`; `SAF.DataAccess.Models` (+ `.StructuralElements`, `.Loads`, `.Libraries`, `.Results`, `.Subtypes`, `.Enums`, `.Interfaces`, `.Infrastructure`, `.Extensions`); `SAF.DataAccess.Excel`; `SAF.DataAccess.Validation`; `SAF.Infrastructure`
- asset: `netstandard2.0` only; the `net10.0` consumer binds `lib/netstandard2.0`, single TFM with no resolution ambiguity
- asset: pure-managed IL, no native binaries, ALC-safe inside the in-Rhino plugin assembly
- depends: `EPPlus` (XLSX engine), `FluentValidation` (validation engine), `UnitsNet` (quantity payload — its structs are the SAF field types)
- rail: saf-exchange

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: codec services and configuration (`SAF.DataAccess.Contracts`)

| [INDEX] | [SYMBOL]                                                | [TYPE_FAMILY]          | [CAPABILITY]                                      |
| :-----: | :------------------------------------------------------ | :--------------------- | :------------------------------------------------ |
|  [01]   | `IExcelImportService`                                   | XLSX → model codec     | `ExcelModel Import(Stream[, Version])`            |
|  [02]   | `IExcelExportService`                                   | model → XLSX codec     | `ExcelExportResult Export(Stream, ExcelModel, …)` |
|  [03]   | `IExcelValidator`                                       | pre-codec validator    | `ValidateForImport`/`ValidateForExport`           |
|  [04]   | `IExcelObjectConfigurator`                              | fluent mapping builder | object→worksheet column configuration             |
|  [05]   | `IExcelObjectConfiguration`                             | resolved mapping       | per-object property/header configuration          |
|  [06]   | `IExcelEnumToStringMapper` / `IExcelStringToEnumMapper` | enum codec             | SAF cell-text ↔ enum mapping                      |
|  [07]   | `IExcelWorksheetReader` / `IExcelWorksheetWriter`       | sheet I/O              | per-worksheet read/write seam                     |

[PUBLIC_TYPE_SCOPE]: model root, base contracts, and receipts (`SAF.DataAccess.Models`)

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]             | [CAPABILITY]                                                      |
| :-----: | :------------------------- | :------------------------ | :---------------------------------------------------------------- |
|  [01]   | `ExcelModel`               | model root (`sealed`)     | `Objects`, `ValidationErrors`, `SystemOfUnits`, `OriginalVersion` |
|  [02]   | `IExcelModuleObject`       | wire-row base             | `RowNumber`, `ObjectGrouping`, `ObjectName`, `ObjectIdentifier`   |
|  [03]   | `IExcelObject`             | identified object         | `: IExcelHasUniqueName, IExcelModuleObject` + `Guid Id`           |
|  [04]   | `ExcelObjectBase`          | object base (`abstract`)  | `Name`, `Guid Id`, overridden `ObjectIdentifier`                  |
|  [05]   | `ExcelExportResult`        | export receipt (`sealed`) | `IsSuccess`, `ExcelModel Model`, `ValidationResults`              |
|  [06]   | `ExcelValidationResult`    | validation receipt        | `Identifier`, `ValidationResults`, `Severity`, `Format(...)`      |
|  [07]   | `ExcelModelInformation`    | header row                | model metadata object                                             |
|  [08]   | `ExcelProjectInformation`  | header row                | project metadata object                                           |
|  [09]   | `Interfaces.IExcelHas*<T>` | capability markers        | quantity-parameterized object traits                              |

- [09]-[MARKERS]: `IExcelHasNodeCoordinates` (non-generic), `IExcelHasTranslationConstraints<T>`, `IExcelHasRotationConstraints<T>`, `IExcelHasLoadDirectionVector<T>` (+ `…Vectors<T>`); e.g. `ExcelStructuralPointAction: …, IExcelHasLoadDirectionVector<Force>`.

[PUBLIC_TYPE_SCOPE]: structural-element model (`SAF.DataAccess.Models.StructuralElements`)
- note: `ExcelRelConnects*` connection relations are `RigidLink`/`RigidMember`/`RigidCross`/`StructuralMember`/`SurfaceEdge`

| [INDEX] | [SYMBOL]                                                | [TYPE_FAMILY]        | [CAPABILITY]                                       |
| :-----: | :------------------------------------------------------ | :------------------- | :------------------------------------------------- |
|  [01]   | `ExcelStructuralPointConnection`                        | node (`sealed`)      | `Length? X`/`Y`/`Z`                                |
|  [02]   | `ExcelStructuralCurveMember`                            | 1D beam/column       | `CrossSection`, `Nodes`/`Segments`, eccentricities |
|  [03]   | `ExcelStructuralSurfaceMember`                          | 2D slab/wall         | `Material`, `Thickness`, `Nodes`, `EdgeShapes`     |
|  [04]   | `ExcelStructuralPointSupport`                           | nodal support        | DOF constraint + stiffness, `BoundaryCondition`    |
|  [05]   | `ExcelStructuralEdgeConnection`                         | line boundary        | edge support + connection                          |
|  [06]   | `ExcelStructuralSurfaceConnection`                      | area boundary        | surface support + connection                       |
|  [07]   | `ExcelStructuralCurveConnection`                        | line boundary        | curve support + connection                         |
|  [08]   | `ExcelStructuralSurfaceMemberOpening` / `…Region`       | surface sub-feature  | opening + sub-region of a 2D member                |
|  [09]   | `ExcelRelConnects*`                                     | connection relations | member-connection edges                            |
|  [10]   | `ExcelStructuralStorey` / `ExcelStructuralProxyElement` | level/proxy          | storey grouping + opaque proxy element             |

- [02]-[CURVE_MEMBER]: `CrossSection`, `ExcelFlexibleEnum<ExcelMember1DType> Type`, `Nodes`, `Segments`, `Length`, eccentricities (`Length`), `Angle`.
- [03]-[SURFACE_MEMBER]: `Material`, `ExcelFlexibleEnum<ExcelMember2DType> Type`, `ExcelMemberThickness Thickness`, `Nodes`, `EdgeShapes`.
- [04]-[POINT_SUPPORT]: `ExcelConstraintType? TranslationX`/`Y`/`ZType` + `ForcePerLength? …Stiffness`, rotational (`RotationalStiffness?`), `BoundaryCondition`.

[PUBLIC_TYPE_SCOPE]: load model (`SAF.DataAccess.Models.Loads`)

| [INDEX] | [SYMBOL]                                 | [TYPE_FAMILY]           | [CAPABILITY]                                                  |
| :-----: | :--------------------------------------- | :---------------------- | :------------------------------------------------------------ |
|  [01]   | `ExcelStructuralLoadCase`                | load case               | action/load-case type, `LoadGroup`, `Duration`                |
|  [02]   | `ExcelStructuralLoadCombination`         | load combination        | category, standard, factors, cases                            |
|  [03]   | `ExcelStructuralLoadGroup`               | load group              | grouping over cases; `LoadGroup` topology                     |
|  [04]   | `ExcelStructuralPointAction`             | point force             | `: ExcelStructuralPointLoadBase<Force,…>` + direction         |
|  [05]   | `ExcelStructuralPointMoment`             | point moment            | the point-moment action                                       |
|  [06]   | `ExcelStructuralPointSupportDeformation` | prescribed displacement | nodal imposed deformation                                     |
|  [07]   | `ExcelStructuralCurveAction`             | line load               | + `Free`/`Thermal`/`Moment`; flexible-enum + `ForcePerLength` |
|  [08]   | `ExcelStructuralSurfaceAction`           | area load               | + `Distribution`/`Free`/`Thermal`; surface-pressure action    |

- [01]-[LOAD_CASE]: `ExcelActionType? ActionType`, `ExcelLoadCaseType? LoadType`, `LoadGroup`, `Duration`.
- [02]-[LOAD_COMBINATION]: `Category`, `ExcelLoadCaseCombinationStandard? NationalStandard`, `double?[] LoadFactors`/`LoadMultipliers`, `string[] LoadCases`.
- [04]-[POINT_ACTION]: `: ExcelStructuralPointLoadBase<Force,…>` + `ExcelLoadDirectionVector<Force>`.
- [07]-[CURVE_ACTION]: `ExcelFlexibleEnum<ExcelActionLoadType> Type`, `ForcePerLength? Value`/`Value1`/`Value2`, `ExcelLoadDirectionVector<ForcePerLength>`.

[PUBLIC_TYPE_SCOPE]: libraries, results, and value subtypes

| [INDEX] | [SYMBOL]                                            | [TYPE_FAMILY]        | [CAPABILITY]                                               |
| :-----: | :-------------------------------------------------- | :------------------- | :--------------------------------------------------------- |
|  [01]   | `Libraries.ExcelStructuralMaterial`                 | material library row | type, quality, `Density`/`Pressure` moduli                 |
|  [02]   | `Libraries.ExcelStructuralCrossSection`             | section library row  | material, profile, parameters, section properties          |
|  [03]   | `Results.ExcelResultInternalForce1D` / `…2D`        | result table         | solver internal-force result rows                          |
|  [04]   | `Subtypes.ExcelFlexibleEnum<T>`                     | open-enum wrapper    | `T Value`, `IsOther`, ctor `(T)`/`(string)`, `→ T`/`T?`    |
|  [05]   | `Subtypes.ExcelLoadDirectionVector<TQuantity>`      | typed direction      | quantity-typed load direction vector                       |
|  [06]   | `Subtypes.ExcelMemberThickness` / `ExcelCurveShape` | geometry subtypes    | thickness, curve segment; 2D point, polygon, composite def |
|  [07]   | `Extensions.UnitsNetExtensions`                     | UnitsNet helpers     | `CreateUnit`/`UnitsNetEquals`/`UnitsNetSequenceEquals`     |

- [01]-[MATERIAL]: `ExcelMaterialType? Type`, `Quality`, `Density? UnitMass`, `Pressure? EModulus`/`GModulus`, `PoissonCoefficient`, `CoefficientOfThermalExpansion?`.
- [02]-[SECTION]: `Material`, `ExcelCrossSectionType?`, `ExcelProfileLibraryId? Shape`, `Length[] Parameters`, `Area?`/`AreaMomentOfInertia?`/`WarpingMomentOfInertia?`/`Volume?`, `FormCode`/`DescriptionId`.
- [06]-[GEOMETRY_SUBTYPES]: `Subtypes.ExcelMemberThickness` / `ExcelCurveShape`; `Subtypes.CrossSectionShape.ExcelPoint2D` / `ExcelPolygonContour` / `ExcelCompositeShapeDef` (the `ExcelStructuralCrossSection.Definition` type).

[PUBLIC_TYPE_SCOPE]: SAF enum vocabulary (`SAF.DataAccess.Models.Enums`) — the boundary axes

| [INDEX] | [SYMBOL]                                         | [TYPE_FAMILY]        | [CAPABILITY]                                                  |
| :-----: | :----------------------------------------------- | :------------------- | :------------------------------------------------------------ |
|  [01]   | `ExcelSystemOfUnits`                             | units regime         | `Metric` / `Imperial` (`ExcelModel.SystemOfUnits`)            |
|  [02]   | `ExcelNationalCode`                              | design-code axis     | `EC_DIN_EN`/`EC_NF_EN`/`EC_UNI_EN`/… + `IBC`/`NBR`/`SIA_26x`  |
|  [03]   | `ExcelActionType`                                | action nature        | `Permanent`/`Variable`/`Accidental` (= `ActionClass`)         |
|  [04]   | `ExcelLoadCaseType`                              | load-case nature     | `SelfWeight`/`Prestress`/`Wind`/`Snow`/`Seismic`/`…`          |
|  [05]   | `ExcelLoadCaseCombinationCategory`               | limit-state category | ULS/SLS/accidental/national-standard categories               |
|  [06]   | `ExcelLoadCaseCombinationStandard`               | combination clause   | the EN/IBC combination-clause members                         |
|  [07]   | `ExcelMaterialType`                              | material family      | `Concrete`/`Steel`/`Timber`/`Aluminium`/`Masonry`/`Other`     |
|  [08]   | `ExcelMember1DType` / `ExcelMember2DType`        | member role          | `Beam`/`Column`/`Rafter`/… (1D), slab/wall/plate (2D)         |
|  [09]   | `ExcelConstraintType`                            | support DOF nature   | free/rigid/flexible per translational/rotational DOF          |
|  [10]   | `ExcelValidationMessageSeverity`                 | validation severity  | `Error`/`Warning`/`Info` (`ExcelValidationResult.Severity`)   |
|  [11]   | `ExcelDuration`                                  | load-duration class  | `Long`/`Medium`/`Short`/`Instantaneous` (timber)              |
|  [12]   | `ExcelLoadGroupType`                             | load-group relation  | the `ExcelStructuralLoadGroup` action-relation kind           |
|  [13]   | `ExcelCoordinateSystem` / `ExcelActionDirection` | load frame + axis    | the coordinate frame + signed axis an action resolves against |
|  [14]   | `ExcelPointForceAction`                          | force discriminant   | point-load-vs-derived-action discriminant                     |
|  [15]   | `ExcelCurveForceAction`                          | force discriminant   | curve-load-vs-derived-action discriminant                     |
|  [16]   | `ExcelSurfaceForceAction`                        | force discriminant   | surface-load-vs-derived-action discriminant                   |

- [05]-[LIMIT_STATE]: `UltimateLimitState`/`ServiceabilityLimitState`/`AccidentalLimitState`/`AccordingNationalStandard`.
- [06]-[COMBINATION_STANDARD]: `EnUlsSetB`/`EnUlsSetC`/`EnAccidental1`/`2`/`EnSeismic`/`EnSlsCharacteristic`/`Frequent`/`QuasiPermanent`/`Ibc*`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: XLSX codec

| [INDEX] | [SURFACE]                                                         | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :---------------------------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `IExcelImportService.Import(Stream)`                              | instance | `.xlsx` stream → `ExcelModel` (default) |
|  [02]   | `IExcelImportService.Import(Stream, Version)`                     | instance | read up/downgrading to a target version |
|  [03]   | `IExcelExportService.Export(Stream, ExcelModel)`                  | instance | model → `.xlsx` → `ExcelExportResult`   |
|  [04]   | `IExcelExportService.Export(Stream, ExcelModel, Version, …)`      | instance | version + validity-gated export         |
|  [05]   | `IExcelValidator.ValidateForImport(ExcelModel, Version, Version)` | instance | pre-import FluentValidation pass        |
|  [06]   | `IExcelValidator.ValidateForExport(ExcelModel, Version, Version)` | instance | pre-export FluentValidation pass        |

[ENTRYPOINT_SCOPE]: model construction, receipts, and UnitsNet helpers
- note: `ExcelModel` ctor is `(IReadOnlyCollection<IExcelModuleObject>, IReadOnlyCollection<ExcelValidationResult>, ExcelSystemOfUnits)`

| [INDEX] | [SURFACE]                                                   | [SHAPE]  | [CAPABILITY]                                          |
| :-----: | :---------------------------------------------------------- | :------- | :---------------------------------------------------- |
|  [01]   | `new ExcelModel(…)`                                         | ctor     | assemble a SAF model for export                       |
|  [02]   | `ExcelModel.Objects`                                        | property | `IReadOnlyList<IExcelModuleObject>` by type           |
|  [03]   | `.SystemOfUnits`/`.OriginalVersion`/`.ValidationErrors`     | property | units regime, source SAF version, decode errors       |
|  [04]   | `ExcelExportResult.IsSuccess`/`.Model`/`.ValidationResults` | property | outcome + revalidated model + messages                |
|  [05]   | `Format(IReadOnlyList<ExcelValidationResult>)`              | static   | flatten validation receipts to a report string        |
|  [06]   | `UnitsNetExtensions.CreateUnit<TUnit>(this double)`         | static   | lift a SAF cell scalar to a typed `UnitsNet` quantity |
|  [07]   | `ExcelFlexibleEnum<T>` ctor `(T)`/`(string)`, `→ T`/`T?`    | ctor     | known SAF enum value OR arbitrary "other" cell text   |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- codec services: `IExcelImportService` (XLSX → `ExcelModel`), `IExcelExportService` (`ExcelModel` → XLSX → `ExcelExportResult`), `IExcelValidator` (`ValidateForImport`/`ValidateForExport`); the SAF SDK resolves these through its `SAF.Infrastructure.Bootstrapping.IBootstrapper` container, the `SAF.DataAccess.Implementation`/`.Excel` impls are wiring, and `IExcelDocumentReader`/`IExcelDocumentWriter` are the low-level per-document I/O seam beneath `IExcelWorksheetReader`/`Writer`
- model shape: `ExcelModel` is a FLAT `IReadOnlyList<IExcelModuleObject>` bag; every object derives `ExcelObjectBase: IExcelObject` (`Name`, `Guid Id`, `ObjectIdentifier`), a consumer discriminates by concrete type, and the wire-row metadata (`RowNumber`, `ObjectGrouping`, `ObjectName`) keys the spreadsheet position
- quantities: every dimensioned field is a `UnitsNet` struct — `Length` (coordinates, eccentricities), `Angle` (rotations), `Force`/`ForcePerLength`/`Pressure` (loads), `Density`/`Pressure`/`CoefficientOfThermalExpansion` (materials), `Area`/`AreaMomentOfInertia`/`WarpingMomentOfInertia`/`Volume` (section properties), `RotationalStiffness`/`ForcePerLength` (support springs)
- open enums: `ExcelFlexibleEnum<T> where T: struct, Enum` wraps a known enum value OR an arbitrary `string` (`IsOther`), so a non-standard member or action type round-trips without data loss

[LOCAL_ADMISSION]:
- SAF is the XLSX EXCHANGE wire over the `Model/structural#STRUCTURAL_PROJECTION` seam payloads, never the canonical model — the seam `ElementGraph` is the in-memory authority, and `ExcelModel` round-trips those payloads to/from `.xlsx` through the pending Exchange-lane arms (the `Exchange/format#FORMAT_AXIS` `Saf` candidate row promotes when they land)
- pending Bim lowering folds `ExcelModel.Objects` by concrete type onto the SAME neutral payloads the IFC ingest lands: `ExcelStructuralCurveMember`/`ExcelStructuralSurfaceMember`→ member `Object` nodes + `StructuralDefinitionSet` bags, `ExcelStructuralPointConnection`/`ExcelStructuralPointSupport`→ connection nodes + the `TranslationX..RotationKz` restraint edge attrs (the `ExcelConstraintType` DOF rows ‖ the six-DOF `Fixity`/`Spring` pairs), `ExcelStructuralLoadCase`/`LoadCombination`/`LoadGroup`→ the `LoadGroupType`/`ActionType`/`Case` bag rows, the `ExcelRelConnects*`→ the `IfcRelKind.ConnectsStructMember`/`ConnectsStructActivity` `Generic` edges
- a SAF read/write fault, an unmapped object type, or a validation `Error` lowers onto `Model/faults#FAULT_BAND` `BimFault.CodecReject`/`ModelRejected` via `.ToError()`, reading `ExcelValidationResult.Severity`; never an exception across the fold
- `ExcelModel.SystemOfUnits` (`Metric`/`Imperial`) and `OriginalVersion` are decode context — coerce every SAF quantity to SI-base on ingest (the `libs/csharp/.api/api-unitsnet.md` `ToUnit(UnitSystem.SI)` path) so the Bim graph is unit-normalized regardless of the SAF workbook regime

[STACKING]:
- `UnitsNet`(`libs/csharp/.api/api-unitsnet.md`): SAF and the Bim `QuantitySet` share the quantity rail — `UnitsNetExtensions.CreateUnit<TUnit>` lifts a SAF cell scalar to a typed quantity, ingest coerces to SI-base through `ToUnit(UnitSystem.SI)`, and the bound `Length`/`Pressure`/`Force` typed-struct surface is the SAF field-type set
- `VividOrange.Loads`(`.api/api-vividorange-loads`): the SAF load model is QUANTITY-ISOMORPHIC — `ExcelStructuralPointAction: ExcelStructuralPointLoadBase<Force,…>` ↔ `PointForce`, `ExcelStructuralCurveAction` (`ForcePerLength`) ↔ `LineForce`, `ExcelStructuralSurfaceAction` ↔ `AreaForce`, `ExcelStructuralPointMoment` ↔ `PointMoment`, `ExcelStructuralPointSupportDeformation` ↔ `PointDisplacement`; the round-trip is a per-component `UnitsNet` map, never a scalar reinterpretation
- `VividOrange.Cases`(`.api/api-vividorange-cases`): `ExcelStructuralLoadCombination.NationalStandard` is `ExcelLoadCaseCombinationStandard` whose members (`EnUlsSetB`/`EnUlsSetC`/`EnAccidental*`/`EnSeismic`/`EnSlsCharacteristic`/`Frequent`/`QuasiPermanent`) are the exact image of `ENCombinationFactory.CreateStrGeoSetB`/`SetC`/`CreateAccidental`/`CreateSeismic`/`CreateCharacteristic`/`CreateFrequent`/`CreateQuasiPermanent`; `ExcelActionType` = `ActionClass`, `ExcelLoadCaseCombinationCategory` = the ULS/SLS limit-state split, so a Eurocode combination set exports with the clause preserved
- `VividOrange.Countries`(`.api/api-vividorange-countries`): `ExcelNationalCode` (`EC_DIN_EN`/`EC_NF_EN`/…) is the SAF design-code axis a model's `ICountry` + `NationalAnnex` map onto at the XLSX boundary
- `GeometryGymIFC_Core`(`.api/api-geometrygym-ifc`): SAF and the IFC structural-analysis entity graph are the two wires that MEET at the seam structural payloads — IFC the in-memory semantic authority, SAF the spreadsheet exchange; both lower onto the `Model/structural#STRUCTURAL_PROJECTION` neutral `Generic` edge/bag attrs over the seam `ElementGraph`, never two parallel structural models
- `Thinktecture.Runtime.Extensions`(`libs/csharp/.api/api-thinktecture-json.md`): the SAF `Excel*` enums are the boundary vocabulary mapped onto the canonical `Model/structural` `StructuralLoadKind`/`StructuralCurveMemberKind` `[SmartEnum]` discriminants at ingest, never re-exported as the canonical Bim shape
- `LanguageExt.Core`: `IExcelExportService.Export` returns an `ExcelExportResult` with `IsSuccess` + `ValidationResults`, not an exception; the Bim codec lifts a failed result BARE onto `Fin<T>` as `new BimFault.CodecReject(key, detail)` (band 2600 IS the `Expected` `Code`), threading the `ExcelValidationResult` rows as the detail

[RAIL_LAW]:
- Package: `StructuralAnalysisFormat`
- Owns: the SAF XLSX structural-analysis exchange codec and the SAF object model (`netstandard2.0`, Apache-2.0)
- Accept: SAF as the exchange wire over the `Model/structural` seam payloads; the flat `ExcelModel.Objects` bag folded by concrete type; quantities normalized to SI-base on ingest; export outcomes read off `ExcelExportResult`
- Reject: SAF as a canonical structural model (the authority is the seam `ElementGraph`), a per-element-type SAF collection mirror, a quantity round-trip that reinterprets a scalar instead of mapping the `UnitsNet` unit, and exception-driven codec control flow
