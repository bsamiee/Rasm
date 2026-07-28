# [RASM_MATERIALS_API_IMAGESHARP]

`SixLabors.ImageSharp` is the fully managed raster container and pixel-plane owner behind the texture asset estate: one `Image<TPixel>` generic over a closed `IPixel<TPixel>` format family, decoded and encoded through per-format `ImageDecoder`/`ImageEncoder` rows carrying 16-bit PNG, 16-bit and float TIFF, WebP, QOI, and JPEG. `Image.WrapMemory` adopts a caller-owned pooled plane with no copy, so the codec never mints a second arena beside the one the texture plane already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `SixLabors.ImageSharp`
- package: `SixLabors.ImageSharp` (Apache-2.0)
- assembly: `SixLabors.ImageSharp`
- namespace: `SixLabors.ImageSharp`, `.PixelFormats`, `.Formats.*`, `.Metadata`, `.Metadata.Profiles.*`, `.ColorSpaces`, `.ColorSpaces.Conversion`, `.Advanced`, `.Memory`
- asset: multi-target; the `net10.0` consumer binds `lib/net6.0/SixLabors.ImageSharp.dll` and takes no package dependency
- rail: raster container

Six Labors' Split License grants Apache-2.0 unconditionally to an open-source consumer. Later majors inject an MSBuild license-validation task that fails `CoreCompile` without a vendor-signed key, file, or `sixlabors.lic`, carrying no open-source opt-out property; admission holds at the last major granting Apache-2.0 without that gate, and the manifest pins where.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: image owners and configuration — `SixLabors.ImageSharp`

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]  | [CAPABILITY]                                |
| :-----: | :---------------------------- | :------------- | :------------------------------------------ |
|  [01]   | `Image`                       | abstract class | pixel-type-erased image and the load facade |
|  [02]   | `Image<TPixel>`               | sealed class   | typed pixel plane and frame collection      |
|  [03]   | `ImageFrame<TPixel>`          | sealed class   | one frame of a multi-frame image            |
|  [04]   | `ImageFrameCollection`        | class          | frame roster and mutation                   |
|  [05]   | `Configuration`               | sealed class   | allocator, parallelism, format registry     |
|  [06]   | `ImageInfo`                   | class          | dimensions and metadata without decode      |
|  [07]   | `Color`                       | struct         | pixel-agnostic color value                  |
|  [08]   | `Rectangle` / `Size`          | struct         | plane extent and window                     |
|  [09]   | `ImageFormatException`        | exception      | malformed payload                           |
|  [10]   | `UnknownImageFormatException` | exception      | no detector claimed the magic bytes         |
|  [11]   | `MemoryAllocator`             | abstract class | pooled buffer source behind every plane     |
|  [12]   | `PixelAccessorAction<TPixel>` | delegate       | the row-span access body                    |

[PUBLIC_TYPE_SCOPE]: pixel formats spanning the texture depth ladder — `SixLabors.ImageSharp.PixelFormats`

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY] | [CAPABILITY]                                  |
| :-----: | :------------ | :------------ | :-------------------------------------------- |
|  [01]   | `IPixel<T>`   | interface     | the closed self-constrained pixel contract    |
|  [02]   | `L8` / `L16`  | struct        | single-channel 8-bit and 16-bit scalar plane  |
|  [03]   | `La32`        | struct        | 16-bit luminance plus alpha                   |
|  [04]   | `Rgb24`       | struct        | 8-bit three-component color                   |
|  [05]   | `Rgba32`      | struct        | 8-bit color with alpha                        |
|  [06]   | `Bgra32`      | struct        | 8-bit host-order color with alpha             |
|  [07]   | `Rgb48`       | struct        | 16-bit three-component color                  |
|  [08]   | `Rgba64`      | struct        | 16-bit color with alpha                       |
|  [09]   | `HalfVector4` | struct        | half-float four-component HDR texel           |
|  [10]   | `RgbaVector`  | struct        | float32 `R`/`G`/`B`/`A` fields, the HDR texel |
|  [11]   | `Rgba1010102` | struct        | packed 10-bit-per-channel wide-gamut texel    |

[PUBLIC_TYPE_SCOPE]: format rows — `SixLabors.ImageSharp.Formats.*`

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [CAPABILITY]                                   |
| :-----: | :---------------------------- | :------------ | :--------------------------------------------- |
|  [01]   | `PngDecoder` / `PngEncoder`   | codec pair    | PNG through 16-bit gray, color, and alpha      |
|  [02]   | `TiffDecoder` / `TiffEncoder` | codec pair    | TIFF through 16-bit integer sample formats     |
|  [03]   | `WebpDecoder` / `WebpEncoder` | codec pair    | WebP lossy, lossless, near-lossless, animated  |
|  [04]   | `QoiDecoder` / `QoiEncoder`   | codec pair    | Quite OK Image fast lossless 8-bit             |
|  [05]   | `JpegDecoder` / `JpegEncoder` | codec pair    | baseline and progressive JPEG                  |
|  [06]   | `BmpDecoder` / `BmpEncoder`   | codec pair    | BMP                                            |
|  [07]   | `GifDecoder` / `GifEncoder`   | codec pair    | animated GIF                                   |
|  [08]   | `TgaDecoder` / `TgaEncoder`   | codec pair    | Targa                                          |
|  [09]   | `PbmDecoder` / `PbmEncoder`   | codec pair    | Netpbm plain and binary                        |
|  [10]   | `DecoderOptions`              | class         | target size, frame limit, skip metadata        |
|  [11]   | `ISpecializedDecoderOptions`  | interface     | the per-format decoder-options contract        |
|  [12]   | `IImageFormat`                | interface     | one registered format identity                 |
|  [13]   | `ImageFormatManager`          | class         | detector and codec registry on `Configuration` |

[PUBLIC_TYPE_SCOPE]: metadata and color science — `Metadata` / `ColorSpaces`

| [INDEX] | [SYMBOL]                                         | [TYPE_FAMILY] | [CAPABILITY]                                  |
| :-----: | :----------------------------------------------- | :------------ | :-------------------------------------------- |
|  [01]   | `ImageMetadata`                                  | class         | ICC, EXIF, XMP, resolution, per-format blocks |
|  [02]   | `IccProfile`                                     | class         | parsed ICC profile carried on the image       |
|  [03]   | `ExifProfile` / `XmpProfile`                     | class         | EXIF tag set and XMP packet                   |
|  [04]   | `ColorSpaceConverter`                            | class         | space-to-space and chromatic-adaptation entry |
|  [05]   | `ColorSpaceConverterOptions`                     | class         | white points, working spaces, adaptation      |
|  [06]   | `RgbWorkingSpaces`                               | static class  | sRGB, Adobe, Rec709, wide-gamut RGB spaces    |
|  [07]   | `Illuminants`                                    | static class  | CIE illuminant white points                   |
|  [08]   | `VonKriesChromaticAdaptation`                    | class         | the adaptation transform                      |
|  [09]   | `CieXyz` / `CieLab` / `CieLch`                   | struct        | CIE carriers for conversion                   |
|  [10]   | `LinearRgb` / `Rgb`                              | struct        | linear and encoded RGB carriers               |
|  [11]   | `CieXyChromaticityCoordinates` / `RgbPrimaries…` | struct        | primaries and white-point coordinates         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: detect, identify, and decode — `Image`

| [INDEX] | [SURFACE]                                                                  | [SHAPE] | [CAPABILITY]                       |
| :-----: | :------------------------------------------------------------------------- | :------ | :--------------------------------- |
|  [01]   | `Image.DetectFormat(ReadOnlySpan<byte>) -> IImageFormat`                   | static  | magic-byte format sniff            |
|  [02]   | `Image.Identify(ReadOnlySpan<byte>) -> ImageInfo`                          | static  | dimensions and metadata, no pixels |
|  [03]   | `Image.Load(ReadOnlySpan<byte>) -> Image`                                  | static  | decode at the file's own depth     |
|  [04]   | `Image.Load<TPixel>(ReadOnlySpan<byte>) -> Image<TPixel>`                  | static  | decode into a demanded pixel type  |
|  [05]   | `Image.Load<TPixel>(DecoderOptions, Stream) -> Image<TPixel>`              | static  | stream decode under options        |
|  [06]   | `Image.LoadAsync<TPixel>(string, CancellationToken)`                       | static  | async path decode                  |
|  [07]   | `Image.LoadPixelData<TPixel>(ReadOnlySpan<TPixel>, int, int)`              | static  | copy raw texels into a new image   |
|  [08]   | `Image.LoadPixelData<TPixel>(Configuration, ReadOnlySpan<byte>, int, int)` | static  | copy raw bytes                     |

- `Image.Load` without a pixel argument resolves the pixel type from the file: a 16-bit PNG lands `Rgba64`, an 8-bit one `Rgba32`, so a caller demanding a fixed arena passes `Load<TPixel>` and lets the decoder convert once.

[ENTRYPOINT_SCOPE]: zero-copy binding and pixel access — `Image` / `Image<TPixel>`

| [INDEX] | [SURFACE]                                                                                | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :--------------------------------------------------------------------------------------- | :------- | :--------------------------------- |
|  [01]   | `Image.WrapMemory<TPixel>(Configuration, IMemoryOwner<TPixel>, int, int, ImageMetadata)` | static   | adopt a pooled rental, no copy     |
|  [02]   | `Image.WrapMemory<TPixel>(Configuration, IMemoryOwner<TPixel>, int, int)`                | static   | adopt without metadata             |
|  [03]   | `Image.WrapMemory<TPixel>(Configuration, Memory<TPixel>, int, int, ImageMetadata)`       | static   | borrow a caller plane, no copy     |
|  [04]   | `Image<TPixel>.ProcessPixelRows(PixelAccessorAction<TPixel>)`                            | instance | row-span access under the accessor |
|  [05]   | `Image<TPixel>.ProcessPixelRows<TPixel2>(Image<TPixel2>, PixelAccessorAction<…>)`        | instance | two-image row-paired access        |
|  [06]   | `Image<TPixel>.CopyPixelDataTo(Span<TPixel>)`                                            | instance | drain texels into a caller span    |
|  [07]   | `Image<TPixel>.CopyPixelDataTo(Span<byte>)`                                              | instance | drain raw bytes                    |
|  [08]   | `Image<TPixel>.DangerousTryGetSinglePixelMemory(out Memory<TPixel>) -> bool`             | instance | probe for one contiguous buffer    |
|  [09]   | `Image<TPixel>.this[int, int]`                                                           | indexer  | single-texel read and write        |
|  [10]   | `Image.CloneAs<TPixel2>() -> Image<TPixel2>`                                             | instance | depth and channel conversion       |

- `DangerousTryGetSinglePixelMemory`: returns false whenever the allocator split the plane into discontiguous groups; `Configuration.PreferContiguousImageBuffers` set before decode is what makes it hold.
- `WrapMemory(Configuration, IMemoryOwner<TPixel>, …)` TRANSFERS ownership — the image disposes the rental — while the `Memory<TPixel>` overload borrows and disposes nothing; the two differ only in that transfer and picking the wrong one either double-returns the rental or leaks it.

[ENTRYPOINT_SCOPE]: encode — `Image` and the encoder rows

| [INDEX] | [SURFACE]                                                   | [SHAPE]  | [CAPABILITY]                 |
| :-----: | :---------------------------------------------------------- | :------- | :--------------------------- |
|  [01]   | `Image.Save(Stream, IImageEncoder)`                         | instance | encode to a caller stream    |
|  [02]   | `Image.SaveAsync(Stream, IImageEncoder, CancellationToken)` | instance | async encode                 |
|  [03]   | `new PngEncoder { … }`                                      | ctor     | PNG depth and filter policy  |
|  [04]   | `new TiffEncoder { … }`                                     | ctor     | TIFF depth and predictor     |
|  [05]   | `new WebpEncoder { … }`                                     | ctor     | WebP quality policy          |
|  [06]   | `new QoiEncoder { … }`                                      | ctor     | QOI lossless encode          |
|  [07]   | `new JpegEncoder { … }`                                     | ctor     | JPEG quality and subsampling |

- Encoder init-only knobs: `PngEncoder` carries `BitDepth`, `ColorType`, `CompressionLevel`, `FilterMethod`, `Gamma`, `InterlaceMethod`, `ChunkFilter`, `TransparentColorMode`, `Threshold`, `TextCompressionThreshold`; `TiffEncoder` carries `BitsPerPixel`, `Compression`, `CompressionLevel`, `PhotometricInterpretation`, `HorizontalPredictor`; `WebpEncoder` carries `FileFormat`, `Quality`, `Method`, `NearLossless`, `NearLosslessQuality`, `UseAlphaCompression`, `EntropyPasses`, `SpatialNoiseShaping`, `FilterStrength`, `TransparentColorMode`.
- Depth-knob vocabularies, each nullable on its encoder so an unset knob infers from the pixel type: `PngBitDepth` `Bit1` `Bit2` `Bit4` `Bit8` `Bit16`; `PngColorType` `Grayscale` `Rgb` `Palette` `GrayscaleWithAlpha` `RgbWithAlpha`; `TiffBitsPerPixel` `Bit1` `Bit4` `Bit6` `Bit8` `Bit10` `Bit12` `Bit14` `Bit16` `Bit24` `Bit30` `Bit32` `Bit36` `Bit42` `Bit48`; `TiffPhotometricInterpretation` `WhiteIsZero` `BlackIsZero` `Rgb` `PaletteColor` `TransparencyMask` `Separated` `YCbCr` `CieLab` `IccLab` `ItuLab` `ColorFilterArray` `LinearRaw`.
- `TiffBitsPerPixel` tops out at `Bit48` and carries NO float row, so this encoder writes no float TIFF and no 16-bit-plus-alpha TIFF; a four-component 16-bit plane routed here drops its alpha lane rather than refusing, and a float plane belongs to the OpenEXR peer.

[ENTRYPOINT_SCOPE]: configuration, metadata, and color conversion

| [INDEX] | [SURFACE]                                                     | [SHAPE]         | [CAPABILITY]                      |
| :-----: | :------------------------------------------------------------ | :-------------- | :-------------------------------- |
|  [01]   | `Configuration.Default`                                       | static property | process-wide default              |
|  [02]   | `Configuration.Clone() -> Configuration`                      | instance        | per-profile copy                  |
|  [03]   | `Configuration.MemoryAllocator`                               | property        | swap the pooled buffer source     |
|  [04]   | `Configuration.PreferContiguousImageBuffers`                  | property        | force one contiguous plane buffer |
|  [05]   | `Configuration.MaxDegreeOfParallelism`                        | property        | codec and processor parallelism   |
|  [06]   | `Configuration.Configure(IImageFormatConfigurationModule)`    | instance        | register one format row           |
|  [07]   | `Image.Metadata` / `ImageMetadata.IccProfile`                 | property        | attach or read the ICC profile    |
|  [08]   | `new ColorSpaceConverter(ColorSpaceConverterOptions)`         | ctor            | bind white points and spaces      |
|  [09]   | `ColorSpaceConverter.ToCieXyz(in Rgb) -> CieXyz`              | instance        | one space-to-space conversion     |
|  [10]   | `ColorSpaceConverter.Convert(ReadOnlySpan<CieLch>, Span<CieLab>)` | instance        | bulk span conversion — one row of a ~100-pair CONCRETE overload matrix; no generic `Convert<TFrom,TTo>` exists at this major (that shape is the 4.x `ColorProfileConverter`'s) |
|  [11]   | `ColorSpaceConverter.Adapt(in CieXyz, in CieXyz) -> CieXyz`   | instance        | chromatic adaptation              |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Image<TPixel>` is the one owner; the pixel type is the storage decision and every codec is a row on `Configuration.ImageFormatsManager`, so a new container is a registered `IImageFormat` pair, never a second image type.
- Decode direction is caller-chosen: `Load` follows the file, `Load<TPixel>` converts once inside the decoder. Texture-grade paths always name their pixel type, because letting the file decide silently forks a 16-bit source and an 8-bit source into two arenas the plane admission then has to reconcile.
- Depth rides the pixel type — `L8`/`L16` scalar, `Rgba32`/`Rgba64` integer color, `HalfVector4`/`RgbaVector` float HDR — and `CloneAs<TPixel2>` is the one conversion, so a float plane narrows to `Rgba64` for PNG16 egress through the same member a `Rgba64` widens to `RgbaVector` through.
- `Image.WrapMemory` inverts ownership: the codec reads and writes the caller's pooled plane in place, so an encode of an already-materialized plane costs one header and the compressed body, never a second full-plane allocation. Its `IMemoryOwner<TPixel>` form hands the rental to the image; its `Memory<TPixel>` form leaves it with the caller.
- `ProcessPixelRows` is the only sanctioned bulk access — the accessor hands back `Span<TPixel>` per row under a scope that pins the discontiguous group layout, so a fold over rows is safe where a whole-image span is not.
- Depth is a per-encoder policy: `PngEncoder.BitDepth`/`ColorType` and `TiffEncoder.BitsPerPixel`/`PhotometricInterpretation` decide what the file carries, and leaving either null lets the encoder infer from the pixel type — an inference that silently narrows where the plane's depth exceeds the encoder's own roster, which is exactly where a float plane meets `TiffBitsPerPixel`.
- Color management is explicit and carried, never applied: an `IccProfile` on `ImageMetadata` rides the file, and `ColorSpaceConverter` over `ColorSpaceConverterOptions` is the one place a space transform runs — a decode never silently color-manages.
- Codecs throw: `ImageFormatException` for a malformed payload and `UnknownImageFormatException` where no detector claims the bytes, both lowered at the folder boundary onto the typed fault rail rather than escaping as exceptions.

[STACKING]:
- `CommunityToolkit.HighPerformance`(`libs/csharp/.api/api-highperformance.md`): `MemoryOwner<T>.Allocate(count)` IS an `IMemoryOwner<T>`, so `Image.WrapMemory(configuration, owner, width, height, metadata)` adopts the texel arena outright and `MemoryOwner<T>.Memory` feeds the borrowing overload where the arena outlives the encode; one pooled rental then serves the arena, the `Memory2D<T>`/`Span2D<T>` neighbourhood folds, and the codec, with `CopyPixelDataTo(Span<byte>)` as the only byte-level drain.
- `TinyEXR.NET`(`.api/api-tinyexr.md`): the OpenEXR owner, whole. This surface carries no EXR codec at the held major, so every EXR read and write — flat scanline, tiled, mip-levelled, deep, multi-part — routes to that peer, and a plane crosses between an `Image<RgbaVector>` and a `TinyEXR.V3.ChannelBuffer` as raw texels in one pooled arena.
- `TextureCompressor`(`.api/api-texturecompressor.md`): an `Image<Rgba32>`/`Image<RgbaVector>` plane crosses into block compression as a `BitmapView<Rgba8UNorm>`/`BitmapView<Rgba32Float>` over the shared pooled span — `CopyPixelDataTo(Span<byte>)` fills the peer's `ArrayBitmap<TPixel>.PixelSpan` and `ITextureCoder.Encode` consumes it, so PNG/TIFF/WebP containers and BCn/ASTC/UASTC payloads never mint two arenas for one plane.
- `System.IO.Hashing`(`libs/csharp/.api/api-hashing.md`): the encoded byte stream feeds `XxHash128.Append` incrementally and `GetCurrentHashAsUInt128` closes the plane's content key, so identity derives from the encoded file the object store holds rather than from a re-encode.
- `Wacton.Unicolour`(`libs/csharp/.api/api-unicolour.md`): perceptual and spectral color work stays on the `Unicolour` owner and this surface contributes container-level ICC carriage alone; `ColorSpaceConverter` runs only where an ingested asset declares a working space the appearance rail must reconcile, and never as a second color authority.
- within-lib: the raster codec fold binds one `Configuration` per encode profile — allocator, `PreferContiguousImageBuffers`, `MaxDegreeOfParallelism`, registered format rows — reused across every plane rather than constructed per call, and each `RasterFormat` row names its encoder instance beside the explicit depth its declared plane demands.

[LOCAL_ADMISSION]:
- Admission holds at the last major granting Apache-2.0 without a build-time license gate. Bumping admits only once the majors either restore an open-source opt-out on the validation task or the estate gains a home for a vendor-issued license artifact, and the bump buys nothing the peers do not already own — OpenEXR is `TinyEXR.NET`'s whole.
- ImageSharp owns the FILE containers alone. Block-compressed payloads, KTX2 containers, and Radiance `.hdr` belong to the `TextureCompressor` family; a hand-rolled BCn or KTX writer over `Image<TPixel>` is the duplication this split forecloses.
- Eight-bit types (`Rgba32`, `Rgb24`, `L8`) admit for display and preview egress alone; a texture channel plane binds `L16`, `Rgba64`, `HalfVector4`, or `RgbaVector`, because an 8-bit intermediate on a texture path is a silent quantization the wire cannot recover.
- Every encoder row states its depth explicitly. Null `PngEncoder.BitDepth` or `TiffEncoder.BitsPerPixel` on a 16-bit plane is an inference, not a declaration, and inference is what quietly ships an 8-bit channel.
- Animated and multi-frame containers (GIF, animated WebP) are outside the texture estate; frame collections serve only where a declared layer law makes a frame sequence the plane.

[RAIL_LAW]:
- Package: `SixLabors.ImageSharp`
- Owns: the managed raster container estate — format detection, decode and encode across PNG through 16-bit, TIFF through 16-bit integer, WebP, QOI, JPEG, BMP, GIF, TGA, and Netpbm; the `IPixel` depth ladder from `L8` to `RgbaVector`; ICC, EXIF, and XMP metadata carriage; and the `ColorSpaceConverter` space and chromatic-adaptation transform.
- Accept: `Load<TPixel>` naming the demanded plane depth; `WrapMemory` over a pooled `MemoryOwner<T>` arena with the ownership form chosen deliberately; `ProcessPixelRows` for bulk access; one reused `Configuration` per encode profile; encoder instances declaring depth, compression, and filter policy explicitly; `CloneAs<TPixel2>` as the one depth conversion.
- Reject: an 8-bit pixel type on a texture channel plane; an inferred encoder depth on a 16-bit or float plane; a second arena where `WrapMemory` binds the existing one; `DangerousTryGetSinglePixelMemory` without `PreferContiguousImageBuffers`; an EXR expectation against this surface; a hand-rolled block-compression or KTX2 writer over it; a decode assumed to have color-managed anything.
