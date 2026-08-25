# [RASM_MATERIALS_API_MAGICK]

`Magick.NET-Q16-HDRI-AnyCPU` is the ingest-only container breadth tier: an ImageMagick binding whose statically-linked native delegates reach the vendor and archive containers no admitted managed engine decodes — AVIF, HEIF/HEIC, JPEG XL, DPX, and Cineon — behind the one `Raster/codec#RASTER_CODEC` `RasterEngine.Breadth` row. The Q16-HDRI build carries a float quantum managed-side, so a decode drains to the plane arena's float rail with no depth loss. Encode never routes here: the breadth case carries no encoder column, and every authored product egresses a managed, EXR, or KTX2 leg.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the decode path — `ImageMagick`

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY] | [CAPABILITY]                                                          |
| :-----: | :------------------------------------ | :------------ | :-------------------------------------------------------------------- |
|  [01]   | `MagickImage`                         | sealed class  | one decoded frame; `Width`/`Height`/`ChannelCount`/`Depth` are `uint` |
|  [02]   | `MagickFormatInfo`                    | sealed class  | per-format identity, `IsReadable`/`IsWritable` verdicts               |
|  [03]   | `IPixelCollection<float>`             | interface     | the Q16-HDRI float pixel window                                       |
|  [04]   | `MagickFormat`                        | enum          | the 276-row format vocabulary                                         |
|  [05]   | `MagickReadSettings`                  | class         | decode bounds, frame, and density knobs                               |
|  [06]   | `Quantum`                             | static class  | `Quantum.Max` = `65535f` on this build                                |
|  [07]   | `MagickMissingDelegateErrorException` | exception     | the unlinked-delegate refusal an unproved format throws               |

## [02]-[ENTRYPOINTS]

| [INDEX] | [SURFACE]                                                             | [SHAPE]  | [CAPABILITY]                        |
| :-----: | :-------------------------------------------------------------------- | :------- | :---------------------------------- |
|  [01]   | `new MagickImage(ReadOnlySpan<byte>)`                                 | ctor     | decode from sniffed bytes           |
|  [02]   | `MagickFormatInfo.Create(ReadOnlySpan<byte>) -> IMagickFormatInfo?`   | static   | span sniff to format identity       |
|  [03]   | `image.GetPixels() -> IPixelCollection<float>`                        | instance | open the float pixel window         |
|  [04]   | `pixels.GetReadOnlyArea(int, int, uint, uint) -> ReadOnlySpan<float>` | instance | arity-honest float drain per region |

## [03]-[IMPLEMENTATION_LAW]

- The native delegate roster is a MEASURED fact per RID, never the package description: the osx-arm64 dylib statically links `bzlib cairo fontconfig freetype heic jng jp2 jpeg jxl lcms lqr lzma openexr pangocairo png raqm raw rsvg tiff webp xml zip zlib`. AVIF and JPEG XL round-trip both directions; HEIC/HEIF decodes read-only (`IsWritable` false, encode throws `MagickMissingDelegateErrorException`); DPX and Cineon round-trip; NO KTX coder exists in the build, so the block container stays `KtxGate`'s.
- `GetReadOnlyArea` returns the region's channels interleaved at the image's own `ChannelCount` — the drain reads that arity rather than assuming four lanes, and the Q16-HDRI quantum is float with `Quantum.Max = 65535f`, so a unit-range plane divides by `Quantum.Max` at the drain.
- One composing site: `Raster/codec#RASTER_CODEC` `RasterEngine.Breadth` is this package's sole seam — the row is reached only where the sniffed container matches no managed row, and no other page spells an `ImageMagick` member. A second composing site re-opens the single-seam pin before it lands.
