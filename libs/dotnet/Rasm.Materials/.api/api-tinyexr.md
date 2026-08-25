# [RASM_MATERIALS_API_TINYEXR]

`TinyEXR.NET` is the pure-managed OpenEXR estate the flat scanline codecs cannot reach: a V3 block-level reader and writer over tiled, mip-levelled, deep, and multi-part files with the full compression roster including DWAA, DWAB, and HTJ2K, beside a V1 whole-image facade for one-call round trips. Its `ImageProcessing` and `Spectral` folds carry transfer, tone-map, colour-matrix, LUT, resize, and wavelength-sampled channel law on the same float planes.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: V3 streaming reader, writer, and their results — `TinyEXR.V3`

| [INDEX] | [SYMBOL]                                              | [TYPE_FAMILY]   | [CAPABILITY]                                    |
| :-----: | :---------------------------------------------------- | :-------------- | :---------------------------------------------- |
|  [01]   | `ExrFile`                                             | static class    | whole-file load and save over the V3 model      |
|  [02]   | `ExrReader`                                           | sealed class    | incremental header, block, tile, and deep reads |
|  [03]   | `ExrWriter`                                           | sealed class    | incremental part, block, tile, and deep writes  |
|  [04]   | `ReaderOptions`                                       | sealed class    | limits and stream ownership                     |
|  [05]   | `WriterOptions`                                       | sealed class    | limits, stream ownership, forced multipart      |
|  [06]   | `ReaderLimits`                                        | sealed class    | header, part, attribute, and block caps         |
|  [07]   | `WriterLimits`                                        | sealed class    | part, dimension, block, and deep-sample caps    |
|  [08]   | `ReaderResult`                                        | readonly struct | `Status`, `Pending`, `Error`, `BytesWritten`    |
|  [09]   | `ReaderResult<T>`                                     | readonly struct | the value-carrying result peer                  |
|  [10]   | `WriterResult`                                        | readonly struct | the write-side result                           |
|  [11]   | `WriterResult<T>`                                     | readonly struct | the value-carrying write result                 |
|  [12]   | `ExrResult`                                           | enum            | the eight-row result vocabulary                 |
|  [13]   | `ReaderState` / `WriterState`                         | enum            | the parse and emit phase                        |
|  [14]   | `BlockInfo`                                           | readonly struct | one block's part, index, and region             |
|  [15]   | `ReaderParseException` and three limit and plan peers | exception       | typed failure classes                           |

[ExrResult]: `Success` `WouldBlock` `InvalidArgument` `InvalidFile` `Unsupported` `OutOfMemory` `IO` `Corrupt`

[PUBLIC_TYPE_SCOPE]: V3 image model — `TinyEXR.V3`

| [INDEX] | [SYMBOL]                                                          | [TYPE_FAMILY]   | [CAPABILITY]                                     |
| :-----: | :---------------------------------------------------------------- | :-------------- | :----------------------------------------------- |
|  [01]   | `Image`                                                           | sealed class    | the part list and its multipart flag             |
|  [02]   | `Part`                                                            | sealed class    | one part's header and level list                 |
|  [03]   | `PartLevel`                                                       | abstract class  | one mip or ripmap level's channel buffers        |
|  [04]   | `FlatLevel` / `DeepLevel`                                         | sealed class    | the flat and deep level realizations             |
|  [05]   | `Header`                                                          | sealed class    | part type, compression, windows, tiles, channels |
|  [06]   | `HeaderAttribute`                                                 | sealed class    | one named, typed attribute payload               |
|  [07]   | `Channel`                                                         | sealed class    | name, pixel type, sampling, perceptual flag      |
|  [08]   | `ChannelBuffer`                                                   | sealed class    | one channel's raw bytes and sample count         |
|  [09]   | `TileDescription`                                                 | sealed class    | tile extent, level mode, rounding mode           |
|  [10]   | `Box2i`                                                           | readonly struct | data and display window                          |
|  [11]   | `Chromaticities`                                                  | readonly struct | RGB primaries and white point                    |
|  [12]   | `InterleavedFloatImage`                                           | sealed class    | `Width`/`Height`/`Channels`/`Data`/`GetSample`   |
|  [13]   | `SpectralImage`                                                   | sealed class    | wavelength-sampled part carrier                  |
|  [14]   | `DeepSampleRange` / `DeepChannelDestination` / `DeepEncodedBlock` | struct, class   | deep sample addressing                           |
|  [15]   | `PartType`                                                        | enum            | `Scanline` `Tiled` `DeepScanline` `DeepTiled`    |
|  [16]   | `PixelType`                                                       | enum            | `UInt` `Half` `Float`                            |
|  [17]   | `Compression`                                                     | enum            | the twelve-row compression roster                |
|  [18]   | `LineOrder`                                                       | enum            | `IncreasingY` `DecreasingY` `RandomY`            |
|  [19]   | `TileLevelMode` / `TileRoundingMode`                              | enum            | one-level, mipmap, ripmap and rounding           |
|  [20]   | `SpectrumType`                                                    | enum            | `None` `Reflective` `Emissive` `Polarised`       |

[Compression]: `None` `RLE` `ZIPS` `ZIP` `PIZ` `PXR24` `B44` `B44A` `DWAA` `DWAB` `HTJ2K256` `HTJ2K32`

[TileLevelMode]: `OneLevel` `MipmapLevels` `RipmapLevels` — [TileRoundingMode]: `RoundDown` `RoundUp`

[PUBLIC_TYPE_SCOPE]: V3 pixel and image folds — `TinyEXR.V3`

| [INDEX] | [SYMBOL]                                                  | [TYPE_FAMILY]      | [CAPABILITY]                                            |
| :-----: | :-------------------------------------------------------- | :----------------- | :------------------------------------------------------ |
|  [01]   | `PixelConversion`                                         | static class       | half, float, uint, byte span conversion                 |
|  [02]   | `ImageProcessing`                                         | static class       | resize, tone map, colour matrix, transfer               |
|  [03]   | `StreamingImageResizer`                                   | sealed class       | row-streaming separable resample                        |
|  [04]   | `PartConversion`                                          | static class       | interleaved-float and luminance-chroma bridging         |
|  [05]   | `Spectral`                                                | static class       | spectral channel naming and part construction           |
|  [06]   | `Lut3D`                                                   | sealed class       | parsed `.cube` LUT and its application                  |
|  [07]   | `ColorMatrix3x3`                                          | readonly struct    | 3x3 colour transform                                    |
|  [08]   | `ToneMapParameters`                                       | readonly struct    | operator parameters                                     |
|  [09]   | `ToneMapOperator`                                         | enum               | `Reinhard` `ReinhardExtended` `Aces` `Hable`            |
|  [10]   | `TransferFunction`                                        | enum               | `Linear` `Srgb` `Gamma22` `Gamma24` `Rec709` `Pq` `Hlg` |
|  [11]   | `ColorSpace`                                              | enum               | `Srgb` `Rec2020` `AcesAp0` `AcesAp1` `Xyz`              |
|  [12]   | `ResizeFilter`                                            | enum               | `Box` `Triangle` `CatmullRom` `Mitchell`                |
|  [13]   | `EdgeMode`                                                | enum               | `Clamp` `Reflect` `Wrap`                                |
|  [14]   | `PixelConversionMode`                                     | enum               | `Raw` `Normalized`                                      |
|  [15]   | `LutInterpolation`                                        | enum               | `Trilinear` `Tetrahedral`                               |
|  [16]   | `SimdRuntime` / `SimdCapabilities` / `SimdConversionPath` | static class, enum | the resolved vector path                                |

[PUBLIC_TYPE_SCOPE]: V3 data sources and sinks — `TinyEXR.V3.IO`

| [INDEX] | [SYMBOL]                                       | [TYPE_FAMILY]   | [CAPABILITY]                         |
| :-----: | :--------------------------------------------- | :-------------- | :----------------------------------- |
|  [01]   | `IExactDataSource` / `IAsyncExactDataSource`   | interface       | the pull contract the reader drives  |
|  [02]   | `ISeekableDataSink` / `IAsyncSeekableDataSink` | interface       | the push contract the writer drives  |
|  [03]   | `MemoryDataSource`                             | sealed class    | in-memory source                     |
|  [04]   | `StreamDataSource` / `StreamDataSink`          | sealed class    | stream-backed source and sink        |
|  [05]   | `SuppliedDataSource`                           | sealed class    | caller-fed segment source            |
|  [06]   | `DataRange` / `DataTransferResult`             | readonly struct | the byte window a `WouldBlock` wants |
|  [07]   | `DataTransferStatus`                           | enum            | the transfer verdict                 |

[PUBLIC_TYPE_SCOPE]: V1 whole-image facade — `TinyEXR`

| [INDEX] | [SYMBOL]                                            | [TYPE_FAMILY] | [CAPABILITY]                            |
| :-----: | :-------------------------------------------------- | :------------ | :-------------------------------------- |
|  [01]   | `Exr`                                               | static class  | the load, save, parse, and layer facade |
|  [02]   | `ExrImage` / `ExrMultipartImage`                    | class         | decoded image and multipart image       |
|  [03]   | `ExrHeader` / `ExrMultipartHeader`                  | class         | parsed header and header set            |
|  [04]   | `ExrChannel` / `ExrImageChannel` / `ExrDeepChannel` | class         | channel description and payload         |
|  [05]   | `ExrImageLevel` / `ExrTile` / `ExrTileDescription`  | class         | level, tile, and tiling description     |
|  [06]   | `ExrDeepImage`                                      | class         | deep image carrier                      |
|  [07]   | `ExrVersion` / `ExrAttribute` / `ExrBox2i`          | class, struct | version flags, attribute, window        |
|  [08]   | `ResultCode`                                        | enum          | the V1 status vocabulary                |
|  [09]   | `CompressionType` and the four V1 header enums      | enum          | the V1 header vocabularies              |
|  [10]   | `SinglePartExrReader` / `ScanlineExrWriter`         | class         | the facade's own reader and writer      |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: whole-file load and save — `TinyEXR.V3.ExrFile`

| [INDEX] | [SURFACE]                                                                                | [SHAPE] | [CAPABILITY]          |
| :-----: | :--------------------------------------------------------------------------------------- | :------ | :-------------------- |
|  [01]   | `ExrFile.IsExr(ReadOnlySpan<byte>) -> bool`                                              | static  | magic-signature probe |
|  [02]   | `ExrFile.LoadFromMemory(ReadOnlyMemory<byte>, ReaderOptions?) -> ReaderResult<Image>`    | static  | decode from memory    |
|  [03]   | `ExrFile.LoadFromStream(Stream, ReaderOptions?) -> ReaderResult<Image>`                  | static  | decode from a stream  |
|  [04]   | `ExrFile.LoadFromFileAsync(string, ReaderOptions?, CancellationToken)`                   | static  | async path decode     |
|  [05]   | `ExrFile.SaveToMemory(Image, Compression, WriterOptions?) -> WriterResult<byte[]>`       | static  | encode to memory      |
|  [06]   | `ExrFile.SaveToStream(Image, Stream, Compression, WriterOptions?) -> WriterResult`       | static  | encode to a stream    |
|  [07]   | `ExrFile.SaveToFileAsync(Image, string, Compression, WriterOptions?, CancellationToken)` | static  | async path encode     |

[ENTRYPOINT_SCOPE]: incremental read — `TinyEXR.V3.ExrReader`

| [INDEX] | [SURFACE]                                                                       | [SHAPE]  | [CAPABILITY]                    |
| :-----: | :------------------------------------------------------------------------------ | :------- | :------------------------------ |
|  [01]   | `ExrReader.OpenMemory(ReadOnlyMemory<byte>, ReaderOptions?) -> ExrReader`       | static   | open over memory                |
|  [02]   | `ExrReader.OpenSource(IExactDataSource, ReaderOptions?) -> ExrReader`           | static   | open over a pull source         |
|  [03]   | `ExrReader.OpenAsyncSource(IAsyncExactDataSource, ReaderOptions?) -> ExrReader` | static   | open over an async pull source  |
|  [04]   | `ExrReader.ParseHeader() -> ReaderResult`                                       | instance | parse every part header         |
|  [05]   | `ExrReader.GetHeader(int) -> Header`                                            | instance | one part's header               |
|  [06]   | `ExrReader.GetNumBlocks(int) -> int` / `GetBlockInfo(int, int) -> BlockInfo`    | instance | block roster and geometry       |
|  [07]   | `ExrReader.DecodeBlock(int, int, Span<byte>) -> ReaderResult`                   | instance | one block into a caller buffer  |
|  [08]   | `ExrReader.ReadPart(int) -> ReaderResult<Part>`                                 | instance | one whole part                  |
|  [09]   | `ExrReader.ReadScanlines(int, int, int) -> ReaderResult<Part>`                  | instance | a scanline window               |
|  [10]   | `ExrReader.ReadTile(int, int, int, int, int) -> ReaderResult<Part>`             | instance | one tile at one level           |
|  [11]   | `ExrReader.DecodeDeepCounts(int, int, Span<int>) -> ReaderResult`               | instance | per-pixel deep sample counts    |
|  [12]   | `ExrReader.DecodeDeepSamples(int, int, ReadOnlySpan<int>, …) -> ReaderResult`   | instance | deep samples                    |
|  [13]   | `ExrReader.NumParts` / `State` / `Pending`                                      | property | part count, phase, wanted range |

- `new Header(…)` continues `LineOrder, Box2i? displayWindow, float pixelAspectRatio, Vector2? screenWindowCenter, float screenWindowWidth, TileDescription?, string? name, Chromaticities?, IEnumerable<HeaderAttribute>?`, every tail argument defaulted.
- Every read member ships an `…Async` peer taking `Memory<T>` in place of `Span<T>` and a `CancellationToken`.
- `ExrReader` and `ExrWriter` implement both `IDisposable` and `IAsyncDisposable`; the async source and sink forms require the async disposal.

[ENTRYPOINT_SCOPE]: incremental write — `TinyEXR.V3.ExrWriter`

| [INDEX] | [SURFACE]                                                                                    | [SHAPE]  | [CAPABILITY]                 |
| :-----: | :------------------------------------------------------------------------------------------- | :------- | :--------------------------- |
|  [01]   | `ExrWriter.OpenSink(ISeekableDataSink, WriterOptions?) -> ExrWriter`                         | static   | open over a seekable sink    |
|  [02]   | `ExrWriter.OpenAsyncSink(IAsyncSeekableDataSink, WriterOptions?) -> ExrWriter`               | static   | open over an async sink      |
|  [03]   | `ExrWriter.AddPart(Header) -> int`                                                           | instance | declare one part             |
|  [04]   | `ExrWriter.Begin() -> WriterResult`                                                          | instance | emit headers, open the body  |
|  [05]   | `ExrWriter.WriteScanlineBlock(int, int, IReadOnlyList<ChannelBuffer>) -> WriterResult`       | instance | one scanline block           |
|  [06]   | `ExrWriter.WriteTile(int, int, int, int, int, IReadOnlyList<ChannelBuffer>) -> WriterResult` | instance | one tile at one level        |
|  [07]   | `ExrWriter.WriteDeepScanlineBlock(int, int, ReadOnlySpan<int>, …) -> WriterResult`           | instance | one deep block               |
|  [08]   | `ExrWriter.WriteDeepTile(int ×5, ReadOnlySpan<int>, …) -> WriterResult`                      | instance | one deep tile                |
|  [09]   | `ExrWriter.End() -> WriterResult`                                                            | instance | patch offsets, seal the file |
|  [10]   | `ExrWriter.GetNumBlocks(int) -> int` / `GetBlockInfo(int, int) -> BlockInfo`                 | instance | planned block geometry       |

[ENTRYPOINT_SCOPE]: image model construction and navigation

| [INDEX] | [SURFACE]                                                                                       | [SHAPE]  | [CAPABILITY]              |
| :-----: | :---------------------------------------------------------------------------------------------- | :------- | :------------------------ |
|  [01]   | `new Header(PartType, Box2i, IEnumerable<Channel>, Compression, …)`                             | ctor     | declare a part            |
|  [02]   | `new Channel(string, PixelType, int, int, bool)`                                                | ctor     | declare one channel       |
|  [03]   | `new ChannelBuffer(string, PixelType, ReadOnlySpan<byte>)`                                      | ctor     | bind one channel's bytes  |
|  [04]   | `new TileDescription(uint, uint, TileLevelMode, TileRoundingMode)`                              | ctor     | declare tiling and levels |
|  [05]   | `new Box2i(int, int, int, int)`                                                                 | ctor     | declare a window          |
|  [06]   | `new Image(IEnumerable<Part>)` / `new Part(Header, IEnumerable<PartLevel>, bool)`               | ctor     | assemble the model        |
|  [07]   | `new FlatLevel(int, int, Box2i, IEnumerable<ChannelBuffer>)`                                    | ctor     | one flat level            |
|  [08]   | `new DeepLevel(int, int, Box2i, ReadOnlySpan<int>, IEnumerable<ChannelBuffer>)`                 | ctor     | one deep level and counts |
|  [09]   | `Image.Parts -> IReadOnlyList<Part>` / `Image.IsMultipart` / `Image.GetPart(string)`            | instance | part roster and lookup    |
|  [10]   | `Part.Header -> Header` / `Part.Levels -> IReadOnlyList<PartLevel>` / `Part.GetLevel(int, int)` | instance | header, levels, lookup    |
|  [11]   | `PartLevel.GetChannel(string) -> ChannelBuffer` / `PartLevel.Channels` / `Region`               | instance | channel access, extent    |
|  [12]   | `DeepLevel.SampleCounts -> ReadOnlySpan<int>` / `DeepLevel.TotalSamples`                        | property | per-texel deep counts     |
|  [13]   | `Header.IsTiled` / `IsDeep` / `Channels` / `Attributes` / `DataWindow` / `Chromaticities`       | property | declared part facts       |
|  [14]   | `ChannelBuffer.Data` / `ByteLength` / `SampleCount` / `Name` / `PixelType`                      | property | the raw channel window    |
|  [15]   | `new Chromaticities(float, float, float, float, float, float, float, float)`                    | ctor     | eight positional f32 xy   |
|  [16]   | `Chromaticities.RedX` / `RedY` / `GreenX` / `GreenY` / `BlueX` / `BlueY` / `WhiteX` / `WhiteY`  | property | declared primaries, f32   |

[ENTRYPOINT_SCOPE]: pixel conversion, resample, tone map, transfer, and LUT

| [INDEX] | [SURFACE]                                                                                 | [SHAPE]  | [CAPABILITY]          |
| :-----: | :---------------------------------------------------------------------------------------- | :------- | :-------------------- |
|  [01]   | `PixelConversion.HalfToFloat(ReadOnlySpan<ushort>, Span<float>)`                          | static   | half to float         |
|  [02]   | `PixelConversion.FloatToHalf(ReadOnlySpan<float>, Span<ushort>)`                          | static   | float to half         |
|  [03]   | `PixelConversion.ByteToFloat(ReadOnlySpan<byte>, Span<float>, PixelConversionMode)`       | static   | byte to float         |
|  [04]   | `PixelConversion.FloatToUInt(ReadOnlySpan<float>, Span<uint>, PixelConversionMode)`       | static   | float to uint         |
|  [05]   | `ImageProcessing.Resize(ReadOnlySpan<float>, int, int, Span<float>, int, int, int, …)`    | static   | separable resample    |
|  [06]   | `ImageProcessing.ToneMap(ReadOnlySpan<float>, Span<float>, int, ToneMapOperator, …)`      | static   | operator tone map     |
|  [07]   | `ImageProcessing.ApplyColorMatrix(ReadOnlySpan<float>, Span<float>, int, ColorMatrix3x3)` | static   | primaries transform   |
|  [08]   | `ImageProcessing.GetColorMatrix(ColorSpace, ColorSpace) -> ColorMatrix3x3`                | static   | primaries reconcile   |
|  [09]   | `ImageProcessing.GetLuminanceWeights(Chromaticities?) -> Vector3`                         | static   | luminance derivation  |
|  [10]   | `ImageProcessing.EncodeTransfer(ReadOnlySpan<float>, Span<float>, TransferFunction)`      | static   | linear to encoded     |
|  [11]   | `ImageProcessing.DecodeTransfer(ReadOnlySpan<float>, Span<float>, TransferFunction)`      | static   | encoded to linear     |
|  [12]   | `StreamingImageResizer.PushRow(int, ReadOnlySpan<float>)` / `PullRow(…) -> ExrResult`     | instance | streamed resample     |
|  [13]   | `Lut3D.TryParseCube(string?, out Lut3D?) -> ExrResult`                                    | static   | parse a `.cube` LUT   |
|  [14]   | `Lut3D.Apply(ReadOnlySpan<float>, Span<float>, int, LutInterpolation)`                    | instance | apply the LUT         |
|  [15]   | `Lut3D.Data -> ReadOnlySpan<float>` / `Lut3D.Size` / `DomainMinimum` / `DomainMaximum`    | property | the parsed lattice    |
|  [16]   | `PartConversion.ToInterleavedFloat(Part) -> InterleavedFloatImage`                        | static   | planar to interleaved |
|  [17]   | `PartConversion.FromInterleavedFloat(ReadOnlySpan<float>, int, int, int, …) -> Part`      | static   | interleaved to a part |
|  [18]   | `PartConversion.LuminanceChromaToRgbaFloat(Part) -> InterleavedFloatImage`                | static   | chroma expansion      |
|  [19]   | `PartConversion.IsLuminanceChroma(Part) -> bool`                                          | static   | chroma-basis probe    |

[ENTRYPOINT_SCOPE]: spectral channels — `TinyEXR.V3.Spectral`

| [INDEX] | [SURFACE]                                                                            | [SHAPE] | [CAPABILITY]                   |
| :-----: | :----------------------------------------------------------------------------------- | :------ | :----------------------------- |
|  [01]   | `Spectral.IsSpectral(Header) -> bool` / `GetSpectrumType(Header) -> SpectrumType`    | static  | classify a part                |
|  [02]   | `Spectral.GetWavelengths(Header) -> float[]` / `GetUnits(Header) -> string?`         | static  | read the sampling grid         |
|  [03]   | `Spectral.GetPolarisationHandedness(Header) -> string?`                              | static  | read the declared handedness   |
|  [04]   | `Spectral.GetChannelName(SpectrumType, float nm, int stokesComponent = 0) -> string` | static  | mint a wavelength channel name |
|  [05]   | `Spectral.TryParseChannelWavelength(string?, out float) -> bool`                     | static  | read a wavelength off a name   |
|  [06]   | `Spectral.TryGetStokesComponent(string?, out int) -> bool`                           | static  | read the Stokes index off it   |
|  [07]   | `Spectral.IsSpectralChannel(string?) -> bool`                                        | static  | classify one channel name      |
|  [08]   | `Spectral.CreateReflectivePart(int, int, ReadOnlySpan<float> ×2, …) -> Part`         | static  | build a reflectance part       |
|  [09]   | `Spectral.CreateEmissivePart(int, int, ReadOnlySpan<float> ×2, …) -> Part`           | static  | build an emission part         |
|  [10]   | `Spectral.WithSpectralAttributes(Header, SpectrumType, units, handedness) -> Header` | static  | stamp the attributes           |
|  [11]   | `Spectral.LayoutVersion` / `Spectral.MaximumWavelengthCount`                         | const   | layout tag, grid bound `4096`  |

- `GetChannelName`'s second parameter is `float wavelengthNanometers` and its third `int stokesComponent = 0`, clamped to `[0, 3]`; `WithSpectralAttributes` defaults its tail `string? units = null, string polarisationHandedness = "left"`, so a stamp that never names the handedness declares LEFT rather than declaring nothing — an unread default the polarised arm inherits as fact.
- Both part builders end `(…, ReadOnlySpan<float> wavelengths, ReadOnlySpan<float> samples, string? units = null, Compression compression = Compression.ZIP)`, so a spectral part defaults to the lossless row without a caller naming it.

[ENTRYPOINT_SCOPE]: V1 whole-image facade — `TinyEXR.Exr`

| [INDEX] | [SURFACE]                                                                                | [SHAPE] | [CAPABILITY]                     |
| :-----: | :--------------------------------------------------------------------------------------- | :------ | :------------------------------- |
|  [01]   | `Exr.IsEXRFromStream(Stream) -> bool`                                                    | static  | signature probe                  |
|  [02]   | `Exr.LoadEXRFromStream(Stream, out float[], out int, out int) -> ResultCode`             | static  | RGBA float32 whole-image load    |
|  [03]   | `Exr.LoadEXRWithLayerFromStream(Stream, string?, out float[], out int, out int)`         | static  | one named layer                  |
|  [04]   | `Exr.EXRLayersFromStream(Stream, out string[]) -> ResultCode`                            | static  | enumerate layer names            |
|  [05]   | `Exr.SaveEXRToMemory(ReadOnlySpan<float>, int, int, int, bool, out byte[])`              | static  | RGBA float or half save          |
|  [06]   | `Exr.ParseEXRHeaderFromStream(Stream, out ExrVersion, out ExrHeader)`                    | static  | header without pixels            |
|  [07]   | `Exr.ParseEXRMultipartHeaderFromStream(Stream, out ExrVersion, out ExrMultipartHeader)`  | static  | every part's header              |
|  [08]   | `Exr.LoadEXRImageFromStream(Stream, ExrHeader, out ExrImage) -> ResultCode`              | static  | full image under a parsed header |
|  [09]   | `Exr.LoadEXRMultipartImageFromStream(Stream, ExrMultipartHeader, out ExrMultipartImage)` | static  | every part                       |
|  [10]   | `Exr.SaveEXRImageToMemory(ExrImage, ExrHeader, out byte[]) -> ResultCode`                | static  | encode a parsed model            |
|  [11]   | `Exr.SaveEXRMultipartImageToMemory(ExrMultipartImage, ExrMultipartHeader, out byte[])`   | static  | encode every part                |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Two API planes, one file format. Its V1 facade (`Exr`, `ExrImage`, `ExrHeader`) round-trips a whole image through `out` parameters and a `ResultCode`; the V3 plane (`ExrReader`, `ExrWriter`, `ExrFile`, `Image`/`Part`/`Header`/`ChannelBuffer`) is the block-level model. Composing surfaces pick ONE and never mix their vocabularies — `TinyEXR.ExrPixelType` and `TinyEXR.V3.PixelType` are distinct types with the same rows.
- V3 models a part planar and named: a `Part` holds `PartLevel`s, each holding `ChannelBuffer`s keyed by channel NAME. There is no fixed RGBA slot — an arbitrary AOV set is the natural shape, and `PartConversion.ToInterleavedFloat` is what flattens one part into an interleaved plane.
- `new ChannelBuffer(string, PixelType, ReadOnlySpan<byte>)` COPIES: it delegates to a private array constructor through `data.ToArray()`, so a rented scratch re-filled per channel is safe and a per-channel array allocated beside it buys a lifetime the buffer already owns. The constructor validates the span's length against whole native samples, and `Data`/`ByteLength`/`SampleCount` read that owned array.
- A block write validates each `ChannelBuffer`'s `ByteLength` against the BLOCK's own region — the region's sample count times the pixel-type width — and refuses any other length by name, so a border tile padded out to the full tile description is a hard refusal rather than a zero-filled remainder the file absorbs.
- Every V3 call returns a `ReaderResult`/`WriterResult` value carrying `Status`, `Pending`, `Error`, and `BytesWritten` rather than throwing on a data fault; `IsSuccess` reads `Status == ExrResult.Success` and the typed exceptions fire only for limit breaches and plan violations.
- `ExrResult.WouldBlock` is the pull protocol: the reader returns it with `Pending` naming the byte `DataRange` it needs, so a `SuppliedDataSource` feeds exactly that window and the call repeats. Callers treating `WouldBlock` as failure stall a perfectly healthy incremental read.
- Writing is a three-phase plan: `AddPart` per part, then `Begin`, then the block, tile, or deep writes in any order, then `End` — which seeks back and patches the offset tables, so the sink must genuinely seek and the writer must reach `End` or the file is headerless.
- Tiling and levels ride `Header.Tiles`: `TileLevelMode` selects one-level, mipmap, or ripmap, and `ExrReader.ReadTile(part, tileX, tileY, levelX, levelY)` addresses a tile within a level. This is the only mip-in-one-file EXR path in the estate.
- Deep parts on the incremental reader are two reads: `DecodeDeepCounts` fills the per-pixel sample counts, then `DecodeDeepSamples` fills the destinations those counts size — reversing the order sizes nothing. The whole-file model materializes counts and samples together, so `DeepLevel.SampleCounts` off a loaded `Image` is the same counts-first law already discharged.
- Compression is per part on `Header.Compression`, spanning the whole roster through DWAA, DWAB, and the HTJ2K rows; `PIZ` and `B44` are lossless-for-half and lossy-for-half respectively, and `PXR24` truncates float to 24 bits — a solver-grade plane takes `ZIP` or `ZIPS`.
- `HTJ2K256` and `HTJ2K32` do NOT round-trip a float plane and their failure is not the graded truncation the other lossy rows carry. MEASURED against the EXR core both branches write, across the extent range a mip ladder spans: a `16×16` level and a `2×512` sheet decode ALL-NaN, extents at or below eight decode inexact, and only a mid band is byte-exact, while `ZIP` is exact at every one. Any pyramid folding to `1×1` crosses the broken range on EVERY set, so the two rows are unusable for a plane rather than merely lossy, and the compression roster's completeness is a package fact this estate does not spend.
- Processing folds operate on interleaved float spans, never on the container: `Resize`, `ToneMap`, `ApplyColorMatrix`, `EncodeTransfer`/`DecodeTransfer`, and `Lut3D.Apply` all take a `ReadOnlySpan<float>` and a channel count, so they compose over any float plane the estate holds.
- Fold arities, each with defaulted tails: `Resize(source, sourceWidth, sourceHeight, destination, destinationWidth, destinationHeight, channels, ResizeFilter = Mitchell, EdgeMode = Clamp, alphaChannel = -1, …)` — the extent groups bracket the two spans and the channel count follows BOTH, so a source-extent-then-channel-count spelling silently transposes the destination; `ToneMap(source, destination, channels, ToneMapOperator, ToneMapParameters? = null)`; `EncodeTransfer`/`DecodeTransfer(source, destination, TransferFunction)`; `Lut3D.Apply(source, destination, channels, LutInterpolation = Trilinear)`; `PartConversion.FromInterleavedFloat(source, width, height, channels, PixelType = Half, Compression = ZIP)` — leaving `destinationType` defaulted narrows a solver-grade plane to half at the container edge.
- The `ImageProcessing` and `Lut3D` folds admit a destination aliasing their source at the SAME start and refuse a partial overlap, so one scratch span threads a whole row rail; the `PixelConversion` family refuses ANY overlap outright. Every refusal is an `ArgumentException`, never a silent miscompute.
- `ToInterleavedFloat` and `LuminanceChromaToRgbaFloat` are ONE DISCRIMINATION over `PartConversion.IsLuminanceChroma(part)`, never a caller choice: a `Y`/`RY`/`BY` part flattened through the plain bridge hands three lanes of a chroma basis to a consumer expecting RGB, and nothing downstream can tell those lanes from colour. Probing reads the part's channel names, so the file decides the arm and the reader never declares it.
- `ApplyColorMatrix` takes a matrix it does not produce: `ImageProcessing.GetColorMatrix(ColorSpace from, ColorSpace to)` mints the primaries transform between two rostered spaces and `ImageProcessing.GetLuminanceWeights(Chromaticities?)` derives the luma triple from a file's own declared primaries (or the default set when the header carries none). Hand-writing a 3×3 beside either mints a second colour authority.
- `SpectrumType.Polarised` names a STOKES-COMPONENT FAMILY, never one more wavelength axis: a channel name carries BOTH a wavelength and a Stokes index — `S<0..3>.<nm>nm` on the emissive and polarised arms, `T.<nm>nm` on the reflective one — so `GetSpectrumType` reports `Polarised` exactly when some channel's Stokes index is nonzero, and a reader parsing only `TryParseChannelWavelength` collapses four polarisation components onto one channel.
- `TryGetStokesComponent` owns the second half of that parse, `GetChannelName`'s third argument its mint side, `GetPolarisationHandedness` the header's declared convention, and `MaximumWavelengthCount` (`4096`) the grid bound a caller sizes against.
- Result values carry `IsSuccess` beside `Status`, and `ReaderResult<T>`/`WriterResult<T>` expose `Value` as `T?`, so a success arm pattern-matches `{ IsSuccess: true, Value: { } payload }` rather than dereferencing after a status compare.

[STACKING]:
- `SixLabors.ImageSharp`(`.api/api-imagesharp.md`): the container split — that surface carries no EXR codec at the held major, so this one is the SOLE EXR owner across every shape, flat scanline included, and the peer owns PNG, TIFF, WebP, QOI, and JPEG alone; a plane crosses as raw bytes between an `Image<RgbaVector>` and a `ChannelBuffer`, both reading one pooled arena, and `ExrFile.SaveToStream(image, stream, Compression.ZIP)` writes the flat per-channel file every cross-branch parity fixture reads.
- `TextureCompressor`(`.api/api-texturecompressor.md`): `PartConversion.ToInterleavedFloat(part)` flattens a multi-channel part into the float samples an `ArrayBitmap<Rgba32Float>` carries, so a tiled or mip-levelled EXR level block-encodes to BC6H through `ITextureCoder.Encode<TPixel>` with no intermediate container; `ImageProcessing.Resize` and `BitmapMipChain.Downsample` are the two mip folds, and `CatmullRom`/`Mitchell` here exceed the peer's `Box`/`Triangle`.
- `CommunityToolkit.HighPerformance`(`libs/dotnet/.api/api-highperformance.md`): `MemoryOwner<byte>.Allocate(byteLength).Span` is the `Span<byte>` `ExrReader.DecodeBlock(part, block, destination)` fills and the window `new ChannelBuffer(name, pixelType, data)` binds on the write side, while `Span2D<T>.GetRowSpan` addresses the block's rows — one pooled rental serves the block staging on both directions.
- `MathNet.Numerics`(`libs/dotnet/.api/api-mathnet-numerics.md`): the row-column fold over `Fourier.Forward(Complex[], FourierOptions)` — the managed-total 2D form, because the `Forward2D` multidim rows throw `NotSupportedException` on the managed provider — consumes the interleaved float plane `PartConversion.ToInterleavedFloat` produces AFTER a float->`Complex[]` pack the caller stages — the transform mutates in place, so the pack is one staging pass, never a direct hand-off, so frequency-domain height integration and periodicity scoring read an EXR level directly.
- `System.IO.Hashing`(`libs/dotnet/.api/api-hashing.md`): `WriterResult.BytesWritten` bounds the encoded window `XxHash128.Append` folds, so the plane's content key derives from the exact bytes the sink received.
- `Wacton.Unicolour`(`libs/dotnet/.api/api-unicolour.md`): `Header.Chromaticities` carries the file's declared primaries and white point, which reconcile against the appearance working space on the `Unicolour` owner; `ImageProcessing.GetColorMatrix(ColorSpace.Srgb, ColorSpace.AcesAp1)` into `ApplyColorMatrix` is the transport-side spelling of that same reconciliation — a MEMBER, never a transcribed matrix — and `GetLuminanceWeights(header.Chromaticities)` is the file-declared luma triple a transport-side reduction reads instead of a constant. Neither is a second colour authority: a perceptual or spectral decision stays on the `Unicolour` owner.
- within-lib: the deep and tiled EXR arm binds `ExrWriter.OpenSink` over the plane sink, declares one `Part` per channel set with `Compression.ZIP`, writes level blocks through `WriteTile`, and always reaches `End`; the environment fold takes `Spectral.CreateReflectivePart`/`CreateEmissivePart` where a wavelength-sampled measurement crosses instead of an RGB triple.

[LOCAL_ADMISSION]:
- V3 is the composed plane. V1 admits only where a whole-image RGBA float round trip in one call is genuinely the whole need; its `out`-parameter and `ResultCode` shape never crosses a folder boundary.
- Per-channel FILES are the canonical cross-branch EXR form; multipart, named-AOV, tiled, and mip-levelled files are branch-local optimization, so no parity fixture depends on a leg this package alone can write.
- `Compression.PXR24`, `B44`, `B44A`, `DWAA`, and `DWAB` are lossy for float data and are refused on a solver-grade or content-keyed plane; `HTJ2K256` and `HTJ2K32` are refused on EVERY float plane, keyed or not, because their small-extent decode is NaN rather than approximate; `Zip`/`Zips` is the durable default and `None` the debug form.
- `ExrResult.WouldBlock` is a protocol state, never a fault row: it lowers to a resumed read, and only the remaining `ExrResult` rows and the typed limit exceptions reach the folder's fault rail.
- `ReaderLimits` and `WriterLimits` are set explicitly on any path admitting an outside file, because the defaults bound a trusted producer rather than untrusted input.
