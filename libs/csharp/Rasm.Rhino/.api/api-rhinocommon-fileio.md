# [RASM_RHINO_API_RHINOCOMMON_FILEIO]

`File3dm` owns the document-free `.3dm` archive — filtered and metadata reads, table mutation, write policy, nullable byte serialization, preview ownership. Each direct format engine folds a live `RhinoDoc` through its typed option carrier, `FilePdf` authors vector pages, and `RhinoDoc` owns import, export, save, template, and general write lifecycles.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `RhinoCommon`
- package: `RhinoCommon` (proprietary host SDK)
- assembly: `RhinoCommon.dll` — in-process managed host assembly
- namespace: `Rhino.FileIO`, `Rhino.DocObjects` (`EarthAnchorPoint`), `Rhino` (`RhinoDoc` document-attached I/O)
- rail: host

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the standalone archive

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY] | [CAPABILITY]                                                             |
| :-----: | :------------------------------- | :------------ | :----------------------------------------------------------------------- |
|  [01]   | `File3dm`                        | class         | document-free `.3dm` archive; filtered, metadata, byte, and table access |
|  [02]   | `File3dmWriteOptions`            | class         | archive write policy; version, user data, render and analysis meshes     |
|  [03]   | `File3dmSettings`                | class         | archive-level document settings                                          |
|  [04]   | `File3dm.TableTypeFilter`        | flags enum    | bounds a partial read to the required tables                             |
|  [05]   | `File3dm.ObjectTypeFilter`       | flags enum    | bounds a partial read to the required object kinds                       |
|  [06]   | `ManifestTable`                  | table         | archive component manifest                                               |
|  [07]   | `File3dmObjectTable`             | table         | archive object collection                                                |
|  [08]   | `File3dmLayerTable`              | table         | archive layer collection                                                 |
|  [09]   | `File3dmMaterialTable`           | table         | archive material collection                                              |
|  [10]   | `File3dmGroupTable`              | table         | archive group collection                                                 |
|  [11]   | `File3dmInstanceDefinitionTable` | table         | archive block definitions read by the block catalog                      |
|  [12]   | `File3dmViewTable`               | table         | archive model and named views                                            |
|  [13]   | `File3dmEmbeddedFiles`           | table         | files embedded in the archive                                            |
|  [14]   | `File3dmRenderMaterials`         | table         | archive render materials                                                 |
|  [15]   | `File3dmRenderEnvironments`      | table         | archive render environments                                              |
|  [16]   | `File3dmRenderTextures`          | table         | archive render textures                                                  |
|  [17]   | `EarthAnchorPoint`               | class         | earth-to-model georeference read from the header                         |

[PUBLIC_TYPE_SCOPE]: the direct format-engine roster — `[DIRECTION]` is the read/write support axis

| [INDEX] | [SYMBOL]               | [DIRECTION]      | [CAPABILITY]                                                |
| :-----: | :--------------------- | :--------------- | :---------------------------------------------------------- |
|  [01]   | `FileObj`              | read + write     | Wavefront OBJ mesh interchange                              |
|  [02]   | `File3ds`              | read + write     | 3D Studio mesh interchange                                  |
|  [03]   | `FileAi`               | read + write     | Adobe Illustrator vector interchange                        |
|  [04]   | `FileDwg`              | read + write     | AutoCAD DWG/DXF interchange                                 |
|  [05]   | `FileFbx`              | read + write     | Autodesk FBX interchange                                    |
|  [06]   | `FileGltf`             | write            | glTF asset export                                           |
|  [07]   | `FileIgs`              | write            | IGES CAD export                                             |
|  [08]   | `FileNwd`              | write            | Navisworks export                                           |
|  [09]   | `FilePly`              | read + write     | Stanford PLY point/mesh interchange                         |
|  [10]   | `FileSkp`              | read + write     | SketchUp interchange                                        |
|  [11]   | `FileStl`              | read + write     | STL mesh interchange                                        |
|  [12]   | `FileStp`              | read + write     | STEP CAD interchange                                        |
|  [13]   | `FileUsd`              | write            | Universal Scene Description export                          |
|  [14]   | `FileSvg`              | read             | SVG vector import                                           |
|  [15]   | `FileX_T`              | write            | Parasolid export                                            |
|  [16]   | `File3mf`              | write            | 3MF additive-manufacturing export                           |
|  [17]   | `FileAmf`              | write            | AMF additive-manufacturing export                           |
|  [18]   | `FileCd`               | write            | CD export                                                   |
|  [19]   | `FileDgn`              | read             | MicroStation DGN import                                     |
|  [20]   | `FileDst`              | read             | DST import                                                  |
|  [21]   | `FileEps`              | read             | Encapsulated PostScript vector import                       |
|  [22]   | `FileGHS`              | read             | GHS import                                                  |
|  [23]   | `FileGts`              | write            | GTS surface export                                          |
|  [24]   | `FileLwo`              | read + write     | LightWave object interchange                                |
|  [25]   | `FilePov`              | write            | POV-Ray scene export                                        |
|  [26]   | `FileSat`              | write            | ACIS SAT export                                             |
|  [27]   | `FileSlc`              | write            | SLC slice export                                            |
|  [28]   | `FileSW`               | read             | SolidWorks part/assembly import                             |
|  [29]   | `FileUdo`              | write            | UDO export                                                  |
|  [30]   | `FileVda`              | write            | VDA-FS export                                               |
|  [31]   | `FileVrml`             | write            | VRML/WRL export                                             |
|  [32]   | `FileX3dv`             | write            | X3DV export                                                 |
|  [33]   | `FileRaw`              | read + write     | RAW triangle interchange                                    |
|  [34]   | `FileTxt`              | read + write     | delimited text interchange                                  |
|  [35]   | `FileCsv`              | write            | CSV property/measurement export                             |
|  [36]   | `FileXamlWriteOptions` | write dictionary | XAML export options; `ToDictionary` feeds `RhinoDoc.Export` |
|  [37]   | `WriteFileResult`      | enum             | direct-engine write outcome                                 |

Each direct engine receives its typed option carrier directly; `RhinoDoc.Import`/`Export`/`ExportSelected` instead receive `ArchivableDictionary?`, and only option types declaring `ToDictionary()` project into that lane. `FileObjReadOptions(FileReadOptions)`, `FileObjWriteOptions(FileWriteOptions)`, and `FilePlyWriteOptions(FileWriteOptions)` consume the shared host carriers without declaring `ToDictionary()`.

[PUBLIC_TYPE_SCOPE]: the per-format option parameter surface — `ToDictionary()` is present on every `*Options` type except the shared-carrier consumers above
- `FileDwgWriteOptions` enums — `AutocadVersion` (`Release12`, `Release13`, `Release14`, `Acad2000`, `Acad2004`, `Acad2007`, `Acad2010`, `Acad2013`, `Acad2018`=default), `ExportMeshMode` (`Meshes`, `ThreeDFace`), `ExportSurfaceMode` (`Solids`, `Curves`=default, `Meshes`), `ExportLineMode` (`Lines`, `Polylines`, `Splines`, `ThreeDPolylines`), `ExportArcMode` (`Lines`, `Arcs`=default, `Polybulges`, `Polylines`, `Splines`, `ThreeDPolylines`), `ExportSplineMode`/`ExportPolylineMode`/`ExportPolycurveMode` over the same `Lines`/`Polylines`/`Splines`/`ThreeDPolylines`/`Polybulges` roster (defaults `Splines`/`Polylines`/`Splines`), `FlattenMode` (`None`, `Cplane`, `View`), `ColorMethodType` (`ACI`, `RGB`=default), `UseColorType` (`USEDISPLAY`, `USEPRINT`)
- `FileDwgWriteOptions` members — `Name`, `Version`, `SimplifyTolerance` (0.05), `MinPointDistance` (1e-06); curve-fit gate/value pairs `CurveUseMaxAngle`/`CurveMaxAngleDegrees` (true, 2.0 — `CurveMaxAngleRadians` mirrors the same backing value), `CurveUseChordHeight`/`CurveChordHeight` (false, 0.1), `CurveUseSegmentLength`/`CurveSegmentLength` (false, 1.0); `ExportMeshesAs`, `ExportSurfacesAs`, `ExportLinesAs`, `ExportArcsAs`, `ExportSplinesAs`, `ExportPolylinesAs`, `ExportPolycurvesAs`, `Flatten`, `SplitPolycurves` (true), `SplitSplines`, `Simplify`, `NoDxfHeader`, `IsDefault`, `FullLayerPath` (true), `ColorMethod`, `UseColor`, `PreserveArcNormals` (true), `UseLWPolylines`, `WriteThickCurves`; `FileDwgWriteOptions(FileDwgWriteOptions)` copy ctor; `AddToDictionary(ArchivableDictionary)`, `SetNamedParameters(NamedParametersEventArgs)`
- `FileDwgReadOptions` — `MeshPrecisionMode` (`Automatic`, `DoublePrecision`, `SinglePrecision`); `ImportUnreferencedLayers`/`ImportUnreferencedBlocks`/`ImportUnreferencedLinetypes` (all true), `ConvertWidePolylinesToSurfaces`, `IgnoreThickness`, `ConvertRegionsToCurves`, `MakeExtrusions` (true), `MeshPrecision`, `ModelUnits`/`LayoutUnits` (`UnitSystem`, `Millimeters`), `SetLayerMaterialToLayerColor`, `NestLayers`
- `FileObjWriteOptions` enums — `AsciiEol` (`Crlf`, `Lf`, `Cr`), `CurveType` (`Polyline`, `Nurbs`=default), `GeometryType` (`Nurbs`, `Mesh`=default), `ObjObjectNames` (`NoObjects`, `ObjectAsGroup`, `ObjectAsObject`), `ObjGroupNames` (`NoGroups`, `LayerAsGroup`, `GroupAsGroup`), `PolylineExportType` (`Bspline`=default, `Single`, `Multiple`), `VertexWelding` (`Normal`, `Welded`, `Unwelded`), `SubDMeshing` (`Surface`, `ControlNet`), `NGons` (`None`, `Preserve`, `Create`)
- `FileObjWriteOptions` members — `ObjectType`, `ExportObjectNames`, `ExportGroupNameLayerNames`, `EolType`, `TrimCurveType`, `PolylineType`, `MeshType`, `SubDMeshType`, `SubDSurfaceMeshingDensity` (4), `ExportMaterialDefinitions` (true), `UseDisplayColorForMaterial` (true), `ExportTcs` (true), `ExportNormals` (true), `ExportVcs`/`VcsFormat` (false, 0), `ExportOpenMeshes` (true), `UseRenderMeshes`, `SortObjGroups` (true), `MergeNestedGroupingNames`, `MapZtoY`, `SignificantDigits` (17), `WrapLongLines`, `ExportAsTriangles`, `UnderbarMaterialNames`, `UseRelativeIndexing`, ngon cluster `CreateNgons`/`NgonMode`/`MinNgonFaceCount` (2)/`IncludeUnweldedEdgesInNgons` (true)/`CullUnnecessaryVertexesInNgons` (true), `MeshParameters : MeshingParameters`; `GetTransform()` derives the Z-to-Y basis from `MapZtoY`; `AngleTolRadians` is a readonly field; `UseSimpleDialog` and `ActualFilePathOnMac` are host dialog plumbing
- `FileObjReadOptions` — group/object routing `UseObjGroupsAs : UseObjGsAs` (`IgnoreObjGroups`, `ObjGroupsAsLayers`, `ObjGroupsAsGroups`, `ObjGroupsAsObjects`=default) and `UseObjObjectsAs : UseObjOsAs` (`IgnoreObjObjects`=default, `ObjObjectsAsLayers`, `ObjObjectsAsGroups`, `ObjObjectsAsObjects`); `MapYtoZ`, `MorphTargetOnly`, `ReverseGroupOrder`, `IgnoreTextures`, `DisplayColorFromObjMaterial` (true), `Split32BitTextures`; `GetTransform()` mirrors the write-side basis
- `FileIgsWriteOptions` enums — `IgesStringTypeMode` (`Unicode`, `BIG5`), `IgeswVersionMode` (`Igv52`=1=default, `Igv53`), `EolMode` (`Crlf`=1=default, `Cr`, `Lf`), `PointObjectsMode` (`PoSeparate`=116=default, `PoSets`=106), `MaxDegreeMode` (`MdNoLimit`=0, `Md3`=3, `Md5`=5), `SurfacesMode` (`Srf143`=143=default, `Srf144`=144, `Srf128`=128), `PolySurfacesMode` (`PsrfSeparate`=0, `PsrfUnorderedGroup`=402), `SolidsMode` (`SldSeparate`=0, `Sld184`=184, `SldManifoldBRep`=186, `SldUnorderedGroup`=402), `MeshesMode` (`MeshNone`=0, `Mesh10612`=12, `Mesh10613`=13)
- `FileIgsWriteOptions` members — header quartet `Author`/`Organization`/`Sender`/`Receiver`, `NotesInStartSection` (true), `Units` (`Millimeters`), `Tolerance` (0.001), `IgesStringType`, `IgesVersion`, `EolType`, `Scale` (1.0), `HideDependentObjects`, `DoublesUseE`, `NoZerosInTSection`, `RenderColorAsIgesColor`, `PointType`; curve cluster `CurveMaxDegree`, `CompositeCurvesAsSingleBsplines`, `SimplifyCurves`, `FitRationalCurves`, `ClampCurveEndKnots`, `UseParentLabelOnCurves` (true), `ForceBezierKnotsOnCurves`, `FlagDependentCurvesAs03`; surface cluster `SurfaceType`, `PolySurfaceType`, `MaxSurfaceDegree`, `SolidType`, `MeshType`, `SimplifySurfaces`, `FitRationalSurfaces`, `ClampSurfaceEndKnots`, `UseParentLabelOnSurfaces` (true), `ForceBezierKnotsOnSurfaces`, `FlagDependentSurfacesAs03`, `SplitClosedSurfaces`, `SplitBiPolarSurfaces`, `ForceTrimmedSurfaces`, `WriteNonPlanarUnitNormal` (true); `CatiaVersion`/`CatiaTolsize` (0, 100000.0)
- `FileGltfWriteOptions` — `SubDMeshing` (`Surface`, `ControlNet`); `MapZToY` (true), `ExportMaterials` (true), `CullBackfaces` (true), `UseDisplayColorForUnsetMaterials` (true), `SubDMeshType`, `SubDSurfaceMeshingDensity` (4), `ExportTextureCoordinates` (true), `ExportVertexNormals` (true), `ExportOpenMeshes` (true), `ExportVertexColors`, `ExportLayers`, `UseDracoCompression`; the Draco setters clamp silently — `DracoCompressionLevel` to `[1, 10]`, `DracoQuantizationBitsPosition`/`DracoQuantizationBitsNormal`/`DracoQuantizationBitsTextureCoordinate` to `[8, 32]`
- `FileFbxWriteOptions` — `ObjectType` (`Nurbs`, `Mesh`=default), `MaterialType` (`Lambert`, `Phong`=default), `FileType` (`Binary7`=default, `Ascii7`, `Binary6`, `Ascii6`); `SaveObjectsAs`, `SaveMaterialsAs`, `SaveFileAs`, `SaveViews` (true), `SaveLights` (true), `SaveVertexNormals` (true), `MapRhinoZtoFbxY`, `MeshingParameters`
- `FileFbxReadOptions` — `Unweld`/`UnweldAngle` (true, 22.5), `ImportMeshesAsSubD`, `ImportLights` (true), `ImportCameras` (true), `MapFbxYtoRhinoZ`
- `FileStpWriteOptions` — `StepSchema` (`SF_203`=default, `SF_214`, `SF_214_CC2`, `SF_242`); `Schema`, `Export2dCurves`, `ExportBlack` (true), `SplitClosedSurfaces`
- `FileStpReadOptions` — `JoinSurfaces` (true), `LimitFaces`/`MaxFaceCount` (false, 2000)
- `FileSatWriteOptions` — `SatTypes` (`Default`, `ACIS15`, `ACIS20`, `ACIS30`, `ACIS40`, `AutoCAD`, `MechanicalDesktop`, `Inventor`, `SolidWorks`, `SolidEdge`); `Type`
- `FileX_TWriteOptions` — `X_T_Types` (`Default`, `Edgecam`, `Mastercam`, `SolidEdge`, `SolidWorks`); `Type`
- `FileSkpWriteOptions` — `SketchUpVersion` (`SketchUp3` through `SketchUp8`, `SketchUp2013` through `SketchUp2021`=default); `Version`, `ExportPlanarRegionsAsPolygons` (true), `GroupObjects` (true), `MaxAngle` (15.0)
- `FileSkpReadOptions` — `ImportFacesAsMeshes` (true), `ImportCurves`, `JoinEdges` (true), `JoinFaces` (true), `Weld`/`WeldAngle` (true, 22.5), `UseGroupLayers`, `AddObjectsToGroups` (true), `EmbedTexturesInModel`, `UseSketchUpTextureWriter`, `DisplayColorBy` (int mode ordinal)
- `FileSvgReadOptions` — `ImportFillMode` (`AsCurves`=default, `AsHatches`, `AsTrimmedPlanes`); `RetainGrouping`, `GroupMultiCurvePaths`, `ImportFilledObjectAs`
- `FileAiReadOptions`/`FileAiWriteOptions`/`FileEpsReadOptions` — each carries a nested `Units` enum (`Inches`, `Centimeters`, `Millimeters`, `Points`) behind its unit property (`AiUnits` on both AI types, `EpsUnits`), `PreserveModelScale`, `RhinoScale` (1.0), and its own scale field (`AiScale`, `AIScale` capital-AI on the write type, `EpsScale`; all 1.0); the write type adds `UseCMYK`, `ExportViewBoundary`, `ExportHatchesAsSolidFills` (true), `OrderLayers`
- `FilePdfReadOptions` — `PDF_UNITS` (`inches`, `centimeters`, `millimeters`, `points` — lowercase host spellings); `PreserveModelScale`, `RhinoScale` (1.0), `PdfUnits`, `PDFScale` (1.0), `ImportFillsAsHatches` (true), `LoadText` (true)
- `FileTxtWriteOptions` — `DelimiterMode` (`Comma`, `Semicolon`, `Space`, `Tab`, `Other`); `Delimiter`, `DelimiterCharacter` (','), `Precision` (16), `ExportVertexColors` (true), `SurroundWithDoubleQuotes` (true)
- `FileTxtReadOptions` — `DelimiterMode` adds `Automatic`=default; `Delimiter`, `DelimiterCharacter` (','), `CreatePointCloud` (true)
- `FileCsvWriteOptions` — column-inclusion booleans `Header` (true), `LayerName` (true), `LayerIndex`, `LayerColor`, `LayerHierarchy` (true), `GroupName`, `GroupIndexes`, `ObjectName` (true), `ObjectColor`, `ObjectID`, `ObjectMaterial`, `ObjectDescription` (true), `SurroundPointsWithDoubleQuotes` (true), `Length`, `Perimeter`, `Area`, `Volume`, `AreaCentroid`, `VolumeCentroid`, `AreaMoments`, `VolumeMoments`, `CumulativeMassProperties`, `AttributesKeys` (true), `AttributesTexts` (true), `ObjectKeys` (true), `ObjectsTexts` (true)
- `FileUsdWriteOptions` — `BlockHandling : USDExportBlockHandling` (namespace-level enum: `SeparateFiles`=default, `Ignore`, `Embedded`), `DefaultLayer` (setter coerces null/empty to `"Default"`), `ModelName`, `ForceMeshes`, `IncludeUserStrings` (true), `MeshingParameters` (unset by default)
- `FileXamlWriteOptions` — `AnimationMode` (`X`, `Y`, `Z`); `UseExistingRenderMeshes` (true), `AddRotationScrollbars`, `UseOriginForRotationCenter` (true), `AddRotationAnimation`, `AnimationAxis`, `MeshingParameters`
- `FileGHSReadOptions` — `ReadViewType` (`Body`, `Profile`, `Plan`, `Wire`, `Solid`=default, `Camera`, `Custom`); `AttachGhsData` (true), `RemoveColinearPoints` (true), `ViewType`
- `FileNwdWriteOptions` — `Version : NavisWorksVersion` (namespace-level enum: `Navisworks2016`=default, `Navisworks2026`, `NavisworksCache`), `MeshingParameters`
- `FileSlcWriteOptions` — `StartPoint` ((0,0,0)), `EndPoint` ((0,0,1)), `SliceDistance` (0.0381), `UseMeshes` (true), `AngleBetweenSegmentsDegrees` (5.0)
- `FileVdaWriteOptions` — header-string fields `SendingCompany`, `SendersName`, `TelephoneNumber`, `Address`, `ProjectName`, `ObjectCode`, `Variant`, `Confidentiality`, `DateEffective`, `CompanyName`, `ReceivingDepartment`, `PointDeviationHairsAsMDI`
- `File3mfWriteOptions` — `Title`, `Designer`, `Description`, `Copyright`, `LicenseTerms`, `Rating`, `MoveOutputToPositiveXYZOctant` (true), get-only `Metadata : Dictionary<string, string>`
- `File3dsWriteOptions` — `SaveViews` (true), `SaveLights` (true), `MeshingParameters`; `File3dsReadOptions` — `Unweld`/`UnweldAngle` (true, 22.5), `ImportLights` (true), `ImportCameras` (true)
- `FileDgnReadOptions` — `ImportUnreferencedLayers`/`ImportUnreferencedBlocks` (both false — the DWG counterparts default true), `ImportUnreferencedLineStyles` (true), `ImportViews`, `GroupCellHeaders` (true)
- `FileDstReadOptions` — `ImportJumps`; `FileSwReadOptions` — `ImportPartsAsBlocks`, `RotateYtoZ` (true), `ImportConstructionGeometry`
- `FileStlWriteOptions` — `BinaryFile` (true), `ExportOpenObjects` (true), `MeshingParameters`; `FileStlReadOptions` — `Weld`/`WeldAngle` (true, 22.5), `SplitDisjointMeshes` (true), `STLModelUnits` (`Millimeters`)
- `FileLwoWriteOptions` — `WriteVersion6` (true), `MeshingParameters`; `FileLwoReadOptions` — `Unweld`/`UnweldAngle` (true, 22.5)
- `FileVrmlWriteOptions` — `Version` (1), `ExportTextureCoordinates`, `ExportVertexNormals`, `ExportVertexColors`, `MeshingParameters`; `FileX3dvWriteOptions` — the same roster without `Version`
- `FilePlyWriteOptions` — `ExportASCII` (true), `ExportDoubles`, `ExportNormals` (true), `ExportColors`, `ExportMaterial`, `MeshingParameters`, `UseSimpleDialog` (host dialog plumbing); `FilePlyReadOptions` — `PLYModelUnits` (`Millimeters`)
- `FileRawReadOptions` — `RawModelUnits` (`Millimeters`); `FileRawWriteOptions`, `FileAmfWriteOptions`, `FileCdWriteOptions`, `FileGtsWriteOptions`, `FileUdoWriteOptions`, `FilePovWriteOptions` — `MeshingParameters` only, `FilePovWriteOptions` adding `ExportAsOneFile` (true)

[PUBLIC_TYPE_SCOPE]: PDF page authoring and document-attached I/O

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [CAPABILITY]                                                               |
| :-----: | :---------------------- | :------------ | :------------------------------------------------------------------------- |
|  [01]   | `FilePdf`               | class         | vector PDF page generation, drawing, and optional-content groups           |
|  [02]   | `FilePdfReadOptions`    | class         | PDF import scale, unit, hatch, and text policy                             |
|  [03]   | `FilePdfEventArgs`      | event args    | payload for the static `PreWrite` stamp hook                               |
|  [04]   | `PrintedPageDefinition` | class         | custom printed-page definition for `GetCustomPages`/`SetCustomPages`       |
|  [05]   | `FileWriteOptions`      | class         | document write policy for `RhinoDoc.WriteFile`                             |
|  [06]   | `ViewCaptureSettings`   | class         | page capture input to `FilePdf.AddPage`; the display catalog owns the type |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: archive reads — static, filtered, metadata, and byte

| [INDEX] | [SURFACE]                                                                                                    | [CAPABILITY]             |
| :-----: | :----------------------------------------------------------------------------------------------------------- | :----------------------- |
|  [01]   | `File3dm.Read(string [, TableTypeFilter, ObjectTypeFilter])` -> `File3dm?`                                   | full or filtered read    |
|  [02]   | `File3dm.ReadWithLog(string [, TableTypeFilter, ObjectTypeFilter], out string)` -> `File3dm?`                | read with diagnostic log |
|  [03]   | `File3dm.FromByteArray(byte[])` -> `File3dm?`                                                                | deserialize from bytes   |
|  [04]   | `File3dm.ReadNotes(string)` -> `string`                                                                      | header notes             |
|  [05]   | `File3dm.ReadArchiveVersion(string)` -> `int`                                                                | archive format version   |
|  [06]   | `File3dm.ReadRevisionHistory(string, out string, out string, out int, out DateTime, out DateTime)` -> `bool` | revision identity        |
|  [07]   | `File3dm.ReadEarthAnchorPoint(string)` -> `EarthAnchorPoint?`                                                | georeference             |
|  [08]   | `File3dm.ReadApplicationData(string, out string, out string, out string)`                                    | authoring app identity   |
|  [09]   | `File3dm.ReadPageViews(string)` -> `ViewInfo[]`                                                              | layout page views        |
|  [10]   | `File3dm.ReadPreviewImage(string)` -> `Bitmap?`                                                              | stored preview           |
|  [11]   | `File3dm.ReadDimensionStyles(string)` -> `DimensionStyle[]?`                                                 | archive dimension styles |

- `File3dm.Read`: a missing path throws `FileNotFoundException`; a native read failure returns `null`.
- `File3dm.ReadPageViews`: an absent file or native failure returns an empty array.
- `File3dm.ReadPreviewImage` / `ReadDimensionStyles`: an absent path throws.
- `File3dm.ReadEarthAnchorPoint`: caller owns the returned disposable value.

[ENTRYPOINT_SCOPE]: archive writes, serialization, and validity

| [INDEX] | [SURFACE]                                                                        | [SHAPE]  | [CAPABILITY]               |
| :-----: | :------------------------------------------------------------------------------- | :------- | :------------------------- |
|  [01]   | `File3dm.WriteOneObject(string, GeometryBase)` -> `bool`                         | static   | standalone minimal archive |
|  [02]   | `File3dm.WriteMultipleObjects(string, IEnumerable<GeometryBase>)` -> `bool`      | static   | standalone minimal archive |
|  [03]   | `File3dm.Write(string, int)` / `Write(string, File3dmWriteOptions?)` -> `bool`   | instance | write archive              |
|  [04]   | `File3dm.WriteWithLog(string, int / File3dmWriteOptions?, out string)` -> `bool` | instance | write with diagnostic log  |
|  [05]   | `File3dm.ToByteArray([File3dmWriteOptions?])` -> `byte[]?`                       | instance | serialize to bytes         |
|  [06]   | `File3dm.GetPreviewImage()` -> `Bitmap?` / `SetPreviewImage(Bitmap?)`            | instance | stored-preview get/set     |
|  [07]   | `CommonObject.IsValidWithLog(out string)` -> `bool`                              | instance | per-object validity        |

- `File3dm.Write` / `WriteWithLog`: `null` options create defaults.
- `File3dm.ToByteArray`: a native serialization failure returns `null`; a consumer admits the value before hashing or dereferencing.
- `File3dm.SetPreviewImage`: `null` clears the stored preview, a non-null image copies into native storage during the call, and the returned bitmap is caller-owned.
- Archive validity is per-object only; no archive-level validity method exists, and a read-time diagnostic comes from the `ReadWithLog`/`WriteWithLog` error log.

[ENTRYPOINT_SCOPE]: archive tables and write options
- table navigation: `File3dm.Settings : File3dmSettings`, `Manifest : ManifestTable`, `Objects : File3dmObjectTable`, `AllLayers : File3dmLayerTable`, `AllMaterials : File3dmMaterialTable`, `AllGroups : File3dmGroupTable`, `AllInstanceDefinitions : File3dmInstanceDefinitionTable`, `AllViews : File3dmViewTable`, `AllNamedViews : File3dmViewTable`, `EmbeddedFiles : File3dmEmbeddedFiles`, `RenderMaterials : File3dmRenderMaterials`, `RenderEnvironments : File3dmRenderEnvironments`, `RenderTextures : File3dmRenderTextures`, `AllDimStyles : File3dmDimStyleTable`
- header identity: `File3dm.ArchiveVersion : int`, `Revision : int`, `CreatedBy`/`LastEditedBy : string`, `Created`/`LastEdited : DateTime`, `ApplicationName`/`ApplicationUrl`/`ApplicationDetails : string`, `EarthAnchorPoint : EarthAnchorPoint` (get/set, disposable)
- `File3dmSettings.ModelUnits`/`PageUnits : LengthUnit` preserve built-in or custom unit identity and meters-per-unit scale; `ModelUnitSystem`/`PageUnitSystem : UnitSystem` are the enum-only projections
- `File3dm.Strings : File3dmStringTable` — `SetString(string, string, string)` / `Delete(string, string)`; `File3dm.Notes : File3dmNotes` (get/set) — `File3dmNotes.Notes : string` writes through the parent archive when attached, beside `IsVisible`/`IsHtml`
- `File3dm.TableTypeFilter` values: `StartSection`, `Properties`, `Settings`, `Bitmap`, `TextureMapping`, `Material`, `Linetype`, `Layer`, `Group`, `Font`, `Dimstyle`, `Light`, `Hatchpattern`, `SectionStyle`, `Markup`, `PageViewGroup`, `InstanceDefinition`, `ObjectTable`, `Historyrecord`, `UserTable`
- `File3dmObject.Geometry : GeometryBase?` — an unrealized native pointer forces a guard before every geometry dereference; `Name`, `Id`, `Attributes` stay non-null projections
- `File3dmObjectTable.Add(File3dmObject)` duplicates a component and throws `NotSupportedException` on native failure; `Add(GeometryBase, ObjectAttributes?)` -> `Guid` dispatches supported geometry kinds and throws `NotSupportedException` for an unsupported kind (no one-argument `Add(GeometryBase)`); typed overloads `AddPoint`, `AddCurve`, `AddExtrusion`, `AddMesh`, `AddBrep`, `AddSubD` each carry a no-attributes and an `ObjectAttributes?` overload returning `Guid`
- `File3dmViewTable.FindName(string)` -> `ViewInfo?` / `Add(ViewInfo)` / `Delete(ViewInfo)` -> `bool`; `File3dmEmbeddedFile.Filename : string` (get) / `SaveToFile(string)` -> `bool`
- `File3dmWriteOptions.Version : int` admits `0` or `[2, RhinoApp.ExeVersion]` (ctor defaults to `RhinoApp.ExeVersion`); `SaveUserData : bool` defaults true; `EnableRenderMeshes(ObjectType, bool)` / `EnableAnalysisMeshes(ObjectType, bool)` mutate per-kind flags — render meshes principally apply to brep, extrusion, and SubD, analysis meshes extend to mesh

[ENTRYPOINT_SCOPE]: document-attached exchange — instance on `RhinoDoc`

| [INDEX] | [SURFACE]                                                                       | [CAPABILITY]                                    |
| :-----: | :------------------------------------------------------------------------------ | :---------------------------------------------- |
|  [01]   | `RhinoDoc.Import(string [, ArchivableDictionary?])` -> `bool`                   | import into document                            |
|  [02]   | `RhinoDoc.Export(string [, ArchivableDictionary?])` -> `bool`                   | export document                                 |
|  [03]   | `RhinoDoc.ExportSelected(string [, ArchivableDictionary?])` -> `bool`           | export selection                                |
|  [04]   | `RhinoDoc.WriteFile(string, FileWriteOptions)` -> `bool`                        | general writer — plug-in, locking, temp, backup |
|  [05]   | `RhinoDoc.Write3dmFile(string, FileWriteOptions)` -> `bool`                     | write `.3dm` without changing document identity |
|  [06]   | `RhinoDoc.Save()` -> `bool`                                                     | write `RhinoDoc.Path`                           |
|  [07]   | `RhinoDoc.SaveAs(string [, int [, bool, bool, bool, bool [, bool]]])` -> `bool` | save-as, updates document path on success       |
|  [08]   | `RhinoDoc.SaveAsTemplate(string [, int])` -> `bool`                             | template save, preserves document path          |

- `RhinoDoc.Save`: an empty `RhinoDoc.Path` throws `InvalidOperationException`.
- `RhinoDoc.SaveAsTemplate`: requires a `.3dm` extension.
- `FileWriteOptions` mutable booleans: `UpdateDocumentPath`, `WriteSelectedObjectsOnly`, `IncludeRenderMeshes`, `IncludePreviewImage`, `IncludeBitmapTable`, `IncludeHistory`, `SuppressDialogBoxes`, `SuppressAllInput`, `WriteGeometryOnly`, `WriteUserData`, `CreateBackupFiles`, `CreateOtherBackupFiles`, `UseCompression`; `WriteAsTemplate` is get-only.

[ENTRYPOINT_SCOPE]: direct format engines — static `Engine.Write`/`Engine.Read`
- `Engine.Write(string, RhinoDoc, <Engine>WriteOptions)` returns `bool`, except `FileObj.Write` and `FilePly.Write` returning `WriteFileResult`; read-capable engines expose `Engine.Read(string, RhinoDoc, <Engine>ReadOptions)` -> `bool`, and roster [02] names each engine's read/write direction. Format selection lives in the chosen engine and its option carrier, never a re-parsed extension string.

[ENTRYPOINT_SCOPE]: PDF page authoring — instance on `FilePdf` except where marked

| [INDEX] | [SURFACE]                                                                  | [SHAPE]  | [CAPABILITY]                     |
| :-----: | :------------------------------------------------------------------------- | :------- | :------------------------------- |
|  [01]   | `FilePdf.Create()` -> `FilePdf?`                                           | static   | plug-in lookup can return `null` |
|  [02]   | `FilePdf.AddPage(ViewCaptureSettings)` / `AddPage(int, int, int)` -> `int` | instance | add a page                       |
|  [03]   | `FilePdf.DrawPolyline(int, PointF[], Color, Color, float)`                 | instance | filled/stroked polyline          |
|  [04]   | `FilePdf.DrawLine(int, PointF, PointF, Color, float)`                      | instance | stroked line                     |
|  [05]   | `FilePdf.DrawBitmap(int, Bitmap, float, float, float, float, float)`       | instance | placed bitmap                    |
|  [06]   | `FilePdf.LayersAsOptionalContentGroups : bool`                             | property | optional-content group emission  |
|  [07]   | `FilePdf.PreWrite : EventHandler<FilePdfEventArgs>?`                       | static   | per-page pre-write stamp hook    |
|  [08]   | `FilePdf.GetCustomPages()` -> `PrintedPageDefinition[]`                    | static   | read custom-page roster          |
|  [09]   | `FilePdf.SetCustomPages(IEnumerable<PrintedPageDefinition>?)`              | static   | replace custom-page roster       |
|  [10]   | `FilePdf.Write(string)` / `Write(Stream)`                                  | instance | commit document                  |
|  [11]   | `FilePdf.Read(string, RhinoDoc, FilePdfReadOptions)` -> `bool`             | static   | PDF import                       |

- `FilePdf.DrawText(int, string, double x, double y, float heightPoints, Font, Color fill, Color stroke, float strokeWidth, float angleDegrees, TextHorizontalAlignment, TextVerticalAlignment)` draws text on one page.
- `FilePdf.LayersAsOptionalContentGroups`: document-level state read at emission, never per-page.
- `FilePdf.SetCustomPages`: REPLACE semantics over the host-process-global custom-page list, so a writer saves and restores the prior roster; `null` clears the set.
- `FilePdfReadOptions`: `PreserveModelScale : bool`, `RhinoScale : double`, `PdfUnits : FilePdfReadOptions.PDF_UNITS`, `PDFScale : double`, `ImportFillsAsHatches : bool`, `LoadText : bool`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `File3dm` is document-free: it reads, mutates, serializes, and writes the archive without opening a `RhinoDoc`, and `TableTypeFilter`/`ObjectTypeFilter` bound a partial read to the required tables and object kinds
- metadata reads (`ReadNotes`, `ReadArchiveVersion`, `ReadRevisionHistory`, `ReadEarthAnchorPoint`, `ReadApplicationData`, `ReadPreviewImage`) touch the header without materializing geometry
- byte serialization through `FromByteArray`/`ToByteArray` is runtime-nullable in both directions, and a consumer admits the value before hashing or dereferencing
- every direct engine folds a live `RhinoDoc` and a typed `*Options` carrier to a `bool` or `WriteFileResult`; the roster is the one conversion surface, and format selection lives in the chosen engine and options
- `FilePdf` authors vector pages directly, groups layers as optional content, and stamps every page through the static `PreWrite` hook before `Write` commits the document

[STACKING]:
- `LanguageExt`(`libs/csharp/.api/api-languageext.md`): nullable archive/bitmap/style/byte returns and engine `bool`/`WriteFileResult` outcomes fold to `Option<T>`/`Fin<T>`, while nonempty `ReadWithLog`/`WriteWithLog`/`IsValidWithLog` diagnostics remain typed evidence or fault detail
- `Thinktecture.Runtime.Extensions`(`libs/csharp/.api/api-thinktecture-runtime-extensions.md`): the format-engine roster wraps as a `Union` format vocabulary, `TableTypeFilter`/`ObjectTypeFilter` wrap as flag `SmartEnum` read policy, and the archive write version wraps as a `ValueObject`
- `Hashing`(`libs/csharp/.api/api-hashing.md`): `File3dm.ToByteArray` feeds the `XxHash128` content key the persistence artifact index dedupes archives on

[LOCAL_ADMISSION]:
- standalone archive work enters through `File3dm`; document-attached I/O enters through the `RhinoDoc` operations, and the two paths never fork the same read
- direct conversion enters through the owning engine's `Write`/`Read` with its typed options carrier
- PDF page authoring enters through `FilePdf.Create` then the page and draw surface
- `Rhino.FileIO.Nrbf` is host-private internal serialization glue with no public entry point; native file-dialog registration (`FileImportPlugIn`/`FileExportPlugIn`/`FileTypeList`/`FileReadOptions`) is owned by `api-rhinocommon-plugins.md`

[RAIL_LAW]:
- Package: `RhinoCommon`
- Owns: the standalone `.3dm` archive, direct format conversion, PDF page authoring, document-attached exchange
- Accept: filtered, metadata, and byte archive access, typed-option conversion, vector PDF authoring
- Reject: document identity and table mutation, block-definition graph depth, native file-dialog registration
