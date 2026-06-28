# [RASM_BIM_API_MAXREV_GDAL]

`MaxRev.Gdal.Core` is the cross-platform GDAL 3.13.1 managed-bindings package: the
SWIG-generated `OSGeo.GDAL` (raster), `OSGeo.OGR` (vector), and `OSGeo.OSR` (spatial
reference) namespaces plus the `MaxRev.Gdal.Core.GdalBase` bootstrap that locates the
bundled `gdal-data`/PROJ resources and the native runtime. It owns the GeoTIFF/COG/DEM
raster ingest and the full OGR universal vector driver set (GeoPackage, FlatGeobuf, KML,
GML, GeoJSON, FileGDB, shapefile, PostGIS, …), the GDAL utility algorithms (Warp/Translate/
VectorTranslate/DEMProcessing/Grid/Contour/Rasterize), and a PROJ-backed reprojection
engine (OSR) parallel to `ProjNET`. The package is managed AnyCPU; the native GDAL/GEOS/PROJ
libraries are supplied by a RID-keyed runtime package (`MaxRev.Gdal.MacosRuntime.Minimal.arm64`
on Apple Silicon — the `api-maxrev-gdal-macos-runtime` catalog).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `MaxRev.Gdal.Core`
- package: `MaxRev.Gdal.Core`
- version: `3.13.1.534` (GDAL 3.13.1)
- license: MIT (the bindings + `GdalBase` bootstrap); the underlying GDAL/OGR/PROJ corpus is its own permissive mix (MIT/X11-style), `gdal-data/LICENSE.TXT` ships in the runtime
- assembly: `MaxRev.Gdal.Core`
- namespace: `MaxRev.Gdal.Core` (`GdalBase` configurator — the bootstrap owner)
- namespace: `OSGeo.GDAL` (`Gdal` statics, `Dataset`, `Band`, `Driver`, `DataType`, `ColorInterp`, `GDAL*Options` algorithm option carriers)
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

| [INDEX] | [SYMBOL]    | [RAIL]   | [CAPABILITY]                                                                                              |
| :-----: | :---------- | :------- | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `GdalBase`  | geometry | static configurator; `ConfigureAll()` (the single bootstrap — registers all GDAL+OGR drivers AND wires `gdal-data`/PROJ paths), `ConfigureGdalDrivers(gdalDataFolder?)`, `ConfigureGdalData(gdalDataFolder?)`, `IsConfigured`, `EnableRuntimeValidation`. `ConfigureAll()` MUST run once before any `Gdal`/`Ogr`/`Osr` call |

[PUBLIC_TYPE_SCOPE]: GDAL raster
- package: `MaxRev.Gdal.Core`
- namespace: `OSGeo.GDAL`
- rail: geometry

| [INDEX] | [SYMBOL]      | [RAIL]   | [CAPABILITY]                                                                                              |
| :-----: | :------------ | :------- | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `Gdal`        | geometry | the raster API statics; `Open`/`OpenEx`/`OpenShared`, `GetDriverByName`/`IdentifyDriver`, `AllRegister`, `Warp`/`BuildVRT`/`MultiDimTranslate`, the `wrapper_GDAL*` utility set, `SetConfigOption`/`GetConfigOption`, the `/vsimem//vsizip//vsicurl/` virtual-filesystem (`FileFromMemBuffer`/`Unlink`/`Mkdir`/`ReadDir` + the `VSIFOpenL`/`VSIFSeekL`/`VSIFTellL`/`VSIFCloseL`/`VSIFWriteL` file-handle API), error handling (`UseExceptions`/`GetLastErrorMsg`) |
|  [02]   | `Dataset`     | geometry | an opened raster (or vector) dataset; `RasterXSize`/`RasterYSize`/`RasterCount`, `GetRasterBand(int)`, typed `ReadRaster<T>`/`WriteRaster<T>` (byte/short/int/float/double overloads + `RasterIOExtraArg` resampling), `GetGeoTransform`/`SetGeoTransform` (the 6-coefficient affine), `GetProjection`/`SetProjection`/`GetSpatialRef`/`SetSpatialRef`, `GetExtent`/`GetExtentWGS84LongLat`, `BuildOverviews`, `GetDriver`, `FlushCache`, `Close`, `GetThreadSafeDataset` |
|  [03]   | `Band`        | geometry | one raster band; `XSize`/`YSize`/`DataType`, typed `ReadRaster<T>`/`WriteRaster<T>` (windowed, with optional `RasterIOExtraArg`), `GetBlockSize`, `GetNoDataValue`/`SetNoDataValue`, `GetStatistics`/`ComputeStatistics` (min/max/mean/stddev), `GetColorInterpretation`/`SetColorInterpretation`, `GetOverview`/`GetOverviewCount` |
|  [04]   | `Driver`      | geometry | a format driver; `Create(path, xsize, ysize, bands, DataType, options)`, `CreateCopy(path, src, strict, options, …)`, `CreateVector(path, options)`, `CreateMultiDimensional`, `Delete`/`Rename`/`CopyFiles`. `ShortName`/`LongName` identify the format |
|  [05]   | `DataType`    | geometry | enum: `GDT_Byte`/`GDT_UInt8`, `GDT_Int8`, `GDT_UInt16`/`GDT_Int16`, `GDT_UInt32`/`GDT_Int32`, `GDT_UInt64`/`GDT_Int64`, `GDT_Float32`/`GDT_Float64`, `GDT_CFloat32`/`GDT_CFloat64` (complex) — the pixel type that selects the `ReadRaster<T>` overload |
|  [06]   | `ColorInterp` | geometry | band color role: `GCI_GrayIndex`/`GCI_RedBand`/`GCI_GreenBand`/`GCI_BlueBand`/`GCI_AlphaBand`/`GCI_PaletteIndex`/… |
|  [07]   | `GDALWarpAppOptions` / `GDALTranslateOptions` / `GDALVectorTranslateOptions` / `GDALDEMProcessingOptions` / `GDALGridOptions` / `GDALContourOptions` / `GDALRasterizeOptions` | geometry | each constructs from a `string[]` of the corresponding CLI flags (`new GDALWarpAppOptions(new[]{"-t_srs","EPSG:3857","-r","bilinear"})`) — the typed carrier the `wrapper_GDAL*`/`Warp` utilities consume |

[PUBLIC_TYPE_SCOPE]: OGR vector
- package: `MaxRev.Gdal.Core`
- namespace: `OSGeo.OGR`
- rail: geometry

| [INDEX] | [SYMBOL]            | [RAIL]   | [CAPABILITY]                                                                                              |
| :-----: | :------------------ | :------- | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `Ogr`               | geometry | the vector API statics; `Open(path, update)`/`OpenShared`, `GetDriverByName`/`GetDriver`, `RegisterAll`, `UseExceptions`/`DontUseExceptions` (the SWIG error-model flip, called once after `GdalBase.ConfigureAll()`), plus the `OLC*`/`ODsC*` capability-string constants (`OLCFastGetArrowStream`, `OLCFastWriteArrowBatch`, `OLCTransactions`, `ODsCCreateLayer`, …) `TestCapability` is queried with |
|  [02]   | `Driver` (OGR)      | geometry | an OGR vector format driver (distinct from the `OSGeo.GDAL.Driver` raster driver above); `CreateDataSource(string name, string[] options)`/`CopyDataSource`/`DeleteDataSource`/`Open` — the create-side surface `Ogr.GetDriverByName` returns for the universal-vector `/vsimem` egress (GeoPackage/KML/GML/FileGDB write through one driver path) |
|  [03]   | `DataSource`        | geometry | an opened vector dataset; `GetLayerCount`/`GetLayerByIndex`/`GetLayerByName`, `CreateLayer(name, SpatialReference, wkbGeometryType, options)`, `CopyLayer`, `ExecuteSQL(statement, spatialFilter, dialect)` (OGR-SQL / SQLite dialect), `StartTransaction`/`CommitTransaction`/`RollbackTransaction`, `TestCapability`, `GetDriver` |
|  [04]   | `Layer`             | geometry | a feature layer; `GetNextFeature`/`ResetReading`/`GetFeature(fid)` cursor, `GetFeatureCount`, `CreateFeature`/`SetFeature`/`UpsertFeature`/`UpdateFeature`/`DeleteFeature`, `SetSpatialFilter`/`SetSpatialFilterRect` (server-side bbox push-down), `SetAttributeFilter(string)` (server-side WHERE), `GetExtent`, `GetLayerDefn`, `CreateField`/`DeleteField`/`ReorderField`, `GetSpatialRef` |
|  [05]   | `Feature`           | geometry | one OGR feature; ctor `new Feature(FeatureDefn)` builds an empty feature against a layer's definition for the write path, `GetGeometryRef`/`SetGeometry`/`SetGeometryDirectly`, the typed field matrix `GetFieldAsString`/`GetFieldAsInteger`/`GetFieldAsInteger64`/`GetFieldAsDouble`/`GetFieldAsDateTime`/`GetFieldAsISO8601DateTime`/`GetFieldAs*List`, `IsFieldSet`, `GetFieldCount`/`GetFieldDefnRef`, multi-geometry-field `GetGeomFieldRef`/`SetGeomField` |
|  [06]   | `Geometry`          | geometry | an OGR geometry; `WkbSize()` (the WKB byte length sizing the `ExportToWkb` buffer), `ExportToWkb(byte[], wkbByteOrder)` / `ExportToWkt`/`ExportToIsoWkt`/`ExportToGML([options])`/`ExportToJson(string[] options)`, static `CreateFromWkb`/`CreateFromWkt`/`CreateFromGML`, `TransformTo(SpatialReference)`/`Transform(CoordinateTransformation)` (geometry-level reprojection), the GEOS-backed `Intersection`/`Union`/`UnionCascaded`/`Buffer`/`Simplify`/`SimplifyPreserveTopology` (native libgeos) |
|  [07]   | `FieldDefn` / `FeatureDefn` | geometry | field schema (`GetName`/`GetFieldType`/`GetWidth`/`GetPrecision`) and the layer's full field+geometry-field definition |
|  [08]   | `wkbGeometryType`   | geometry | enum: `wkbUnknown`/`wkbPoint`/`wkbLineString`/`wkbPolygon`/`wkbMultiPoint`/`wkbMultiLineString`/`wkbMultiPolygon`/`wkbGeometryCollection`/`wkbNone` and the `*M`/`*ZM`/`*25D` variants — the layer geometry-column type |
|  [09]   | `wkbByteOrder`      | geometry | enum: `wkbXDR` (big-endian) / `wkbNDR` (little-endian) — the byte order `Geometry.ExportToWkb(byte[], wkbByteOrder)` writes the WKB in (`wkbNDR` for the NTS `WKBReader` bridge) |

[PUBLIC_TYPE_SCOPE]: OSR spatial reference (PROJ-backed)
- package: `MaxRev.Gdal.Core`
- namespace: `OSGeo.OSR`
- rail: geometry

| [INDEX] | [SYMBOL]                      | [RAIL]   | [CAPABILITY]                                                                                              |
| :-----: | :---------------------------- | :------- | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `Osr`                         | geometry | the OSR API statics; `UseExceptions`/`DontUseExceptions` — the SWIG error-model flip called once after `GdalBase.ConfigureAll()` alongside `Gdal.UseExceptions()`/`Ogr.UseExceptions()` so a failed CRS import/transform throws rather than silent-returning a code |
|  [02]   | `SpatialReference`            | geometry | a CRS; `SpatialReference(wkt)` ctor, the import family `ImportFromEPSG(int)`/`ImportFromEPSGA`/`ImportFromWkt`/`ImportFromProj4`/`ImportFromESRI`/`ImportFromUrl`/`ImportFromXML`, `SetWellKnownGeogCS`/`SetUTM`/`SetFromUserInput("EPSG:4326")`, the export `ExportToWkt`/`ExportToPrettyWkt`/`ExportToProj4`/`ExportToPROJJSON`, predicates `IsProjected`/`IsGeographic`/`IsSame`, `GetAuthorityCode`/`GetAuthorityName`, `IsDynamic`/`GetCoordinateEpoch` (datum-epoch for plate-motion), `SetAxisMappingStrategy` |
|  [03]   | `CoordinateTransformation`    | geometry | a PROJ transformation pipeline; `CoordinateTransformation(src, dst[, options])`, `GetInverse`, `TransformPoint(double[])`/`TransformPoint4D`, `TransformPoints(n, x[], y[], z[])` (batch), `TransformPointWithErrorCode` (per-point validity) |
|  [04]   | `CoordinateTransformationOptions` | geometry | pipeline tuning; `SetAreaOfInterest(westDeg, southDeg, eastDeg, northDeg)`, `SetOperation(proj_op, inverse)`, `SetDesiredAccuracy(m)` — the control over which PROJ datum pipeline is chosen |
|  [05]   | `AxisMappingStrategy`         | geometry | enum: `OAMS_TRADITIONAL_GIS_ORDER` (lon/lat), `OAMS_AUTHORITY_COMPLIANT` (EPSG axis order), `OAMS_CUSTOM` — the axis-order knob that prevents the lat/lon-vs-lon/lat swap GDAL 3 introduced |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: bootstrap and open
- package: `MaxRev.Gdal.Core`
- namespace: `MaxRev.Gdal.Core`, `OSGeo.GDAL`, `OSGeo.OGR`
- rail: geometry

| [INDEX] | [SURFACE]                | [CALL_SHAPE]                                                              | [CAPABILITY]                                       |
| :-----: | :----------------------- | :----------------------------------------------------------------------- | :------------------------------------------------- |
|  [01]   | `GdalBase.ConfigureAll`  | `()` (static)                                                            | the once-per-process bootstrap; registers every GDAL+OGR driver and resolves `gdal-data`/PROJ paths from the runtime package. Every other call faults without it |
|  [02]   | `Gdal.UseExceptions`     | `()` (static)                                                            | flips the SWIG error model from return-code to thrown `ApplicationException` — call right after `ConfigureAll()` so a failed open surfaces as an exception to lower onto `BimFault.CodecReject` |
|  [03]   | `Gdal.Open`              | `(string path, Access eAccess)` → `Dataset`                             | open a raster (or any GDAL-readable) dataset; `Access.GA_ReadOnly`/`GA_Update` |
|  [04]   | `Gdal.OpenEx`            | `(string path, uint openFlags, string[] allowedDrivers, string[] openOptions, string[] siblingFiles)` → `Dataset` | the full open: `GDAL_OF_RASTER`/`GDAL_OF_VECTOR`/`GDAL_OF_UPDATE` flags + driver allow-list + per-driver open options |
|  [05]   | `Ogr.Open`               | `(string path, int update)` → `DataSource`                              | open a vector dataset (the OGR-side open; `update`=0 read, 1 write) |
|  [06]   | `Gdal.GetDriverByName` / `Ogr.GetDriverByName` | `(string name)` → `Driver`                                | resolve a named driver (`"GTiff"`, `"COG"`, `"GPKG"`, `"FlatGeobuf"`, `"ESRI Shapefile"`, `"GeoJSON"`) for `Create`/`CreateCopy` |
|  [07]   | `Gdal.VSIFOpenL` / `VSIFSeekL` / `VSIFTellL` / `VSIFCloseL` | `(string path, string mode)`→`nint` · `(nint fp, int offset, int whence)`→`int` · `(nint fp)`→`int` · `(nint fp)`→`int` | the `/vsimem` file-handle API — open a virtual file then seek-to-end/tell to size it and close it; the handle-level complement to `FileFromMemBuffer`/`Unlink`. NOTE: this SWIG build exposes only `VSIFWriteL(string, …)` and NO `byte[]` `VSIFReadL` overload, so a `/vsimem` byte read-back has no handle-level read primitive here (a catalog or design page naming `Gdal.VSIFReadL(byte[], …)` against this package documents a phantom) |

[ENTRYPOINT_SCOPE]: raster IO and algorithms
- package: `MaxRev.Gdal.Core`
- namespace: `OSGeo.GDAL`
- rail: geometry

| [INDEX] | [SURFACE]                          | [CALL_SHAPE]                                                                                        | [CAPABILITY]                                       |
| :-----: | :--------------------------------- | :------------------------------------------------------------------------------------------------ | :------------------------------------------------- |
|  [01]   | `Dataset.ReadRaster<T>`            | `(xOff, yOff, xSize, ySize, T[] buffer, buf_xSize, buf_ySize, bandCount, int[] bandMap, pixelSpace, lineSpace, bandSpace[, RasterIOExtraArg])` → `CPLErr` | windowed multi-band pixel read into a managed `T[]` (T ∈ byte/short/int/float/double); `RasterIOExtraArg` selects the resampling kernel for on-read downsample |
|  [02]   | `Dataset.GetGeoTransform`          | `(double[] argout)` (6 coefficients)                                                               | the affine `[originX, pxW, rowRot, originY, colRot, pxH]` mapping pixel→georeferenced space |
|  [03]   | `Dataset.GetExtent`                | `(Envelope extent, SpatialReference srs)` → `CPLErr`                                               | the dataset bbox in a target CRS — directly fills an NTS `Envelope` |
|  [04]   | `Dataset.BuildOverviews`           | `(string resampling, int[] overviewList[, callback])` → `int`                                      | pyramid generation (`"AVERAGE"`/`"GAUSS"`/`"CUBIC"`) for COG/tiled output |
|  [05]   | `Gdal.Warp`                        | `(string dest, Dataset[] src, GDALWarpAppOptions, callback, data)` → `Dataset`                     | reproject + mosaic + resample (the `gdalwarp` algorithm); options carry `-t_srs`/`-r`/`-te`/`-tr` |
|  [06]   | `Gdal.wrapper_GDALTranslate`       | `(string dest, Dataset src, GDALTranslateOptions, callback, data)` → `Dataset`                     | format/subset/rescale convert (the `gdal_translate` algorithm) — the GeoTIFF→COG transcode |
|  [07]   | `Gdal.wrapper_GDALDEMProcessing`   | `(string dest, Dataset src, string mode, string colorFile, GDALDEMProcessingOptions, callback, data)` → `Dataset` | hillshade/slope/aspect/color-relief from a DEM (`mode` ∈ `"hillshade"`/`"slope"`/`"aspect"`) |
|  [08]   | `Gdal.wrapper_GDALContourDestName` | `(string dest, Dataset src, GDALContourOptions, callback, data)` → `Dataset`                       | iso-contour vectorization of a DEM band → an OGR layer (the terrain contour lines into the BIM site model) |
|  [09]   | `Gdal.BuildVRT`                    | `(string dest, Dataset[] src, GDALBuildVRTOptions, callback, data)` → `Dataset`                    | a virtual mosaic dataset over many sources without materializing pixels |

[ENTRYPOINT_SCOPE]: vector iteration and the NTS bridge
- package: `MaxRev.Gdal.Core`
- namespace: `OSGeo.OGR`
- rail: geometry

| [INDEX] | [SURFACE]                          | [CALL_SHAPE]                                                            | [CAPABILITY]                                       |
| :-----: | :--------------------------------- | :--------------------------------------------------------------------- | :------------------------------------------------- |
|  [01]   | `DataSource.GetLayerByIndex` / `GetLayerByName` | `(int)` / `(string)` → `Layer`                            | resolve a layer to iterate                         |
|  [02]   | `Layer.SetSpatialFilterRect`       | `(double minx, miny, maxx, maxy)`                                      | server-side bbox push-down — only features in the window are returned (the site/context clip) |
|  [03]   | `Layer.SetAttributeFilter`         | `(string whereClause)`                                                 | server-side attribute WHERE (driver-evaluated)     |
|  [04]   | `Layer.GetNextFeature`             | `()` → `Feature` (null at end; reset with `ResetReading`)             | the forward cursor over the (filtered) feature set |
|  [05]   | `Feature.GetGeometryRef`           | `()` → `Geometry`                                                      | the OGR geometry of the current feature            |
|  [06]   | `Geometry.ExportToWkb`             | `(byte[] buffer, wkbByteOrder)` / `(byte[] buffer)` → `int`           | serialize to WKB → `NetTopologySuite.IO.WKBReader.Read(buffer)` materializes the NTS `Geometry`. This is THE OGR→NTS bridge |
|  [07]   | `Geometry.CreateFromWkb`           | `(byte[] wkb)` (static) → `Geometry`                                  | the reverse: an NTS `WKBWriter.Write` buffer → an OGR geometry for write-back through `Feature.SetGeometry` |
|  [08]   | `DataSource.ExecuteSQL`            | `(string statement, Geometry spatialFilter, string dialect)` → `Layer` | run OGR-SQL or the `"SQLITE"` dialect against the datasource; the result is a temporary `Layer` |

[ENTRYPOINT_SCOPE]: OSR reprojection (PROJ pipeline)
- package: `MaxRev.Gdal.Core`
- namespace: `OSGeo.OSR`
- rail: geometry

| [INDEX] | [SURFACE]                              | [CALL_SHAPE]                                              | [CAPABILITY]                                       |
| :-----: | :------------------------------------- | :------------------------------------------------------- | :------------------------------------------------- |
|  [01]   | `SpatialReference.ImportFromEPSG`      | `(int code)` → `int` (0 = OK)                            | build a CRS from an EPSG code                       |
|  [02]   | `SpatialReference.SetFromUserInput`    | `(string)` → `int`                                      | parse `"EPSG:3857"`/a WKT/a PROJ string/a URL into the CRS |
|  [03]   | `SpatialReference.SetAxisMappingStrategy` | `(AxisMappingStrategy.OAMS_TRADITIONAL_GIS_ORDER)`   | force lon/lat order — the guard against the GDAL-3 axis swap |
|  [04]   | `new CoordinateTransformation`         | `(SpatialReference src, SpatialReference dst[, CoordinateTransformationOptions])` | build the PROJ pipeline between two CRS |
|  [05]   | `CoordinateTransformation.TransformPoints` | `(int n, double[] x, double[] y, double[] z)`        | batch in-place reprojection of `n` coordinates      |
|  [06]   | `Geometry.TransformTo`                 | `(SpatialReference target)` → `int`                     | reproject an OGR geometry in place to a target CRS (PROJ-backed, datum-grid-aware) |

## [04]-[IMPLEMENTATION_LAW]

[PLATFORM_BOUNDARY]:
- the bindings DLL is managed AnyCPU (binds `lib/net10.0`), but it is NOT self-contained: every `OSGeo.*` call P/Invokes the native `libgdal_wrap`/`libgdalconst_wrap` (and through them `libgdal`/`libgeos`/`libproj`) shipped by a separate RID-keyed runtime package. On osx-arm64 that is `MaxRev.Gdal.MacosRuntime.Minimal.arm64`; a Windows/Linux publish needs the matching RID runtime. Without a runtime for the publish RID, `GdalBase.ConfigureAll()` or the first `OSGeo.*` call faults — this is the package's hard platform constraint and the reason the runtime pin tracks the same `3.13.1.534` version.
- `runtimes/any/native/gdal-data/**` is the PROJ/EPSG resource set (CSV/WKT/grid metadata) the `.targets` stages into the output and `GdalBase.ConfigureGdalData` points the `GDAL_DATA`/`PROJ_LIB` config at. CRS resolution and datum transforms fail without it even when the native libraries load.

[BOOTSTRAP]:
- `GdalBase.ConfigureAll()` is the mandatory once-per-process bootstrap: it registers every GDAL raster driver and OGR vector driver and resolves the `gdal-data`/PROJ paths. The canonical owner runs it exactly once at module init behind an idempotency guard (`GdalBase.IsConfigured`), never per-open.
- follow it immediately with `Gdal.UseExceptions()` (and `Ogr`/`Osr` equivalents) so a failed open/transform throws rather than returning a `CPLErr` that silent-passes — the thrown exception is caught at the boundary and lowered onto `BimFault.CodecReject`. Domain code never branches on raw `CPLErr` return codes.

[RASTER_IO]:
- raster pixels move through `Dataset.ReadRaster<T>`/`Band.ReadRaster<T>` into a managed `T[]` whose element type matches the `Band.DataType` (`GDT_Byte`→`byte[]`, `GDT_Float32`→`float[]`, `GDT_Float64`→`double[]`). The `bufXSize`/`bufYSize` arguments differing from the window size trigger GDAL's on-read resampling (kernel selected by `RasterIOExtraArg`) — a DEM is downsampled to a working resolution in one call.
- `Dataset.GetGeoTransform` + `Dataset.GetSpatialRef` together place the raster in georeferenced space; `Dataset.GetExtent(Envelope, srs)` fills an NTS `Envelope` directly in the target CRS — the bbox the `STRtree`/site-model uses to position the raster against vector features.
- raster output is `Driver.Create`/`Driver.CreateCopy` (e.g. the `"COG"` driver with creation options) or the `wrapper_GDALTranslate`/`Gdal.Warp` algorithms; `BuildOverviews` adds the pyramid a COG/3D-Tiles terrain layer needs.

[VECTOR_INGEST]:
- the OGR side is the universal vector reader: `Ogr.Open` (or `Gdal.OpenEx` with `GDAL_OF_VECTOR`) opens GeoPackage/FlatGeobuf/KML/GML/GeoJSON/FileGDB/PostGIS/shapefile/… through one API. Iterate `Layer.GetNextFeature` after applying `SetSpatialFilterRect` (bbox push-down) and `SetAttributeFilter` (WHERE) so the driver evaluates the filter server-side and only matching features cross the boundary.
- `Layer.TestCapability(Ogr.OLCFastGetArrowStream)` reports whether a driver exposes the Arrow C-stream bulk path — the columnar fast read for a GeoParquet/FlatGeobuf source the `Rasm.Persistence` Arrow rail consumes without per-feature marshalling (the capability string is the `Ogr` constant; the stream itself is reached through the SWIG Arrow-array interface).

[OGR_VS_NTS_GEOS]:
- the native runtime bundles `libgeos` (GDAL's GEOS), so OGR `Geometry` exposes GEOS-backed `Intersection`/`Union`/`Buffer`/`Simplify`. The admitted policy keeps the planar topology algebra in MANAGED `NetTopologySuite` (the `api-nettopologysuite` core), NOT in OGR's GEOS — there is one canonical planar engine, and GEOS stays inside the GDAL native boundary. OGR geometry is bridged OUT to NTS at the wire (`ExportToWkb` → `WKBReader.Read`) the moment it leaves the ingest, and NTS geometry is bridged back IN (`WKBWriter.Write` → `CreateFromWkb`) only when writing through an OGR driver. Running boolean ops on the OGR side is the rejected form — it fragments the topology owner.

[OSR_VS_PROJNET]:
- OSR (`SpatialReference`/`CoordinateTransformation`) is a FULL PROJ-backed reprojection engine bundled inside this package, parallel to the managed `ProjNET` (the `api-projnet` catalog). The seam law: `ProjNET` is the admitted managed CRS/datum owner for geometry already in the managed planar algebra (no native dependency, the `georeference#GEODETIC_TRANSFORM` leg); OSR is used only when reprojection happens INSIDE a GDAL pipeline — `Gdal.Warp` with `-t_srs`, or `Geometry.TransformTo` on an OGR geometry before it crosses to NTS, or `Dataset.GetExtent(env, srs)`. OSR is strictly more capable for exotic datum-grid transforms (it carries PROJ's full grid set and `IsDynamic`/`GetCoordinateEpoch` plate-motion handling), so a transform `ProjNET` cannot express is escalated to a one-shot OSR `CoordinateTransformation`. Always `SetAxisMappingStrategy(OAMS_TRADITIONAL_GIS_ORDER)` on an OSR CRS to avoid the GDAL-3 lat/lon axis swap.

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
