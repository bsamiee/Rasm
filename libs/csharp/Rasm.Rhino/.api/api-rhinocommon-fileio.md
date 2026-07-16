# [RASM_RHINO_API_RHINOCOMMON_FILEIO]

`File3dm` owns standalone `.3dm` archives outside a live document: filtered and metadata-only reads, table mutation, write policy, nullable byte serialization, and preview ownership. Direct format engines accept typed option carriers for document-attached interchange, `FilePdf` owns vector page authoring, and `RhinoDoc` owns import, export, save, template, and general write lifecycles.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `RhinoCommon`
- package: `RhinoCommon`
- license: proprietary host SDK
- namespace: `Rhino.FileIO`, `Rhino.DocObjects` (`EarthAnchorPoint`), `Rhino` (`RhinoDoc` document-attached I/O)
- asset: `RhinoCommon.dll` — the in-process managed host assembly
- rail: host

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the standalone archive
- rail: host

| [INDEX] | [SYMBOL]                         | [KIND]     | [CAPABILITY]                                                             |
| :-----: | :------------------------------- | :--------- | :----------------------------------------------------------------------- |
|  [01]   | `File3dm`                        | class      | document-free `.3dm` archive; filtered, metadata, byte, and table access |
|  [02]   | `File3dmWriteOptions`            | class      | archive write policy; version, user data, render and analysis meshes     |
|  [03]   | `File3dmSettings`                | class      | archive-level document settings                                          |
|  [04]   | `File3dm.TableTypeFilter`        | flags enum | bounds a partial read to the required tables                             |
|  [05]   | `File3dm.ObjectTypeFilter`       | flags enum | bounds a partial read to the required object kinds                       |
|  [06]   | `ManifestTable`                  | table      | archive component manifest                                               |
|  [07]   | `File3dmObjectTable`             | table      | archive object collection                                                |
|  [08]   | `File3dmLayerTable`              | table      | archive layer collection                                                 |
|  [09]   | `File3dmMaterialTable`           | table      | archive material collection                                              |
|  [10]   | `File3dmGroupTable`              | table      | archive group collection                                                 |
|  [11]   | `File3dmInstanceDefinitionTable` | table      | archive block definitions read by the block catalog                      |
|  [12]   | `File3dmViewTable`               | table      | archive model and named views                                            |
|  [13]   | `File3dmEmbeddedFiles`           | table      | files embedded in the archive                                            |
|  [14]   | `File3dmRenderMaterials`         | table      | archive render materials                                                 |
|  [15]   | `File3dmRenderEnvironments`      | table      | archive render environments                                              |
|  [16]   | `File3dmRenderTextures`          | table      | archive render textures                                                  |
|  [17]   | `EarthAnchorPoint`               | class      | earth-to-model georeference read from the header                         |

[PUBLIC_TYPE_SCOPE]: the direct format-engine roster
- rail: host

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

Direct engines receive their typed option carrier directly; `RhinoDoc.Import`/`Export`/`ExportSelected` instead receive `ArchivableDictionary?`, and only option types that declare `ToDictionary()` project into that lane. `FileObjReadOptions(FileReadOptions)`, `FileObjWriteOptions(FileWriteOptions)`, and `FilePlyWriteOptions(FileWriteOptions)` consume the shared host carriers without declaring `ToDictionary()`.

[PUBLIC_TYPE_SCOPE]: the per-format option parameter surface
- rail: host

- `FileDwgWriteOptions` nested enums — `AutocadVersion` (`Release12`, `Release13`, `Release14`, `Acad2000`, `Acad2004`, `Acad2007`, `Acad2010`, `Acad2013`, `Acad2018`; default `Acad2018`), `ExportMeshMode` (`Meshes`, `ThreeDFace`), `ExportSurfaceMode` (`Solids`, `Curves`, `Meshes`; default `Curves`), `ExportLineMode` (`Lines`, `Polylines`, `Splines`, `ThreeDPolylines`), `ExportArcMode` (`Lines`, `Arcs`, `Polybulges`, `Polylines`, `Splines`, `ThreeDPolylines`; default `Arcs`), `ExportSplineMode` (`Lines`, `Polylines`, `Splines`, `ThreeDPolylines`; default `Splines`), `ExportPolylineMode` (`Lines`, `Polylines`, `Splines`, `ThreeDPolylines`; default `Polylines`), `ExportPolycurveMode` (`Lines`, `Polybulges`, `Polylines`, `Splines`, `ThreeDPolylines`; default `Splines`), `FlattenMode` (`None`, `Cplane`, `View`), `ColorMethodType` (`ACI`, `RGB`; default `RGB`), `UseColorType` (`USEDISPLAY`, `USEPRINT`)
- `FileDwgWriteOptions` members — `Name`, `Version`, `SimplifyTolerance` (0.05), `MinPointDistance` (1e-06); the curve-fit gate/value pairs `CurveUseMaxAngle`/`CurveMaxAngleDegrees` (true, 2.0 — `CurveMaxAngleRadians` mirrors the same backing value), `CurveUseChordHeight`/`CurveChordHeight` (false, 0.1), `CurveUseSegmentLength`/`CurveSegmentLength` (false, 1.0); `ExportMeshesAs`, `ExportSurfacesAs`, `ExportLinesAs`, `ExportArcsAs`, `ExportSplinesAs`, `ExportPolylinesAs`, `ExportPolycurvesAs`, `Flatten`, `SplitPolycurves` (true), `SplitSplines`, `Simplify`, `NoDxfHeader`, `IsDefault`, `FullLayerPath` (true), `ColorMethod`, `UseColor`, `PreserveArcNormals` (true), `UseLWPolylines`, `WriteThickCurves`; the copy constructor `FileDwgWriteOptions(FileDwgWriteOptions)`; the dictionary lanes `ToDictionary()`, `AddToDictionary(ArchivableDictionary)`, and `SetNamedParameters(NamedParametersEventArgs)`
- `FileDwgReadOptions` — `MeshPrecisionMode` (`Automatic`, `DoublePrecision`, `SinglePrecision`); `ImportUnreferencedLayers`/`ImportUnreferencedBlocks`/`ImportUnreferencedLinetypes` (all true), `ConvertWidePolylinesToSurfaces`, `IgnoreThickness`, `ConvertRegionsToCurves`, `MakeExtrusions` (true), `MeshPrecision`, `ModelUnits`/`LayoutUnits` (`UnitSystem`, `Millimeters`), `SetLayerMaterialToLayerColor`, `NestLayers`
- `FileObjWriteOptions` nested enums — `AsciiEol` (`Crlf`, `Lf`, `Cr`), `CurveType` (`Polyline`, `Nurbs`; default `Nurbs`), `GeometryType` (`Nurbs`, `Mesh`; default `Mesh`), `ObjObjectNames` (`NoObjects`, `ObjectAsGroup`, `ObjectAsObject`), `ObjGroupNames` (`NoGroups`, `LayerAsGroup`, `GroupAsGroup`), `PolylineExportType` (`Bspline`, `Single`, `Multiple`; default `Bspline`), `VertexWelding` (`Normal`, `Welded`, `Unwelded`), `SubDMeshing` (`Surface`, `ControlNet`), `NGons` (`None`, `Preserve`, `Create`)
- `FileObjWriteOptions` members — `ObjectType`, `ExportObjectNames`, `ExportGroupNameLayerNames`, `EolType`, `TrimCurveType`, `PolylineType`, `MeshType`, `SubDMeshType`, `SubDSurfaceMeshingDensity` (4), `ExportMaterialDefinitions` (true), `UseDisplayColorForMaterial` (true), `ExportTcs` (true), `ExportNormals` (true), `ExportVcs`/`VcsFormat` (false, 0), `ExportOpenMeshes` (true), `UseRenderMeshes`, `SortObjGroups` (true), `MergeNestedGroupingNames`, `MapZtoY`, `SignificantDigits` (17), `WrapLongLines`, `ExportAsTriangles`, `UnderbarMaterialNames`, `UseRelativeIndexing`, the ngon cluster `CreateNgons`/`NgonMode`/`MinNgonFaceCount` (2)/`IncludeUnweldedEdgesInNgons` (true)/`CullUnnecessaryVertexesInNgons` (true), `MeshParameters : MeshingParameters`; `GetTransform()` derives the Z-to-Y basis from `MapZtoY`; `AngleTolRadians` is a readonly field; `UseSimpleDialog` and `ActualFilePathOnMac` are host dialog plumbing
- `FileObjReadOptions` — the group/object routing properties `UseObjGroupsAs : UseObjGsAs` (`IgnoreObjGroups`, `ObjGroupsAsLayers`, `ObjGroupsAsGroups`, `ObjGroupsAsObjects`; default `ObjGroupsAsObjects`) and `UseObjObjectsAs : UseObjOsAs` (`IgnoreObjObjects`, `ObjObjectsAsLayers`, `ObjObjectsAsGroups`, `ObjObjectsAsObjects`; default `IgnoreObjObjects`); `MapYtoZ`, `MorphTargetOnly`, `ReverseGroupOrder`, `IgnoreTextures`, `DisplayColorFromObjMaterial` (true), `Split32BitTextures`; `GetTransform()` mirrors the write-side basis
- `FileIgsWriteOptions` nested enums — `IgesStringTypeMode` (`Unicode`, `BIG5`), `IgeswVersionMode` (`Igv52`=1, `Igv53`; default `Igv52`), `EolMode` (`Crlf`=1, `Cr`, `Lf`; default `Crlf`), `PointObjectsMode` (`PoSeparate`=116, `PoSets`=106; default `PoSeparate`), `MaxDegreeMode` (`MdNoLimit`=0, `Md3`=3, `Md5`=5), `SurfacesMode` (`Srf143`=143, `Srf144`=144, `Srf128`=128; default `Srf143`), `PolySurfacesMode` (`PsrfSeparate`=0, `PsrfUnorderedGroup`=402), `SolidsMode` (`SldSeparate`=0, `Sld184`=184, `SldManifoldBRep`=186, `SldUnorderedGroup`=402), `MeshesMode` (`MeshNone`=0, `Mesh10612`=12, `Mesh10613`=13)
- `FileIgsWriteOptions` members — the header quartet `Author`/`Organization`/`Sender`/`Receiver`, `NotesInStartSection` (true), `Units` (`Millimeters`), `Tolerance` (0.001), `IgesStringType`, `IgesVersion`, `EolType`, `Scale` (1.0), `HideDependentObjects`, `DoublesUseE`, `NoZerosInTSection`, `RenderColorAsIgesColor`, `PointType`; the curve cluster `CurveMaxDegree`, `CompositeCurvesAsSingleBsplines`, `SimplifyCurves`, `FitRationalCurves`, `ClampCurveEndKnots`, `UseParentLabelOnCurves` (true), `ForceBezierKnotsOnCurves`, `FlagDependentCurvesAs03`; the surface cluster `SurfaceType`, `PolySurfaceType`, `MaxSurfaceDegree`, `SolidType`, `MeshType`, `SimplifySurfaces`, `FitRationalSurfaces`, `ClampSurfaceEndKnots`, `UseParentLabelOnSurfaces` (true), `ForceBezierKnotsOnSurfaces`, `FlagDependentSurfacesAs03`, `SplitClosedSurfaces`, `SplitBiPolarSurfaces`, `ForceTrimmedSurfaces`, `WriteNonPlanarUnitNormal` (true); `CatiaVersion`/`CatiaTolsize` (0, 100000.0); `ToDictionary()`
- `FileGltfWriteOptions` — `SubDMeshing` (`Surface`, `ControlNet`); `MapZToY` (true), `ExportMaterials` (true), `CullBackfaces` (true), `UseDisplayColorForUnsetMaterials` (true), `SubDMeshType`, `SubDSurfaceMeshingDensity` (4), `ExportTextureCoordinates` (true), `ExportVertexNormals` (true), `ExportOpenMeshes` (true), `ExportVertexColors`, `ExportLayers`, `UseDracoCompression`; the Draco setters clamp silently — `DracoCompressionLevel` to `[1, 10]`, `DracoQuantizationBitsPosition`/`DracoQuantizationBitsNormal`/`DracoQuantizationBitsTextureCoordinate` to `[8, 32]`; `ToDictionary()`
- `FileFbxWriteOptions` — `ObjectType` (`Nurbs`, `Mesh`; default `Mesh`), `MaterialType` (`Lambert`, `Phong`; default `Phong`), `FileType` (`Binary7`, `Ascii7`, `Binary6`, `Ascii6`; default `Binary7`); `SaveObjectsAs`, `SaveMaterialsAs`, `SaveFileAs`, `SaveViews` (true), `SaveLights` (true), `SaveVertexNormals` (true), `MapRhinoZtoFbxY`, `MeshingParameters`; `ToDictionary()`
- `FileFbxReadOptions` — `Unweld`/`UnweldAngle` (true, 22.5), `ImportMeshesAsSubD`, `ImportLights` (true), `ImportCameras` (true), `MapFbxYtoRhinoZ`; `ToDictionary()`
- `FileStpWriteOptions` — `StepSchema` (`SF_203`, `SF_214`, `SF_214_CC2`, `SF_242`; default `SF_203`); `Schema`, `Export2dCurves`, `ExportBlack` (true), `SplitClosedSurfaces`; `ToDictionary()`
- `FileStpReadOptions` — `JoinSurfaces` (true), `LimitFaces`/`MaxFaceCount` (false, 2000); `ToDictionary()`
- `FileSatWriteOptions` — `SatTypes` (`Default`, `ACIS15`, `ACIS20`, `ACIS30`, `ACIS40`, `AutoCAD`, `MechanicalDesktop`, `Inventor`, `SolidWorks`, `SolidEdge`); `Type`; `ToDictionary()`
- `FileX_TWriteOptions` — `X_T_Types` (`Default`, `Edgecam`, `Mastercam`, `SolidEdge`, `SolidWorks`); `Type`; `ToDictionary()`
- `FileSkpWriteOptions` — `SketchUpVersion` (`SketchUp3` through `SketchUp8`, `SketchUp2013` through `SketchUp2021`; default `SketchUp2021`); `Version`, `ExportPlanarRegionsAsPolygons` (true), `GroupObjects` (true), `MaxAngle` (15.0); `ToDictionary()`
- `FileSkpReadOptions` — `ImportFacesAsMeshes` (true), `ImportCurves`, `JoinEdges` (true), `JoinFaces` (true), `Weld`/`WeldAngle` (true, 22.5), `UseGroupLayers`, `AddObjectsToGroups` (true), `EmbedTexturesInModel`, `UseSketchUpTextureWriter`, `DisplayColorBy` (int mode ordinal); `ToDictionary()`
- `FileSvgReadOptions` — `ImportFillMode` (`AsCurves`, `AsHatches`, `AsTrimmedPlanes`; default `AsCurves`); `RetainGrouping`, `GroupMultiCurvePaths`, `ImportFilledObjectAs`; `ToDictionary()`
- `FileAiReadOptions` / `FileAiWriteOptions` / `FileEpsReadOptions` — each carries a nested `Units` enum (`Inches`, `Centimeters`, `Millimeters`, `Points`) behind its unit property (`AiUnits` on both AI types, `EpsUnits`) plus `PreserveModelScale`, `RhinoScale` (1.0), and its own scale field (`AiScale`, `AIScale` — capital-AI on the write type, `EpsScale`; all 1.0); the write type adds `UseCMYK`, `ExportViewBoundary`, `ExportHatchesAsSolidFills` (true), `OrderLayers`; all three declare `ToDictionary()`
- `FilePdfReadOptions` — `PDF_UNITS` (`inches`, `centimeters`, `millimeters`, `points` — lowercase host spellings); `PreserveModelScale`, `RhinoScale` (1.0), `PdfUnits`, `PDFScale` (1.0), `ImportFillsAsHatches` (true), `LoadText` (true); `ToDictionary()`
- `FileTxtWriteOptions` — `DelimiterMode` (`Comma`, `Semicolon`, `Space`, `Tab`, `Other`); `Delimiter`, `DelimiterCharacter` (','), `Precision` (16), `ExportVertexColors` (true), `SurroundWithDoubleQuotes` (true); `ToDictionary()`
- `FileTxtReadOptions` — `DelimiterMode` adds `Automatic` (the default); `Delimiter`, `DelimiterCharacter` (','), `CreatePointCloud` (true); `ToDictionary()`
- `FileCsvWriteOptions` — twenty-six column booleans `Header` (true), `LayerName` (true), `LayerIndex`, `LayerColor`, `LayerHierarchy` (true), `GroupName`, `GroupIndexes`, `ObjectName` (true), `ObjectColor`, `ObjectID`, `ObjectMaterial`, `ObjectDescription` (true), `SurroundPointsWithDoubleQuotes` (true), `Length`, `Perimeter`, `Area`, `Volume`, `AreaCentroid`, `VolumeCentroid`, `AreaMoments`, `VolumeMoments`, `CumulativeMassProperties`, `AttributesKeys` (true), `AttributesTexts` (true), `ObjectKeys` (true), `ObjectsTexts` (true); `ToDictionary()`
- `FileUsdWriteOptions` — `BlockHandling : USDExportBlockHandling` (namespace-level enum: `SeparateFiles`, `Ignore`, `Embedded`; default `SeparateFiles`), `DefaultLayer` (setter coerces null/empty to `"Default"`), `ModelName`, `ForceMeshes`, `IncludeUserStrings` (true), `MeshingParameters` (unset by default)
- `FileXamlWriteOptions` — `AnimationMode` (`X`, `Y`, `Z`); `UseExistingRenderMeshes` (true), `AddRotationScrollbars`, `UseOriginForRotationCenter` (true), `AddRotationAnimation`, `AnimationAxis`, `MeshingParameters`; `ToDictionary()`
- `FileGHSReadOptions` — `ReadViewType` (`Body`, `Profile`, `Plan`, `Wire`, `Solid`, `Camera`, `Custom`; default `Solid`); `AttachGhsData` (true), `RemoveColinearPoints` (true), `ViewType`
- `FileNwdWriteOptions` — `Version : NavisWorksVersion` (namespace-level enum: `Navisworks2016`, `Navisworks2026`, `NavisworksCache`; default `Navisworks2016`), `MeshingParameters`
- `FileSlcWriteOptions` — `StartPoint` ((0,0,0)), `EndPoint` ((0,0,1)), `SliceDistance` (0.0381), `UseMeshes` (true), `AngleBetweenSegmentsDegrees` (5.0)
- `FileVdaWriteOptions` — the eleven header strings `SendingCompany`, `SendersName`, `TelephoneNumber`, `Address`, `ProjectName`, `ObjectCode`, `Variant`, `Confidentiality`, `DateEffective`, `CompanyName`, `ReceivingDepartment` plus `PointDeviationHairsAsMDI`
- `File3mfWriteOptions` — `Title`, `Designer`, `Description`, `Copyright`, `LicenseTerms`, `Rating`, `MoveOutputToPositiveXYZOctant` (true), and the get-only `Metadata : Dictionary<string, string>`
- `File3dsWriteOptions` — `SaveViews` (true), `SaveLights` (true), `MeshingParameters`; `File3dsReadOptions` — `Unweld`/`UnweldAngle` (true, 22.5), `ImportLights` (true), `ImportCameras` (true)
- `FileDgnReadOptions` — `ImportUnreferencedLayers`/`ImportUnreferencedBlocks` (both false — the DWG counterparts default true), `ImportUnreferencedLineStyles` (true), `ImportViews`, `GroupCellHeaders` (true)
- `FileDstReadOptions` — `ImportJumps`; `FileSwReadOptions` — `ImportPartsAsBlocks`, `RotateYtoZ` (true), `ImportConstructionGeometry`
- `FileStlWriteOptions` — `BinaryFile` (true), `ExportOpenObjects` (true), `MeshingParameters`; `FileStlReadOptions` — `Weld`/`WeldAngle` (true, 22.5), `SplitDisjointMeshes` (true), `STLModelUnits` (`Millimeters`)
- `FileLwoWriteOptions` — `WriteVersion6` (true), `MeshingParameters`; `FileLwoReadOptions` — `Unweld`/`UnweldAngle` (true, 22.5)
- `FileVrmlWriteOptions` — `Version` (1), `ExportTextureCoordinates`, `ExportVertexNormals`, `ExportVertexColors`, `MeshingParameters`; `FileX3dvWriteOptions` — the same roster without `Version`
- `FilePlyWriteOptions` — `ExportASCII` (true), `ExportDoubles`, `ExportNormals` (true), `ExportColors`, `ExportMaterial`, `MeshingParameters`, `UseSimpleDialog` (host dialog plumbing); `FilePlyReadOptions` — `PLYModelUnits` (`Millimeters`)
- `FileRawReadOptions` — `RawModelUnits` (`Millimeters`); `FileRawWriteOptions`, `FileAmfWriteOptions`, `FileCdWriteOptions`, `FileGtsWriteOptions`, `FileUdoWriteOptions`, `FilePovWriteOptions` — `MeshingParameters` only, with `FilePovWriteOptions` adding `ExportAsOneFile` (true)

Exchange-relevant knobs include `File3dsWriteOptions.SaveViews`/`SaveLights`; the AI/EPS model-scale fields; the OBJ and PLY grouping, material, normal, and mesh fields; the DWG surface/color fields; `FileStlWriteOptions.BinaryFile`; the FBX object/view/light/normal fields; the SketchUp grouping field; the VRML/X3DV texture-coordinate and normal fields; the text/CSV column fields; the glTF Draco family; `FileUsdWriteOptions.ForceMeshes`/`IncludeUserStrings`/`BlockHandling`/`ModelName`; and `FileXamlWriteOptions.UseExistingRenderMeshes`.

[PUBLIC_TYPE_SCOPE]: PDF page authoring and document-attached I/O
- rail: host

| [INDEX] | [SYMBOL]                | [KIND]     | [CAPABILITY]                                                               |
| :-----: | :---------------------- | :--------- | :------------------------------------------------------------------------- |
|  [01]   | `FilePdf`               | class      | vector PDF page generation, drawing, and optional-content groups           |
|  [02]   | `FilePdfReadOptions`    | class      | PDF import scale, unit, hatch, and text policy                             |
|  [03]   | `FilePdfEventArgs`      | event args | payload for the static `PreWrite` stamp hook                               |
|  [04]   | `PrintedPageDefinition` | class      | custom printed-page definition for `GetCustomPages`/`SetCustomPages`       |
|  [05]   | `FileWriteOptions`      | class      | document write policy for `RhinoDoc.WriteFile`                             |
|  [06]   | `ViewCaptureSettings`   | class      | page capture input to `FilePdf.AddPage`; the display catalog owns the type |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: archive reads — filtered, metadata, and byte
- rail: host

- runtime-nullable `public static File3dm? File3dm.Read(string path)` / `Read(string path, File3dm.TableTypeFilter tableTypeFilterFilter, File3dm.ObjectTypeFilter objectTypeFilter)`; a missing path throws `FileNotFoundException`, while a native read failure returns `null`
- runtime-nullable `public static File3dm? File3dm.ReadWithLog(string path, out string errorLog)` / `ReadWithLog(string path, File3dm.TableTypeFilter tableTypeFilterFilter, File3dm.ObjectTypeFilter objectTypeFilter, out string errorLog)`
- runtime-nullable `public static File3dm? File3dm.FromByteArray(byte[] bytes)`
- `public static string File3dm.ReadNotes(string path)` / `public static int ReadArchiveVersion(string path)`
- `public static bool File3dm.ReadRevisionHistory(string path, out string createdBy, out string lastEditedBy, out int revision, out DateTime createdOn, out DateTime lastEditedOn)`
- runtime-nullable `public static EarthAnchorPoint? File3dm.ReadEarthAnchorPoint(string path)`; the caller owns the returned disposable value
- `public static void File3dm.ReadApplicationData(string path, out string applicationName, out string applicationUrl, out string applicationDetails)`
- `public static ViewInfo[] File3dm.ReadPageViews(string path)` returns an empty array when the file is absent or the native read fails
- runtime-nullable `public static Bitmap? File3dm.ReadPreviewImage(string path)` / `public static DimensionStyle[]? ReadDimensionStyles(string path)`; an absent path throws for both

[ENTRYPOINT_SCOPE]: archive writes, serialization, and validity
- rail: host

- `public static bool File3dm.WriteOneObject(string path, GeometryBase geometry)` / `public static bool WriteMultipleObjects(string path, IEnumerable<GeometryBase> geometry)` create standalone minimal archives
- `public bool File3dm.Write(string path, int version)` / `public bool Write(string path, File3dmWriteOptions? options)`; `null` options create defaults
- `public bool File3dm.WriteWithLog(string path, int version, out string errorLog)` / `public bool WriteWithLog(string path, File3dmWriteOptions? options, out string errorLog)`
- runtime-nullable `public byte[]? File3dm.ToByteArray()` / `public byte[]? ToByteArray(File3dmWriteOptions? options)`; native serialization failure returns `null`
- `[Obsolete]` `File3dm.IsValid` returns true, and `[Obsolete]` `Audit`/`Polish` do not provide archive validation; `public bool CommonObject.IsValidWithLog(out string log)` is the per-object validity surface
- runtime-nullable `public Bitmap? File3dm.GetPreviewImage()` / `public void SetPreviewImage(Bitmap? image)`; the returned bitmap is caller-owned, `null` clears the stored preview, and a non-null image is copied into native storage during the call

[ENTRYPOINT_SCOPE]: archive tables and write options
- rail: host

- `File3dm.Settings : File3dmSettings` / `Manifest : ManifestTable` / `Objects : File3dmObjectTable`
- `File3dmSettings.ModelUnits : LengthUnit` / `PageUnits : LengthUnit` preserve built-in or custom unit identity and meters-per-unit scale; `ModelUnitSystem`/`PageUnitSystem : UnitSystem` are the enum-only projections
- `File3dm.AllLayers : File3dmLayerTable` / `AllMaterials : File3dmMaterialTable` / `AllGroups : File3dmGroupTable` / `AllInstanceDefinitions : File3dmInstanceDefinitionTable`
- `File3dm.AllViews : File3dmViewTable` / `AllNamedViews : File3dmViewTable` / `EmbeddedFiles : File3dmEmbeddedFiles`
- `File3dm.RenderMaterials : File3dmRenderMaterials` / `RenderEnvironments : File3dmRenderEnvironments` / `RenderTextures : File3dmRenderTextures`
- `File3dm.Strings : File3dmStringTable` — `SetString(string section, string entry, string value)` / `Delete(string section, string entry)`
- `File3dm.Notes : File3dmNotes` (get/set) — `File3dmNotes.Notes : string` writes through the parent archive when attached, beside `IsVisible`/`IsHtml`
- `File3dm.ArchiveVersion : int` / `Revision : int` / `CreatedBy`/`LastEditedBy : string` / `Created`/`LastEdited : DateTime` — the in-memory header identity
- `File3dm.ApplicationName`/`ApplicationUrl`/`ApplicationDetails : string` / `EarthAnchorPoint : EarthAnchorPoint` (get/set, disposable) / `AllDimStyles : File3dmDimStyleTable`
- `File3dm.TableTypeFilter` values: `StartSection`/`Properties`/`Settings`/`Bitmap`/`TextureMapping`/`Material`/`Linetype`/`Layer`/`Group`/`Font`/`Dimstyle`/`Light`/`Hatchpattern`/`SectionStyle`/`Markup`/`PageViewGroup`/`InstanceDefinition`/`ObjectTable`/`Historyrecord`/`UserTable`
- runtime-nullable `public GeometryBase? File3dmObject.Geometry` — the native pointer can be unrealized, so every geometry read guards before dereference; `Name`, `Id`, and `Attributes` stay non-null projections
- `public override void File3dmObjectTable.Add(File3dmObject item)` duplicates a model component and throws `NotSupportedException` when native addition fails
- `public Guid File3dmObjectTable.Add(GeometryBase item, ObjectAttributes? attributes)` dispatches supported geometry kinds and throws `NotSupportedException` for an unsupported kind; no one-argument polymorphic `Add(GeometryBase)` exists
- typed object-table overloads include `AddPoint`, `AddCurve`, `AddExtrusion`, `AddMesh`, `AddBrep`, and `AddSubD`, each with a no-attributes overload and an `ObjectAttributes?` overload returning `Guid`
- runtime-nullable `public ViewInfo? File3dmViewTable.FindName(string name)` / `public void Add(ViewInfo item)` / `public bool Delete(ViewInfo item)`
- `public string File3dmEmbeddedFile.Filename { get; }` / `public bool SaveToFile(string filename)`
- `public int File3dmWriteOptions.Version { get; set; }` admits `0` or `[2, RhinoApp.ExeVersion]`; the constructor defaults to `RhinoApp.ExeVersion`. `public bool SaveUserData { get; set; }` defaults true
- `public void File3dmWriteOptions.EnableRenderMeshes(ObjectType objectType, bool enable)` / `public void EnableAnalysisMeshes(ObjectType objectType, bool enable)` mutate the per-kind flags; render meshes principally apply to brep, extrusion, and SubD, while analysis meshes additionally apply to mesh

[ENTRYPOINT_SCOPE]: document-attached exchange
- rail: host

- `public bool RhinoDoc.Import(string filePath)` / `public bool Import(string filePath, ArchivableDictionary? options)`; `public bool Export(string filePath)` / `public bool Export(string filePath, ArchivableDictionary? options)`; `public bool ExportSelected(string filePath)` / `public bool ExportSelected(string filePath, ArchivableDictionary? options)`
- `public bool RhinoDoc.WriteFile(string path, FileWriteOptions options)` is the general writer with plug-in, locking, temporary-file, and backup handling; `public bool Write3dmFile(string path, FileWriteOptions options)` writes `.3dm` without changing document identity
- `public bool RhinoDoc.Save()` writes `RhinoDoc.Path` and throws `InvalidOperationException` when it is empty
- `public bool RhinoDoc.SaveAs(string file3dmPath)` / `SaveAs(string file3dmPath, int version)` / `SaveAs(string file3dmPath, int version, bool saveSmall, bool saveTextures, bool saveGeometryOnly, bool savePluginData)` / `SaveAs(string file3dmPath, int version, bool saveSmall, bool saveTextures, bool saveGeometryOnly, bool savePluginData, bool useCompression)` update document path on success
- `public bool RhinoDoc.SaveAsTemplate(string file3dmTemplatePath)` / `SaveAsTemplate(string file3dmTemplatePath, int version)` preserve document path and require a `.3dm` extension
- `FileWriteOptions`: `UpdateDocumentPath`, `WriteSelectedObjectsOnly`, `IncludeRenderMeshes`, `IncludePreviewImage`, `IncludeBitmapTable`, `IncludeHistory`, `SuppressDialogBoxes`, `SuppressAllInput`, `WriteGeometryOnly`, `WriteUserData`, `CreateBackupFiles`, `CreateOtherBackupFiles`, and `UseCompression` are public mutable booleans; `WriteAsTemplate` is get-only

[ENTRYPOINT_SCOPE]: direct format engines
- rail: host

- `public static WriteFileResult FileObj.Write(string filename, RhinoDoc doc, FileObjWriteOptions options)` / `public static bool FileObj.Read(string filename, RhinoDoc doc, FileObjReadOptions options)`
- `public static bool File3ds.Write(string path, RhinoDoc doc, File3dsWriteOptions options)` / `public static bool File3ds.Read(string path, RhinoDoc doc, File3dsReadOptions options)`
- `public static bool FileAi.Write(string path, RhinoDoc doc, FileAiWriteOptions options)` / `public static bool FileAi.Read(string path, RhinoDoc doc, FileAiReadOptions options)`
- `public static bool FileDwg.Write(string path, RhinoDoc doc, FileDwgWriteOptions options)` / `public static bool FileDwg.Read(string path, RhinoDoc doc, FileDwgReadOptions options)`
- `public static bool FileFbx.Write(string path, RhinoDoc doc, FileFbxWriteOptions options)` / `public static bool FileFbx.Read(string path, RhinoDoc doc, FileFbxReadOptions options)`
- `public static WriteFileResult FilePly.Write(string filename, RhinoDoc doc, FilePlyWriteOptions options)` / `public static bool FilePly.Read(string path, RhinoDoc doc, FilePlyReadOptions options)`
- `public static bool FileSkp.Write(string filename, RhinoDoc doc, FileSkpWriteOptions options)` / `public static bool FileSkp.Read(string path, RhinoDoc doc, FileSkpReadOptions options)`
- `public static bool FileStl.Write(string path, RhinoDoc doc, FileStlWriteOptions options)` / `public static bool FileStl.Read(string path, RhinoDoc doc, FileStlReadOptions options)`
- `public static bool FileStp.Write(string filename, RhinoDoc doc, FileStpWriteOptions options)` / `public static bool FileStp.Read(string path, RhinoDoc doc, FileStpReadOptions options)`
- `public static bool FileGltf.Write(string filename, RhinoDoc doc, FileGltfWriteOptions options)` / `public static bool FileIgs.Write(string path, RhinoDoc doc, FileIgsWriteOptions options)`
- `public static bool FileNwd.Write(string path, RhinoDoc doc, FileNwdWriteOptions options)` / `public static bool FileUsd.Write(string path, RhinoDoc doc, FileUsdWriteOptions options)` / `public static bool FileX_T.Write(string filename, RhinoDoc doc, FileX_TWriteOptions options)`
- `public static bool FileSvg.Read(string path, RhinoDoc doc, FileSvgReadOptions options)`

[ENTRYPOINT_SCOPE]: PDF page authoring
- rail: host

- runtime-nullable `public static FilePdf? FilePdf.Create()`; the PDF plug-in lookup can return `null`
- `public abstract int FilePdf.AddPage(ViewCaptureSettings settings)` / `public abstract int AddPage(int widthInDots, int heightInDots, int dotsPerInch)`
- `public abstract void FilePdf.DrawText(int pageNumber, string text, double x, double y, float heightPoints, Font onfont, Color fillColor, Color strokeColor, float strokeWidth, float angleDegrees, TextHorizontalAlignment horizontalAlignment, TextVerticalAlignment verticalAlignment)`
- `public abstract void FilePdf.DrawPolyline(int pageNumber, PointF[] polyline, Color fillColor, Color strokeColor, float strokeWidth)`
- `public void FilePdf.DrawLine(int pageNumber, PointF from, PointF to, Color strokeColor, float strokeWidth)`
- `public abstract void FilePdf.DrawBitmap(int pageNumber, Bitmap bitmap, float left, float top, float width, float height, float rotationInDegrees)`
- `public bool FilePdf.LayersAsOptionalContentGroups { get; set; }` — document-level state on the `FilePdf` instance read at emission, never per-page; runtime-nullable `public static event EventHandler<FilePdfEventArgs>? PreWrite`
- `public static PrintedPageDefinition[] FilePdf.GetCustomPages()` / `public static void SetCustomPages(IEnumerable<PrintedPageDefinition>? pages)` — REPLACE semantics over the host-process-global custom-page list, so a writer saves and restores the prior roster; `null` clears the set
- `public abstract void FilePdf.Write(string filename)` / `public abstract void Write(Stream stream)` / `public static bool Read(string path, RhinoDoc doc, FilePdfReadOptions options)`
- `FilePdfReadOptions.PreserveModelScale : bool` / `RhinoScale : double` / `PdfUnits : FilePdfReadOptions.PDF_UNITS` / `PDFScale : double` / `ImportFillsAsHatches : bool` / `LoadText : bool`

## [04]-[IMPLEMENTATION_LAW]

[FILEIO_TOPOLOGY]:
- `File3dm` is document-free: it reads, mutates, serializes, and writes the archive without opening a `RhinoDoc`, and `TableTypeFilter`/`ObjectTypeFilter` bound a partial read to the required tables and object kinds
- the metadata reads (`ReadNotes`, `ReadArchiveVersion`, `ReadRevisionHistory`, `ReadEarthAnchorPoint`, `ReadApplicationData`, `ReadPreviewImage`) touch the header without materializing geometry
- byte serialization through `FromByteArray`/`ToByteArray` is runtime-nullable in both directions, and a consumer admits the value before hashing or dereferencing it
- every direct engine takes a live `RhinoDoc` plus a typed `*Options` carrier and returns a `bool` or `WriteFileResult`; the roster is the one conversion surface, and format selection lives in which engine and options a caller picks, never a re-parsed extension string
- `FilePdf` authors vector pages directly, groups layers as optional content, and stamps every page through the static `PreWrite` hook before `Write` commits the document

[STACKING]:
- `LanguageExt`(`libs/csharp/.api/api-languageext.md`): nullable archive/bitmap/style/byte returns and engine `bool`/`WriteFileResult` outcomes fold to `Option<T>`/`Fin<T>`, while nonempty `ReadWithLog`/`WriteWithLog`/`IsValidWithLog` diagnostics remain typed evidence or fault detail
- `Thinktecture.Runtime.Extensions`(`libs/csharp/.api/api-thinktecture-runtime-extensions.md`): the format-engine roster wraps as a `Union` format vocabulary, `TableTypeFilter`/`ObjectTypeFilter` wrap as flag `SmartEnum` read policy, and the archive write version wraps as a `ValueObject`
- `Hashing`(`libs/csharp/.api/api-hashing.md`): `File3dm.ToByteArray` feeds the `XxHash128` content key the persistence artifact index dedupes archives on

[LOCAL_ADMISSION]:
- standalone archive work enters through `File3dm`; document-attached I/O enters through the `RhinoDoc` operations, and the two paths never fork the same read
- direct conversion enters through the owning engine's `Write`/`Read` with its typed options carrier
- PDF page authoring enters through `FilePdf.Create` then the page and draw surface

[RAIL_LAW]:
- Package: `RhinoCommon`
- Owns: the standalone `.3dm` archive, direct format conversion, PDF page authoring, document-attached exchange
- Accept: filtered, metadata, and byte archive access, typed-option conversion, vector PDF authoring
- Reject: document identity and table mutation, block-definition graph depth, native file-dialog registration
