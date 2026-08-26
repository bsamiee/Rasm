# [RASM_MATERIALS_API_TEXTURECOMPRESSOR]

`TextureCompressor` is the pure-managed GPU texture-format engine: a `TextureFormat` value record describes any block or packed layout, a `TextureCoderManager` registry maps each format to an `ITextureCoder`, and every coder encodes and decodes generically over `IPixel<TSelf>` so a float HDR plane reaches BC6H and an 8-bit plane reaches BC7 through the one member. Its `TextureConverter` facade sits above that surface and is 8-bit-bound, so a texture-grade path binds the coder registry directly.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: format description and texture carriers — `TextureCompressor.Formats`

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY]          | [CAPABILITY]                                                       |
| :-----: | :------------------------------- | :--------------------- | :----------------------------------------------------------------- |
|  [01]   | `TextureFormat`                  | readonly record struct | any layout: kind, components, value kind, bit counts, block extent |
|  [02]   | `TextureFormats`                 | static class           | the standing catalogue of named format values                      |
|  [03]   | `TextureFormatCatalog`           | static class           | name-keyed lookup over every declared format                       |
|  [04]   | `TextureImage`                   | class                  | format plus the subresource pyramid                                |
|  [05]   | `TextureSubresource`             | sealed class           | one mip, array layer, face payload                                 |
|  [06]   | `TextureSubresourceSelection`    | readonly record struct | mip, array layer, cube face selector                               |
|  [07]   | `TextureSubresourceFilter`       | readonly record struct | subresource predicate                                              |
|  [08]   | `TextureMipmapGenerationOptions` | static class           | texture-level mip policy                                           |
|  [09]   | `TextureFormatKind`              | enum                   | `Uncompressed` `Paletted` `BlockCompressed`                        |
|  [10]   | `TextureValueKind`               | enum                   | sample interpretation                                              |
|  [11]   | `TextureComponents`              | enum                   | channel set and order                                              |
|  [12]   | `TextureCubeFace`                | enum                   | the six cube faces                                                 |
|  [13]   | `TexturePayloadSizeMode`         | enum                   | block payload sizing convention                                    |

[TextureValueKind]: `UNorm` `SNorm` `UInt` `SInt` `Float` `Srgb` `XR` `XRSrgb` `DepthStencil`
[TextureComponents]: `R` `Rg` `Rgb` `Yuv` `Yuva` `Rgba` `Bgr` `Bgra` `Bgrx` `Argb` `Abgr` `Alpha` `Luminance` `LuminanceAlpha` `Intensity` `Depth` `Stencil` `DepthStencil` (implicitly numbered, so the roster is DECLARATION-ordered)

[PUBLIC_TYPE_SCOPE]: pixel carriers — `TextureCompressor.Colors`

| [INDEX] | [SYMBOL]                                      | [TYPE_FAMILY] | [CAPABILITY]                              |
| :-----: | :-------------------------------------------- | :------------ | :---------------------------------------- |
|  [01]   | `IPixel<TSelf>`                               | interface     | self-constrained unmanaged texel contract |
|  [02]   | `Rgba8UNorm`                                  | struct        | 8-bit normalized texel                    |
|  [03]   | `Rgba8SNorm`                                  | struct        | 8-bit signed-normalized texel             |
|  [04]   | `Rgba16UNorm`                                 | struct        | 16-bit normalized texel                   |
|  [05]   | `Rgba16SNorm`                                 | struct        | 16-bit signed-normalized texel            |
|  [06]   | `Rgba16Float`                                 | struct        | half-float texel                          |
|  [07]   | `Rgba32Float`                                 | struct        | float32 texel, the HDR encode carrier     |
|  [08]   | `Rgba32UNorm` / `Rgba32SNorm`                 | struct        | 32-bit integer-normalized texel           |
|  [09]   | `Rgba64Float` / `Rgba64UNorm` / `Rgba64SNorm` | struct        | 64-bit-per-channel texel                  |
|  [10]   | `RgbaColorConversions`                        | static class  | sRGB and linear UNorm8 lookup conversion  |

[PUBLIC_TYPE_SCOPE]: plane carriers and mip generation — `TextureCompressor.Bitmaps`

| [INDEX] | [SYMBOL]                                     | [TYPE_FAMILY]    | [CAPABILITY]                               |
| :-----: | :------------------------------------------- | :--------------- | :----------------------------------------- |
|  [01]   | `IBitmap<TPixel>`                            | interface        | width, height, texel access contract       |
|  [02]   | `ArrayBitmap<TPixel>`                        | sealed class     | array-backed plane exposing `PixelSpan`    |
|  [03]   | `PooledBitmap<TPixel>`                       | class            | pool-backed plane                          |
|  [04]   | `NativeMemoryBitmap<TPixel>`                 | class            | unmanaged-allocation plane                 |
|  [05]   | `BitmapView<TPixel>`                         | ref struct       | borrowed `Span<TPixel>` plane view         |
|  [06]   | `IVolumeBitmap<TPixel>` and its three owners | interface, class | 3D plane peers                             |
|  [07]   | `VolumeBitmapView<TPixel>`                   | ref struct       | borrowed 3D plane view                     |
|  [08]   | `BitmapMipChain`                             | static class     | mip-pyramid generation and downsample      |
|  [09]   | `MipmapGenerationOptions`                    | sealed class     | level cap, color space, alpha mode, filter |
|  [10]   | `MipmapFilter`                               | enum             | `Box` `Triangle`                           |
|  [11]   | `MipmapColorSpace`                           | enum             | `Linear` `Srgb`                            |
|  [12]   | `MipmapAlphaMode`                            | enum             | `Premultiplied` `Straight`                 |

[PUBLIC_TYPE_SCOPE]: coder contracts and the block-format engines — `TextureCompressor.Codecs` / `.Registry`

| [INDEX] | [SYMBOL]                                                         | [TYPE_FAMILY] | [CAPABILITY]                                       |
| :-----: | :--------------------------------------------------------------- | :------------ | :------------------------------------------------- |
|  [01]   | `ITextureCoder`                                                  | interface     | generic encode, decode, encoded byte count         |
|  [02]   | `ITextureCoder3D`                                                | interface     | the volume peer                                    |
|  [03]   | `IPitchTextureCoder`                                             | interface     | row-pitch-aware coder                              |
|  [04]   | `TextureCoderManager`                                            | sealed class  | format-to-coder registry with a global instance    |
|  [05]   | `BptcTextureCoder`                                               | sealed class  | BC6H unsigned and signed float, BC7 UNorm and sRGB |
|  [06]   | `S3tcTextureCoder`                                               | sealed class  | BC1, BC2, BC3 (DXT1/3/5)                           |
|  [07]   | `RgtcLatcTextureCoder`                                           | sealed class  | BC4 and BC5 scalar and two-channel                 |
|  [08]   | `AstcTextureCoder` / `Astc3DTextureCoder`                        | sealed class  | ASTC 2D and 3D block extents                       |
|  [09]   | `EtcTextureCoder`                                                | sealed class  | ETC1, ETC2, and EAC R11/RG11                       |
|  [10]   | `BasisUastcLdr4x4TextureCoder`                                   | sealed class  | Basis UASTC LDR 4x4                                |
|  [11]   | `BasisEtc1sTextureCoder`                                         | sealed class  | Basis ETC1S with endpoint and selector palettes    |
|  [12]   | `PvrtcTextureCoder` / `AtcTextureCoder` / `FxtcTextureCoder`     | sealed class  | PVRTC, ATC, FXT1 mobile families                   |
|  [13]   | `PackedUNorm`/`PackedSNorm`/`PackedFloat`/`PackedInteger` coders | sealed class  | packed layouts incl. `Rgb9E5`, `R11G11B10Float`    |
|  [14]   | `SequentialUncompressedTextureCoder`                             | sealed class  | straight component-order layouts                   |
|  [15]   | `PalettedTextureCoder` / `IndexedTextureCoder`                   | sealed class  | palette and index layouts                          |
|  [16]   | `DepthStencilTextureCoder`                                       | sealed class  | depth and stencil layouts                          |
|  [17]   | `Rgbm`/`Xr`/`BitPackedUNorm` `TextureCoder`                      | sealed class  | RGBM, XR, sub-byte layouts                         |
|  [18]   | `PlanarYuv`/`PackedYuv422`/`PackedYuva444`/`PackedRgb422` coders | sealed class  | video chroma layouts                               |
|  [19]   | `TextureArrayCoder` / `PitchTextureArrayCoder`                   | internal      | array-layer coding over a per-slice coder          |

- `AstcTextureCoder` spans the `RgbaAstc4x4*`..`RgbaAstc12x12*` 2D extents and `Astc3DTextureCoder` the `RgbaAstc3x3x3*`..`RgbaAstc6x6x6*` 3D extents, each extent carrying `UNorm`, `Srgb`, and `Float` arms — the HDR arm spells `Float`.
- `BasisEtc1sTextureCoder` and `BasisUastcLdr4x4TextureCoder`: `TextureFormats.RgbaBasisEtc1sUNorm`/`RgbaBasisEtc1sSrgb` and `TextureFormats.RgbaBasisUastcLdr4x4UNorm`/`RgbaBasisUastcLdr4x4Srgb` are the four `TextureFormats` fields the KTX2 wire-legal payload classes ride.
- `BptcTextureCoder`, `S3tcTextureCoder`, and `RgtcLatcTextureCoder` bind these `TextureFormats` fields: `Bc1Rgb` `Bc1RgbSrgb` `Bc1Rgba` `Bc1RgbaSrgb` `Bc2Rgba` `Bc2RgbaSrgb` `Bc3Rgba` `Bc3RgbaSrgb` `Bc4UNorm` `Bc4SNorm` `Bc5UNorm` `Bc5SNorm` `Bc6HUFloat` `Bc6HSFloat` `Bc7UNorm` `Bc7Srgb` — the field name, never a `BC1_RGB_UNORM_BLOCK`-style token, is what a static reference spells.
- `TextureFormatCatalog.TryGet(string, out TextureFormat)` matches EITHER the declaring field name or the format's own `Name`, ordinal-ignore-case, so a caller holding either spelling resolves; binding the static field directly is the compile-checked form and the lookup serves a runtime-sourced name alone.
- `TextureFormats.Rgba8Srgb` (`TextureFormat.Uncompressed("RGBA8_SRGB", Rgba, Srgb, 8, 8, 8, 8)`) is the uncompressed sRGB-declared field beside the block rows — the declared-transfer probe a container sniff compares against.

[PUBLIC_TYPE_SCOPE]: conversion facade, options, and file-format registry

| [INDEX] | [SYMBOL]                                                         | [TYPE_FAMILY] | [CAPABILITY]                                       |
| :-----: | :--------------------------------------------------------------- | :------------ | :------------------------------------------------- |
|  [01]   | `TextureConverter`                                               | sealed class  | path, stream, and in-memory conversion facade      |
|  [02]   | `TextureConversionOptions`                                       | sealed class  | conversion policy row                              |
|  [03]   | `TextureConversionResult`                                        | sealed record | conversion result                                  |
|  [04]   | `TextureConversionFileKind`                                      | enum          | image versus texture container classification      |
|  [05]   | `TextureConversionMipmaps`                                       | enum          | `None` `Generate`                                  |
|  [06]   | `TextureAssembler` / `TextureExtractor`                          | class         | subresource assembly and extraction                |
|  [07]   | `TextureExtractedImage<TPixel>`                                  | sealed record | one extracted subresource image                    |
|  [08]   | `TextureCompressionOptions`                                      | class         | `CompressionMode` carrier, per-coder options       |
|  [09]   | `TextureCompressionLevel`                                        | enum          | `Fast` `Normal` `High` `Exhaustive`                |
|  [10]   | `TextureFileFormatManager`                                       | sealed class  | file-format registry with a global instance        |
|  [11]   | `IFileFormat` / `IImageFileFormat` / `ITextureFileFormat`        | interface     | format identity, image codec, texture codec        |
|  [12]   | `ITextureFile`                                                   | interface     | a decoded container carrying its `TextureImage`    |
|  [13]   | `IFileFormatOptions`                                             | interface     | the per-format options contract                    |
|  [14]   | `BigEndianByteSwap` / `SwizzledHelper` / `TextureCodingParallel` | static class  | byte-order, swizzle, and parallel coding utilities |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: format description and size math — `TextureFormat` / `TextureFormatCatalog`

| [INDEX] | [SURFACE]                                                                        | [SHAPE]         | [CAPABILITY]                |
| :-----: | :------------------------------------------------------------------------------- | :-------------- | :-------------------------- |
|  [01]   | `TextureFormat.BlockCompressed(string, TextureComponents, TextureValueKind, …)`  | static          | describe a block layout     |
|  [02]   | `TextureFormat.Uncompressed(string, TextureComponents, TextureValueKind, …)`     | static          | describe a packed layout    |
|  [03]   | `TextureFormat.Paletted(string, TextureComponents, TextureValueKind, …)`         | static          | describe a palette layout   |
|  [04]   | `TextureFormat.GetByteCount(int, int) -> int`                                    | instance        | payload bytes for an extent |
|  [05]   | `TextureFormat.GetByteCount64(int, int) -> long`                                 | instance        | 64-bit payload bytes        |
|  [06]   | `TextureFormat.GetRowByteCount(int) -> int`                                      | instance        | one row or block-row pitch  |
|  [07]   | `TextureFormat.BitsPerTexel` / `BytesPerBlock` / `ChannelCount` / `IsCompressed` | property        | derived layout facts        |
|  [08]   | `TextureFormatCatalog.All`                                                       | static property | every declared format value |
|  [09]   | `TextureFormatCatalog.TryGet(string, out TextureFormat) -> bool`                 | static          | name-keyed lookup           |
|  [10]   | `TextureFormatCatalog.GetFieldName(TextureFormat) -> string`                     | static          | the declaring field's name  |

[ENTRYPOINT_SCOPE]: the generic coding surface — `ITextureCoder` / `TextureCoderManager`

| [INDEX] | [SURFACE]                                                                   | [SHAPE]         | [CAPABILITY]                         |
| :-----: | :-------------------------------------------------------------------------- | :-------------- | :----------------------------------- |
|  [01]   | `ITextureCoder.Encode<TPixel>(BitmapView<TPixel>, Span<byte>)`              | instance        | encode any texel type to the payload |
|  [02]   | `ITextureCoder.Decode<TPixel>(ReadOnlySpan<byte>, BitmapView<TPixel>)`      | instance        | decode the payload to any texel type |
|  [03]   | `ITextureCoder.GetEncodedByteCount(int, int) -> int`                        | instance        | payload sizing for an extent         |
|  [04]   | `ITextureCoder.Format`                                                      | property        | the format the coder claims          |
|  [05]   | `TextureCoderManager.Global`                                                | static property | the process-wide registry            |
|  [06]   | `TextureCoderManager.Register(TextureFormat, ITextureCoder) -> IDisposable` | instance        | scoped one-format registration       |
|  [07]   | `TextureCoderManager.Register(IEnumerable<TextureFormat>, Func<…>)`         | instance        | scoped family registration           |
|  [08]   | `TextureCoderManager.TryGetCoder(TextureFormat, out ITextureCoder) -> bool` | instance        | probe a registration                 |
|  [09]   | `TextureCoderManager.GetCoder(TextureFormat) -> ITextureCoder`              | instance        | resolve or throw                     |
|  [10]   | `TextureCoderManager.Register3D` / `TryGetCoder3D` / `GetCoder3D`           | instance        | the volume peers                     |
|  [11]   | `TextureCoderManager.Combine(params ReadOnlySpan<IDisposable>)`             | instance        | one scope over many registrations    |

- Every `Register*` returns the registration's `IDisposable` scope; dropping it leaves the coder resident on `Global` for the process.
- `TextureCompressionRegistrationFactory` is INTERNAL — its `Create` is public on an unreachable class, so no consumer registers through it; the PUBLIC spelling of the same ladder is the per-family coder constructor over `(TextureFormat, TextureCompressionOptions)` handed to `Register`, and the family classes (`S3tcTextureCoder`, `EtcTextureCoder`, `RgtcLatcTextureCoder`, `BptcTextureCoder`, `AstcTextureCoder`, `BasisUastcLdr4x4TextureCoder`, `BasisEtc1sTextureCoder`, …) each carry `IsSupported(TextureFormat)` and `SupportedFormats`.
- `TryGetCoder` CREATES the built-in coder for a standard format lazily ON ITS OWN INSTANCE and caches it there, so a bare `new TextureCoderManager()` resolves the standard roster with zero registrations — a bake-scoped manager is one constructor call, and only an options-bearing coder (a compression level) needs an explicit `Register`.
- `KtxCodec.Encode`/`EncodeMipChain`/`Decode<TPixel>` resolve their coders from `TextureCoderManager.Global` INTERNALLY — a bake-scoped manager reaches only the per-subresource `ITextureCoder.Decode`/`Encode` path a caller drives itself, so an options-bearing encode through the container codec is not expressible without touching the process-global registry.

[ENTRYPOINT_SCOPE]: plane carriers and mip generation

| [INDEX] | [SURFACE]                                                                           | [SHAPE]         | [CAPABILITY]                  |
| :-----: | :---------------------------------------------------------------------------------- | :-------------- | :---------------------------- |
|  [01]   | `new ArrayBitmap<TPixel>(int, int)` / `new ArrayBitmap<TPixel>(int, int, TPixel[])` | ctor            | own or adopt a plane array    |
|  [02]   | `ArrayBitmap<TPixel>.PixelSpan` / `.Pixels`                                         | property        | the texel window              |
|  [03]   | `ArrayBitmap<TPixel>.AsView() -> BitmapView<TPixel>`                                | instance        | borrow the plane              |
|  [04]   | `new BitmapView<TPixel>(Span<TPixel>, int, int)`                                    | ctor            | wrap a caller span as a plane |
|  [05]   | `BitmapView<TPixel>.GetRowSpan(int) -> Span<TPixel>`                                | instance        | one row                       |
|  [06]   | `BitmapView<TPixel>.this[int, int] -> ref TPixel`                                   | indexer         | by-reference texel access     |
|  [07]   | `BitmapMipChain.Generate<TPixel>(IBitmap<TPixel>, …) -> IReadOnlyList<…>`           | static          | full pyramid                  |
|  [08]   | `BitmapMipChain.Downsample<TPixel>(BitmapView<TPixel>, …) -> ArrayBitmap<TPixel>`   | static          | one level                     |
|  [09]   | `MipmapGenerationOptions.Default`                                                   | static property | box, linear, straight default |
|  [10]   | `new MipmapGenerationOptions { MaxLevelCount, ColorSpace, AlphaMode, Filter }`      | ctor            | init-only mip policy          |

[ENTRYPOINT_SCOPE]: the conversion facade and the file-format registry

| [INDEX] | [SURFACE]                                                                                | [SHAPE]         | [CAPABILITY]               |
| :-----: | :--------------------------------------------------------------------------------------- | :-------------- | :------------------------- |
|  [01]   | `new TextureConverter(TextureFileFormatManager, TextureCoderManager)`                    | ctor            | bind explicit registries   |
|  [02]   | `TextureConverter.Convert(Stream, string, Stream, string, …) -> TextureConversionResult` | instance        | container convert          |
|  [03]   | `TextureConverter.EncodeTexture(IBitmap<Rgba8UNorm>, TextureFormat, …) -> TextureImage`  | instance        | 8-bit encode facade        |
|  [04]   | `TextureConverter.DecodeTexture(TextureImage, …) -> ArrayBitmap<Rgba8UNorm>`             | instance        | 8-bit decode facade        |
|  [05]   | `TextureConverter.TranscodeTexture(TextureImage, TextureFormat, …) -> TextureImage`      | instance        | payload transcode          |
|  [06]   | `new TextureConversionOptions { … }`                                                     | ctor            | conversion policy          |
|  [07]   | `TextureFileFormatManager.Global`                                                        | static property | process-wide registry      |
|  [08]   | `TextureFileFormatManager.Register(IFileFormat) -> IDisposable`                          | instance        | scoped format registration |
|  [09]   | `TextureFileFormatManager.ReadTexture(Stream, string?, …) -> ITextureFile`               | instance        | container decode           |
|  [10]   | `TextureFileFormatManager.WriteTexture(TextureImage, Stream, string, …)`                 | instance        | container encode           |
|  [11]   | `TextureFileFormatManager.ReadImage<TPixel>(Stream, string?, …) -> ArrayBitmap<TPixel>`  | instance        | flat-image decode          |
|  [12]   | `TextureFileFormatManager.WriteImage<TPixel>(IBitmap<TPixel>, Stream, string, …)`        | instance        | flat-image encode          |
|  [13]   | `TextureFileFormatManager.TryGetTextureFormat(string, out ITextureFileFormat) -> bool`   | instance        | container lookup           |

- `EncodeTexture`/`DecodeTexture` are `Rgba8UNorm`-bound at the SIGNATURE, so a float or 16-bit plane routed through the facade quantizes to 8 bits before the coder ever sees it; the generic `ITextureCoder.Encode<TPixel>` surface is the only non-quantizing path.

[ENTRYPOINT_SCOPE]: texture pyramid navigation — `TextureImage`

| [INDEX] | [SURFACE]                                                                               | [SHAPE]  | [CAPABILITY]                      |
| :-----: | :-------------------------------------------------------------------------------------- | :------- | :-------------------------------- |
|  [01]   | `new TextureImage(TextureFormat, int, int, byte[])`                                     | ctor     | single-subresource texture        |
|  [02]   | `new TextureImage(TextureFormat, IReadOnlyList<TextureSubresource>, int, int)`          | ctor     | array-layer and face pyramid      |
|  [03]   | `TextureImage.GetSubresource(int, TextureCubeFace, int) -> TextureSubresource`          | instance | mip and face lookup               |
|  [04]   | `TextureImage.GetSubresource(int, int = 0, int = 0) -> TextureSubresource`              | instance | mip, layer, face-index lookup     |
|  [05]   | `TextureImage.Format` / `MipLevelCount` / `ArrayLayerCount` / `FaceCount` / `IsCubeMap` | property | declared format, pyramid shape    |
|  [06]   | `TextureImage.GetFullMipLevelCount(int, int) -> int`                                    | static   | full-chain level count            |
|  [07]   | `TextureImage.GetMipDimension(int, int) -> int`                                         | static   | one level's extent                |
|  [08]   | `new TextureSubresource(int, int, int, int, int, byte[])`                               | ctor     | mip, layer, face, extent, payload |
|  [09]   | `new TextureSubresource(int, int, int, int, int, int, byte[])`                          | ctor     | the depth-bearing volume peer     |
|  [10]   | `TextureSubresource.Width` / `Height` / `Depth`                                         | property | one level's own extent            |
|  [11]   | `TextureSubresource.MipLevel` / `ArrayLayer` / `FaceIndex`                              | property | the slot this payload occupies    |
|  [12]   | `TextureSubresource.Payload -> byte[]`                                                  | property | the block bytes a coder decodes   |
|  [13]   | `TextureSubresource.Data -> byte[]`                                                     | property | the `Payload` alias               |
|  [14]   | `TextureImage.Subresources -> IReadOnlyList<TextureSubresource>`                        | property | the whole ordered slot list       |
|  [15]   | `TextureImage.Width` / `Height` / `Depth` / `Payload` / `Data`                          | property | subresource-zero projections      |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `TextureFormat` is a VALUE, not an enum: name, kind, components, value kind, per-channel bit counts, block extent, and bits-per-block fully describe a layout, so a format the standing catalogue omits is a `BlockCompressed`/`Uncompressed`/`Paletted` construction rather than a library change, and `GetByteCount`/`GetRowByteCount` size any payload from that description alone.
- Coding is a two-level dispatch: `TextureCoderManager` maps a `TextureFormat` to an `ITextureCoder`, and the coder encodes and decodes generically over `IPixel<TSelf>`. Pixel type belongs to the caller, not the coder — `Rgba32Float` in for BC6H, `Rgba8UNorm` in for BC7, one member either way.
- Registration is scoped and lazy: `TryGetCoder` on a fresh manager creates each standard coder on demand, and an options-bearing coder registers through the family constructor + `Register`, whose `IDisposable` retires it — so a bake never loads every coder to write one channel.
- `BitmapView<TPixel>` is the coding boundary. Every coder takes and returns a borrowed span plane, so the arena belongs to the caller and `ArrayBitmap<TPixel>` is a convenience owner rather than a required one.
- `TextureImage` carries the whole pyramid — mip levels, array layers, cube faces — as an ordered `TextureSubresource` list, and `GetSubresource(mip, face, layer)` is the one navigator; `IsCubeMap` reads `FaceCount == 6`. Each slot states its OWN `Width`/`Height`/`Depth`, so a level's extent reads off the subresource rather than halving the base — and `Payload` is a bare `byte[]`, which the coder's `ReadOnlySpan<byte>` parameter takes directly; a `.Span` projection on it does not compile.
- Mip generation is deliberately minimal: `MipmapFilter` offers `Box` and `Triangle` alone, and `MipmapColorSpace`/`MipmapAlphaMode` decide whether the fold decodes sRGB and un-premultiplies first. Any wider mip law — windowed-sinc, normal renormalization, roughness-variance coupling — is the composing folder's fold over `BitmapView` rows, never a knob here.
- `TextureCompressionLevel` (`Fast`/`Normal`/`High`/`Exhaustive`) is the sole quality dial and it reaches the block encoders through `TextureCompressionOptions.CompressionMode`; per-block RDO and error-metric selection are not exposed.
- `TextureFileFormatManager` splits the way the coder surface does: `IImageFileFormat` reads and writes a FLAT pixel image generically over `IPixel`, `ITextureFileFormat` reads and writes a `TextureImage` container. Container packages register one row each on this manager.

[STACKING]:
- `TextureCompressor.FileFormats.Ktx`(`.api/api-texturecompressor-fileformats-ktx.md`): the container leg — `KtxFileFormatRegistration.RegisterKtxFileFormat(this TextureFileFormatManager)` seats the KTX row, and a `TextureImage` this engine's coders filled with `RGBA_BASIS_UASTC_LDR_4X4_*` or `RGBA_BASIS_ETC1S_*` blocks writes through `KtxFileFormat.WriteTexture` under `KtxEncodingOptions`.
- `TextureCompressor.FileFormats.Hdr`(`.api/api-texturecompressor-fileformats-hdr.md`): the Radiance leg — `HdrCodec.Decode(stream) -> ArrayBitmap<Rgba32Float>` lands an HDR environment source directly in the float texel type `BptcTextureCoder` BC6H encodes from, with no intermediate conversion.
- `SixLabors.ImageSharp`(`.api/api-imagesharp.md`): the container split — that surface owns PNG, TIFF, WebP, QOI, and JPEG files and this one owns GPU block payloads; a plane crosses as raw bytes through `Image<TPixel>.CopyPixelDataTo(Span<byte>)` into an `ArrayBitmap<TPixel>.PixelSpan` reinterpretation, so one pooled arena serves both and no `Image` instance enters a coder.
- `CommunityToolkit.HighPerformance`(`libs/dotnet/.api/api-highperformance.md`): `MemoryOwner<T>.Allocate(width * height)` rents the texel arena and `MemoryOwner<T>.Span` is exactly the `Span<TPixel>` `new BitmapView<TPixel>(span, width, height)` binds, while `ArrayPoolBufferWriter<byte>` receives the `GetEncodedByteCount`-sized payload; `Span2D<T>.GetRowSpan` and `BitmapView<TPixel>.GetRowSpan` window the same rows, so a neighbourhood fold and a block encode read one plane.
- `Silk.NET.WebGPU`(`libs/dotnet/.api/api-silk-webgpu.md`): a `TextureFormat` row and a wgpu `TextureFormat` enum row are two spellings of one layout — `GetRowByteCount(width)` computes the `TextureDataLayout.BytesPerRow` `QueueWriteTexture` demands (padded to the 256-byte copy alignment), so a block payload uploads as a compressed GPU texture wherever the device declares the matching `FeatureName` row.
- within-lib: the codec fold owns one `TextureCoderManager` per bake rather than `Global`, resolving standard coders through the instance's own lazy creation and registering options-bearing family coders per channel format, disposing each scope with the bake, so a long-lived process never accumulates every coder.

[LOCAL_ADMISSION]:
- `TextureConverter` is admitted for container-to-container transcode alone (`Convert`, `TranscodeTexture`); `EncodeTexture` and `DecodeTexture` are `Rgba8UNorm`-bound and never touch a texture channel plane.
- `Global` registries are process-static; a folder composing this engine binds its own `TextureCoderManager` and `TextureFileFormatManager` instances so a registration made for one bake never leaks into another's format resolution.
- Video chroma coders (`PlanarYuv*`, `PackedYuv*`, `PackedRgb422*`), the depth-stencil coder, and the palette and index coders describe layouts outside the texture-channel vocabulary; they stay unregistered rather than reachable-but-unused.
- This is a pre-1.0 single-maintainer surface pinned exact at the manifest: it composes behind ONE internal gate in the composing folder, the provisioned `ktx` CLI holds the encode floor beneath it, and a version bump re-verifies that gate's members before it lands.
