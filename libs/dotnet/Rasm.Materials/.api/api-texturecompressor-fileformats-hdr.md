# [RASM_MATERIALS_API_TEXTURECOMPRESSOR_FILEFORMATS_HDR]

`TextureCompressor.FileFormats.Hdr` is the managed Radiance `.hdr` leg: `HdrCodec` decodes the RGBE scanline format straight into an `ArrayBitmap<Rgba32Float>` and encodes any `IPixel` plane back out under run-length encoding. It closes the one HDR-environment container the raster estate otherwise cannot read, and its float output is the exact texel type BC6H encodes from.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `TextureCompressor.FileFormats.Hdr`
- package: `TextureCompressor.FileFormats.Hdr` (MIT)
- assembly: `TextureCompressor.FileFormats.Hdr`
- namespace: `TextureCompressor.FileFormats.Hdr`
- asset: `lib/net10.0` managed only
- depends: `TextureCompressor`
- rail: hdr image container

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: codec, format row, and options

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CAPABILITY]                                          |
| :-----: | :-------------------------- | :------------ | :---------------------------------------------------- |
|  [01]   | `HdrCodec`                  | static class  | header probe, typed decode, generic encode            |
|  [02]   | `HdrFileFormat`             | sealed class  | the `IImageFileFormat` row for `.hdr`                 |
|  [03]   | `HdrFileFormatRegistration` | static class  | the `TextureFileFormatManager` registration extension |
|  [04]   | `HdrEncodingOptions`        | sealed class  | `UseRunLengthEncoding`, defaulting true               |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: probe and decode — `HdrCodec`

| [INDEX] | [SURFACE]                                                         | [SHAPE] | [CAPABILITY]                        |
| :-----: | :---------------------------------------------------------------- | :------ | :---------------------------------- |
|  [01]   | `HdrCodec.HasRadianceHeader(ReadOnlySpan<byte>) -> bool`          | static  | magic-signature probe               |
|  [02]   | `HdrCodec.Decode(Stream) -> ArrayBitmap<Rgba32Float>`             | static  | RGBE to float32, the default decode |
|  [03]   | `HdrCodec.Decode(ReadOnlySpan<byte>) -> ArrayBitmap<Rgba32Float>` | static  | decode from memory                  |
|  [04]   | `HdrCodec.Decode(string) -> ArrayBitmap<Rgba32Float>`             | static  | decode from a path                  |
|  [05]   | `HdrCodec.Decode<TPixel>(Stream) -> ArrayBitmap<TPixel>`          | static  | decode into a demanded texel type   |
|  [06]   | `HdrCodec.DecodeRgba32Float(Stream) -> ArrayBitmap<Rgba32Float>`  | static  | the explicitly named float overload |
|  [07]   | `HdrCodec.DecodeRgba8(Stream) -> ArrayBitmap<Rgba8UNorm>`         | static  | the clamped 8-bit preview overload  |

[ENTRYPOINT_SCOPE]: encode — `HdrCodec`

| [INDEX] | [SURFACE]                                                                  | [SHAPE] | [CAPABILITY]            |
| :-----: | :------------------------------------------------------------------------- | :------ | :---------------------- |
|  [01]   | `HdrCodec.Encode<TPixel>(BitmapView<TPixel>, Stream, HdrEncodingOptions?)` | static  | encode a borrowed plane |
|  [02]   | `HdrCodec.Encode<TPixel>(IBitmap<TPixel>, Stream, HdrEncodingOptions?)`    | static  | encode an owned plane   |
|  [03]   | `HdrCodec.Encode<TPixel>(IBitmap<TPixel>, HdrEncodingOptions?) -> byte[]`  | static  | encode to memory        |
|  [04]   | `HdrCodec.Encode<TPixel>(IBitmap<TPixel>, string, HdrEncodingOptions?)`    | static  | encode to a path        |

[ENTRYPOINT_SCOPE]: format row and registration

| [INDEX] | [SURFACE]                                                                             | [SHAPE]  | [CAPABILITY]                  |
| :-----: | :------------------------------------------------------------------------------------ | :------- | :---------------------------- |
|  [01]   | `RegisterHdrFileFormat(this TextureFileFormatManager) -> IDisposable`                 | static   | seat the row on a manager     |
|  [02]   | `HdrFileFormat.ReadImage<TPixel>(Stream, IFileFormatOptions?) -> ArrayBitmap<TPixel>` | instance | the manager-routed read       |
|  [03]   | `HdrFileFormat.WriteImage<TPixel>(IBitmap<TPixel>, Stream, IFileFormatOptions?)`      | instance | the manager-routed write      |
|  [04]   | `HdrFileFormat.CanRead(ReadOnlySpan<byte>, string?) -> bool`                          | instance | signature and extension claim |
|  [05]   | `HdrFileFormat.Name` / `HdrFileFormat.Extensions`                                     | property | `Radiance HDR` and `.hdr`     |
|  [06]   | `new HdrEncodingOptions { UseRunLengthEncoding = true }`                              | ctor     | RLE on or flat scanlines      |

- `HdrEncodingOptions.UseRunLengthEncoding` is settable and defaults true; RLE off writes a valid but materially larger file and exists for a reader that mishandles the run form.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Radiance `.hdr` stores RGBE — three 8-bit mantissas under one shared 8-bit exponent — so the format carries wide dynamic range at low precision. Decode expands to `Rgba32Float` and the mantissa quantization does not recover; a `.hdr` source is an INGEST format, never a texture-grade intermediate.
- Radiance is flat and single-level: no mips, no tiles, no layers, no alpha, no named channels. Pyramids, cube face sets, and prefiltered specular chains are different containers, and this codec contributes the base level alone.
- `Decode` without a pixel argument lands `Rgba32Float`, which is the correct default because that is the only depth the RGBE expansion fills without loss; `DecodeRgba8` clamps and quantizes and serves a preview alone.
- Encode is generic over `IPixel<TSelf>` and RLE-on by default, so a float environment plane round-trips out as a compact `.hdr` with no depth argument.
- Its `IImageFileFormat` row reads and writes a FLAT plane, unlike the KTX `ITextureFileFormat` peer that carries a whole `TextureImage`; registering both on one manager gives extension routing across the image and container halves of the estate.

[STACKING]:
- `TextureCompressor`(`.api/api-texturecompressor.md`): `HdrCodec.Decode(stream) -> ArrayBitmap<Rgba32Float>` yields exactly the texel type `BptcTextureCoder`'s BC6H arm encodes from, so an ingested environment reaches `TextureFormats.Bc6HUFloat` blocks with no intermediate conversion, and `ArrayBitmap<Rgba32Float>.AsView()` is the `BitmapView<TPixel>` `ITextureCoder.Encode<TPixel>` takes.
- `TextureCompressor.FileFormats.Ktx`(`.api/api-texturecompressor-fileformats-ktx.md`): the ingest-to-wire path — a decoded `.hdr` plane block-encodes and writes as a KTX2 container under `KtxEncodingOptions { Version = KtxVersion.Version2, TextureFormat = TextureFormats.Bc6HUFloat }`, so the environment estate produces one GPU-ready file from one Radiance source.
- `TinyEXR.NET`(`.api/api-tinyexr.md`): the two HDR containers split by precision — this surface owns the low-precision RGBE ingest form and that peer owns every half and float EXR egress form, so an ingested `.hdr` decodes once and re-emits through `ExrFile.SaveToStream(image, stream, Compression.ZIP)` for any downstream needing real float precision, crossing as raw texels in one pooled arena.
- `Wacton.Unicolour`(`libs/dotnet/.api/api-unicolour.md`): a decoded `Rgba32Float` texel is scene-linear with no declared primaries, so any working-space or white-point reconciliation runs on the `Unicolour` owner before the plane admits to the appearance rail; this codec declares no color management of its own.
- within-lib: the environment ingest fold registers this row on the bake-scoped `TextureFileFormatManager` beside the KTX row, decodes to `Rgba32Float`, and admits the plane only after the 2:1 equirect extent gate — the codec reports extent, never validates it.

[LOCAL_ADMISSION]:
- `.hdr` is admitted as an INGEST container. Every produced environment product — equirect, irradiance, prefiltered specular, BRDF LUT — egresses as EXR or KTX2, because RGBE cannot carry the precision a prefiltered chain accumulates.
- `DecodeRgba8` is preview-only; an environment plane decodes through the float overload.
- Radiance carries no color-space declaration, so a decoded plane's primaries are the caller's to assert against the working space, never inferred from the file.
- This package rides the same pre-1.0 hold as its engine: it composes behind the one raster codec gate, and a version bump re-verifies that gate's members before it lands.

[RAIL_LAW]:
- Package: `TextureCompressor.FileFormats.Hdr`
- Owns: the managed Radiance `.hdr` RGBE container — signature probe, scanline decode into any `IPixel` texel type with `Rgba32Float` as the lossless default, and run-length-encoded generic encode.
- Accept: `HasRadianceHeader` as the sniff; `Decode`/`DecodeRgba32Float` for every environment ingest; `Encode<TPixel>` over a `BitmapView`/`IBitmap` plane; `RegisterHdrFileFormat` on a bake-scoped manager; the decoded float plane handed straight to BC6H block encoding.
- Reject: `.hdr` as an egress format for a produced environment product; `DecodeRgba8` on an ingest path; an assumed color space on a decoded plane; a mip, cube, or layer expectation against a flat single-level container; a hand-rolled RGBE expander.
