# [RASM_BIM_API_MAXREV_GDAL_MACOS_RUNTIME]

`MaxRev.Gdal.MacosRuntime.Minimal.arm64` ships the osx-arm64 native GDAL dylib closure backing `MaxRev.Gdal.Core` on Apple Silicon. Its managed assembly is an empty `<Module>` carrier; the payload is `runtimes/osx-arm64/native/` — `libgdal`, the SWIG `*_wrap` P/Invoke targets, native GEOS and PROJ, and the raster/vector format and compression tree. Presence on the publish RID is the precondition for every `OSGeo.GDAL`/`OGR`/`OSR` call to bind, and the bundled native GEOS and PROJ are the ABI substrate the `MaxRev.Gdal.Core` seam decisions escalate to.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `MaxRev.Gdal.MacosRuntime.Minimal.arm64`
- package: `MaxRev.Gdal.MacosRuntime.Minimal.arm64` (MIT)
- assembly: empty `<Module>` carrier, zero public types — carries the RID and triggers NuGet runtime-asset resolution
- target: `osx-arm64` only; sibling `*.WindowsRuntime.*`, `*.LinuxRuntime.*`, and `*.MacosRuntime.Minimal.x64` packages own the other publish RIDs
- asset: `runtimes/osx-arm64/native/**` — the native dylib closure with the `maxrev.gdal.core.libshared` load manifest
- rail: geometry (native substrate)

## [02]-[NATIVE_PAYLOAD]

Every dylib lands under `runtimes/osx-arm64/native/` on the geometry rail; `GdalBase.ConfigureAll()` reads `maxrev.gdal.core.libshared` to discover and load the set.

[CORE_AND_SWIG_BRIDGES]:

| [INDEX] | [DYLIB]             | [ROLE]                                                                                 |
| :-----: | :------------------ | :------------------------------------------------------------------------------------- |
|  [01]   | `libgdal`           | GDAL core: drivers + utilities behind `OSGeo.GDAL.Gdal`/`OSGeo.OGR.Ogr`                |
|  [02]   | `libgdal_wrap`      | SWIG `OSGeo.GDAL.GdalPINVOKE` — `Dataset`/`Band`/`Driver` ↔ `libgdal` raster bridge    |
|  [03]   | `libogr_wrap`       | SWIG `OSGeo.OGR.OgrPINVOKE` — `DataSource`/`Layer`/`Feature`/`Geometry` vector bridge  |
|  [04]   | `libosr_wrap`       | SWIG `OSGeo.OSR.OsrPINVOKE` — `SpatialReference`/`CoordinateTransformation` CRS bridge |
|  [05]   | `libgdalconst_wrap` | SWIG wrapper for GDAL constant tables (`DataType`/`CPLErr`/`ColorInterp` values)       |

[TOPOLOGY_AND_REPROJECTION]:

| [INDEX] | [DYLIB]                  | [ROLE]                                                        |
| :-----: | :----------------------- | :------------------------------------------------------------ |
|  [01]   | `libgeos` (+`libgeos_c`) | native planar topology behind OGR `Geometry` boolean ops      |
|  [02]   | `libproj`                | datum/grid reprojection behind OSR `CoordinateTransformation` |

[RASTER_CODECS]: `MultiDimTranslate`/`BuildVRT` drive the multidimensional path over the netCDF/HDF set.

| [INDEX] | [DYLIB_GROUP]                                                                       | [ROLE]                                        |
| :-----: | :---------------------------------------------------------------------------------- | :-------------------------------------------- |
|  [01]   | `libtiff` + `libgeotiff`                                                            | GeoTIFF/COG/BigTIFF (`Dataset.ReadRaster<T>`) |
|  [02]   | `libopenjp2` + `libopenjph`                                                         | JPEG2000 + HTJ2K imagery/elevation tiles      |
|  [03]   | `libjxl` (+`libjxl_cms`/`libjxl_threads`) + `libwebp` (+`libsharpyuv`)              | JPEG-XL and WebP raster codecs                |
|  [04]   | `libjpeg` + `libpng16` + `libgif`                                                   | baseline JPEG/PNG/GIF raster codecs           |
|  [05]   | `libnetcdf` + `libhdf5`/`libhdf5_hl`/`libhdf`/`libmfhdf`                            | netCDF/HDF5/HDF4 multidimensional raster      |
|  [06]   | `libcfitsio`                                                                        | FITS raster + DEM/climate-grid ingest         |
|  [07]   | `libOpenEXR`/`libOpenEXRCore`/`libOpenEXRUtil` + `libImath`/`libIex`/`libIlmThread` | OpenEXR high-dynamic-range raster             |
|  [08]   | `libpoppler` + `liblcms2`                                                           | PDF driver ingest + LittleCMS color           |

[VECTOR_DRIVERS]:

| [INDEX] | [DYLIB_GROUP]                                           | [ROLE]                                                    |
| :-----: | :------------------------------------------------------ | :-------------------------------------------------------- |
|  [01]   | `libsqlite3` + `libfreexl`                              | GeoPackage/SpatiaLite store + Excel `.xls` ingest         |
|  [02]   | `libpq`                                                 | PostgreSQL/PostGIS driver (`PostgreSQL`/`PostGISRaster`)  |
|  [03]   | `libmysqlclient` + `libodbc`/`libodbcinst`              | MySQL and ODBC vector data-source drivers                 |
|  [04]   | `libkmlbase`/`libkmldom`/`libkmlengine` + `libxerces-c` | KML/GML/CityGML parsers (LibKML + Xerces)                 |
|  [05]   | `libexpat` + `libxml2` + `libtinyxml2` + `liburiparser` | XML-family parsers + URI                                  |
|  [06]   | `libarrow` + `libparquet` + `libthrift`                 | GeoParquet driver + `OLCFastGetArrowStream` columnar bulk |
|  [07]   | `libcurl` + `libssl` + `libcrypto`                      | HTTP/TLS — `/vsicurl/` remote/cloud dataset open          |

[COMPRESSION]: this set backs the raster (DEFLATE/ZSTD/LZW TIFF), GeoPackage, Parquet, and `/vsizip/` paths.

| [INDEX] | [DYLIB_GROUP]                                                         | [ROLE]                                              |
| :-----: | :-------------------------------------------------------------------- | :-------------------------------------------------- |
|  [01]   | `libz` + `libdeflate` + `libzstd` + `liblz4`                          | DEFLATE/ZSTD/LZ4 general compression                |
|  [02]   | `libbz2` + `liblzma` + `libsnappy` + `libblosc`                       | bzip2/LZMA/Snappy/Blosc compression                 |
|  [03]   | `libbrotli*` + `libaec`/`libsz` + `libminizip`                        | Brotli + AEC + minizip (`/vsizip/`)                 |
|  [04]   | `libfontconfig` + `libfreetype` + `libpcre2-8` + `libhwy` + `libltdl` | text render + regex + Highway SIMD + libtool loader |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: none — the package is consumed transitively, never called

Restoring the package for the `osx-arm64` RID stages its native assets into the output, and `MaxRev.Gdal.Core.GdalBase.ConfigureAll()` discovers and loads them from there. A design page composes `MaxRev.Gdal.Core`; this package is its silent native prerequisite.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Pure native-asset carrier for one RID (`osx-arm64`); the managed assembly is empty by design. Cross-platform publish binds the sibling runtime packages, each carrying the GDAL/PROJ build matching the bindings.
- "Minimal" names the reduced driver subset — the common raster/vector/database drivers above, never the full GDAL plugin set (no GRASS/MrSID/ECW/Oracle/proprietary drivers); a format outside this closure faults at open with an unknown-driver error.
- `MSBuildRuntimeType == 'Core'` (the .NET SDK) resolves `runtimes/<RID>/native/**` through the NuGet runtime-asset graph, and a RID-targeted build copies the matching dylibs into the output. `build/net461/*.targets` gates its copy step on full-framework MSBuild and stays inert under the SDK, so the runtime-asset graph stages the natives.
- A RID-agnostic publish (no `-r osx-arm64`, no `RuntimeIdentifier`) does not stage the natives and faults at the first `OSGeo.*` call; the Apple-Silicon publish binds `osx-arm64` (or self-contained) for the dylibs to land.

[STACKING]:
- `MaxRev.Gdal.Core`(`.api/api-maxrev-gdal.md`): this closure is the native half its AnyCPU bindings P/Invoke — `libgdal_wrap`/`libogr_wrap`/`libosr_wrap` are the exact `*PINVOKE` targets, and bundled native GEOS/PROJ are the ABI substrate under the `[OGR_VS_NTS_GEOS]`/`[OSR_VS_PROJNET]` seam decisions the sibling owns.
- within-lib: SWIG `*_wrap` dylibs are generated against a specific `libgdal` ABI and the managed `*PINVOKE` classes against the same SWIG interface, so bindings and runtime bind as one unit and a version skew between them faults at load.
- `Rasm.Persistence` Arrow rail: bundled `libarrow`/`libparquet` are the ABI basis for `OLCFastGetArrowStream` — a GeoParquet source streams Arrow batches through `libarrow` without per-feature marshalling.

[LOCAL_ADMISSION]:
- Admitted as the transitive native prerequisite of `MaxRev.Gdal.Core` on osx-arm64, never referenced or called directly by `Rasm.Bim`.
- Admission obligates a RID-pinned `osx-arm64` publish so the natives stage, in version lockstep with the bindings.

[RAIL_LAW]:
- Package: `MaxRev.Gdal.MacosRuntime.Minimal.arm64`
- Owns: the osx-arm64 native GDAL dylib closure backing `MaxRev.Gdal.Core` — `libgdal`, the SWIG `*_wrap` P/Invoke targets, native GEOS/PROJ, and the raster/vector format and compression tree
- Accept: being the RID-keyed native runtime that binds every `OSGeo.GDAL`/`OGR`/`OSR` call on Apple Silicon, and the ABI source for the GEOS/PROJ seam escalations
- Reject: exposing any managed API (it has none), covering RIDs other than osx-arm64 (sibling packages own those), and the full GDAL plugin set (this is the minimal driver subset)
