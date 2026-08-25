# [RASM_MATERIALS_API_TEXTURECOMPRESSOR_FILEFORMATS_KTX]

`TextureCompressor.FileFormats.Ktx` is the managed KTX container leg over the `TextureCompressor` coder engine: `KtxCodec` reads and writes both KTX1 and KTX2, and `KtxEncodingOptions` decides container version, supercompression scheme, and the GL or Vulkan format tokens the header carries. Zstandard supercompression rides `ZstdSharp.Port` in pure managed code, so a KTX2 payload writes with no native toolchain.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: container codec, format row, and options

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CAPABILITY]                                          |
| :-----: | :-------------------------- | :------------ | :---------------------------------------------------- |
|  [01]   | `KtxCodec`                  | static class  | read, write, decode, encode, mip-chain encode         |
|  [02]   | `KtxFileFormat`             | sealed class  | the `ITextureFileFormat` row for `.ktx` and `.ktx2`   |
|  [03]   | `KtxFileFormatRegistration` | static class  | the `TextureFileFormatManager` registration extension |
|  [04]   | `KtxTexture`                | sealed class  | an `ITextureFile` carrying its GL and Vulkan tokens   |
|  [05]   | `KtxEncodingOptions`        | sealed class  | version, format, mips, supercompression, sRGB flag    |

[PUBLIC_TYPE_SCOPE]: header vocabularies

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CAPABILITY]                                    |
| :-----: | :-------------------------- | :------------ | :---------------------------------------------- |
|  [01]   | `KtxVersion`                | enum          | `Version1` (the default) and `Version2`         |
|  [02]   | `KtxSupercompressionScheme` | enum          | `None` `BasisLz` `Zstandard` `Zlib`             |
|  [03]   | `KtxGlFormat`               | enum          | the GL type, format, and internal-format tokens |
|  [04]   | `KtxVkFormat`               | enum          | the `VK_FORMAT_*` tokens, including `Undefined` |

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: container read and typed decode — `KtxCodec`

| [INDEX] | [SURFACE]                                                                | [SHAPE] | [CAPABILITY]                        |
| :-----: | :----------------------------------------------------------------------- | :------ | :---------------------------------- |
|  [01]   | `KtxCodec.Read(Stream) -> KtxTexture`                                    | static  | parse the container, payload intact |
|  [02]   | `KtxCodec.Read(ReadOnlySpan<byte>) -> KtxTexture`                        | static  | parse from memory                   |
|  [03]   | `KtxCodec.Read(string) -> KtxTexture`                                    | static  | parse from a path                   |
|  [04]   | `KtxCodec.Decode<TPixel>(KtxTexture) -> ArrayBitmap<TPixel>`             | static  | decode an already-parsed container  |
|  [05]   | `KtxCodec.Decode<TPixel>(Stream) -> ArrayBitmap<TPixel>`                 | static  | parse and decode in one call        |
|  [06]   | `KtxCodec.Decode(Stream) -> ArrayBitmap<Rgba8UNorm>`                     | static  | the 8-bit convenience overload      |
|  [07]   | `KtxCodec.DecodeVolume<TPixel>(KtxTexture) -> ArrayVolumeBitmap<TPixel>` | static  | decode a 3D payload                 |
|  [08]   | `KtxCodec.DecodeVolume<TPixel>(Stream) -> ArrayVolumeBitmap<TPixel>`     | static  | parse and decode a 3D payload       |

[ENTRYPOINT_SCOPE]: container write and encode — `KtxCodec`

| [INDEX] | [SURFACE]                                                                      | [SHAPE] | [CAPABILITY]                    |
| :-----: | :----------------------------------------------------------------------------- | :------ | :------------------------------ |
|  [01]   | `KtxCodec.Write(KtxTexture, Stream, KtxEncodingOptions?)`                      | static  | write a prepared payload        |
|  [02]   | `KtxCodec.Write(KtxTexture, KtxEncodingOptions?) -> byte[]`                    | static  | write to memory                 |
|  [03]   | `KtxCodec.Encode<TPixel>(BitmapView<TPixel>, Stream, KtxEncodingOptions?)`     | static  | encode one plane to a container |
|  [04]   | `KtxCodec.Encode<TPixel>(IBitmap<TPixel>, KtxEncodingOptions?) -> byte[]`      | static  | encode one plane to memory      |
|  [05]   | `KtxCodec.EncodeMipChain<TPixel>(IReadOnlyList<IBitmap<TPixel>>, Stream, …)`   | static  | encode a whole pyramid          |
|  [06]   | `KtxCodec.EncodeMipChain<TPixel>(IReadOnlyList<IBitmap<TPixel>>, …) -> byte[]` | static  | encode a pyramid to memory      |

[ENTRYPOINT_SCOPE]: container carrier, options, and registration

| [INDEX] | [SURFACE]                                                                    | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :--------------------------------------------------------------------------- | :------- | :--------------------------------- |
|  [01]   | `new KtxTexture(TextureImage)`                                               | ctor     | adopt a coder-built pyramid        |
|  [02]   | `new KtxTexture(TextureFormat, IReadOnlyList<TextureSubresource>, int, int)` | ctor     | layers and faces from subresources |
|  [03]   | `new KtxTexture(TextureFormat, int, int, byte[])`                            | ctor     | one payload, one level             |
|  [04]   | `new KtxTexture(TextureImage, KtxGlFormat? ×4, KtxVkFormat?)`                | ctor     | pin the header tokens explicitly   |
|  [05]   | `KtxTexture.Texture`                                                         | property | the carried `TextureImage`         |
|  [06]   | `KtxTexture.VkFormat` and the four `Gl*` tokens                              | property | parsed header tokens               |
|  [07]   | `new KtxEncodingOptions { … }`                                               | ctor     | container policy                   |
|  [08]   | `RegisterKtxFileFormat(this TextureFileFormatManager) -> IDisposable`        | static   | seat the row on a manager          |
|  [09]   | `KtxFileFormat.ReadTexture(Stream, IFileFormatOptions?) -> ITextureFile`     | instance | the manager-routed read            |
|  [10]   | `KtxFileFormat.WriteTexture(TextureImage, Stream, IFileFormatOptions?)`      | instance | the manager-routed write           |
|  [11]   | `KtxFileFormat.CanRead(ReadOnlySpan<byte>, string?) -> bool`                 | instance | magic-byte and extension claim     |
|  [12]   | `KtxFileFormat.Extensions`                                                   | property | `.ktx` and `.ktx2`                 |

- `KtxEncodingOptions` settable knobs: `Version`, `TextureFormat`, `GenerateMipmaps`, `MipmapOptions`, `SupercompressionScheme`, `ZstandardCompressionLevel`, `ZlibCompressionLevel`, `GlInternalFormat`, `VkFormat`, `IsSrgb`.
- `KtxEncodingOptions.Version` defaults to `KtxVersion.Version1`; an options object left unset writes a KTX1 file no `ktx-parse` or Basis-transcoder consumer can read.
- `KtxEncodingOptions` properties are settable, not init-only, so one instance mutated between writes silently re-versions a later payload — mint one per encode profile.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Two paths reach a file: the static `KtxCodec` for a caller holding a plane or a pyramid, and the `KtxFileFormat` row registered on a `TextureFileFormatManager` for a caller routing by extension. Both write the same bytes; the row exists so `TextureConverter` can reach the container by name.
- `KtxTexture` is the parsed container: it carries the `TextureImage` pyramid beside the header's `KtxVkFormat` and the four `KtxGlFormat` tokens, so a read preserves what the file declared rather than normalizing it away.
- Version is a WRITE decision and defaults to KTX1. `KtxVersion.Version2` is the row every web and glTF consumer needs, and it is the row a wire-bound payload sets explicitly on every encode.
- Supercompression is orthogonal to block format: `KtxSupercompressionScheme.Zstandard` (level on `ZstandardCompressionLevel`) and `Zlib` (`ZlibCompressionLevel`) compress an already-block-encoded payload, while `BasisLz` is the scheme a Basis ETC1S payload carries. `None` writes the blocks raw.
- Block encoding happens BEFORE this surface: a plane reaches `Encode<TPixel>` and the container leg resolves the coder for `KtxEncodingOptions.TextureFormat` off the `TextureCompressor` registry, so the payload class is the format row the caller names, never a container-side choice.
- `EncodeMipChain` writes the pyramid the caller built; a KTX file holds its own mip levels, so a per-mip file series is the wrong shape against this container.
- `KtxCodec.Encode`/`EncodeMipChain`/`Decode<TPixel>` resolve their coders from `TextureCoderManager.Global` INTERNALLY — a bake-scoped manager never reaches a container-codec call, so an options-bearing coder either registers on `Global` (resident for the process) or the caller drives the per-subresource `ITextureCoder` path itself.
- Supercompressed KTX2 declares `vk_format = VK_FORMAT_UNDEFINED` until transcode, so `KtxTexture.VkFormat` reading `KtxVkFormat.Undefined` is the NORMAL state of a wire-legal UASTC or ETC1S file; a reader branching on that token classes every transcodable payload as malformed.

[STACKING]:
- `TextureCompressor`(`.api/api-texturecompressor.md`): the payload engine beneath this container — `ITextureCoder.Encode<TPixel>(BitmapView<TPixel>, Span<byte>)` fills the `TextureSubresource.Payload` a `new KtxTexture(TextureImage)` adopts, and `TextureFormats.RgbaBasisUastcLdr4x4UNorm`/`RgbaBasisEtc1sSrgb` are the two format rows whose blocks a Basis-transcoding consumer accepts; `BitmapMipChain.Generate` builds exactly the `IReadOnlyList<IBitmap<TPixel>>` `EncodeMipChain` takes.
- `TextureCompressor.FileFormats.Hdr`(`.api/api-texturecompressor-fileformats-hdr.md`): the HDR ingest-to-container path — `HdrCodec.Decode(stream) -> ArrayBitmap<Rgba32Float>` lands the float plane that BC6H-encodes into a `KtxEncodingOptions.TextureFormat` of `Bc6HUFloat`, so a Radiance environment source reaches a GPU-ready container without an 8-bit step.
- `SixLabors.ImageSharp`(`.api/api-imagesharp.md`): the container split — a `.ktx2` never routes through the `ImageSharp` `ImageFormatManager`, and a PNG16 or TIFF sidecar never routes here; a plane crosses between the two estates as raw texels in one pooled arena.
- `Silk.NET.WebGPU`(`libs/dotnet/.api/api-silk-webgpu.md`): the `KtxVkFormat` token and the wgpu `TextureFormat` enum row name the same layout, so a parsed container uploads through `QueueWriteTexture` with `TextureDataLayout.BytesPerRow` from `TextureFormat.GetRowByteCount` wherever `AdapterHasFeature` confirms the matching compressed-texture `FeatureName`.
- within-lib: the KTX gate is the ONE composer of this package — it mints a fresh `KtxEncodingOptions` per encode with `Version = KtxVersion.Version2` and the supercompression scheme the channel's payload class demands, registers the format row on the bake-scoped `TextureFileFormatManager`, and holds the provisioned `ktx` CLI arm beside the in-process arm as the encode floor.

[LOCAL_ADMISSION]:
- Every wire-bound encode sets `Version = KtxVersion.Version2` explicitly. KTX1 defaults admit only for a desktop-local payload no consumer transcodes.
- `KtxCodec.Decode`/`DecodeVolume` without a pixel argument returns `Rgba8UNorm` and quantizes; a float or 16-bit container decodes through `Decode<TPixel>` naming its own texel type.
- `KtxEncodingOptions` is minted per encode, never shared across profiles, because its properties are mutable and a carried instance re-versions a later payload silently.
- This package rides the same pre-1.0 hold as its engine: it composes behind the one KTX gate, and the provisioned `ktx` CLI is the encode floor a bump re-verifies against.
