# [RASM_BIM_API_STRUCTURALANALYSISFORMAT]

`StructuralAnalysisFormat` is the SAF (Structural Analysis Format) SDK — the open buildingSMART/
IDEA StatiCa XLSX schema (`saf.guide`) for round-tripping a structural-analysis model between
authoring tools (SCIA Engineer, IDEA StatiCa, Dlubal RFEM, Autodesk Robot, …). In Rasm.Bim it is
the XLSX EXCHANGE WIRE for the `Model/structural#ANALYSIS_MODEL` graph, never the canonical model:
the in-memory authority is the GeometryGym `IfcStructuralAnalysisModel` semantic graph
(`.api/api-geometrygym-ifc`) and the host-neutral `AnalysisModel` is its view, while SAF imports/
exports that view as a spreadsheet. The codec is two service interfaces — `IExcelImportService.Import(Stream)`
→ `ExcelModel` and `IExcelExportService.Export(Stream, ExcelModel)` → `ExcelExportResult` — over a
FLAT model: `ExcelModel.Objects` is one `IReadOnlyList<IExcelModuleObject>` bag discriminated by
concrete SAF type (`is ExcelStructuralCurveMember`), not a per-element typed collection, so the
Bim projection folds the bag by runtime type exactly as `AnalysisProjection.Project` folds the IFC
`IfcStructuralItem` set. Every dimensioned field is a `UnitsNet` quantity (`Length`, `Angle`,
`Pressure`, `Density`, `Force`, `ForcePerLength`, `RotationalStiffness`, `AreaMomentOfInertia`),
so the SAF object model is QUANTITY-ISOMORPHIC to the `VividOrange.Loads` taxonomy
(`.api/api-vividorange-loads`): `ExcelStructuralPointAction : ExcelStructuralPointLoadBase<Force,…>`
↔ `PointForce`, `ExcelStructuralCurveAction` (`ForcePerLength`) ↔ `LineForce`, and the SAF
combination/national-code enums are the image of the `VividOrange.Cases` factory set
(`.api/api-vividorange-cases`). ABI/RUNTIME: the package is `netstandard2.0`-only (binds forward
under `net10.0`) and transitively references `UnitsNet 4.72.0`, `EPPlus 4.5.3.3` (the last LGPL
EPPlus before the v5 Polyform-Noncommercial relicense), and `FluentValidation 10.2.3` — but the
workspace centrally pins `UnitsNet 5.75.0`, so SAF (compiled against UnitsNet 4.x) runs against
UnitsNet 5.x; the UnitsNet 4→5 break makes the bound quantity surface a real verification point
(confirm the `Length`/`Pressure`/`Force` typed-struct surface binds, or the SAF read/write of those
properties faults at runtime).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `StructuralAnalysisFormat`
- package: `StructuralAnalysisFormat`
- version: `1.7.3`
- license: Apache-2.0 (`requireLicenseAcceptance=true`); project `https://www.saf.guide`
- assembly: `SAF.DataAccess.Contracts` (services), `SAF.DataAccess.Models` (object model), `SAF.DataAccess.Excel` (XLSX wrapper), `SAF.DataAccess.Implementation` (service impls), `SAF.DataAccess.Validation` (FluentValidation rules), `SAF.Infrastructure` (events/config/bootstrap) — `ref/` ships `Contracts`/`Models`/`Infrastructure` as the compile surface
- namespace: `SAF.DataAccess.Contracts`; `SAF.DataAccess.Models` (+ `.StructuralElements`, `.Loads`, `.Libraries`, `.Results`, `.Subtypes`, `.Enums`, `.Interfaces`, `.Infrastructure`, `.Extensions`); `SAF.DataAccess.Excel`; `SAF.DataAccess.Validation`; `SAF.Infrastructure`
- asset: `netstandard2.0` ONLY — the `net10.0` consumer binds `lib/netstandard2.0` (no .NET-Core asset ships; single TFM, no resolution ambiguity)
- asset: pure-managed IL-only; no native binaries; ALC-safe inside the in-Rhino plugin assembly
- dependency: `EPPlus 4.5.3.3` (LGPL XLSX engine), `FluentValidation 10.2.3` (validation engine), `UnitsNet 4.72.0` (quantity payload) — [ABI] the workspace pins `UnitsNet 5.75.0`; the consumer binds 5.x while SAF compiled against 4.x (verify the typed-quantity surface)
- rail: saf-exchange

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: codec services and configuration (`SAF.DataAccess.Contracts`)
- rail: saf-exchange

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY]             | [RAIL]                                            |
| :-----: | :---------------------------------- | :----------------------- | :----------------------------------------------- |
|  [01]   | `IExcelImportService`               | XLSX → model codec       | `ExcelModel Import(Stream[, Version])`            |
|  [02]   | `IExcelExportService`               | model → XLSX codec       | `ExcelExportResult Export(Stream, ExcelModel, …)` |
|  [03]   | `IExcelValidator`                   | pre-codec validator      | `ValidateForImport`/`ValidateForExport`          |
|  [04]   | `IExcelObjectConfigurator`          | fluent mapping builder   | object→worksheet column configuration            |
|  [05]   | `IExcelObjectConfiguration`         | resolved mapping         | per-object property/header configuration         |
|  [06]   | `IExcelEnumToStringMapper` / `IExcelStringToEnumMapper` | enum codec | SAF cell-text ↔ enum mapping                     |
|  [07]   | `IExcelWorksheetReader` / `IExcelWorksheetWriter` | sheet I/O      | per-worksheet read/write seam                    |

[PUBLIC_TYPE_SCOPE]: model root, base contracts, and receipts (`SAF.DataAccess.Models`)
- rail: saf-exchange

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]             | [RAIL]                                                       |
| :-----: | :----------------------- | :----------------------- | :--------------------------------------------------------- |
|  [01]   | `ExcelModel`             | model root (`sealed`)    | `Objects`, `ValidationErrors`, `SystemOfUnits`, `OriginalVersion` |
|  [02]   | `IExcelModuleObject`     | wire-row base            | `RowNumber`, `ObjectGrouping`, `ObjectName`, `ObjectIdentifier` |
|  [03]   | `IExcelObject`           | identified object        | `: IExcelHasUniqueName, IExcelModuleObject` + `Guid Id`     |
|  [04]   | `ExcelObjectBase`        | object base (`abstract`) | `Name`, `Guid Id`, overridden `ObjectIdentifier`            |
|  [05]   | `ExcelExportResult`      | export receipt (`sealed`)| `IsSuccess`, `ExcelModel Model`, `ValidationResults`        |
|  [06]   | `ExcelValidationResult`  | validation receipt       | `Identifier`, `ValidationResults`, `Severity`, `static Format(...)` |
|  [07]   | `ExcelModelInformation` / `ExcelProjectInformation` | header rows | model + project metadata objects                |
|  [08]   | `IExcelHasNodeCoordinates`/`HasTranslationConstraints<T>`/`HasRotationConstraints<T>`/`HasLoadDirectionVector<T>` | capability markers | generic, quantity-parameterized object traits   |

[PUBLIC_TYPE_SCOPE]: structural-element model (`SAF.DataAccess.Models.StructuralElements`)
- rail: saf-exchange

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY]            | [RAIL]                                                       |
| :-----: | :---------------------------------------- | :----------------------- | :--------------------------------------------------------- |
|  [01]   | `ExcelStructuralPointConnection`          | node (`sealed`)          | `Length? X/Y/Z`; ↔ `AnalysisMember.PointConnection`        |
|  [02]   | `ExcelStructuralCurveMember`              | 1D beam/column           | `CrossSection`, `ExcelFlexibleEnum<ExcelMember1DType> Type`, `Nodes`, `Segments`, `Length`, eccentricities (`Length`), `Angle`; ↔ `AnalysisMember.Curve` |
|  [03]   | `ExcelStructuralSurfaceMember`            | 2D slab/wall             | `Material`, `ExcelFlexibleEnum<ExcelMember2DType> Type`, `ExcelMemberThickness Thickness`, `Nodes`, `EdgeShapes`; ↔ `AnalysisMember.Surface` |
|  [04]   | `ExcelStructuralPointSupport`             | nodal support            | `ExcelConstraintType? Translation{X,Y,Z}Type` + `ForcePerLength? …Stiffness`, rotational (`RotationalStiffness?`), `BoundaryCondition`; ↔ `Support`/`SupportRestraint` |
|  [05]   | `ExcelStructuralEdgeConnection` / `ExcelStructuralSurfaceConnection` / `ExcelStructuralCurveConnection` | line/area boundary | edge/surface support + connection                |
|  [06]   | `ExcelStructuralSurfaceMemberOpening` / `…Region` | surface sub-feature | opening + sub-region of a 2D member               |
|  [07]   | `ExcelRelConnectsRigidLink`/`RigidMember`/`RigidCross`/`StructuralMember`/`SurfaceEdge` | connection relations | ↔ `MemberConnection` edges                       |
|  [08]   | `ExcelStructuralStorey` / `ExcelStructuralProxyElement` | level/proxy   | storey grouping + opaque proxy element            |

[PUBLIC_TYPE_SCOPE]: load model (`SAF.DataAccess.Models.Loads`)
- rail: saf-exchange

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY]            | [RAIL]                                                       |
| :-----: | :---------------------------------------- | :----------------------- | :--------------------------------------------------------- |
|  [01]   | `ExcelStructuralLoadCase`                 | load case               | `ExcelActionType? ActionType`, `ExcelLoadCaseType? LoadType`, `LoadGroup`, `Duration`; ↔ `ILoadCase` |
|  [02]   | `ExcelStructuralLoadCombination`          | load combination        | `Category`, `ExcelLoadCaseCombinationStandard? NationalStandard`, `double?[] LoadFactors`/`LoadMultipliers`, `string[] LoadCases`; ↔ `ILoadCombination` |
|  [03]   | `ExcelStructuralLoadGroup`                | load group              | grouping over cases; ↔ `LoadGroup` topology                |
|  [04]   | `ExcelStructuralPointAction`              | point force             | `: ExcelStructuralPointLoadBase<Force,…>` + `ExcelLoadDirectionVector<Force>`; ↔ `PointForce` |
|  [05]   | `ExcelStructuralPointMoment`              | point moment            | point moment action; ↔ `PointMoment`                       |
|  [06]   | `ExcelStructuralPointSupportDeformation`  | prescribed displacement | nodal imposed deformation; ↔ `PointDisplacement`           |
|  [07]   | `ExcelStructuralCurveAction` (+ `Free`/`Thermal`/`Moment`) | line load | `ForcePerLength? Value/Value1/Value2`, `ExcelLoadDirectionVector<ForcePerLength>`; ↔ `LineForce` |
|  [08]   | `ExcelStructuralSurfaceAction` (+ `Distribution`/`Free`/`Thermal`) | area load | surface pressure action; ↔ `AreaForce`            |

[PUBLIC_TYPE_SCOPE]: libraries, results, and value subtypes
- rail: saf-exchange

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY]            | [RAIL]                                                       |
| :-----: | :---------------------------------- | :----------------------- | :--------------------------------------------------------- |
|  [01]   | `Libraries.ExcelStructuralMaterial` | material library row     | `ExcelMaterialType? Type`, `Quality`, `Density? UnitMass`, `Pressure? EModulus`/`GModulus`, `PoissonCoefficient`, `CoefficientOfThermalExpansion?` |
|  [02]   | `Libraries.ExcelStructuralCrossSection` | section library row  | `Material`, `ExcelCrossSectionType?`, `ExcelProfileLibraryId? Shape`, `Length[] Parameters`, `Area?`/`AreaMomentOfInertia?`/`WarpingMomentOfInertia?`/`Volume?` properties, `FormCode`/`DescriptionId` |
|  [03]   | `Results.ExcelResultInternalForce1D` / `…2D` | result table     | solver internal-force result rows                          |
|  [04]   | `Subtypes.ExcelFlexibleEnum<T>`     | open-enum wrapper        | `T Value`, `IsOther`, ctor `(T)`/`(string other)`, explicit `→ T`/`T?` |
|  [05]   | `Subtypes.ExcelLoadDirectionVector<TQuantity>` | typed direction | quantity-typed load direction vector                      |
|  [06]   | `Subtypes.ExcelMemberThickness` / `ExcelCurveShape` / `ExcelPoint2D` / `ExcelPolygonContour` / `ExcelCompositeShapeDef` | geometry subtypes | thickness, curve segment, 2D point, polygon contour, composite section |
|  [07]   | `Extensions.UnitsNetExtensions`     | UnitsNet helpers         | `CreateUnit<TUnit>`, `UnitsNetEquals<TUnit>`, `UnitsNetSequenceEquals<TUnit>` |

[PUBLIC_TYPE_SCOPE]: SAF enum vocabulary (`SAF.DataAccess.Models.Enums`) — the boundary axes
- rail: saf-exchange

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]             | [RAIL]                                                       |
| :-----: | :-------------------------------- | :------------------------ | :--------------------------------------------------------- |
|  [01]   | `ExcelSystemOfUnits`              | units regime              | `Metric` / `Imperial` (`ExcelModel.SystemOfUnits`)         |
|  [02]   | `ExcelNationalCode`               | design-code axis          | `EC_DIN_EN`/`EC_NF_EN`/`EC_UNI_EN`/… + `IBC`/`NBR`/`SIA_26x`; ‖ `NationalAnnex` |
|  [03]   | `ExcelActionType`                 | action nature             | `Permanent`/`Variable`/`Accidental` (= `ActionClass`)      |
|  [04]   | `ExcelLoadCaseType`               | load-case nature          | `SelfWeight`/`Prestress`/`Wind`/`Snow`/`Seismic`/`Temperature`/… |
|  [05]   | `ExcelLoadCaseCombinationCategory`| limit-state category      | `UltimateLimitState`/`ServiceabilityLimitState`/`AccidentalLimitState`/`AccordingNationalStandard` |
|  [06]   | `ExcelLoadCaseCombinationStandard`| combination clause        | `EnUlsSetB`/`EnUlsSetC`/`EnAccidental1`/`2`/`EnSeismic`/`EnSlsCharacteristic`/`Frequent`/`QuasiPermanent`/`Ibc*` |
|  [07]   | `ExcelMaterialType`               | material family           | `Concrete`/`Steel`/`Timber`/`Aluminium`/`Masonry`/`Other`  |
|  [08]   | `ExcelMember1DType` / `ExcelMember2DType` | member role       | `Beam`/`Column`/`Rafter`/… (1D), slab/wall/plate (2D)      |
|  [09]   | `ExcelConstraintType`             | support DOF nature        | free/rigid/flexible per translational/rotational DOF       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: XLSX codec
- rail: saf-exchange

| [INDEX] | [SURFACE]                                                            | [ENTRY_FAMILY]    | [RAIL]                                                  |
| :-----: | :------------------------------------------------------------------ | :---------------- | :----------------------------------------------------- |
|  [01]   | `IExcelImportService.Import(Stream)`                                | XLSX read         | one `.xlsx` stream → `ExcelModel` (default version)     |
|  [02]   | `IExcelImportService.Import(Stream, Version targetVersion)`         | versioned read    | read upgrading/downgrading to a target SAF version     |
|  [03]   | `IExcelExportService.Export(Stream, ExcelModel)`                    | XLSX write        | model → `.xlsx` stream → `ExcelExportResult`            |
|  [04]   | `IExcelExportService.Export(Stream, ExcelModel, Version, [Version, ExcelModelValidity])` | versioned write | target/source-version and validity-gated export        |
|  [05]   | `IExcelValidator.ValidateForImport(ExcelModel, Version, Version)`   | import gate       | pre-import FluentValidation pass                       |
|  [06]   | `IExcelValidator.ValidateForExport(ExcelModel, Version, Version)`   | export gate       | pre-export FluentValidation pass                       |

[ENTRYPOINT_SCOPE]: model construction, receipts, and UnitsNet helpers
- rail: saf-exchange

| [INDEX] | [SURFACE]                                                            | [ENTRY_FAMILY]    | [RAIL]                                                  |
| :-----: | :------------------------------------------------------------------ | :---------------- | :----------------------------------------------------- |
|  [01]   | `new ExcelModel(IReadOnlyCollection<IExcelModuleObject>, IReadOnlyCollection<ExcelValidationResult>, ExcelSystemOfUnits)` | model ctor | assemble a SAF model for export                         |
|  [02]   | `ExcelModel.Objects`                                                | heterogeneous bag | `IReadOnlyList<IExcelModuleObject>`, filtered by concrete type |
|  [03]   | `ExcelModel.SystemOfUnits` / `.OriginalVersion` / `.ValidationErrors` | model facts     | units regime, source SAF version, decode errors        |
|  [04]   | `ExcelExportResult.IsSuccess` / `.Model` / `.ValidationResults`     | export receipt    | export outcome + revalidated model + messages          |
|  [05]   | `ExcelValidationResult.Format(IReadOnlyList<ExcelValidationResult>)`| message render    | flatten validation receipts to a report string         |
|  [06]   | `UnitsNetExtensions.CreateUnit<TUnit>(this double) where TUnit : IQuantity` | scalar → quantity | lift a SAF cell scalar to a typed `UnitsNet` quantity   |
|  [07]   | `ExcelFlexibleEnum<T>` ctor `(T)` / `(string other)`, explicit `→ T`/`T?` | open enum   | known SAF enum value OR arbitrary "other" cell text     |

## [04]-[IMPLEMENTATION_LAW]

[SAF_TOPOLOGY]:
- codec services: `IExcelImportService` (XLSX → `ExcelModel`), `IExcelExportService` (`ExcelModel` → XLSX, returns `ExcelExportResult`), `IExcelValidator` (`ValidateForImport`/`ValidateForExport`; the bare `Validate` is `[Obsolete]`); the SAF SDK resolves these through its `SAF.Infrastructure.Bootstrapping.IBootstrapper` container — the `SAF.DataAccess.Implementation`/`SAF.DataAccess.Excel` impls are wiring, not the consumed surface, and `IExcelDocumentReader`/`IExcelDocumentWriter` are internal
- model shape: `ExcelModel` is a FLAT `IReadOnlyList<IExcelModuleObject>` bag — every object derives `ExcelObjectBase : IExcelObject` (`Name`, `Guid Id`, `ObjectIdentifier`); a consumer discriminates by concrete type, and the wire-row metadata (`RowNumber`, `ObjectGrouping`, `ObjectName`) keys the spreadsheet position
- quantities: every dimensioned field is a `UnitsNet` struct — `Length` (coordinates, eccentricities), `Angle` (rotations), `Force`/`ForcePerLength`/`Pressure` (loads), `Density`/`Pressure`/`CoefficientOfThermalExpansion` (materials), `Area`/`AreaMomentOfInertia`/`WarpingMomentOfInertia`/`Volume` (section properties), `RotationalStiffness`/`ForcePerLength` (support springs)
- open enums: `ExcelFlexibleEnum<T> where T : struct, Enum` wraps a known enum value OR an arbitrary `string` (`IsOther`), so a non-standard member type or action type round-trips without data loss

[LOCAL_ADMISSION]:
- SAF is the XLSX EXCHANGE wire for `Model/structural#ANALYSIS_MODEL`, never the canonical model — the GeometryGym `IfcStructuralAnalysisModel` graph is the in-memory authority and `AnalysisModel` is the host-neutral view; `ExcelModel` round-trips that view to/from `.xlsx`
- the Bim projection folds `ExcelModel.Objects` by concrete type exactly as `AnalysisProjection.Project` folds the IFC `IfcStructuralItem` set: `ExcelStructuralCurveMember`→`AnalysisMember.Curve`, `ExcelStructuralSurfaceMember`→`Surface` (its `ExcelMemberThickness`→`Surface.Thickness`), `ExcelStructuralPointConnection`→`PointConnection`, `ExcelStructuralPointSupport`→`Support`/`SupportRestraint` (the `ExcelConstraintType` DOF rows ‖ the IFC `IsFixed` six-DOF predicate), `ExcelStructuralLoadCase`/`LoadCombination`/`LoadGroup`→`LoadGroup` + `StructuralLoadKind`, the `ExcelRelConnects*`→`MemberConnection` edges
- a SAF read/write fault, an unmapped object type, or a validation `Error` lowers onto `Model/faults#FAULT_BAND` `BimFault.CodecReject`/`ModelRejected` via `.ToError()`, reading `ExcelValidationResult.Severity`; never an exception across the fold
- the `ExcelModel.SystemOfUnits` (`Metric`/`Imperial`) and `OriginalVersion` are decode context — coerce every SAF quantity to SI-base on ingest (the `.api/api-unitsnet` `ToUnit(UnitSystem.SI)` path) so the Bim graph is unit-normalized regardless of the SAF workbook regime

[STACKING]:
- with `UnitsNet` (`.api/api-unitsnet`): SAF and the Bim `QuantitySet` share the quantity rail — `UnitsNetExtensions.CreateUnit<TUnit>` lifts a SAF cell scalar to a typed quantity, and ingest coerces to SI-base through `ToUnit(UnitSystem.SI)`; [ABI] SAF binds `UnitsNet 4.72.0` while the workspace pins `5.75.0`, so the bound `Length`/`Pressure`/`Force` typed-struct surface is a verification point (the UnitsNet 4→5 break can shift the public surface SAF reads/writes)
- with `VividOrange.Loads` (`.api/api-vividorange-loads`): the SAF load model is QUANTITY-ISOMORPHIC — `ExcelStructuralPointAction : ExcelStructuralPointLoadBase<Force,…>` ↔ `PointForce`, `ExcelStructuralCurveAction` (`ForcePerLength`) ↔ `LineForce`, `ExcelStructuralSurfaceAction` ↔ `AreaForce`, `ExcelStructuralPointMoment` ↔ `PointMoment`, `ExcelStructuralPointSupportDeformation` ↔ `PointDisplacement`; the SAF↔VividOrange round-trip is a per-component `UnitsNet` map, never a scalar reinterpretation
- with `VividOrange.Cases` (`.api/api-vividorange-cases`): `ExcelStructuralLoadCombination.NationalStandard` is `ExcelLoadCaseCombinationStandard` whose members (`EnUlsSetB`/`EnUlsSetC`/`EnAccidental*`/`EnSeismic`/`EnSlsCharacteristic`/`Frequent`/`QuasiPermanent`) are the exact image of `ENCombinationFactory.CreateStrGeoSetB`/`SetC`/`CreateAccidental`/`CreateSeismic`/`CreateCharacteristic`/`CreateFrequent`/`CreateQuasiPermanent`; `ExcelActionType` = `ActionClass`, `ExcelLoadCaseCombinationCategory` = the ULS/SLS limit-state split — so a Eurocode combination set exports to SAF with the clause preserved
- with `VividOrange.Countries` (`.api/api-vividorange-countries`): `ExcelNationalCode` (`EC_DIN_EN`/`EC_NF_EN`/…) is the SAF design-code axis a model's `ICountry` + `NationalAnnex` map onto at the XLSX boundary
- with `GeometryGymIFC_Core` (`.api/api-geometrygym-ifc`): SAF and the IFC `IfcStructuralAnalysisModel` graph are the two wires that MEET at `AnalysisModel` — IFC is the in-memory semantic authority, SAF is the spreadsheet exchange; both fold onto the one host-neutral `AnalysisMember`/`Support`/`LoadGroup` record set, never two parallel structural models
- with `Thinktecture.Runtime.Extensions` (`.api/api-thinktecture-json`): the SAF `Excel*` enums are the boundary vocabulary mapped onto the canonical `Model/structural` `StructuralLoadKind`/`StructuralCurveMemberKind` `[SmartEnum]` discriminants at ingest, never re-exported as the canonical Bim shape
- with `LanguageExt.Core`: `IExcelExportService.Export` returns an `ExcelExportResult` with `IsSuccess` + `ValidationResults` (not an exception); the Bim codec maps a failed result onto `BimFault`/`Fin<T>` through `.ToError()`, threading the `ExcelValidationResult` rows as the diagnostic

[RAIL_LAW]:
- Package: `StructuralAnalysisFormat`
- Owns: the SAF XLSX structural-analysis exchange codec and the SAF object model (`netstandard2.0`, Apache-2.0)
- Accept: SAF as the exchange wire for `Model/structural` `AnalysisModel`; the flat `ExcelModel.Objects` bag folded by concrete type; quantities normalized to SI-base on ingest; export outcomes read off `ExcelExportResult`
- Reject: SAF as the canonical structural model (the authority is the IFC graph + `AnalysisModel`), a per-element-type SAF collection mirror, a quantity round-trip that reinterprets a scalar instead of mapping the `UnitsNet` unit, exception-driven codec control flow, and an unverified UnitsNet 4↔5 binding assumption on the SAF quantity surface
