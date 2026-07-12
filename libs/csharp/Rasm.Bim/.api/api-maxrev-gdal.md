# [RASM_BIM_API_MAXREV_GDAL]

`MaxRev.Gdal.Core` owns the SWIG-generated `OSGeo.GDAL`, `OSGeo.OGR`, and `OSGeo.OSR` managed bindings plus the `GdalBase` bootstrap for bundled GDAL and PROJ resources. Its surface covers GeoTIFF, COG, and DEM raster ingest; the universal OGR vector drivers; the GDAL utility algorithms; and PROJ-backed reprojection parallel to `ProjNET`. The managed AnyCPU assembly binds native GDAL, GEOS, and PROJ libraries from a RID-keyed runtime package; Apple Silicon binds `MaxRev.Gdal.MacosRuntime.Minimal.arm64`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `MaxRev.Gdal.Core`
- package: `MaxRev.Gdal.Core`
- license: MIT (the bindings + `GdalBase` bootstrap); the underlying GDAL/OGR/PROJ corpus is its own permissive mix (MIT/X11-style), `gdal-data/LICENSE.TXT` ships in the runtime
- assembly: `MaxRev.Gdal.Core`
- namespace: `MaxRev.Gdal.Core` (`GdalBase` configurator — the bootstrap owner)
- namespace: `OSGeo.GDAL` (`Gdal` statics, `Dataset`, `Band`, `Driver`, `DataType`, `ColorInterp`, the `ColorTable`/`ColorEntry`/`RasterAttributeTable` palette-and-category legend triad, `GDAL*Options` algorithm option carriers)
- namespace: `OSGeo.OGR` (`Ogr` statics, `DataSource`, `Layer`, `Feature`, `Geometry`, `FieldDefn`, `FeatureDefn`, `wkbGeometryType`)
- namespace: `OSGeo.OSR` (`SpatialReference`, `CoordinateTransformation`, `CoordinateTransformationOptions`, `AxisMappingStrategy`)
- asset: net10.0, net9.0, net8.0, net7.0, net6.0, net461, netstandard2.1, netstandard2.0; the net10.0 consumer binds the `lib/net10.0` asset
- asset: IL-only AnyCPU managed assembly. P/Invokes the native `libgdal_wrap`/`libgdalconst_wrap` from the RID-keyed runtime package; the bindings DLL ships NO native code itself
- asset: `runtimes/any/native/gdal-data/**` (the PROJ `proj.db`-adjacent CSV/WKT resource set, ~hundreds of EPSG/datum/grid files) and the `MaxRev.Gdal.Core.targets` that stages them — `GdalBase` reads these for CRS/driver resolution
- dependency: a `MaxRev.Gdal.*Runtime*` package supplies the native `libgdal`/`libgeos`/`libproj`; without a matching RID runtime on the publish target every P/Invoke faults at first call
- rail: geometry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: bootstrap
- package: `MaxRev.Gdal.Core`
- namespace: `MaxRev.Gdal.Core`
- rail: geometry

[GdalBase]:

- Kind: static configurator
- Members: `ConfigureAll()`, `ConfigureGdalDrivers(gdalDataFolder?)`, `ConfigureGdalData(gdalDataFolder?)`, `IsConfigured`, `EnableRuntimeValidation`
- Bootstrap: `ConfigureAll()` registers every GDAL and OGR driver, wires the `gdal-data` and PROJ paths, and runs once before any `Gdal`, `Ogr`, or `Osr` call.

[PUBLIC_TYPE_SCOPE]: GDAL raster
- package: `MaxRev.Gdal.Core`
- namespace: `OSGeo.GDAL`
- rail: geometry

Every raster public type belongs to the geometry rail.

| [INDEX] | [SYMBOL]               | [KIND]          |
| :-----: | :--------------------- | :-------------- |
|  [01]   | `Gdal`                 | static API      |
|  [02]   | `Dataset`              | dataset         |
|  [03]   | `Band`                 | raster band     |
|  [04]   | `Driver`               | format driver   |
|  [05]   | `DataType`             | storage enum    |
|  [06]   | `ColorInterp`          | channel enum    |
|  [07]   | `ColorTable`           | palette         |
|  [08]   | `ColorEntry`           | palette entry   |
|  [09]   | `RasterAttributeTable` | attribute table |
|  [10]   | `GDAL*Options`         | option carriers |

[Gdal]:

- Open: `Open`, `OpenEx`, `OpenShared`
- Drivers: `GetDriverByName`, `IdentifyDriver`, `AllRegister`
- Algorithms: `Warp`, `BuildVRT`, `MultiDimTranslate`, `wrapper_GDAL*`
- Configuration: `SetConfigOption`, `GetConfigOption`, `UseExceptions`, `GetLastErrorMsg`
- Virtual filesystem: `FileFromMemBuffer`, `Unlink`, `Mkdir`, `ReadDir`, `VSIFOpenL`, `VSIFSeekL`, `VSIFTellL`, `VSIFCloseL`, `VSIFWriteL` over `/vsimem/`, `/vsizip/`, and `/vsicurl/`.

[Dataset]:

- Shape: `RasterXSize`, `RasterYSize`, `RasterCount`, `GetRasterBand(int)`
- Pixels: typed `ReadRaster<T>` and `WriteRaster<T>` overloads for byte, short, int, float, and double plus `RasterIOExtraArg` resampling
- Georeference: `GetGeoTransform`, `SetGeoTransform`, `GetProjection`, `SetProjection`, `GetSpatialRef`, `SetSpatialRef`, `GetExtent`, `GetExtentWGS84LongLat`
- Lifecycle: `BuildOverviews`, `GetDriver`, `FlushCache`, `Close`, `GetThreadSafeDataset`

[Band]:

- Shape: `XSize`, `YSize`, `DataType`, typed windowed `ReadRaster<T>` and `WriteRaster<T>` with optional `RasterIOExtraArg`
- Tiling: `GetBlockSize(out blockX, out blockY)`, `GetOverview(int)`, `GetOverviewCount`
- No-data and scale: `GetNoDataValue`, `SetNoDataValue`, `DeleteNoDataValue`, `GetOffset`, `GetScale`, `GetUnitType`
- Range and distribution: `GetMinimum`, `GetMaximum`, `ComputeRasterMinMax`, `GetStatistics`, `ComputeStatistics`, `GetDefaultHistogram`, `GetHistogram`, `GetSampleOverview`
- Validity and role: `GetMaskBand`, `GetMaskFlags`, `GetColorInterpretation`, `SetColorInterpretation`
- Legend: `GetRasterColorTable`, `GetColorTable`, `GetDefaultRAT`, `GetCategoryNames`, `GetRasterCategoryNames`
- Lowering: The `CoverageBand` schema internalizes the optional sentinel, `Real(raw)=Offset+Scale·raw`, stored or computed range, distribution, per-pixel validity, display role, palette, and category labels.

[Driver]:

- Create: `Create(path, xsize, ysize, bands, DataType, options)`, `CreateCopy(path, src, strict, options, …)`, `CreateVector(path, options)`, `CreateMultiDimensional`
- Files: `Delete`, `Rename`, `CopyFiles`
- Identity: `ShortName`, `LongName`

[DataType]:

- Scalar: `GDT_Unknown=0`, `GDT_Byte=1`, `GDT_UInt8=1`, `GDT_Int8=14`, `GDT_UInt16=2`, `GDT_Int16=3`, `GDT_UInt32=4`, `GDT_Int32=5`, `GDT_UInt64=12`, `GDT_Int64=13`, `GDT_Float16=15`, `GDT_Float32=6`, `GDT_Float64=7`
- Complex: `GDT_CInt16=8`, `GDT_CInt32=9`, `GDT_CFloat16=16`, `GDT_CFloat32=10`, `GDT_CFloat64=11`
- Sentinel: `GDT_TypeCount=17`
- Lowering: `RasterSampleType` internalizes every non-sentinel value; the projector rejects `GDT_Unknown` and `GDT_TypeCount`.

[ColorInterp]:

- Values: `GCI_Undefined`, `GCI_GrayIndex`, `GCI_PaletteIndex`, `GCI_RedBand`, `GCI_GreenBand`, `GCI_BlueBand`, `GCI_AlphaBand`, `GCI_HueBand`, `GCI_SaturationBand`, `GCI_LightnessBand`, `GCI_CyanBand`, `GCI_MagentaBand`, `GCI_YellowBand`, `GCI_BlackBand`, `GCI_YCbCr_*`
- Lowering: `BandRole` reduces the enum to its consumer-facing channel set; `GCI_PaletteIndex` pairs with a `ColorTable`.

[ColorTable]:

- Members: `ctor(PaletteInterp)`, `GetCount()`, `GetColorEntry(int entry)`, `GetPaletteInterpretation()`, `Clone()`
- Palettes: `GPI_Gray`, `GPI_RGB`, `GPI_CMYK`, `GPI_HLS`
- Boundary: The `IDisposable` SWIG handle lowers each indexed `ColorEntry` onto a `CoverageBand.Palette` `ColorBin` under `using`.

[ColorEntry]:

- Shape: `short c1`, `c2`, `c3`, `c4`
- RGB: `c1` through `c4` carry R, G, B, and A in the 0-255 byte domain; the projector clamps each `short` onto `ColorBin`.
- Other palettes: `GPI_HLS` and `GPI_CMYK` entries carry their model components.
- Lifetime: `IDisposable`

[RasterAttributeTable]:

- Shape: `GetRowCount`, `GetColumnCount`, `GetNameOfCol`, `GetUsageOfCol`, `GetColOfUsage`, `GetTypeOfCol`
- Values: `GetValueAsString`, `GetValueAsInt`, `GetValueAsDouble`, `GetValueAsBoolean`, `GetValueAsDateTime`
- Binning: `GetRowOfValue`, `GetLinearBinning`, `GetTableType`, `Clone`
- Usage: `GFU_Generic`, `GFU_PixelCount`, `GFU_Name`, `GFU_Min`, `GFU_Max`, `GFU_MinMax`, `GFU_Red`, `GFU_Green`, `GFU_Blue`, `GFU_Alpha`, …
- Lowering: The seam reads the `GFU_Name` column as the `ColorBin.Category` label.
- Lifetime: `IDisposable`

[GDAL_OPTIONS]:
- Types: `GDALWarpAppOptions`, `GDALTranslateOptions`, `GDALVectorTranslateOptions`, `GDALDEMProcessingOptions`, `GDALGridOptions`, `GDALContourOptions`, `GDALRasterizeOptions`
- Construction: Each type accepts the corresponding CLI flags as `string[]`; `new GDALWarpAppOptions(new[]{"-t_srs","EPSG:3857","-r","bilinear"})` carries warp options.
- Consumer: `wrapper_GDAL*` and `Warp`

[PUBLIC_TYPE_SCOPE]: OGR vector
- package: `MaxRev.Gdal.Core`
- namespace: `OSGeo.OGR`
- rail: geometry

Every OGR public type belongs to the geometry rail.

| [INDEX] | [SYMBOL]          | [KIND]          |
| :-----: | :---------------- | :-------------- |
|  [01]   | `Ogr`             | static API      |
|  [02]   | `Driver`          | vector driver   |
|  [03]   | `DataSource`      | dataset         |
|  [04]   | `Layer`           | feature layer   |
|  [05]   | `Feature`         | feature         |
|  [06]   | `Geometry`        | geometry        |
|  [07]   | `FieldDefn`       | field schema    |
|  [08]   | `FeatureDefn`     | layer schema    |
|  [09]   | `wkbGeometryType` | geometry enum   |
|  [10]   | `wkbByteOrder`    | byte-order enum |

[Ogr]:

- Members: `Open`, `OpenShared`, `GetDriverByName`, `GetDriver`, `RegisterAll`, `UseExceptions`, `DontUseExceptions`
- Capabilities: `OLCFastGetArrowStream`, `OLCFastWriteArrowBatch`, `OLCTransactions`, `ODsCCreateLayer`, and the remaining `OLC*` and `ODsC*` strings feed `TestCapability`.
- Error model: `UseExceptions` flips the SWIG model once after `GdalBase.ConfigureAll()`.

[OGR_DRIVER]:
- Members: `CreateDataSource`, `CopyDataSource`, `DeleteDataSource`, `Open`
- Boundary: `Ogr.GetDriverByName` returns this vector driver, distinct from `OSGeo.GDAL.Driver`, for one GeoPackage, KML, GML, FileGDB, or `/vsimem` write path.

[DataSource]:

- Layers: `GetLayerCount`, `GetLayerByIndex`, `GetLayerByName`, `CreateLayer`, `CopyLayer`
- Query: `ExecuteSQL(statement, spatialFilter, dialect)` accepts OGR-SQL and SQLite dialects.
- Transaction: `StartTransaction`, `CommitTransaction`, `RollbackTransaction`
- Introspection: `TestCapability`, `GetDriver`

[Layer]:

- Cursor: `GetNextFeature`, `ResetReading`, `GetFeature(fid)`, `GetFeatureCount`
- Mutation: `CreateFeature`, `SetFeature`, `UpsertFeature`, `UpdateFeature`, `DeleteFeature`
- Filters: `SetSpatialFilter`, `SetSpatialFilterRect`, `SetAttributeFilter`
- Schema: `GetExtent`, `GetLayerDefn`, `CreateField`, `DeleteField`, `ReorderField`, `GetSpatialRef`
- Push-down: `SetSpatialFilterRect` applies the bounding box and `SetAttributeFilter` applies the driver-side `WHERE` before features cross the boundary.

[Feature]:

- Construction: `new Feature(FeatureDefn)` creates an empty write-path feature against a layer definition.
- Geometry: `GetGeometryRef`, `SetGeometry`, `SetGeometryDirectly`, `GetGeomFieldRef`, `SetGeomField`
- Fields: `GetFieldAsString`, `GetFieldAsInteger`, `GetFieldAsInteger64`, `GetFieldAsDouble`, `GetFieldAsDateTime`, `GetFieldAsISO8601DateTime`, `GetFieldAs*List`
- Introspection: `IsFieldSet`, `GetFieldCount`, `GetFieldDefnRef`

[Geometry]:

- Wire: `WkbSize`, `ExportToWkb`, `ExportToWkt`, `ExportToIsoWkt`, `ExportToGML`, `ExportToJson`, `CreateFromWkb`, `CreateFromWkt`, `CreateFromGML`
- Transform: `TransformTo(SpatialReference)`, `Transform(CoordinateTransformation)`
- GEOS: `Intersection`, `Union`, `UnionCascaded`, `Buffer`, `Simplify`, `SimplifyPreserveTopology`
- Boundary: `WkbSize()` sizes the `ExportToWkb` buffer, and the topology members bind native `libgeos`.

[FIELD_DEFINITIONS]:
- `FieldDefn` exposes `GetName`, `GetFieldType`, `GetWidth`, and `GetPrecision`.
- `FeatureDefn` carries the layer's field and geometry-field definitions.

[wkbGeometryType]:

- Values: `wkbUnknown`, `wkbPoint`, `wkbLineString`, `wkbPolygon`, `wkbMultiPoint`, `wkbMultiLineString`, `wkbMultiPolygon`, `wkbGeometryCollection`, `wkbNone`
- Dimensions: `*M`, `*ZM`, and `*25D` variants express the layer geometry-column type.

[wkbByteOrder]:

- Values: `wkbXDR` is big-endian and `wkbNDR` is little-endian.
- Bridge: `Geometry.ExportToWkb(byte[], wkbByteOrder)` uses `wkbNDR` for the NTS `WKBReader` bridge.

[PUBLIC_TYPE_SCOPE]: OSR spatial reference (PROJ-backed)
- package: `MaxRev.Gdal.Core`
- namespace: `OSGeo.OSR`
- rail: geometry

Every OSR public type belongs to the geometry rail and binds PROJ.

| [INDEX] | [SYMBOL]                          | [KIND]          |
| :-----: | :-------------------------------- | :-------------- |
|  [01]   | `Osr`                             | static API      |
|  [02]   | `SpatialReference`                | CRS             |
|  [03]   | `CoordinateTransformation`        | pipeline        |
|  [04]   | `CoordinateTransformationOptions` | pipeline policy |
|  [05]   | `AxisMappingStrategy`             | axis-order enum |

[Osr]:

- Members: `UseExceptions`, `DontUseExceptions`
- Error model: `UseExceptions` runs once after `GdalBase.ConfigureAll()` beside the GDAL and OGR equivalents, so failed CRS import or transformation throws.

[SpatialReference]:

- Construction: `SpatialReference(wkt)`, `ImportFromEPSG`, `ImportFromEPSGA`, `ImportFromWkt`, `ImportFromProj4`, `ImportFromESRI`, `ImportFromUrl`, `ImportFromXML`
- Configuration: `SetWellKnownGeogCS`, `SetUTM`, `SetFromUserInput`, `SetAxisMappingStrategy`
- Export: `ExportToWkt`, `ExportToPrettyWkt`, `ExportToProj4`, `ExportToPROJJSON`
- Identity: `IsProjected`, `IsGeographic`, `IsSame`, `GetAuthorityCode`, `GetAuthorityName`
- Dynamics: `IsDynamic`, `GetCoordinateEpoch`, `SetCoordinateEpoch(double)` carry datum epochs for plate motion.

[CoordinateTransformation]:

- Construction: `CoordinateTransformation(src, dst[, options])`, `GetInverse`
- Transform: `TransformPoint(double[])`, `TransformPoint4D`, `TransformPoints(n, x[], y[], z[])`, `TransformPointWithErrorCode`
- Validity: `TransformPointWithErrorCode` carries per-point validity.

[CoordinateTransformationOptions]:

- Members: `SetAreaOfInterest`, `SetOperation`, `SetDesiredAccuracy`, `SetBallparkAllowed`, `SetOnlyBest`
- Accuracy: `SetBallparkAllowed(false)` faults for a gridless pair, and `SetOnlyBest(true)` faults when the best operation cannot instantiate.
- Survey policy: A survey-grade escalation sets both accuracy gates.

[AxisMappingStrategy]:

- Values: `OAMS_TRADITIONAL_GIS_ORDER`, `OAMS_AUTHORITY_COMPLIANT`, `OAMS_CUSTOM`
- Semantics: The strategies select longitude/latitude order, EPSG authority order, or custom order and prevent the GDAL 3 axis swap.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: bootstrap and open
- package: `MaxRev.Gdal.Core`
- namespace: `MaxRev.Gdal.Core`, `OSGeo.GDAL`, `OSGeo.OGR`
- rail: geometry

| [INDEX] | [SURFACE]               | [CAPABILITY]   |
| :-----: | :---------------------- | :------------- |
|  [01]   | `GdalBase.ConfigureAll` | bootstrap      |
|  [02]   | `Gdal.UseExceptions`    | error model    |
|  [03]   | `Gdal.Open`             | raster open    |
|  [04]   | `Gdal.OpenEx`           | flagged open   |
|  [05]   | `Ogr.Open`              | vector open    |
|  [06]   | `Gdal.GetDriverByName`  | raster driver  |
|  [07]   | `Ogr.GetDriverByName`   | vector driver  |
|  [08]   | `Gdal.VSIF*L`           | virtual handle |

[GdalBase.ConfigureAll]:

- Call: `()` static
- Semantics: The once-per-process bootstrap registers every GDAL and OGR driver and resolves the runtime package's `gdal-data` and PROJ paths; every other call faults without it.

[Gdal.UseExceptions]:

- Call: `()` static
- Semantics: The call follows `ConfigureAll()` and changes SWIG return-code failures into `ApplicationException` values lowered onto `BimFault.CodecReject`.

[Gdal.Open]:

- Call: `(string path, Access eAccess) → Dataset`
- Access: `Access.GA_ReadOnly`, `Access.GA_Update`
- Semantics: Opens a raster or any GDAL-readable dataset.

[Gdal.OpenEx]:

- Call: `(string path, uint openFlags, string[] allowedDrivers, string[] openOptions, string[] siblingFiles) → Dataset`
- Flags: `GDAL_OF_RASTER`, `GDAL_OF_VECTOR`, `GDAL_OF_UPDATE`
- Semantics: Applies a driver allow-list and per-driver open options.

[Ogr.Open]:

- Call: `(string path, int update) → DataSource`
- Semantics: Opens a vector dataset; `update=0` reads and `update=1` writes.

[GetDriverByName]:

- Call: `(string name) → Driver`
- Owners: `Gdal.GetDriverByName`, `Ogr.GetDriverByName`
- Drivers: `"GTiff"`, `"COG"`, `"GPKG"`, `"FlatGeobuf"`, `"ESRI Shapefile"`, `"GeoJSON"`
- Semantics: Resolves the driver consumed by `Create` or `CreateCopy`.

[Gdal.VSIF_L]:

- Calls: `VSIFOpenL(string path, string mode) → nint`, `VSIFSeekL(nint fp, int offset, int whence) → int`, `VSIFTellL(nint fp) → int`, `VSIFCloseL(nint fp) → int`
- Semantics: The `/vsimem` handle API opens a virtual file, seeks to its end, reports its size, and closes it beside `FileFromMemBuffer` and `Unlink`.
- Boundary: This SWIG build exposes `VSIFWriteL(string, …)` but no `VSIFReadL(byte[], …)`, so handle-level `/vsimem` byte read-back is absent.

[ENTRYPOINT_SCOPE]: raster IO and algorithms
- package: `MaxRev.Gdal.Core`
- namespace: `OSGeo.GDAL`
- rail: geometry

| [INDEX] | [SURFACE]                          | [CAPABILITY]     |
| :-----: | :--------------------------------- | :--------------- |
|  [01]   | `Dataset.ReadRaster<T>`            | pixel read       |
|  [02]   | `Dataset.GetGeoTransform`          | affine           |
|  [03]   | `Dataset.GetExtent`                | target extent    |
|  [04]   | `Dataset.GetExtentWGS84LongLat`    | WGS84 extent     |
|  [05]   | `Dataset.BuildOverviews`           | pyramid          |
|  [06]   | `Gdal.Warp`                        | warp             |
|  [07]   | `Gdal.wrapper_GDALTranslate`       | transcode        |
|  [08]   | `Gdal.wrapper_GDALDEMProcessing`   | terrain analysis |
|  [09]   | `Gdal.wrapper_GDALContourDestName` | contours         |
|  [10]   | `Gdal.BuildVRT`                    | virtual mosaic   |

[Dataset.ReadRaster]:

- Call: `(xOff, yOff, xSize, ySize, T[] buffer, buf_xSize, buf_ySize, bandCount, int[] bandMap, pixelSpace, lineSpace, bandSpace[, RasterIOExtraArg]) → CPLErr`
- Types: `T` is byte, short, int, float, or double.
- Semantics: The windowed multi-band read writes a managed buffer, and `RasterIOExtraArg` selects on-read downsampling.

[Dataset.GetGeoTransform]:

- Call: `(double[] argout)`
- Affine: `[originX, pxW, rowRot, originY, colRot, pxH]` maps pixels into georeferenced space.

[Dataset.GetExtent]:

- Calls: `GetExtent(Envelope extent, SpatialReference srs) → CPLErr`, `GetExtentWGS84LongLat(Envelope extent) → CPLErr`
- Shape: `OSGeo.OGR.Envelope` carries `MinX`, `MaxX`, `MinY`, and `MaxY`; it is not the NTS type.
- Boundary: The second call reports WGS84 longitude and latitude, and a windowed or resampled read derives its tile extent from the re-anchored affine corners.

[Dataset.BuildOverviews]:

- Call: `(string resampling, int[] overviewList[, callback]) → int`
- Kernels: `"AVERAGE"`, `"GAUSS"`, `"CUBIC"`
- Semantics: Builds a COG or tiled-output pyramid.

[Gdal.Warp]:

- Call: `(string dest, Dataset[] src, GDALWarpAppOptions, callback, data) → Dataset`
- Options: `-t_srs`, `-r`, `-te`, `-tr`
- Semantics: The `gdalwarp` algorithm reprojects, mosaics, and resamples.

[Gdal.wrapper_GDALTranslate]:

- Call: `(string dest, Dataset src, GDALTranslateOptions, callback, data) → Dataset`
- Semantics: The `gdal_translate` algorithm converts format, subset, and scale, including GeoTIFF-to-COG transcoding.

[Gdal.wrapper_GDALDEMProcessing]:

- Call: `(string dest, Dataset src, string mode, string colorFile, GDALDEMProcessingOptions, callback, data) → Dataset`
- Modes: `"hillshade"`, `"slope"`, `"aspect"`, color relief

[Gdal.wrapper_GDALContourDestName]:

- Call: `(string dest, Dataset src, GDALContourOptions, callback, data) → Dataset`
- Semantics: Vectorizes a DEM band's iso-contours into an OGR layer for terrain contour lines.

[Gdal.BuildVRT]:

- Call: `(string dest, Dataset[] src, GDALBuildVRTOptions, callback, data) → Dataset`
- Semantics: Builds a virtual mosaic over multiple sources without materializing pixels.

[ENTRYPOINT_SCOPE]: band metadata and the palette/RAT legend lowering
- package: `MaxRev.Gdal.Core`
- namespace: `OSGeo.GDAL`
- rail: geometry

| [INDEX] | [SURFACE]                     | [CAPABILITY]      |
| :-----: | :---------------------------- | :---------------- |
|  [01]   | `Band.GetBlockSize`           | tile shape        |
|  [02]   | `Band.GetOffset`              | decode offset     |
|  [03]   | `Band.GetScale`               | decode scale      |
|  [04]   | `Band.GetNoDataValue`         | no-data sentinel  |
|  [05]   | `Band.GetMinimum`             | stored minimum    |
|  [06]   | `Band.GetMaximum`             | stored maximum    |
|  [07]   | `Band.GetUnitType`            | units             |
|  [08]   | `Band.GetColorInterpretation` | channel role      |
|  [09]   | `Band.GetRasterColorTable`    | palette           |
|  [10]   | `Band.GetDefaultRAT`          | category table    |
|  [11]   | `Band.GetCategoryNames`       | category names    |
|  [12]   | `Band.GetStatistics`          | cached statistics |
|  [13]   | `Band.ComputeStatistics`      | statistics scan   |
|  [14]   | `Band.ComputeRasterMinMax`    | range scan        |
|  [15]   | `Band.GetMaskBand`            | validity band     |
|  [16]   | `Band.GetMaskFlags`           | validity flags    |
|  [17]   | `Band.GetDefaultHistogram`    | cached histogram  |
|  [18]   | `Band.GetHistogram`           | ranged histogram  |

[Band.GetBlockSize]:

- Call: `(out int blockX, out int blockY)`
- Lowering: The tile shape maps to `OverviewLevel.BlockX`, `OverviewLevel.BlockY`, `CoverageGrid.BaseBlockX`, and `CoverageGrid.BaseBlockY`.
- Semantics: Windowed reads align to the containing tile; a zero or strip height maps to a full-width row band.

[Band.GetOffset_GetScale]:

- Call: `(out double val, out int hasval)` for each member
- Lowering: `CoverageBand.Offset` and `CoverageBand.Scale` apply `Real(raw)=Offset+Scale·raw` so scaled-integer DEM values enter real units.

[Band.GetNoDataValue]:

- Call: `(out double val, out int hasval)`
- Lowering: `hasval==0` maps to `CoverageBand.NoData=None`; every present value maps to NaN-safe `Some(val)` and `IsNoData` behavior.

[Band.GetMinimum_GetMaximum]:

- Call: `(out double val, out int hasval)` for each member
- Lowering: Stored metadata maps to `CoverageBand.Range` as `Option<(Min,Max)>`; an absent stored value falls through to computed statistics before remaining `None`.

[Band.GetUnitType]:

- Call: `() → string`
- Lowering: The returned unit, including `"m"` or `"W/m^2"`, maps to `CoverageBand.Units`.

[Band.GetColorInterpretation]:

- Call: `() → ColorInterp`
- Lowering: Generated `BandRole.TryGet` maps `GCI_*` to `Gray`, `Red`, `Green`, `Blue`, `Alpha`, or `Palette` and defaults to `Undefined`.

[Band.GetRasterColorTable]:

- Call: `() → ColorTable`; null means no palette.
- Lowering: Iterate `0..GetCount()`, call `GetColorEntry(i)`, clamp `c1` through `c4` from `short` to `byte`, and produce `ColorBin(i, r, g, b, a)` rows for `CoverageBand.Palette`.
- Lifetime: A `using` boundary frees the `IDisposable` SWIG handle.

[Band.GetDefaultRAT]:

- Call: `() → RasterAttributeTable`; null means no RAT.
- Lowering: `GetColOfUsage(RATFieldUsage.GFU_Name)` resolves the name column, `GetValueAsString(row, nameCol)` yields `ColorBin.Category`, and `GetRowOfValue(rawCellValue)` resolves the category row.
- Semantics: The RAT carries a richer land-cover or soil legend than `ColorTable` alone.

[Band.GetCategoryNames]:

- Call: `() → string[]`; an empty array means no names.
- Lowering: The lightweight index-to-class array supplies `ColorBin.Category` when a palette has names but no RAT.

[Band.Statistics]:

- Calls: `GetStatistics(int approxOk, int force, out double min, out double max, out double mean, out double stddev) → CPLErr`; `ComputeStatistics(bool approxOk, out min, out max, out mean, out stddev, callback, data) → CPLErr`; `ComputeRasterMinMax(double[2] argout, int approxOk)`
- Cached path: `GetStatistics(force=0)` reads cached minimum, maximum, mean, and standard deviation and lowers the range onto `CoverageBand.Range`.
- Compute path: `ComputeStatistics` or `ComputeRasterMinMax` performs the optional approximate pass when no cache exists.
- Sampling: `GetSampleOverview(nDesiredSamples) → Band` selects a decimated level for the approximate scan.

[Band.Mask]:

- Calls: `GetMaskBand() → Band`, `GetMaskFlags() → int`
- Flags: `GMF_ALL_VALID=0x01`, `GMF_PER_DATASET=0x02`, `GMF_ALPHA=0x04`, `GMF_NODATA=0x08`
- Binding: This SWIG build exposes the C bit constants through a raw `int`, not named `GMF_*` members.
- Lowering: A nontrivial flag maps the real per-pixel validity band beside `CoverageBand.NoData`; an all-valid flag maps no synthetic mask.
- Read: The mask band uses the same windowed `ReadRaster<byte>` path as every other band.

[Band.Histogram]:

- Calls: `GetDefaultHistogram(out min, out max, out buckets, out int[] hist, int force, callback, data) → CPLErr`; `GetHistogram(double min, double max, int buckets, int[] hist, int includeOutOfRange, int approxOk, callback, data) → CPLErr`
- Semantics: The default member reads a cached or auto-ranged histogram, and the ranged member computes caller-selected buckets.
- Consumer: Display normalization and unsupervised classification compose the distribution when `CoverageBand.Range` cannot drive a robust percentile stretch.

[ENTRYPOINT_SCOPE]: vector iteration and the NTS bridge
- package: `MaxRev.Gdal.Core`
- namespace: `OSGeo.OGR`
- rail: geometry

| [INDEX] | [SURFACE]                    | [CAPABILITY]     |
| :-----: | :--------------------------- | :--------------- |
|  [01]   | `DataSource.GetLayerByIndex` | indexed lookup   |
|  [02]   | `DataSource.GetLayerByName`  | named lookup     |
|  [03]   | `Layer.SetSpatialFilterRect` | spatial filter   |
|  [04]   | `Layer.SetAttributeFilter`   | attribute filter |
|  [05]   | `Layer.GetNextFeature`       | cursor           |
|  [06]   | `Feature.GetGeometryRef`     | geometry access  |
|  [07]   | `Geometry.ExportToWkb`       | NTS egress       |
|  [08]   | `Geometry.CreateFromWkb`     | NTS ingress      |
|  [09]   | `DataSource.ExecuteSQL`      | query            |

[DataSource.GetLayer]:

- Calls: `GetLayerByIndex(int) → Layer`, `GetLayerByName(string) → Layer`
- Semantics: Resolves the layer consumed by iteration.

[Layer.SetSpatialFilterRect]:

- Call: `(double minx, double miny, double maxx, double maxy)`
- Semantics: Pushes the site or context bounding box into the driver, so only windowed features return.

[Layer.SetAttributeFilter]:

- Call: `(string whereClause)`
- Semantics: Pushes the driver-evaluated `WHERE` clause into the source.

[Layer.GetNextFeature]:

- Call: `() → Feature`
- Semantics: Advances the filtered forward cursor, returns null at the end, and restarts through `ResetReading`.

[Feature.GetGeometryRef]:

- Call: `() → Geometry`
- Semantics: Returns the current feature's OGR geometry.

[Geometry.ExportToWkb]:

- Calls: `(byte[] buffer, wkbByteOrder) → int`, `(byte[] buffer) → int`
- Bridge: `NetTopologySuite.IO.WKBReader.Read(buffer)` materializes the exported OGR geometry as NTS `Geometry`.

[Geometry.CreateFromWkb]:

- Call: `(byte[] wkb) → Geometry` static
- Bridge: The member converts an NTS `WKBWriter.Write` buffer into OGR geometry for `Feature.SetGeometry` write-back.

[DataSource.ExecuteSQL]:

- Call: `(string statement, Geometry spatialFilter, string dialect) → Layer`
- Dialects: OGR-SQL, `"SQLITE"`
- Semantics: Returns a temporary query-result layer.

[ENTRYPOINT_SCOPE]: OSR reprojection (PROJ pipeline)
- package: `MaxRev.Gdal.Core`
- namespace: `OSGeo.OSR`
- rail: geometry

| [INDEX] | [SURFACE]                                  | [CAPABILITY]       |
| :-----: | :----------------------------------------- | :----------------- |
|  [01]   | `SpatialReference.ImportFromEPSG`          | EPSG import        |
|  [02]   | `SpatialReference.SetFromUserInput`        | CRS parse          |
|  [03]   | `SpatialReference.SetAxisMappingStrategy`  | axis order         |
|  [04]   | `CoordinateTransformation`                 | pipeline           |
|  [05]   | `CoordinateTransformation.TransformPoints` | batch transform    |
|  [06]   | `Geometry.TransformTo`                     | geometry transform |

[SpatialReference.ImportFromEPSG]:

- Call: `(int code) → int`; zero means success.
- Semantics: Builds a CRS from an EPSG code.

[SpatialReference.SetFromUserInput]:

- Call: `(string) → int`
- Inputs: `"EPSG:3857"`, WKT, PROJ string, URL

[SpatialReference.SetAxisMappingStrategy]:

- Call: `(AxisMappingStrategy.OAMS_TRADITIONAL_GIS_ORDER)`
- Semantics: Forces longitude and latitude order and prevents the GDAL 3 axis swap.

[CoordinateTransformation.ctor]:

- Call: `(SpatialReference src, SpatialReference dst[, CoordinateTransformationOptions])`
- Semantics: Builds a PROJ pipeline between two coordinate reference systems.

[CoordinateTransformation.TransformPoints]:

- Call: `(int n, double[] x, double[] y, double[] z)`
- Semantics: Reprojects `n` coordinates in place.

[Geometry.TransformTo]:

- Call: `(SpatialReference target) → int`
- Semantics: Reprojects an OGR geometry in place through PROJ with datum-grid awareness.

## [04]-[IMPLEMENTATION_LAW]

[PLATFORM_BOUNDARY]:
- the managed AnyCPU bindings bind `lib/net10.0` and P/Invoke `libgdal_wrap` and `libgdalconst_wrap`, which bind native `libgdal`, `libgeos`, and `libproj` from a separate RID-keyed runtime package.
- osx-arm64 binds `MaxRev.Gdal.MacosRuntime.Minimal.arm64`; Windows and Linux publications bind their matching RID runtime.
- a missing publish-RID runtime faults at `GdalBase.ConfigureAll` or the first `OSGeo.*` call, so the core and runtime pins track the same version.
- `runtimes/any/native/gdal-data/**` is the PROJ/EPSG resource set (CSV/WKT/grid metadata) the `.targets` stages into the output and `GdalBase.ConfigureGdalData` points the `GDAL_DATA`/`PROJ_LIB` config at. CRS resolution and datum transforms fail without it even when the native libraries load.

[BOOTSTRAP]:
- `GdalBase.ConfigureAll()` is the mandatory once-per-process bootstrap: it registers every GDAL raster driver and OGR vector driver and resolves the `gdal-data`/PROJ paths. The canonical owner runs it exactly once at module init behind an idempotency guard (`GdalBase.IsConfigured`), never per-open.
- follow it immediately with `Gdal.UseExceptions()` (and `Ogr`/`Osr` equivalents) so a failed open/transform throws rather than returning a `CPLErr` that silent-passes — the thrown exception is caught at the boundary and lowered onto `BimFault.CodecReject`. Domain code never branches on raw `CPLErr` return codes.

[RASTER_IO]:
- raster pixels move through `Dataset.ReadRaster<T>`/`Band.ReadRaster<T>` into a managed `T[]` whose element type matches the `Band.DataType` (`GDT_Byte`→`byte[]`, `GDT_Float32`→`float[]`, `GDT_Float64`→`double[]`). The `bufXSize`/`bufYSize` arguments differing from the window size trigger GDAL's on-read resampling (kernel selected by `RasterIOExtraArg`) — a DEM is downsampled to a working resolution in one call.
- `Dataset.GetGeoTransform` + `Dataset.GetSpatialRef` together place the raster in georeferenced space; `Dataset.GetExtent(Envelope, srs)` fills the SWIG `OSGeo.OGR.Envelope` (NOT the NTS type — the bindings reference no NTS) with the whole-dataset bbox in the target CRS, its four doubles lowered onto an NTS `Envelope` at the boundary for the `STRtree`/site-model. A windowed or resampled read folds its tile extent off the window-re-anchored affine's four corners (rotation honored), never the source-dataset call.
- raster output is `Driver.Create`/`Driver.CreateCopy` (e.g. the `"COG"` driver with creation options) or the `wrapper_GDALTranslate`/`Gdal.Warp` algorithms; `BuildOverviews` adds the pyramid a COG/3D-Tiles terrain layer needs.

[BAND_SCHEMA_LOWERING]:
- the host-neutral `CoverageGrid` projector reads the full `Band` metadata surface once per ingest and lowers it onto self-describing `CoverageBand` values; consumers never retain `OSGeo.GDAL` enums.
- `Band.DataType` maps through `RasterSampleType.Parse`, while `GDT_Unknown` and `GDT_TypeCount` rail `BimFault.CodecReject`; `Band.GetColorInterpretation()` maps through generated `BandRole.TryGet` and defaults to `Undefined`.
- `GetNoDataValue` maps to `Option<double>`, `GetOffset` and `GetScale` map to their namesake fields, and `GetUnitType()` maps to `Units`.
- `CoverageBand.Range` reads `GetMinimum` and `GetMaximum`, then `GetStatistics(force=0)`, then on-demand `ComputeRasterMinMax` or `ComputeStatistics`; approximate computation samples a decimated `GetSampleOverview` level.
- a non-`GMF_ALL_VALID` flag maps the `GMF_ALPHA`, `GMF_NODATA`, or `GMF_PER_DATASET` validity band through windowed `GetMaskBand().ReadRaster<byte>`; an all-valid band lowers no mask.
- display-normalization and classification consumers compose `GetDefaultHistogram` or `GetHistogram` when scalar `Range` cannot drive a robust percentile stretch.
- a `GCI_PaletteIndex` legend reads `GetRasterColorTable()`, iterates `0..GetCount()` through `GetColorEntry(i)`, and clamps `ColorEntry.c1` through `c4` from `short` to `byte`.
- the legend pairs each index with the `GetDefaultRAT()` `GFU_Name` value resolved through `GetColOfUsage`, `GetValueAsString`, and `GetRowOfValue`, or with `GetCategoryNames()[index]` when no RAT ships.
- the projector emits `ColorBin(index, r, g, b, a, category)` rows, so an indexed land-cover or soil cell carries color and class without a sidecar.
- `using` frees every `IDisposable` `ColorTable`, `RasterAttributeTable`, and `ColorEntry` handle at the boundary; only lowered value rows cross the seam.
- `Band.GetBlockSize` maps onto `OverviewLevel.BlockX`, `OverviewLevel.BlockY`, `CoverageGrid.BaseBlockX`, and `CoverageGrid.BaseBlockY`.
- decreasing-resolution `Dataset.GetOverviewCount` and `GetOverview(i)` values map to an index-ordered `OverviewLevel` set whose entries are content-keyed as individual `RasterKey` objects.
- `CoverageGrid.Of` rejects a pyramid unless every level is coarser than its base and the set is strictly monotone.

[VECTOR_INGEST]:
- the OGR side is the universal vector reader: `Ogr.Open` (or `Gdal.OpenEx` with `GDAL_OF_VECTOR`) opens GeoPackage/FlatGeobuf/KML/GML/GeoJSON/FileGDB/PostGIS/shapefile/… through one API. Iterate `Layer.GetNextFeature` after applying `SetSpatialFilterRect` (bbox push-down) and `SetAttributeFilter` (WHERE) so the driver evaluates the filter server-side and only matching features cross the boundary.
- `Layer.TestCapability(Ogr.OLCFastGetArrowStream)` reports whether a driver exposes the Arrow C-stream bulk path — the columnar fast read for a GeoParquet/FlatGeobuf source the `Rasm.Persistence` Arrow rail consumes without per-feature marshalling (the capability string is the `Ogr` constant; the stream itself is reached through the SWIG Arrow-array interface).

[OGR_VS_NTS_GEOS]:
- the native runtime bundles `libgeos`, so OGR `Geometry` exposes GEOS-backed `Intersection`, `Union`, `Buffer`, and `Simplify`.
- managed `NetTopologySuite` owns the planar topology algebra, and GEOS remains inside the GDAL native boundary as one canonical planar engine.
- ingest bridges OGR geometry into NTS through `ExportToWkb` and `WKBReader.Read`; write-back bridges NTS through `WKBWriter.Write` and `CreateFromWkb` only at the OGR boundary.
- OGR-side boolean operations fragment the topology owner and remain outside the admitted path.

[OSR_VS_PROJNET]:
- OSR exposes a PROJ-backed reprojection engine parallel to managed `ProjNET`.
- `ProjNET` owns CRS and datum transforms for geometry already in the managed planar algebra; OSR owns reprojection inside `Gdal.Warp`, `Geometry.TransformTo`, and `Dataset.GetExtent` GDAL pipelines.
- transforms requiring PROJ datum grids or `IsDynamic` and `GetCoordinateEpoch` plate-motion handling escalate to a one-shot OSR `CoordinateTransformation`.
- every OSR CRS applies `SetAxisMappingStrategy(OAMS_TRADITIONAL_GIS_ORDER)` to prevent the GDAL 3 latitude and longitude axis swap.

[STACK_INTEGRATION]:
- NTS seam: OGR `Geometry.ExportToWkb`/`CreateFromWkb` ↔ `NetTopologySuite.IO.WKBReader`/`WKBWriter` is the canonical bidirectional bridge. Universal vector ingest enters as OGR features, converts to NTS `Feature` at the boundary, and the planar algebra/index/predicates run on NTS.
- shapefile seam: GDAL's `"ESRI Shapefile"` OGR driver reads the same `.shp` the managed `NetTopologySuite.IO.Esri.Shapefile` codec does; the managed codec is the default for shapefiles (no native dependency), GDAL is the path for the formats only it covers.
- raster→vector seam: `wrapper_GDALContourDestName` and `RasterizeLayer` cross the raster/vector boundary — a DEM becomes contour `Layer`s (then NTS geometry), and NTS footprints rasterize into a `Band`.
- virtual-filesystem seam: `Gdal.FileFromMemBuffer("/vsimem/...", bytes)` and the `/vsizip/`/`/vsicurl/` prefixes let a dataset open from an in-memory buffer or a remote/zipped source without a filesystem path — the `Rasm.Persistence` fsspec/stream ingest opens GDAL datasets over `/vsimem/`.

[LOCAL_ADMISSION]:
- bootstrap enters once through `GdalBase.ConfigureAll()` + `Gdal.UseExceptions()` at module init.
- raster ingest enters through `Gdal.Open`/`OpenEx` → `Dataset.ReadRaster<T>`; raster transform through `Gdal.Warp`/`wrapper_GDAL*`.
- vector ingest enters through `Ogr.Open` → `Layer` with `SetSpatialFilterRect`/`SetAttributeFilter` → `Feature.GetGeometryRef.ExportToWkb` → NTS.
- reprojection inside a GDAL pipeline enters through OSR; reprojection of managed geometry stays with `ProjNET`.
- the rejected forms: per-open `ConfigureAll`, branching on raw `CPLErr`, running planar boolean ops on the OGR/GEOS side, and using OSR for transforms `ProjNET` already covers.

[RAIL_LAW]:
- Package: `MaxRev.Gdal.Core` (paired with a RID-keyed `MaxRev.Gdal.*Runtime*`)
- Owns: GDAL/OGR/OSR managed bindings — GeoTIFF/COG/DEM raster IO, the universal OGR vector driver set, the GDAL utility algorithms (warp/translate/DEM/grid/contour/rasterize), the virtual filesystem, and an escalation-path PROJ reprojection engine
- Accept: raster ingest/transcode/reproject, universal vector ingest with server-side filters, raster↔vector conversion, exotic datum transforms PROJ-only grids require, in-memory/remote dataset open
- Reject: the managed planar geometry algebra (`NetTopologySuite` owns it — OGR geometry is bridged out at the wire), routine managed CRS transforms (`ProjNET` owns them), the native binaries themselves (the runtime package owns them), shapefile-only I/O when the managed codec suffices
