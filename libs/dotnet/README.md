# [DOTNET_LIBS]

The .NET library area holds the interop layer. Each `Rasm.Interop.*` facade runs the process-global setup its external library requires before first use, and `Rasm.Interop` aggregates every facade into one initialization call.

## [01]-[DEPENDENCIES]

- `Emgu.CV` — OpenCV wrapper for image processing, calibration, features, dnn, codecs, and video IO
- `EPPlus` — Excel workbook engine with formulas, charts, and pivot tables
- `MaxRev.Gdal.Core` — GDAL bindings for raster and vector geospatial formats
- `MinVer` — assembly and package version from the nearest `v` tag
- `PDFsharp` — PDF document creation and drawing API
- `PureHDF` — HDF5 file reading and writing in pure managed code
- `PureHDF.Filters.Blosc2` — Blosc2 filter support for HDF5 dataset compression
- `PureHDF.Filters.BZip2.SharpZipLib` — BZip2 filter support for HDF5 dataset compression
- `PureHDF.Filters.Lzf` — LZF filter support for HDF5 dataset compression
- `Rasm.Native.Blosc2` — c-blosc2 shared library and codec dependency closure staged per runtime identifier
