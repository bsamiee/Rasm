# [RASM_BIM_API_MAXREV_GDAL_MACOS_RUNTIME]

`MaxRev.Gdal.MacosRuntime.Minimal.arm64` is the osx-arm64 native runtime package backing
`MaxRev.Gdal.Core` on Apple Silicon. It ships NO managed API — the assembly is an empty
`<Module>` carrier and the package's entire payload is the `runtimes/osx-arm64/native/`
dylib closure: `libgdal`, the SWIG `*_wrap` P/Invoke targets the `OSGeo.GDAL`/`OGR`/ `OSR` bindings call, OGR's `libgeos`, OSR's `libproj`, and the full raster/vector format and
compression dependency tree. It is the RID-keyed native half of the geospatial seam: it has
no design surface of its own, but its presence on the publish RID is the precondition for
every `MaxRev.Gdal.Core` call to bind, and its bundled `libgeos`/`libproj` versions are the
ABI fact that fixes the OGR-vs-NTS and OSR-vs-ProjNET seam decisions.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `MaxRev.Gdal.MacosRuntime.Minimal.arm64`
- package: `MaxRev.Gdal.MacosRuntime.Minimal.arm64`
- license: MIT (the package); the bundled native libraries carry their own upstream licenses (`gdal-data/LICENSE.TXT` in `MaxRev.Gdal.Core` covers the GDAL/PROJ corpus)
- assembly: `MaxRev.Gdal.MacosRuntime.Minimal.arm64` — an EMPTY managed assembly (`<Module>` only, zero public types). It exists to carry the RID and trigger NuGet runtime-asset resolution, not to expose a managed surface
- RID: `osx-arm64` ONLY (Apple Silicon). A `MaxRev.Gdal.MacosRuntime.Minimal.x64` (Intel), `*.WindowsRuntime.*`, or `*.LinuxRuntime.*` is a SEPARATE package for a different publish RID — this catalog is the arm64 host
- asset: `runtimes/osx-arm64/native/**` — 75 `.dylib` files plus the `maxrev.gdal.core.libshared` manifest. This is the entire useful payload
- asset: `lib/{net10.0,net9.0,…,netstandard2.0}/MaxRev.Gdal.MacosRuntime.Minimal.arm64.dll` — the empty carrier assembly per TFM; the net10.0 consumer binds `lib/net10.0`
- asset: `build/net461/MaxRev.Gdal.MacosRuntime.Minimal.arm64.targets` — a net461-MSBuild-only copy step (see [04])
- rail: geometry (native substrate)

## [02]-[NATIVE_PAYLOAD]

[NATIVE_SCOPE]: GDAL core + SWIG P/Invoke targets
- package: `MaxRev.Gdal.MacosRuntime.Minimal.arm64`
- asset: `runtimes/osx-arm64/native/`
- rail: geometry

| [INDEX] | [DYLIB]                                                  | [RAIL]   | [ROLE]                                                                                                                                                           |
| :-----: | :------------------------------------------------------- | :------- | :--------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `libgdal.39.3.13.1.dylib` (+ `libgdal.39.dylib` symlink) | geometry | the GDAL core: every raster driver, the OGR vector driver registry, the GDAL utility algorithms — the library `OSGeo.GDAL.Gdal`/`OSGeo.OGR.Ogr` ultimately drive |
|  [02]   | `libgdal_wrap.dylib`                                     | geometry | the SWIG C-wrapper `OSGeo.GDAL.GdalPINVOKE` P/Invokes — the bridge between the managed `Dataset`/`Band`/`Driver` and `libgdal`'s raster C API                    |
|  [03]   | `libogr_wrap.dylib`                                      | geometry | the SWIG wrapper `OSGeo.OGR.OgrPINVOKE` P/Invokes — the bridge for the managed `DataSource`/`Layer`/`Feature`/`Geometry` vector API                              |
|  [04]   | `libosr_wrap.dylib`                                      | geometry | the SWIG wrapper `OSGeo.OSR.OsrPINVOKE` P/Invokes — the bridge for the managed `SpatialReference`/`CoordinateTransformation` CRS API                             |
|  [05]   | `libgdalconst_wrap.dylib`                                | geometry | the SWIG wrapper for the GDAL constant tables (the `DataType`/`CPLErr`/`ColorInterp` enum values the bindings read at load)                                      |
|  [06]   | `maxrev.gdal.core.libshared`                             | geometry | the dylib manifest `MaxRev.Gdal.Core.GdalBase.ConfigureAll()` reads to discover and load the native library set from the runtime directory                       |

[NATIVE_SCOPE]: topology and reprojection engines
- package: `MaxRev.Gdal.MacosRuntime.Minimal.arm64`
- asset: `runtimes/osx-arm64/native/`
- rail: geometry

| [INDEX] | [DYLIB]                                             | [RAIL]   | [ROLE]                                                                                                                                                                                                                                                                                                                                                                                          |
| :-----: | :-------------------------------------------------- | :------- | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `libgeos.3.14.1.dylib` (+ `libgeos_c.1.20.5.dylib`) | geometry | GEOS — the native planar-topology engine behind OGR `Geometry.Intersection`/`Union`/`Buffer`/`Simplify`. The ABI fact behind the OGR-vs-NTS seam: a second GEOS-equivalent engine exists in the closure, so the admitted policy keeps managed `NetTopologySuite` as the ONE canonical planar owner and bridges OGR geometry out at the wire rather than running boolean ops on this native GEOS |
|  [02]   | `libproj.25.9.7.1.dylib`                            | geometry | PROJ — the datum/grid reprojection engine behind OSR `CoordinateTransformation` and `Gdal.Warp -t_srs`. The ABI fact behind the OSR-vs-ProjNET seam: OSR carries PROJ's full grid set (the exotic-datum escalation path managed `ProjNET` cannot match)                                                                                                                                         |

[NATIVE_SCOPE]: raster format codecs
- package: `MaxRev.Gdal.MacosRuntime.Minimal.arm64`
- asset: `runtimes/osx-arm64/native/`
- rail: geometry

| [INDEX] | [DYLIB_GROUP]                                                                              | [RAIL]   | [ROLE]                                                                                                                                    |
| :-----: | :----------------------------------------------------------------------------------------- | :------- | :---------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `libtiff.6.2.0` + `libgeotiff.5.2.4`                                                       | geometry | GeoTIFF/COG/BigTIFF — the primary site-context raster format the `Dataset.ReadRaster<T>` path decodes                                     |
|  [02]   | `libopenjp2.2.5.4` + `libopenjph.0.26.3`                                                   | geometry | JPEG2000 (and HTJ2K) — the codec for compressed imagery and elevation tiles                                                               |
|  [03]   | `libjxl.0.11.2` (+ `libjxl_cms`/`libjxl_threads`) + `libwebp.7.2.0` (+ `libsharpyuv`)      | geometry | JPEG-XL and WebP raster codecs                                                                                                            |
|  [04]   | `libjpeg.62.4.0` + `libpng16.16.55.0` + `libgif`                                           | geometry | the baseline JPEG/PNG/GIF raster codecs                                                                                                   |
|  [05]   | `libnetcdf.22` + `libhdf5.320`/`libhdf5_hl`/`libhdf.10`/`libmfhdf.10` + `libcfitsio.4.6.3` | geometry | scientific multidimensional raster (netCDF/HDF5/HDF4/FITS) — the `MultiDimTranslate`/`BuildVRT` multidim path and DEM/climate-grid ingest |
|  [06]   | `libOpenEXR-3_4`/`libOpenEXRCore`/`libOpenEXRUtil` + `libImath`/`libIex`/`libIlmThread`    | geometry | OpenEXR high-dynamic-range raster                                                                                                         |
|  [07]   | `libpoppler.151` + `liblcms2`                                                              | geometry | PDF raster/vector ingest (the `PDF` driver) and the LittleCMS color-management the codecs share                                           |

[NATIVE_SCOPE]: vector format and data-source drivers
- package: `MaxRev.Gdal.MacosRuntime.Minimal.arm64`
- asset: `runtimes/osx-arm64/native/`
- rail: geometry

| [INDEX] | [DYLIB_GROUP]                                                                                                                   | [RAIL]   | [ROLE]                                                                                                     |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------ | :------- | :--------------------------------------------------------------------------------------------------------- |
|  [01]   | `libsqlite3` + `libfreexl.1`                                                                                                    | geometry | the GeoPackage/SpatiaLite vector store (`libsqlite3`) and the Excel `.xls` ingest (`libfreexl`)            |
|  [02]   | `libpq.5`                                                                                                                       | geometry | PostgreSQL/PostGIS client — the `PostgreSQL`/`PostGISRaster` OGR driver connecting to a live PostGIS store |
|  [03]   | `libmysqlclient.21` + `libodbc.2`/`libodbcinst.2`                                                                               | geometry | the MySQL and ODBC vector data-source drivers                                                              |
|  [04]   | `libkmlbase`/`libkmldom`/`libkmlengine` + `libxerces-c-3.3` + `libexpat.1` + `libxml2.16` + `libtinyxml2.11` + `liburiparser.1` | geometry | the KML/GML/CityGML/XML-family vector parsers (LibKML + Xerces + expat + libxml2)                          |
|  [05]   | `libarrow.2300.1.0` + `libparquet.2300.1.0` + `libthrift.0.22`                                                                  | geometry | Arrow/Parquet — the GeoParquet vector driver and the `OLCFastGetArrowStream` columnar bulk path            |
|  [06]   | `libcurl.4.8.0` + `libssl.3` + `libcrypto.3`                                                                                    | geometry | HTTP/TLS — the `/vsicurl/` virtual-filesystem and the remote/cloud-hosted dataset open                     |

[NATIVE_SCOPE]: compression and low-level
- package: `MaxRev.Gdal.MacosRuntime.Minimal.arm64`
- asset: `runtimes/osx-arm64/native/`
- rail: geometry

| [INDEX] | [DYLIB_GROUP]                                                                                                                                                                                      | [RAIL]   | [ROLE]                                                                                                                              |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------- | :---------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `libz.1.3.1` + `libdeflate.0` + `libzstd.1.5.7` + `liblz4.1.10` + `libbz2` + `liblzma.5` + `libsnappy.1.2.2` + `libblosc.1.21.6` + `libbrotli*` (common/dec/enc) + `libaec`/`libsz` + `libminizip` | geometry | the compression codec set the raster (DEFLATE/ZSTD/LZW TIFF), GeoPackage, Parquet, and `/vsizip/` paths share                       |
|  [02]   | `libfontconfig.1` + `libfreetype.6.20` + `libpcre2-8` + `libhwy.1.3` + `libltdl.7`                                                                                                                 | geometry | text rendering for raster annotation (`gdaldem` color-relief, PDF), regex, the Highway SIMD kernels, and the libtool dynamic loader |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: this package has no managed entrypoints
- package: `MaxRev.Gdal.MacosRuntime.Minimal.arm64`
- rail: geometry

The runtime package is consumed transitively, never called directly. The only "entrypoint"
is restoring the package for the `osx-arm64` RID so its native assets resolve into the
output directory; from there `MaxRev.Gdal.Core.GdalBase.ConfigureAll()` discovers and loads
the dylibs. There is no managed type, method, or property in this package to compose against
— a design page references `MaxRev.Gdal.Core` (the `api-maxrev-gdal` catalog) and this
package is the silent native prerequisite that makes those calls bind.

## [04]-[IMPLEMENTATION_LAW]

[PLATFORM_BOUNDARY]:
- this is a pure native-asset carrier for ONE RID (`osx-arm64`). The managed assembly is empty by design; the value is the 75-dylib closure under `runtimes/osx-arm64/native/`. Cross-platform CI/publish needs the sibling runtime packages for each target RID (`*.WindowsRuntime.*`, `*.LinuxRuntime.*`, `*.MacosRuntime.Minimal.x64`); each pins the same GDAL/PROJ version as the bindings.
- "Minimal" names the driver subset: this is the reduced GDAL build (the common raster/vector/database drivers above), not the full GDAL plugin set (no GRASS/MrSID/ECW/Oracle/proprietary drivers). A format outside this closure faults at open with an unknown-driver error — the admitted scope is the standard open-source format set.

[ASSET_RESOLUTION]:
- under the.NET SDK / `dotnet publish` (`MSBuildRuntimeType == 'Core'`), the `runtimes/<RID>/native/**` assets resolve through the standard NuGet runtime-asset graph: a RID-targeted build/publish copies the matching dylibs into the output's native directory automatically. This is the load-bearing path — no custom MSBuild step is involved.
- the bundled `build/net461/*.targets` is a MSBuild-only fallback: its `<None Include=".../runtimes/osx-arm64/native/**">` copy step is gated on `MSBuildRuntimeType != 'Core'` (full-framework MSBuild), so it is inert under the SDK. A design that assumes the `.targets` does the copy on a modern build is wrong — the runtime asset graph does.
- a publish that is RID-agnostic (no `-r osx-arm64` / no `RuntimeIdentifier`) does NOT stage the native assets, so the host faults at the first `OSGeo.*` call. The Apple-Silicon publish MUST be RID-pinned to `osx-arm64` (or self-contained) for the dylibs to land — this is the deployment precondition the `Rasm.Bim` host carries.

[VERSION_LOCKSTEP]:
- the runtime MUST equal the `MaxRev.Gdal.Core` version: the SWIG `*_wrap` dylibs are generated against a specific `libgdal` ABI, and the managed `*PINVOKE` classes are generated against the same SWIG interface. A version skew between bindings and runtime is an ABI mismatch that faults at load — the central manifest pins both to the identical version for this reason.

[SEAM_ABI_FACTS]:
- `libgeos.3.14.1` in this payload is the native topology engine OGR uses. The closure therefore contains TWO planar-topology engines (this native GEOS and managed `NetTopologySuite`). The seam decision is settled by this fact: NTS is the one canonical planar owner; OGR geometry is bridged to NTS via WKB at the ingest wire, and the native GEOS stays inside the GDAL boundary, never the topology owner for domain geometry.
- `libproj.25.9.7.1` is the reprojection engine OSR uses. The closure contains it AND managed `ProjNET`. The seam decision: `ProjNET` is the default managed CRS owner; OSR/PROJ is the escalation path for datum-grid transforms only the native PROJ corpus covers (and for reprojection already inside a GDAL pipeline). The presence of full PROJ here is what makes that escalation viable.
- the `libarrow`/`libparquet` presence is the ABI basis for the `OLCFastGetArrowStream` columnar bulk path — a GeoParquet source streams Arrow batches through `libarrow` without per-feature marshalling, feeding the `Rasm.Persistence` Arrow rail.

[LOCAL_ADMISSION]:
- this package is admitted as the transitive native prerequisite of `MaxRev.Gdal.Core` on osx-arm64; it is never referenced or called directly by `Rasm.Bim` code.
- the admission obligation is the RID-pinned publish (`osx-arm64`) so the native assets stage, and the version lockstep with the bindings.
- the rejected forms: a RID-agnostic publish (assets do not stage), a version skew with `MaxRev.Gdal.Core` (ABI fault), and treating the bundled `.targets` as the asset-copy mechanism under the SDK (it is MSBuild-only).

[RAIL_LAW]:
- Package: `MaxRev.Gdal.MacosRuntime.Minimal.arm64`
- Owns: the osx-arm64 native GDAL dylib closure backing `MaxRev.Gdal.Core` — `libgdal` + the SWIG `*_wrap` P/Invoke targets + `libgeos`/`libproj` + the raster/vector format and compression dependency tree
- Accept: being the RID-keyed native runtime that makes every `OSGeo.GDAL`/`OGR`/`OSR` call bind on Apple Silicon, and being the ABI source for the GEOS/PROJ seam decisions
- Reject: exposing any managed API (it has none), covering RIDs other than osx-arm64 (sibling packages own those), and the full GDAL plugin set (this is the minimal driver subset)
