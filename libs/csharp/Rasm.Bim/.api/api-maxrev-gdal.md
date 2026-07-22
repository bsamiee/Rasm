# [RASM_BIM_API_MAXREV_GDAL]

`MaxRev.Gdal.Core` owns the SWIG-generated `OSGeo.GDAL`, `OSGeo.OGR`, and `OSGeo.OSR` managed bindings under the `GdalBase` bootstrap: GeoTIFF/COG/DEM raster ingest, the universal OGR vector driver set, the GDAL utility algorithms, and PROJ-backed reprojection. It feeds the geometry rail as the universal format reader and the escalation-path reprojection engine `ProjNET` cannot express, bridging OGR geometry out to `NetTopologySuite` at the wire and holding GEOS inside the native boundary.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `MaxRev.Gdal.Core`
- package: `MaxRev.Gdal.Core` (MIT bindings + `GdalBase`; the bundled GDAL/OGR/PROJ corpus is permissive MIT/X11-style, `gdal-data/LICENSE.TXT` ships in the runtime)
- assembly: `MaxRev.Gdal.Core`
- namespace: `MaxRev.Gdal.Core`, `OSGeo.GDAL`, `OSGeo.OGR`, `OSGeo.OSR`
- asset: IL-only AnyCPU managed assembly (net10.0 binds `lib/net10.0`); P/Invokes `libgdal_wrap`/`libgdalconst_wrap` and ships no native code
- asset: `runtimes/any/native/gdal-data/**` PROJ/EPSG CSV/WKT/grid resource set the `.targets` stages into output; `GdalBase` points `GDAL_DATA`/`PROJ_LIB` at it for CRS and driver resolution
- depends: a RID-keyed `MaxRev.Gdal.*Runtime*` supplies native `libgdal`/`libgeos`/`libproj`; osx-arm64 binds `MaxRev.Gdal.MacosRuntime.Minimal.arm64`, and a missing publish-RID runtime faults every P/Invoke at first call
- rail: geometry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: `MaxRev.Gdal.Core` bootstrap

[GdalBase]: static configurator — `ConfigureAll()` `ConfigureGdalDrivers(gdalDataFolder?)` `ConfigureGdalData(gdalDataFolder?)` `IsConfigured` `EnableRuntimeValidation`.

[PUBLIC_TYPE_SCOPE]: `OSGeo.GDAL` raster

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                        |
| :-----: | :--------------------- | :------------ | :---------------------------------- |
|  [01]   | `Gdal`                 | class         | static raster/algorithm API root    |
|  [02]   | `Dataset`              | class         | multi-band raster dataset           |
|  [03]   | `Band`                 | class         | raster band, IO and metadata        |
|  [04]   | `Driver`               | class         | raster format driver                |
|  [05]   | `DataType`             | enum          | pixel storage type                  |
|  [06]   | `ColorInterp`          | enum          | band channel role                   |
|  [07]   | `ColorTable`           | class         | indexed `IDisposable` palette       |
|  [08]   | `ColorEntry`           | class         | one `IDisposable` palette entry     |
|  [09]   | `RasterAttributeTable` | class         | `IDisposable` category/legend table |
|  [10]   | `GDAL*Options`         | class         | algorithm CLI-flag option carriers  |

- `ColorEntry`: `short c1` `c2` `c3` `c4` — RGBA in the 0-255 domain (`GPI_HLS`/`GPI_CMYK` carry their model components).
- `GDAL*Options` set: `GDALWarpAppOptions` `GDALTranslateOptions` `GDALVectorTranslateOptions` `GDALDEMProcessingOptions` `GDALGridOptions` `GDALContourOptions` `GDALRasterizeOptions` `GDALBuildVRTOptions`; each `ctor(string[])` takes the corresponding CLI flags.
- `DataType` values: `GDT_Byte` `GDT_Int8` `GDT_UInt16` `GDT_Int16` `GDT_UInt32` `GDT_Int32` `GDT_UInt64` `GDT_Int64` `GDT_Float16` `GDT_Float32` `GDT_Float64` `GDT_CInt16` `GDT_CInt32` `GDT_CFloat16` `GDT_CFloat32` `GDT_CFloat64`; `GDT_Unknown`/`GDT_TypeCount` are sentinels the `RasterSampleType` projector rejects.
- `ColorInterp` values: `GCI_Undefined` `GCI_GrayIndex` `GCI_PaletteIndex` `GCI_RedBand` `GCI_GreenBand` `GCI_BlueBand` `GCI_AlphaBand` `GCI_HueBand` `GCI_SaturationBand` `GCI_LightnessBand` `GCI_CyanBand` `GCI_MagentaBand` `GCI_YellowBand` `GCI_BlackBand` `GCI_YCbCr_*`.
- `PaletteInterp` values: `GPI_Gray` `GPI_RGB` `GPI_CMYK` `GPI_HLS`; `RATFieldUsage`: `GFU_Generic` `GFU_PixelCount` `GFU_Name` `GFU_Min` `GFU_Max` `GFU_MinMax` `GFU_Red` `GFU_Green` `GFU_Blue` `GFU_Alpha`.

[PUBLIC_TYPE_SCOPE]: `OSGeo.OGR` vector

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [CAPABILITY]                      |
| :-----: | :---------------- | :------------ | :-------------------------------- |
|  [01]   | `Ogr`             | class         | static vector API root            |
|  [02]   | `Driver`          | class         | vector format driver              |
|  [03]   | `DataSource`      | class         | vector dataset                    |
|  [04]   | `Layer`           | class         | feature layer, cursor and filters |
|  [05]   | `Feature`         | class         | one feature, geometry and fields  |
|  [06]   | `Geometry`        | class         | OGR geometry, wire and GEOS ops   |
|  [07]   | `FieldDefn`       | class         | field schema                      |
|  [08]   | `FeatureDefn`     | class         | layer field/geometry schema       |
|  [09]   | `wkbGeometryType` | enum          | geometry-column type              |
|  [10]   | `wkbByteOrder`    | enum          | WKB endianness                    |

- `Ogr.GetDriverByName` returns the `OSGeo.OGR.Driver`, distinct from `OSGeo.GDAL.Driver`, for one GeoPackage/KML/GML/FileGDB/`/vsimem` write path.
- `FieldDefn`: `GetName` `GetFieldType` `GetWidth` `GetPrecision`; `FeatureDefn` carries the layer's field and geometry-field definitions.
- `wkbGeometryType` values: `wkbUnknown` `wkbPoint` `wkbLineString` `wkbPolygon` `wkbMultiPoint` `wkbMultiLineString` `wkbMultiPolygon` `wkbGeometryCollection` `wkbNone`; `*M`/`*ZM`/`*25D` variants carry the dimension.
- `wkbByteOrder` values: `wkbXDR` big-endian, `wkbNDR` little-endian (the NTS `WKBReader` bridge).
- `Layer.TestCapability` strings: `OLCFastGetArrowStream` `OLCFastWriteArrowBatch` `OLCTransactions` `ODsCCreateLayer` and the remaining `OLC*`/`ODsC*` set.

[PUBLIC_TYPE_SCOPE]: `OSGeo.OSR` spatial reference (PROJ-backed)

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [CAPABILITY]                |
| :-----: | :-------------------------------- | :------------ | :-------------------------- |
|  [01]   | `Osr`                             | class         | static OSR API root         |
|  [02]   | `SpatialReference`                | class         | CRS, import/export/identity |
|  [03]   | `CoordinateTransformation`        | class         | PROJ transform pipeline     |
|  [04]   | `CoordinateTransformationOptions` | class         | transform accuracy policy   |
|  [05]   | `AxisMappingStrategy`             | enum          | axis-order strategy         |

- `AxisMappingStrategy` values: `OAMS_TRADITIONAL_GIS_ORDER` `OAMS_AUTHORITY_COMPLIANT` `OAMS_CUSTOM` — select lon/lat, EPSG-authority, or custom order; the traditional strategy prevents the GDAL 3 axis swap.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: bootstrap and open

| [INDEX] | [SURFACE]                                                            | [SHAPE] | [CAPABILITY]               |
| :-----: | :------------------------------------------------------------------- | :------ | :------------------------- |
|  [01]   | `GdalBase.ConfigureAll()`                                            | static  | once-per-process bootstrap |
|  [02]   | `Gdal.UseExceptions()`                                               | static  | CPLErr → exception         |
|  [03]   | `Gdal.Open(string, Access) -> Dataset`                               | static  | raster/any open            |
|  [04]   | `Gdal.OpenEx(string, uint, string[], string[], string[]) -> Dataset` | static  | flagged open               |
|  [05]   | `Ogr.Open(string, int) -> DataSource`                                | static  | vector open                |
|  [06]   | `Gdal.GetDriverByName(string) -> Driver`                             | static  | raster driver              |
|  [07]   | `Ogr.GetDriverByName(string) -> Driver`                              | static  | vector driver              |
|  [08]   | `Gdal.VSIFOpenL(string, string) -> nint`                             | static  | virtual-file handle        |

- `GdalBase.ConfigureAll`: registers every GDAL and OGR driver and resolves `gdal-data`/PROJ paths; every other call faults without it. `Ogr.RegisterAll` and `Gdal.AllRegister` are the per-namespace equivalents.
- `Gdal.OpenEx` flags: `GDAL_OF_RASTER` `GDAL_OF_VECTOR` `GDAL_OF_UPDATE`; `Access`: `GA_ReadOnly` `GA_Update`; `Ogr.Open` `update=0` reads, `update=1` writes.
- `GetDriverByName` names: `"GTiff"` `"COG"` `"GPKG"` `"FlatGeobuf"` `"ESRI Shapefile"` `"GeoJSON"`.
- `Gdal.VSIF*L` family: `VSIFSeekL(nint, int, int)` `VSIFTellL(nint)` `VSIFCloseL(nint)` `VSIFWriteL` beside `FileFromMemBuffer(string, byte[])`/`Unlink`/`Mkdir`/`ReadDir` over `/vsimem/`, `/vsizip/`, `/vsicurl/`; this build exposes `VSIFWriteL(string, …)` but no `VSIFReadL(byte[], …)`, so handle-level `/vsimem` byte read-back is absent.
- `Gdal` open/driver variants: `OpenShared` `IdentifyDriver` `AllRegister` `MultiDimTranslate`; `Ogr` adds `OpenShared` `GetDriver` `RegisterAll`; config: `SetConfigOption` `GetConfigOption` `GetLastErrorMsg`; `Ogr`/`Osr` carry their own `UseExceptions`/`DontUseExceptions`.

[ENTRYPOINT_SCOPE]: raster IO and algorithms

| [INDEX] | [SURFACE]                                                                                   | [SHAPE]  | [CAPABILITY]             |
| :-----: | :------------------------------------------------------------------------------------------ | :------- | :----------------------- |
|  [01]   | `Dataset.ReadRaster(int×4, T[], int×2, int, int[], int×3, RasterIOExtraArg?) -> CPLErr`     | instance | windowed multi-band read |
|  [02]   | `Dataset.GetGeoTransform(double[])`                                                         | instance | affine out               |
|  [03]   | `Dataset.GetExtent(Envelope, SpatialReference) -> CPLErr`                                   | instance | dataset extent           |
|  [04]   | `Dataset.GetExtentWGS84LongLat(Envelope) -> CPLErr`                                         | instance | WGS84 extent             |
|  [05]   | `Dataset.BuildOverviews(string, int[]) -> int`                                              | instance | pyramid                  |
|  [06]   | `Gdal.Warp(string, Dataset[], GDALWarpAppOptions)`                                          | static   | reproject/mosaic         |
|  [07]   | `Gdal.wrapper_GDALTranslate(string, Dataset, GDALTranslateOptions)`                         | static   | transcode                |
|  [08]   | `Gdal.wrapper_GDALDEMProcessing(string, Dataset, string, string, GDALDEMProcessingOptions)` | static   | terrain analysis         |
|  [09]   | `Gdal.wrapper_GDALContourDestName(string, Dataset, GDALContourOptions)`                     | static   | contour vectorize        |
|  [10]   | `Gdal.BuildVRT(string, Dataset[], GDALBuildVRTOptions)`                                     | static   | virtual mosaic           |
|  [11]   | `Driver.Create(string, int, int, int, DataType, string[]) -> Dataset`                       | instance | new raster               |
|  [12]   | `Driver.CreateCopy(string, Dataset, int, string[]) -> Dataset`                              | instance | copy/transcode           |

- `Dataset.ReadRaster`/`Band.ReadRaster` are per-type overloads over `byte[]`/`short[]`/`int[]`/`float[]`/`double[]` (and a `nint`+`DataType` untyped form), never a generic `<T>`; `WriteRaster` mirrors them. `bufXSize`/`bufYSize` differing from the window triggers on-read resampling, the kernel selected by `RasterIOExtraArg`.
- `Dataset` shape/lifecycle: `RasterXSize` `RasterYSize` `RasterCount` `GetRasterBand(int)` `GetProjection`/`SetProjection` `SetGeoTransform` `SetSpatialRef` `GetDriver` `FlushCache` `Close` `GetThreadSafeDataset(int)`. `Driver` write/identity: `CreateVector` `CreateMultiDimensional` `Delete` `Rename` `CopyFiles` `ShortName` `LongName`.
- `Envelope` is `OSGeo.OGR.Envelope` (`MinX`/`MaxX`/`MinY`/`MaxY`), not the NTS type; a windowed read derives its tile extent off the re-anchored affine corners, never the source-dataset call.
- `Warp`/`wrapper_GDAL*`/`BuildVRT` each take a trailing `(GDALProgressFuncDelegate, string)` progress pair and return `Dataset`. `Warp` options: `-t_srs` `-r` `-te` `-tr`; `BuildOverviews` kernels: `"AVERAGE"` `"GAUSS"` `"CUBIC"`; DEM modes: `"hillshade"` `"slope"` `"aspect"`, color relief.

[ENTRYPOINT_SCOPE]: band metadata and palette/RAT legend

| [INDEX] | [SURFACE]                                                                                | [SHAPE]  | [CAPABILITY]      |
| :-----: | :--------------------------------------------------------------------------------------- | :------- | :---------------- |
|  [01]   | `Band.GetBlockSize(out int, out int)`                                                    | instance | tile shape        |
|  [02]   | `Band.GetOffset(out double, out int)`                                                    | instance | decode offset     |
|  [03]   | `Band.GetScale(out double, out int)`                                                     | instance | decode scale      |
|  [04]   | `Band.GetNoDataValue(out double, out int)`                                               | instance | no-data sentinel  |
|  [05]   | `Band.GetMinimum(out double, out int)`                                                   | instance | stored minimum    |
|  [06]   | `Band.GetMaximum(out double, out int)`                                                   | instance | stored maximum    |
|  [07]   | `Band.GetUnitType() -> string`                                                           | instance | units             |
|  [08]   | `Band.GetColorInterpretation() -> ColorInterp`                                           | instance | channel role      |
|  [09]   | `Band.GetRasterColorTable() -> ColorTable`                                               | instance | palette           |
|  [10]   | `Band.GetDefaultRAT() -> RasterAttributeTable`                                           | instance | category table    |
|  [11]   | `Band.GetCategoryNames() -> string[]`                                                    | instance | category names    |
|  [12]   | `Band.GetStatistics(int, int, out double×4) -> CPLErr`                                   | instance | cached statistics |
|  [13]   | `Band.ComputeStatistics(bool, out double×4, …) -> CPLErr`                                | instance | statistics scan   |
|  [14]   | `Band.ComputeRasterMinMax(double[], int)`                                                | instance | range scan        |
|  [15]   | `Band.GetSampleOverview(ulong) -> Band`                                                  | instance | decimated level   |
|  [16]   | `Band.GetMaskBand() -> Band`                                                             | instance | validity band     |
|  [17]   | `Band.GetMaskFlags() -> int`                                                             | instance | validity flags    |
|  [18]   | `Band.GetDefaultHistogram(out double, out double, out int, out int[], int, …) -> CPLErr` | instance | cached histogram  |
|  [19]   | `Band.GetHistogram(double, double, int, int[], int, int, …) -> CPLErr`                   | instance | ranged histogram  |

- `Band.GetBlockSize` maps to `OverviewLevel.BlockX/BlockY` and `CoverageGrid.BaseBlockX/BaseBlockY`; a zero or strip height reads as a full-width row band. `GetOverview(int)`/`GetOverviewCount` supply the decreasing-resolution level set.
- Mask flags ride a raw `int`, not named `GMF_*` members: `0x01` all-valid, `0x02` per-dataset, `0x04` alpha, `0x08` nodata; the mask band reads through the same windowed `ReadRaster(byte[])` path.
- `Band` shape: `XSize` `YSize` `DataType`; `WriteRaster` mirrors `ReadRaster` per-type; `SetNoDataValue`/`DeleteNoDataValue` and `SetColorInterpretation` are the write-side peers.
- `ColorTable`: `ctor(PaletteInterp)` `GetCount()` `GetColorEntry(int)` `GetPaletteInterpretation()` `Clone()`. `RasterAttributeTable`: `GetRowCount` `GetColumnCount` `GetNameOfCol` `GetUsageOfCol` `GetTypeOfCol` `GetColOfUsage(GFU_Name)` → name column, `GetValueAsString`/`GetValueAsInt`/`GetValueAsDouble`/`GetValueAsBoolean`/`GetValueAsDateTime`, `GetRowOfValue(raw)` → row, `GetLinearBinning` `GetTableType` `Clone`. A `using` frees each `IDisposable` handle at the boundary.

[ENTRYPOINT_SCOPE]: vector iteration and the NTS bridge

| [INDEX] | [SURFACE]                                                  | [SHAPE]  | [CAPABILITY]    |
| :-----: | :--------------------------------------------------------- | :------- | :-------------- |
|  [01]   | `DataSource.GetLayerByIndex(int) -> Layer`                 | instance | indexed lookup  |
|  [02]   | `DataSource.GetLayerByName(string) -> Layer`               | instance | named lookup    |
|  [03]   | `DataSource.ExecuteSQL(string, Geometry, string) -> Layer` | instance | query layer     |
|  [04]   | `Layer.SetSpatialFilterRect(double×4)`                     | instance | bbox push-down  |
|  [05]   | `Layer.SetAttributeFilter(string)`                         | instance | WHERE push-down |
|  [06]   | `Layer.GetNextFeature() -> Feature`                        | instance | forward cursor  |
|  [07]   | `Layer.CreateFeature(Feature) -> OGRErr`                   | instance | write feature   |
|  [08]   | `Feature.GetGeometryRef() -> Geometry`                     | instance | geometry access |
|  [09]   | `Feature.GetFieldAsString(int) -> string`                  | instance | field read      |
|  [10]   | `Geometry.ExportToWkb(byte[], wkbByteOrder) -> int`        | instance | NTS egress      |
|  [11]   | `Geometry.CreateFromWkb(byte[]) -> Geometry`               | static   | NTS ingress     |

- `Layer` cursor family: `ResetReading` `GetFeature(long)` `GetFeatureCount`; mutation: `SetFeature` `UpsertFeature` `UpdateFeature` `DeleteFeature`; schema: `GetLayerDefn` `CreateField` `DeleteField` `ReorderField` `GetSpatialRef` `SetSpatialFilter`. `DataSource` layers/txn: `GetLayerCount` `CreateLayer` `CopyLayer` `GetDriver` `StartTransaction` `CommitTransaction` `RollbackTransaction`; `TestCapability(string)` gates the fast paths.
- `Feature` fields: `GetFieldAsInteger` `GetFieldAsInteger64` `GetFieldAsDouble` `GetFieldAsDateTime` `GetFieldAsISO8601DateTime` `GetFieldAs*List` `IsFieldSet` `GetFieldCount` `GetFieldDefnRef`; geometry: `SetGeometry` `SetGeometryDirectly` `GetGeomFieldRef` `SetGeomField`; `new Feature(FeatureDefn)` builds the write-path feature.
- `Geometry` wire: `ExportToWkt` `ExportToIsoWkt` `ExportToGML` `ExportToJson` `CreateFromWkt` `CreateFromGML` `WkbSize`; GEOS ops: `Intersection` `Union` `UnionCascaded` `Buffer` `Simplify` `SimplifyPreserveTopology`. `NetTopologySuite.IO.WKBReader.Read` materializes an exported buffer; `WKBWriter.Write` feeds `CreateFromWkb` write-back.
- `ExecuteSQL` dialects: OGR-SQL, `"SQLITE"`.

[ENTRYPOINT_SCOPE]: OSR reprojection (PROJ pipeline)

| [INDEX] | [SURFACE]                                                                                        | [SHAPE]  | [CAPABILITY]       |
| :-----: | :----------------------------------------------------------------------------------------------- | :------- | :----------------- |
|  [01]   | `SpatialReference.ImportFromEPSG(int) -> int`                                                    | instance | EPSG import        |
|  [02]   | `SpatialReference.SetFromUserInput(string) -> int`                                               | instance | CRS parse          |
|  [03]   | `SpatialReference.SetAxisMappingStrategy(AxisMappingStrategy)`                                   | instance | axis order         |
|  [04]   | `CoordinateTransformation(SpatialReference, SpatialReference, CoordinateTransformationOptions?)` | ctor     | build pipeline     |
|  [05]   | `CoordinateTransformation.TransformPoints(int, double[], double[], double[])`                    | instance | batch transform    |
|  [06]   | `Geometry.TransformTo(SpatialReference) -> int`                                                  | instance | geometry transform |

- `SpatialReference` construction: `ImportFromEPSGA` `ImportFromWkt` `ImportFromProj4` `ImportFromESRI` `ImportFromUrl` `ImportFromXML` `SetWellKnownGeogCS` `SetUTM`; export: `ExportToWkt` `ExportToPrettyWkt` `ExportToProj4` `ExportToPROJJSON`; identity: `IsProjected` `IsGeographic` `IsSame` `GetAuthorityCode` `GetAuthorityName`; dynamics: `IsDynamic` `GetCoordinateEpoch` `SetCoordinateEpoch(double)` carry the plate-motion datum epoch.
- `Geometry.TransformTo(SpatialReference)` reprojects in place through PROJ; `Geometry.Transform(CoordinateTransformation)` applies a prebuilt pipeline.
- `SetFromUserInput` inputs: `"EPSG:3857"`, WKT, PROJ string, URL.
- `CoordinateTransformation`: `TransformPoint(double[])` `TransformPoint4D` `TransformPointWithErrorCode` (per-point validity) `GetInverse`. `CoordinateTransformationOptions`: `SetAreaOfInterest` `SetOperation` `SetDesiredAccuracy` `SetBallparkAllowed(false)` faults a gridless pair, `SetOnlyBest(true)` faults when the best operation cannot instantiate.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every `OSGeo.*` op depends on `GdalBase.ConfigureAll()` running once at module init behind the `GdalBase.IsConfigured` guard; it registers every raster and vector driver and points `GDAL_DATA`/`PROJ_LIB` at the staged `gdal-data`, and a missing publish-RID native runtime faults it at first call.
- `Gdal.UseExceptions()` (with the `Ogr`/`Osr` peers) follows the bootstrap so a failed open or transform throws; the boundary catches it and lowers it onto `BimFault.CodecReject`, and domain code never branches on a raw `CPLErr`.
- Raster pixels move through `Dataset.ReadRaster`/`Band.ReadRaster` into a managed array whose element type matches `Band.DataType`; `GetGeoTransform` with `GetSpatialRef` places the raster, and the four `OSGeo.OGR.Envelope` doubles lower onto an NTS `Envelope` at the boundary. Output is `Driver.Create`/`CreateCopy`, `wrapper_GDALTranslate`, or `Gdal.Warp`, with `BuildOverviews` adding the pyramid a COG or 3D-Tiles terrain layer needs.
- `CoverageGrid` reads the full `Band` metadata surface once per ingest and lowers it onto self-describing `CoverageBand` values under `Real(raw)=Offset+Scale·raw`; consumers retain no `OSGeo.GDAL` enum. `RasterSampleType` rejects `GDT_Unknown`/`GDT_TypeCount`, `BandRole.TryGet` maps `GCI_*` and defaults to `Undefined`, and every `IDisposable` `ColorTable`/`RasterAttributeTable`/`ColorEntry` frees at the boundary so only value rows cross.
- OGR is the universal vector reader: `Ogr.Open` (or `Gdal.OpenEx` with `GDAL_OF_VECTOR`) opens every driver through one API, and `SetSpatialFilterRect`/`SetAttributeFilter` push the bbox and `WHERE` server-side so only matching features cross. `NetTopologySuite` owns the planar topology algebra; GEOS stays inside the native boundary as one canonical engine, and OGR-side boolean ops fragment the topology owner and stay out of the admitted path.
- OSR reprojection is the escalation path parallel to managed `ProjNET`: `ProjNET` owns CRS and datum transforms for managed planar geometry, OSR owns reprojection inside `Gdal.Warp`/`Geometry.TransformTo`/`Dataset.GetExtent` pipelines and the PROJ datum-grid or `IsDynamic`/`GetCoordinateEpoch` plate-motion cases. Every OSR CRS applies `SetAxisMappingStrategy(OAMS_TRADITIONAL_GIS_ORDER)`.

[STACKING]:
- `NetTopologySuite`(`.api/api-nettopologysuite.md`): `Geometry.ExportToWkb`/`CreateFromWkb(wkbNDR)` ↔ `NetTopologySuite.IO.WKBReader.Read`/`WKBWriter.Write` is the canonical bidirectional bridge; universal vector ingest enters as OGR features and the planar algebra, index, and predicates run on NTS after the wire hop.
- `ProjNET`(`.api/api-projnet.md`): OSR is the escalation counterpart. A transform the managed engine cannot express — a PROJ grid-shift or dynamic/plate-motion datum — escalates to a one-shot `CoordinateTransformation`; `ProjNET` owns every transform that stays managed.
- `NetTopologySuite.IO.Esri.Shapefile`(`.api/api-nts-esri-shapefile.md`): the managed codec is the default for `.shp` (no native dependency); GDAL's `"ESRI Shapefile"` driver is the path only for formats the managed codec does not cover.
- within-lib: `wrapper_GDALContourDestName` and `RasterizeLayer` cross the raster/vector boundary — a DEM becomes contour `Layer`s (then NTS geometry) and NTS footprints rasterize into a `Band`.
- within-lib: `Gdal.FileFromMemBuffer("/vsimem/...", bytes)` with the `/vsizip/`/`/vsicurl/` prefixes opens a dataset from an in-memory or remote source, feeding the `Rasm.Persistence` fsspec/stream ingest; `Layer.TestCapability(OLCFastGetArrowStream)` reports the Arrow C-stream path the Arrow rail consumes without per-feature marshalling.

[LOCAL_ADMISSION]:
- Bootstrap enters once through `GdalBase.ConfigureAll()` + `Gdal.UseExceptions()` at module init.
- Raster ingest enters through `Gdal.Open`/`OpenEx` → `Dataset.ReadRaster`; raster transform through `Gdal.Warp`/`wrapper_GDAL*`.
- Vector ingest enters through `Ogr.Open` → `Layer` with `SetSpatialFilterRect`/`SetAttributeFilter` → `Feature.GetGeometryRef.ExportToWkb` → NTS.
- Reprojection inside a GDAL pipeline enters through OSR; reprojection of managed geometry stays with `ProjNET`.

[RAIL_LAW]:
- Package: `MaxRev.Gdal.Core` (paired with a RID-keyed `MaxRev.Gdal.*Runtime*`)
- Owns: GDAL/OGR/OSR managed bindings — GeoTIFF/COG/DEM raster IO, the universal OGR vector driver set, the GDAL utility algorithms (warp/translate/DEM/grid/contour/rasterize), the virtual filesystem, and an escalation-path PROJ reprojection engine
- Accept: raster ingest/transcode/reproject, universal vector ingest with server-side filters, raster↔vector conversion, exotic datum transforms PROJ-only grids require, in-memory/remote dataset open
- Reject: the managed planar geometry algebra (`NetTopologySuite` owns it, OGR geometry bridged out at the wire), routine managed CRS transforms (`ProjNET` owns them), the native binaries (the runtime package owns them), shapefile-only IO where the managed codec suffices
