# [RASM_BIM_API_STRUCTURALANALYSISFORMAT]

`StructuralAnalysisFormat` is the SAF (Structural Analysis Format) SDK — the open buildingSMART/
IDEA StatiCa XLSX schema (`saf.guide`) for round-tripping a structural-analysis model between
authoring tools (SCIA Engineer, IDEA StatiCa, Dlubal RFEM, Autodesk Robot, …). In Rasm.Bim it is
the XLSX EXCHANGE WIRE over the seam structural payloads `Model/structural#STRUCTURAL_PROJECTION`
defines, never the canonical model: the in-memory authority is the seam `ElementGraph` (the GeometryGym
`IfcStructuralAnalysisModel` graph lowered onto neutral `Generic` edge/bag attrs — the retired
host-neutral `AnalysisModel` store is the deleted form), while SAF imports/exports those payloads
as a spreadsheet through the pending Exchange-lane lowering onto `ExcelModel`. The codec is two service interfaces — `IExcelImportService.Import(Stream)`
→ `ExcelModel` and `IExcelExportService.Export(Stream, ExcelModel)` → `ExcelExportResult` — over a
FLAT model: `ExcelModel.Objects` is one `IReadOnlyList<IExcelModuleObject>` bag discriminated by
concrete SAF type (`is ExcelStructuralCurveMember`), not a per-element typed collection, so the
Bim projection folds the bag by runtime type exactly as `AnalysisProjection.Project` folds the IFC
`IfcStructuralItem` set. Every dimensioned field is a `UnitsNet` quantity (`Length`, `Angle`,
`Pressure`, `Density`, `Force`, `ForcePerLength`, `RotationalStiffness`, `AreaMomentOfInertia`),
so the SAF object model is QUANTITY-ISOMORPHIC to the `VividOrange.Loads` taxonomy
(`.api/api-vividorange-loads`): `ExcelStructuralPointAction: ExcelStructuralPointLoadBase<Force,…>`
↔ `PointForce`, `ExcelStructuralCurveAction` (`ForcePerLength`) ↔ `LineForce`, and the SAF
combination/national-code enums are the image of the `VividOrange.Cases` factory set
(`.api/api-vividorange-cases`). ABI/RUNTIME: the package is `netstandard2.0`-only (binds forward
under `net10.0`); its nuspec declares `UnitsNet, `EPPlus, `FluentValidation,
but the workspace runs `CentralPackageTransitivePinningEnabled` and pins `UnitsNet / `EPPlus / `FluentValidation, so all three transitive deps bind UP to the central
major: SAF compiled against UnitsNet 4.x / EPPlus 4.x (LGPL) / FluentValidation 10.x binds
UnitsNet 5.x / EPPlus 8.x (Polyform-Noncommercial) / FluentValidation 12.x. `UnitsNet` is the
PUBLIC-surface break — its quantity structs (`Length`/`Pressure`/`Force`/`ForcePerLength`/
`RotationalStiffness`/…) ARE the SAF object-model field types, so the 4→5 typed-struct change is the
load-bearing verification point; `EPPlus` and `FluentValidation` are the INTERNAL-engine breaks behind
the `SAF.DataAccess.Excel`/`.Validation` impls (runtime risk inside `Import`/`Export`/`Validate`, not
on the consumed contract surface).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `StructuralAnalysisFormat`
- package: `StructuralAnalysisFormat`
- license: Apache-2.0 (`requireLicenseAcceptance=true`); project `https://www.saf.guide`
- assembly: `SAF.DataAccess.Contracts` (services), `SAF.DataAccess.Models` (object model), `SAF.DataAccess.Excel` (XLSX wrapper), `SAF.DataAccess.Implementation` (service impls), `SAF.DataAccess.Validation` (FluentValidation rules), `SAF.Infrastructure` (events/config/bootstrap) — `ref/` ships `Contracts`/`Models`/`Infrastructure` as the compile surface
- namespace: `SAF.DataAccess.Contracts`; `SAF.DataAccess.Models` (+ `.StructuralElements`, `.Loads`, `.Libraries`, `.Results`, `.Subtypes`, `.Enums`, `.Interfaces`, `.Infrastructure`, `.Extensions`); `SAF.DataAccess.Excel`; `SAF.DataAccess.Validation`; `SAF.Infrastructure`
- asset: `netstandard2.0` ONLY — the `net10.0` consumer binds `lib/netstandard2.0` (no.NET-Core asset ships; single TFM, no resolution ambiguity)
- asset: pure-managed IL-only; no native binaries; ALC-safe inside the in-Rhino plugin assembly
- dependency (nuspec, all transitively pinned UP by `CentralPackageTransitivePinningEnabled`): `EPPlus → (XLSX engine; LGPL→Polyform-Noncommercial), `FluentValidation → (validation engine), `UnitsNet → (quantity payload) — [ABI] the consumed binding is the central major; UnitsNet 4→5 is the public-surface verification point (its quantity structs ARE the SAF field types), EPPlus 4→8 and FluentValidation 10→12 are internal-engine risk behind the Excel/Validation impls
- rail: saf-exchange

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: codec services and configuration (`SAF.DataAccess.Contracts`)
- rail: saf-exchange

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
- rail: saf-exchange

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
- rail: saf-exchange
- note: the `ExcelRelConnects*` connection relations are `RigidLink`/`RigidMember`/`RigidCross`/`StructuralMember`/`SurfaceEdge`

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
- rail: saf-exchange

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
- rail: saf-exchange

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
- rail: saf-exchange

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
- rail: saf-exchange

| [INDEX] | [SURFACE]                                                         | [ENTRY_FAMILY]  | [CAPABILITY]                            |
| :-----: | :---------------------------------------------------------------- | :-------------- | :-------------------------------------- |
|  [01]   | `IExcelImportService.Import(Stream)`                              | XLSX read       | `.xlsx` stream → `ExcelModel` (default) |
|  [02]   | `IExcelImportService.Import(Stream, Version targetVersion)`       | versioned read  | read up/downgrading to a target version |
|  [03]   | `IExcelExportService.Export(Stream, ExcelModel)`                  | XLSX write      | model → `.xlsx` → `ExcelExportResult`   |
|  [04]   | `IExcelExportService.Export(Stream, ExcelModel, Version, …)`      | versioned write | version + validity-gated export         |
|  [05]   | `IExcelValidator.ValidateForImport(ExcelModel, Version, Version)` | import gate     | pre-import FluentValidation pass        |
|  [06]   | `IExcelValidator.ValidateForExport(ExcelModel, Version, Version)` | export gate     | pre-export FluentValidation pass        |

[ENTRYPOINT_SCOPE]: model construction, receipts, and UnitsNet helpers
- rail: saf-exchange
- note: the `ExcelModel` ctor is `(IReadOnlyCollection<IExcelModuleObject>, IReadOnlyCollection<ExcelValidationResult>, ExcelSystemOfUnits)`

| [INDEX] | [SURFACE]                                                   | [ENTRY_FAMILY]    | [CAPABILITY]                                          |
| :-----: | :---------------------------------------------------------- | :---------------- | :---------------------------------------------------- |
|  [01]   | `new ExcelModel(…)`                                         | model ctor        | assemble a SAF model for export                       |
|  [02]   | `ExcelModel.Objects`                                        | heterogeneous bag | `IReadOnlyList<IExcelModuleObject>` by type           |
|  [03]   | `.SystemOfUnits`/`.OriginalVersion`/`.ValidationErrors`     | model facts       | units regime, source SAF version, decode errors       |
|  [04]   | `ExcelExportResult.IsSuccess`/`.Model`/`.ValidationResults` | export receipt    | outcome + revalidated model + messages                |
|  [05]   | `Format(IReadOnlyList<ExcelValidationResult>)`              | message render    | flatten validation receipts to a report string        |
|  [06]   | `UnitsNetExtensions.CreateUnit<TUnit>(this double)`         | scalar → quantity | lift a SAF cell scalar to a typed `UnitsNet` quantity |
|  [07]   | `ExcelFlexibleEnum<T>` ctor `(T)`/`(string)`, `→ T`/`T?`    | open enum         | known SAF enum value OR arbitrary "other" cell text   |

## [04]-[IMPLEMENTATION_LAW]

[SAF_TOPOLOGY]:
- codec services: `IExcelImportService` (XLSX → `ExcelModel`), `IExcelExportService` (`ExcelModel` → XLSX, returns `ExcelExportResult`), `IExcelValidator` (`ValidateForImport`/`ValidateForExport`; the bare `Validate` is `[Obsolete]`); the SAF SDK resolves these through its `SAF.Infrastructure.Bootstrapping.IBootstrapper` container — the `SAF.DataAccess.Implementation`/`SAF.DataAccess.Excel` impls are wiring, not the consumed surface, and the public `IExcelDocumentReader`/`IExcelDocumentWriter` are the low-level per-document I/O seam beneath `IExcelWorksheetReader`/`Writer`, not the consumed codec surface
- model shape: `ExcelModel` is a FLAT `IReadOnlyList<IExcelModuleObject>` bag — every object derives `ExcelObjectBase: IExcelObject` (`Name`, `Guid Id`, `ObjectIdentifier`); a consumer discriminates by concrete type, and the wire-row metadata (`RowNumber`, `ObjectGrouping`, `ObjectName`) keys the spreadsheet position
- quantities: every dimensioned field is a `UnitsNet` struct — `Length` (coordinates, eccentricities), `Angle` (rotations), `Force`/`ForcePerLength`/`Pressure` (loads), `Density`/`Pressure`/`CoefficientOfThermalExpansion` (materials), `Area`/`AreaMomentOfInertia`/`WarpingMomentOfInertia`/`Volume` (section properties), `RotationalStiffness`/`ForcePerLength` (support springs)
- open enums: `ExcelFlexibleEnum<T> where T: struct, Enum` wraps a known enum value OR an arbitrary `string` (`IsOther`), so a non-standard member type or action type round-trips without data loss

[LOCAL_ADMISSION]:
- SAF is the XLSX EXCHANGE wire over the `Model/structural#STRUCTURAL_PROJECTION` seam payloads, never the canonical model — the seam `ElementGraph` is the in-memory authority (the retired `AnalysisModel` view is the deleted form); `ExcelModel` round-trips those payloads to/from `.xlsx` through the pending Exchange-lane arms (the `Exchange/format#FORMAT_AXIS` `Saf` candidate row promotes when they land)
- the pending Bim lowering folds `ExcelModel.Objects` by concrete type onto the SAME neutral payloads the IFC ingest lands: `ExcelStructuralCurveMember`/`ExcelStructuralSurfaceMember`→ member `Object` nodes + `StructuralDefinitionSet` bags, `ExcelStructuralPointConnection`/`ExcelStructuralPointSupport`→ connection nodes + the `TranslationX..RotationKz` restraint edge attrs (the `ExcelConstraintType` DOF rows ‖ the six-DOF `Fixity`/`Spring` pairs), `ExcelStructuralLoadCase`/`LoadCombination`/`LoadGroup`→ the `LoadGroupType`/`ActionType`/`Case` bag rows, the `ExcelRelConnects*`→ the `IfcRelKind.ConnectsStructMember`/`ConnectsStructActivity` `Generic` edges
- a SAF read/write fault, an unmapped object type, or a validation `Error` lowers onto `Model/faults#FAULT_BAND` `BimFault.CodecReject`/`ModelRejected` via `.ToError()`, reading `ExcelValidationResult.Severity`; never an exception across the fold
- the `ExcelModel.SystemOfUnits` (`Metric`/`Imperial`) and `OriginalVersion` are decode context — coerce every SAF quantity to SI-base on ingest (the `libs/csharp/.api/api-unitsnet.md` `ToUnit(UnitSystem.SI)` path) so the Bim graph is unit-normalized regardless of the SAF workbook regime

[STACKING]:
- with `UnitsNet` (`libs/csharp/.api/api-unitsnet.md`): SAF and the Bim `QuantitySet` share the quantity rail — `UnitsNetExtensions.CreateUnit<TUnit>` lifts a SAF cell scalar to a typed quantity, and ingest coerces to SI-base through `ToUnit(UnitSystem.SI)`; [ABI] SAF binds `UnitsNet while the workspace pins, so the bound `Length`/`Pressure`/`Force` typed-struct surface is a verification point (the UnitsNet 4→5 break can shift the public surface SAF reads/writes)
- with `VividOrange.Loads` (`.api/api-vividorange-loads`): the SAF load model is QUANTITY-ISOMORPHIC — `ExcelStructuralPointAction: ExcelStructuralPointLoadBase<Force,…>` ↔ `PointForce`, `ExcelStructuralCurveAction` (`ForcePerLength`) ↔ `LineForce`, `ExcelStructuralSurfaceAction` ↔ `AreaForce`, `ExcelStructuralPointMoment` ↔ `PointMoment`, `ExcelStructuralPointSupportDeformation` ↔ `PointDisplacement`; the SAF↔VividOrange round-trip is a per-component `UnitsNet` map, never a scalar reinterpretation
- with `VividOrange.Cases` (`.api/api-vividorange-cases`): `ExcelStructuralLoadCombination.NationalStandard` is `ExcelLoadCaseCombinationStandard` whose members (`EnUlsSetB`/`EnUlsSetC`/`EnAccidental*`/`EnSeismic`/`EnSlsCharacteristic`/`Frequent`/`QuasiPermanent`) are the exact image of `ENCombinationFactory.CreateStrGeoSetB`/`SetC`/`CreateAccidental`/`CreateSeismic`/`CreateCharacteristic`/`CreateFrequent`/`CreateQuasiPermanent`; `ExcelActionType` = `ActionClass`, `ExcelLoadCaseCombinationCategory` = the ULS/SLS limit-state split — so a Eurocode combination set exports to SAF with the clause preserved
- with `VividOrange.Countries` (`.api/api-vividorange-countries`): `ExcelNationalCode` (`EC_DIN_EN`/`EC_NF_EN`/…) is the SAF design-code axis a model's `ICountry` + `NationalAnnex` map onto at the XLSX boundary
- with `GeometryGymIFC_Core` (`.api/api-geometrygym-ifc`): SAF and the IFC structural-analysis entity graph are the two wires that MEET at the seam structural payloads — IFC the in-memory semantic authority, SAF the spreadsheet exchange; both lower onto the `Model/structural#STRUCTURAL_PROJECTION` neutral `Generic` edge/bag attrs over the seam `ElementGraph` (the retired `AnalysisModel`/`AnalysisMember`/`Support`/`LoadGroup` record set is the deleted form), never two parallel structural models
- with `Thinktecture.Runtime.Extensions` (`libs/csharp/.api/api-thinktecture-json.md`): the SAF `Excel*` enums are the boundary vocabulary mapped onto the canonical `Model/structural` `StructuralLoadKind`/`StructuralCurveMemberKind` `[SmartEnum]` discriminants at ingest, never re-exported as the canonical Bim shape
- with `LanguageExt.Core`: `IExcelExportService.Export` returns an `ExcelExportResult` with `IsSuccess` + `ValidationResults` (not an exception); the Bim codec lifts a failed result BARE onto `Fin<T>` as `new BimFault.CodecReject(key, detail)` (band 2600 IS the `Expected` `Code` — the `.ToError()` hop is the deleted form), threading the `ExcelValidationResult` rows as the detail

[RAIL_LAW]:
- Package: `StructuralAnalysisFormat`
- Owns: the SAF XLSX structural-analysis exchange codec and the SAF object model (`netstandard2.0`, Apache-2.0)
- Accept: SAF as the exchange wire over the `Model/structural` seam payloads; the flat `ExcelModel.Objects` bag folded by concrete type; quantities normalized to SI-base on ingest; export outcomes read off `ExcelExportResult`
- Reject: SAF as a canonical structural model (the authority is the seam `ElementGraph`; the retired `AnalysisModel` store is the deleted form), a per-element-type SAF collection mirror, a quantity round-trip that reinterprets a scalar instead of mapping the `UnitsNet` unit, exception-driven codec control flow, and an unverified UnitsNet 4↔5 binding assumption on the SAF quantity surface
