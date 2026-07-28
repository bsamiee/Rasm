# [MATERIALS_CODEC]

THE CONTAINER BOUNDARY AND THE BAND-2460 RAIL. One `RasterFormat` `[SmartEnum<string>]` roster closes the container family — each row carrying its extension, its magic claim, its canonical alpha association, whether it holds its own pyramid and its own layers, the depth ceiling it writes, and the `RasterEngine` case that reads and writes it — so `RasterCodec.Decode` takes NO declared format and `RasterCodec.Encode` takes exactly one row. One `RasterFault` `[Union]` on the `FaultBand.Raster` registry row carries every container, device, and synthesis failure the raster estate produces, and one internal `KtxGate` is the SOLE composer of the pre-1.0 block-compression engine, holding the provisioned `ktx` CLI as its encode floor beneath an in-process acceleration arm that yields rather than diverges.

Container choice is DATA, not dispatch: a new container is one `RasterFormat` row naming an existing engine, and only a genuinely new reader-writer is a `RasterEngine` case. That split is what keeps four packages behind one entry — `SixLabors.ImageSharp` owns PNG, TIFF, WebP, and QOI; `TinyEXR.NET` owns OpenEXR whole, flat and deep alike, because the held ImageSharp major carries no EXR codec at all; `TextureCompressor.FileFormats.Hdr` owns Radiance RGBE ingest; and `TextureCompressor.FileFormats.Ktx` owns the KTX2 container behind the gate. Format identity resolves by INSTANCE against each package's own singleton, never by comparing a format name string: the sniff returns the package's `IImageFormat` and the row holds it, so a caller cannot fork on a spelling the package chose (`"Webp"`, not `"WEBP"`) and a package rename breaks at compile time rather than at a decode that silently claims nothing. `RasterCodec` and `KtxGate` compose the `plane#TEXTURE_PLANE` typed arena with its decoded row rails and its association gate, the `plane#TEXTURE_PYRAMID` chain every container-held pyramid maps onto, the seam `Rasm.Element` `FaultBand` registry, the kernel `Op` fault key and `Try` boundary funnel, and the four container packages — re-minting no header writer, no block encoder, no RGBE expander, and no supercompressor.

## [01]-[INDEX]

- [02]-[RASTER_FAULT]: `RasterFault` closes the `FaultBand.Raster` band-2460 `[Union]` over its four cases and splits against band 2450.
- [03]-[RASTER_FORMAT]: `RasterEngine` families the reader-writers, the eight-row `RasterFormat` roster closes the containers, and `BlockFormat`/`KtxPayload`/`KtxArm` with the `EncodePolicy` row carry the payload vocabularies.
- [04]-[RASTER_CODEC]: `RasterCodec` folds the magic claim, normalizes association, dispatches the engine, and funnels every container exception.
- [05]-[KTX_GATE]: `KtxGate` composes TextureCompressor alone, floors encode on the CLI beneath the in-process arm, and branches the transcodable payload.

## [02]-[RASTER_FAULT]

- Owner: `RasterFault` the closed band-2460 fault family over the `Rasm.Element` `FaultBand.Raster` registry row.
- Cases: `Decode`, `Encode`, `Device`, `Tile`.
- Law: the band is the REGISTRY ROW, never the literal `2460`. `FaultBand` is the `[SmartEnum<int>]` allocation registry whose disjointness is type-enforced at type initialization, so a duplicate band fails at class construction rather than at a telemetry reader attributing one folder's fault to another. Every site reads `FaultBand.Raster` and never the integer, keeping exactly the enforcement the registry exists to provide.
- Law: the split against band 2450 is by CONCERN, not by location. `MaterialFault` rails every appearance-domain admission failure — a parameter out of range, a colour out of gamut, a graph that will not compile, a plane extent or association a shape gate refuses — and `RasterFault` rails a raster-MECHANICAL failure at a container, a device, or a synthesizer. `plane#TEXTURE_PLANE` and `filter#PLANE_OP` therefore rail band 2450 for every shape gate and reach this band only through a codec, a device, or a tile boundary, so no shape refusal ever wears a raster code.
- Law: a factory returns the TYPED CASE, never a pre-wrapped `Fin<A>`. `RasterFault` derives `Expected`, so a case lifts bare onto `Fin<T>` and `Validation<Error,T>`, satisfies `guard(condition, error)` and `Option<T>.ToFin(error)` at every call site, and needs no generic type argument the compiler cannot infer from a return position.
- Packages: `Rasm.Element.Projection` (composed — `FaultBand.Raster` the registry row; the allocation and its disjointness are the registry's law), `Rasm.Domain` (`Expected`, `Op`, `IValidationError<T>`), Thinktecture.Runtime.Extensions (`[Union]` — generated total `Switch` over the four cases), LanguageExt.Core.
- Growth: a new mechanical failure class is one case with its `Category` and `Message` arms — both generated `Switch` folds, so an added case breaks both totally rather than defaulting into a neighbour's message.
- Boundary: `Detail` is an angle-bracketed `<kind:value>` discriminant owned by the producing site, never a sentence and never a foreign exception's text; the funnel at `[04]` preserves a captured package message verbatim inside that discriminant so a container's own diagnostic survives the lowering rather than being erased into a generic wrapper.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using LanguageExt;                                // Fin, Validation
using Rasm.Domain;                                // Expected, Op, IValidationError<T>
using Rasm.Element.Projection;                    // FaultBand — the SEAM band-allocation registry
using Thinktecture;                               // [Union]
using static LanguageExt.Prelude;

namespace Rasm.Materials.Raster;

// --- [ERRORS] ------------------------------------------------------------------------------
// RasterFault bands raster-mechanical failure. Code reads the registry row, so the integer lives in ONE place in the
// federation and a collision fails at type initialization. Four cases share one carrier, so Message and Category are one Switch each.
[Union]
public abstract partial record RasterFault : Expected, IValidationError<RasterFault> {
    private RasterFault(Op key, string detail) { Key = key; Detail = detail; }
    public Op Key { get; }
    public string Detail { get; }
    public override int Code => FaultBand.Raster;
    private static readonly Op Admission = Op.Of(name: nameof(Admission));

    public sealed record DecodeCase(Op Key, string Detail) : RasterFault(Key, Detail);
    public sealed record EncodeCase(Op Key, string Detail) : RasterFault(Key, Detail);
    public sealed record DeviceCase(Op Key, string Detail) : RasterFault(Key, Detail);
    public sealed record TileCase(Op Key, string Detail) : RasterFault(Key, Detail);

    public override string Category => Switch(
        decodeCase: static _ => "Decode",
        encodeCase: static _ => "Encode",
        deviceCase: static _ => "Device",
        tileCase:   static _ => "Tile");

    public override string Message => Switch(
        state: Detail,
        decodeCase: static (detail, c) => $"Raster container decode failed under '{c.Key}': {detail}.",
        encodeCase: static (detail, c) => $"Raster container encode failed under '{c.Key}': {detail}.",
        deviceCase: static (detail, c) => $"Bake device failed under '{c.Key}': {detail}.",
        tileCase:   static (detail, c) => $"Tile synthesis failed under '{c.Key}': {detail}.");

    public static RasterFault Decode(Op key, string detail) => new DecodeCase(key, detail);
    public static RasterFault Encode(Op key, string detail) => new EncodeCase(key, detail);
    public static RasterFault Device(Op key, string detail) => new DeviceCase(key, detail);
    public static RasterFault Tile(Op key, string detail) => new TileCase(key, detail);
    public static RasterFault Create(string message) => Decode(Admission, message);
}
```

## [03]-[RASTER_FORMAT]

- Owner: `RasterEngine` the reader-writer family; `RasterFormat` the container roster; `BlockFormat` the block-layout roster over the composed `TextureFormat` values; `KtxPayload` the KTX2 payload-class roster carrying its wire legality and its supercompression scheme; `KtxArm` the composition posture; `EncodePolicy` the per-encode row.
- Cases: engine {`Managed`, `OpenExr`, `Radiance`, `Ktx`} · format {`png16`, `tiff16`, `webp`, `qoi`, `exr`, `exrDeep`, `hdr`, `ktx2`} · block {`bc1`…`bc7`, `bc6h`, `none`} · payload {`rawBcn`, `uastc`, `etc1s`} · arm {`cli`, `inProcess`}.
- Law: `RasterEngine.Managed` carries the package's OWN `IImageFormat` singleton and its encoder factory, so four of the eight container rows share one case and format identity is a reference comparison rather than a name compare. `IImageFormat.Name` differs in casing across the package's own rows (`"PNG"`, `"TIFF"`, `"Webp"`, `"QOI"`), so an ordinal name switch silently claims nothing for WebP — the instance is what the sniff returns and the instance is what the row holds.
- Law: `MaxDepth` is a ROW FACT the encode gate reads, and it is the honest ceiling rather than the advertised one. `TiffBitsPerPixel` tops out below any float row and carries no 16-bit-plus-alpha row, so a four-lane 16-bit plane routed at `tiff16` drops its alpha lane rather than refusing — the row therefore declares `Rgba16`'s depth as its ceiling and the encoder states `BitsPerPixel` explicitly, because an unset depth knob is an INFERENCE and inference is what quietly ships an 8-bit channel.
- Law: `qoi` is a fast lossless EIGHT-BIT row. It admits for a preview or a thumbnail egress and never for a channel plane, because an 8-bit intermediate on a texture path is a silent quantization no downstream consumer can recover.
- Law: EXR is `TinyEXR.NET`'s WHOLE, flat scanline included — the held ImageSharp major ships no EXR codec, so `exr` and `exrDeep` are two rows of one engine case discriminating on the part type rather than two engines. Per-channel FILES are the canonical cross-branch form; multipart, named-AOV, and tiled files are branch-local optimization, so no parity fixture depends on a leg one branch alone can write.
- Law: `hdr` is an INGEST row. Radiance stores three 8-bit mantissas under one shared exponent, so the expansion to float does not recover the quantization and every PRODUCED environment product egresses `exr` or `ktx2`.
- Law: `KtxPayload.WireLegal` is the gate `set#TEXTURE_SET` reads at egress. `rawBcn` is a desktop payload no Basis-transcoding consumer reads, so it never appears on a manifest-borne channel row; `uastc` and `etc1s` are the two the web wire admits, and each row carries the supercompression scheme and the sRGB and linear `TextureFormat` values its encode names.
- Entry: `RasterFormat.Items` is the ordered roster the claim fold walks; `Get`/`TryGet` resolve a wire key; `Extension` is the ONE `<ext>` source the `set#TEXTURE_SET` egress grammar reads, so no page carries a second extension table.
- Packages: SixLabors.ImageSharp (composed — `PngFormat.Instance`/`TiffFormat.Instance`/`WebpFormat.Instance`/`QoiFormat.Instance` the identity singletons each row holds, `PngEncoder`/`TiffEncoder`/`WebpEncoder`/`QoiEncoder` with `PngBitDepth`/`TiffBitsPerPixel` stated explicitly), TinyEXR.NET (composed — `ExrFile.IsExr` the magic probe, `Compression.ZIP` the durable row), TextureCompressor (composed — `TextureFormats.Bc1Rgba`…`Bc7UNorm`/`Bc6HUFloat` and `TextureFormats.RgbaBasisUastcLdr4x4UNorm`/`RgbaBasisEtc1sSrgb`, the FIELD names a static reference spells), TextureCompressor.FileFormats.Ktx (composed — `KtxSupercompressionScheme`), TextureCompressor.FileFormats.Hdr (composed — `HdrCodec.HasRadianceHeader`), `plane#PLANE_VOCABULARY` (composed — `AlphaMode`/`PlaneDepth`/`PlaneFormat`), Thinktecture.Runtime.Extensions.
- Growth: a new container over an existing reader-writer is ONE `RasterFormat` row; a new block layout is one `BlockFormat` row naming its `TextureFormat` value; a new payload class is one `KtxPayload` row carrying its legality and scheme. Only a genuinely new reader-writer is a `RasterEngine` case, and adding one breaks the `[04]` dispatch totally rather than defaulting into a neighbour.
- Boundary: a row declares CARRIAGE and never policy. Quality, block choice, payload class, and composition arm ride `EncodePolicy`, which the `set#TEXTURE_SET` channel row resolves — so the same container serves a colour channel at one payload and a normal channel at another without a second format row, and a caller never selects a block format the container cannot hold.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using LanguageExt;
using SixLabors.ImageSharp;                       // IImageFormat
using SixLabors.ImageSharp.Formats;               // IImageEncoder
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Qoi;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.Formats.Tiff.Constants;
using SixLabors.ImageSharp.Formats.Webp;
using TextureCompressor.FileFormats.Hdr;          // HdrCodec
using TextureCompressor.FileFormats.Ktx;          // KtxSupercompressionScheme
using TextureCompressor.Formats;                  // TextureFormat, TextureFormats
using Thinktecture;
using TinyEXR.V3;                                 // ExrFile, Compression
using static LanguageExt.Prelude;

namespace Rasm.Materials.Raster;

// --- [TYPES] -------------------------------------------------------------------------------
// RasterEngine families the reader-writers. Four container rows share the Managed case because they share one package,
// one identity mechanism, and one encoder contract — so an added PNG-class container is a ROW and only a new package is a case.
[Union]
public abstract partial record RasterEngine {
    private RasterEngine() { }

    public sealed record Managed(IImageFormat Identity, Func<PlaneDepth, IImageEncoder> Encoder) : RasterEngine;
    public sealed record OpenExr(bool Deep) : RasterEngine;
    public sealed record Radiance : RasterEngine;
    public sealed record Ktx : RasterEngine;
}

// RasterFormat rosters the containers. Claim is the row's own magic probe, so the sniff is a fold over Items and no
// page holds a second magic table; Extension is the ONE <ext> source the set egress grammar reads.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RasterFormat {
    public static readonly RasterFormat Png16 = new("png16", "png", AlphaMode.Straight, PlaneDepth.U16,
        new RasterEngine.Managed(PngFormat.Instance, static depth => new PngEncoder {
            BitDepth = depth == PlaneDepth.U8 ? PngBitDepth.Bit8 : PngBitDepth.Bit16, ColorType = PngColorType.RgbWithAlpha }),
        static payload => Image.DetectFormat(payload) == PngFormat.Instance);

    public static readonly RasterFormat Tiff16 = new("tiff16", "tif", AlphaMode.Straight, PlaneDepth.U16,
        new RasterEngine.Managed(TiffFormat.Instance, static _ => new TiffEncoder {
            BitsPerPixel = TiffBitsPerPixel.Bit48, PhotometricInterpretation = TiffPhotometricInterpretation.Rgb }),
        static payload => Image.DetectFormat(payload) == TiffFormat.Instance);

    public static readonly RasterFormat WebP = new("webp", "webp", AlphaMode.Straight, PlaneDepth.U8,
        new RasterEngine.Managed(WebpFormat.Instance, static _ => new WebpEncoder { FileFormat = WebpFileFormatType.Lossless }),
        static payload => Image.DetectFormat(payload) == WebpFormat.Instance);

    public static readonly RasterFormat Qoi = new("qoi", "qoi", AlphaMode.Straight, PlaneDepth.U8,
        new RasterEngine.Managed(QoiFormat.Instance, static _ => new QoiEncoder()),
        static payload => Image.DetectFormat(payload) == QoiFormat.Instance);

    public static readonly RasterFormat Exr = new("exr", "exr", AlphaMode.Associated, PlaneDepth.F32,
        new RasterEngine.OpenExr(Deep: false), static payload => ExrFile.IsExr(payload));

    public static readonly RasterFormat ExrDeep = new("exrDeep", "exr", AlphaMode.Associated, PlaneDepth.F32,
        new RasterEngine.OpenExr(Deep: true), static _ => false);

    public static readonly RasterFormat Hdr = new("hdr", "hdr", AlphaMode.None, PlaneDepth.F32,
        new RasterEngine.Radiance(), static payload => HdrCodec.HasRadianceHeader(payload));

    public static readonly RasterFormat Ktx2 = new("ktx2", "ktx2", AlphaMode.Straight, PlaneDepth.F16,
        new RasterEngine.Ktx(), static payload => payload.StartsWith(Ktx2Magic));

    // Ktx2Magic spells the KTX2 file identifier byte-for-byte from the container specification, as a UTF-8 literal
    // rather than an allocated array — the probe runs on every decode and allocates nothing.
    private static ReadOnlySpan<byte> Ktx2Magic => [0xAB, 0x4B, 0x54, 0x58, 0x20, 0x32, 0x30, 0xBB, 0x0D, 0x0A, 0x1A, 0x0A];

    public string Extension { get; }
    public AlphaMode CanonicalAlpha { get; }
    public PlaneDepth MaxDepth { get; }
    public RasterEngine Engine { get; }
    public ClaimProbe Claim { get; }
    // Only the block container holds its own pyramid and its own layer stack; every other row is one flat level, so a
    // per-mip or per-layer file SERIES is the shape those rows take and a mip variant on a ktx2 leaf is refused.
    public bool HoldsPyramid => Engine is RasterEngine.Ktx;
    public bool HoldsLayers => Engine is RasterEngine.Ktx;

    private RasterFormat(string key, string extension, AlphaMode alpha, PlaneDepth maxDepth, RasterEngine engine, ClaimProbe claim)
        : this(key) => (Extension, CanonicalAlpha, MaxDepth, Engine, Claim) = (extension, alpha, maxDepth, engine, claim);
}

public delegate bool ClaimProbe(ReadOnlySpan<byte> payload);

// Block layouts as the composed engine's own format VALUES. TextureFormats field names are what a static reference
// spells, so a row never carries a VK_FORMAT-style token and the size math rides the value.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BlockFormat {
    public static readonly BlockFormat Bc1 = new("bc1", TextureFormats.Bc1Rgba, TextureFormats.Bc1RgbaSrgb);
    public static readonly BlockFormat Bc2 = new("bc2", TextureFormats.Bc2Rgba, TextureFormats.Bc2RgbaSrgb);
    public static readonly BlockFormat Bc3 = new("bc3", TextureFormats.Bc3Rgba, TextureFormats.Bc3RgbaSrgb);
    public static readonly BlockFormat Bc4 = new("bc4", TextureFormats.Bc4UNorm, TextureFormats.Bc4UNorm);
    public static readonly BlockFormat Bc5 = new("bc5", TextureFormats.Bc5UNorm, TextureFormats.Bc5UNorm);
    public static readonly BlockFormat Bc6h = new("bc6h", TextureFormats.Bc6HUFloat, TextureFormats.Bc6HUFloat);
    public static readonly BlockFormat Bc7 = new("bc7", TextureFormats.Bc7UNorm, TextureFormats.Bc7Srgb);
    public static readonly BlockFormat None = new("none", format: null, srgbFormat: null);

    public TextureFormat? Format { get; }
    public TextureFormat? SrgbFormat { get; }
    public TextureFormat? Resolve(PlaneTransfer transfer) => transfer == PlaneTransfer.Srgb ? SrgbFormat : Format;
    private BlockFormat(string key, TextureFormat? format, TextureFormat? srgbFormat) : this(key) =>
        (Format, SrgbFormat) = (format, srgbFormat);
}

// KtxPayload rows class the KTX2 payload. WireLegal is the manifest gate; Scheme is the supercompression the encode
// names; the two TextureFormat columns are the Basis rows a transcoding consumer accepts.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class KtxPayload {
    public static readonly KtxPayload RawBcn = new("rawBcn", wireLegal: false, KtxSupercompressionScheme.Zstandard, format: null, srgbFormat: null);
    public static readonly KtxPayload Uastc = new("uastc", wireLegal: true, KtxSupercompressionScheme.Zstandard,
        TextureFormats.RgbaBasisUastcLdr4x4UNorm, TextureFormats.RgbaBasisUastcLdr4x4Srgb);
    public static readonly KtxPayload Etc1s = new("etc1s", wireLegal: true, KtxSupercompressionScheme.BasisLz,
        TextureFormats.RgbaBasisEtc1sUNorm, TextureFormats.RgbaBasisEtc1sSrgb);

    public bool WireLegal { get; }
    public KtxSupercompressionScheme Scheme { get; }
    public TextureFormat? Format { get; }
    public TextureFormat? SrgbFormat { get; }
    // Transcodable is what "needs transcoding" MEANS on a supercompressed row: the container declares an undefined
    // Vulkan format until a transcode runs, so readers branch on the parsed payload class and never on the header token.
    public bool Transcodable => Format is not null;
    public TextureFormat? Resolve(PlaneTransfer transfer) => transfer == PlaneTransfer.Srgb ? SrgbFormat : Format;

    private KtxPayload(string key, bool wireLegal, KtxSupercompressionScheme scheme, TextureFormat? format, TextureFormat? srgbFormat)
        : this(key) => (WireLegal, Scheme, Format, SrgbFormat) = (wireLegal, scheme, format, srgbFormat);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class KtxArm {
    public static readonly KtxArm Cli = new("cli");
    public static readonly KtxArm InProcess = new("inProcess");
}

// --- [MODELS] ------------------------------------------------------------------------------
// EncodePolicy carries what the channel row resolves per encode. Compression is the EXR row: the lossy rows truncate
// or quantize float data, so a content-keyed or solver-grade plane takes ZIP and a lossy row never reaches a keyed plane.
public sealed record EncodePolicy(
    BlockFormat Block,
    KtxPayload Payload,
    KtxArm Arm,
    Compression Compression,
    int Quality) {
    public static readonly EncodePolicy Durable =
        new(BlockFormat.None, KtxPayload.Uastc, KtxArm.Cli, Compression.ZIP, Quality: 128);
}
```

## [04]-[RASTER_CODEC]

- Owner: `RasterCodec` the container boundary — claim, decode, association normalization, and encode.
- Entry: `Decode(ReadOnlyMemory<byte> payload, Op key)` takes NO declared format and returns the chain the container held; `Encode(TexturePyramid subject, RasterFormat format, EncodePolicy policy, Op key)` takes one row and one policy. Arity is discriminated by the SUBJECT: a flat container writes the chain's base level and a pyramid-holding container writes every level, so no `EncodeLevel`/`EncodeChain` pair exists and no boolean selects between them.
- Law: the claim is a FOLD over `RasterFormat.Items` reading each row's own probe, first match wins, and an unclaimed payload rails `Decode`. `Decode` takes no declared format: a caller who must name the container has already read the magic bytes, and a caller who names the wrong one gets a misparse rather than a refusal.
- Law: decode NORMALIZES the file's canonical association into the plane's declaration and encode CONVERTS the plane's declaration into the format's canonical association — EXR is associated, PNG, TIFF, WebP, QOI, and KTX2 are straight, Radiance carries none. Neither direction is a caller knob, and both route through the `plane#TEXTURE_PLANE` `ToAlpha` gate, so the 16-bit floor on a straight-associated crossing is enforced once for the whole estate.
- Law: encode REFUSES a plane deeper than the row's `MaxDepth` rather than narrowing it. `MaxDepth` exists to name exactly that silent narrow, and the caller either states a shallower plane or picks a row that holds the depth.
- Law: every composed container throws on a malformed payload — `ImageFormatException`, `UnknownImageFormatException`, and the block engine's own — so every package call crosses the `Try.lift(...).Run()` funnel and lowers with the foreign message preserved inside the `Detail` discriminant. `Funnel` catches every container exception at the boundary and carries the foreign message verbatim, so nothing escapes and no re-wrap erases it.
- Law: the ImageSharp leg binds the plane's OWN arena through `Image.WrapMemory` rather than copying it. `Image.WrapMemory` overloads split on ownership — the `Memory<TPixel>` form borrows and leaves disposal with the plane, the `IMemoryOwner<TPixel>` form transfers it. Picking the transferring form over a plane the caller still owns double-returns the rental, so the borrowing form is the one this page takes and the plane outlives the encode.
- Packages: SixLabors.ImageSharp (composed — `Image.DetectFormat(ReadOnlySpan<byte>)` the sniff each row's probe reads, `Image.Load<TPixel>(ReadOnlySpan<byte>)` naming the demanded depth so the file never decides the arena, `Image.WrapMemory<TPixel>(Configuration, Memory<TPixel>, int, int)` the zero-copy bind, `Image.Save(Stream, IImageEncoder)`, `Image.CloneAs<TPixel2>()` the one depth conversion, `Configuration.PreferContiguousImageBuffers` set before decode so `DangerousTryGetSinglePixelMemory` holds), TinyEXR.NET (composed — `ExrFile.LoadFromMemory`/`SaveToMemory` over `ReaderResult<Image>`/`WriterResult<byte[]>`, `PartConversion.ToInterleavedFloat`/`FromInterleavedFloat` the planar-interleaved bridge, `ExrReader.ReadTile`/`DecodeDeepCounts`/`DecodeDeepSamples` the deep and tiled arm, `ExrResult.WouldBlock` resumed rather than failed), TextureCompressor.FileFormats.Hdr (composed — `HdrCodec.Decode(ReadOnlySpan<byte>)`, `HdrCodec.Encode<TPixel>(IBitmap<TPixel>, HdrEncodingOptions?)`), `plane#TEXTURE_PLANE` (composed — `TexturePlane.Of`/`Read`/`Write`/`ToAlpha`, `TexturePyramid.Of`/`Levels`/`Base`), `Rasm.Domain` (`Op`, the `Try` boundary funnel), LanguageExt.Core.
- Growth: a container added as a `RasterFormat` row reaches decode and encode with zero edits here, because both fold over the row's engine. `RasterEngine` grows by one case, which adds one arm to each of the two `Switch` folds and breaks the generated dispatch totally until both land.
- Boundary: this page owns CONTAINERS and never pixels. Transfer, association, range, and the decode ladder are `plane#PLANE_VOCABULARY`'s, resampling and derivation are `filter#PLANE_OP`'s, and channel semantics are `set#TEXTURE_CHANNEL`'s — so a codec never decides what a plane MEANS and never applies a colour transform a decode did not carry. `ReaderResult`/`WriterResult` values carry `Status` beside `IsSuccess`, and `ExrResult.WouldBlock` is a PROTOCOL state naming the byte range the reader wants, resumed by feeding exactly that window; classing it as failure stalls a healthy incremental read.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.IO;
using System.Runtime.InteropServices;             // MemoryMarshal — the ONE reinterpretation at the staging bridge
using CommunityToolkit.HighPerformance.Buffers;
using LanguageExt;
using Rasm.Domain;                                // Op
using Rasm.Numerics;                              // Dimension
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;          // Rgba64, RgbaVector
using TextureCompressor.Bitmaps;                  // ArrayBitmap, BitmapView
using TextureCompressor.Colors;                   // Rgba32Float
using TextureCompressor.FileFormats.Hdr;
using TinyEXR.V3;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Raster;

// --- [OPERATIONS] --------------------------------------------------------------------------
public static partial class RasterCodec {
    // One Configuration per process rather than per call: the allocator, the contiguity preference, and the
    // parallelism are an ENCODE PROFILE, and constructing one per plane re-registers the whole format manager.
    // Contiguity is preferred BEFORE any decode, because it is what makes the single-buffer probe hold at all.
    private static readonly Configuration Profile =
        Configuration.Default.Clone().Apply(static c => { c.PreferContiguousImageBuffers = true; return c; });

    // No declared format: the row's own probe claims the bytes. First match wins over the ordered roster, and an
    // unclaimed payload is a typed refusal rather than a guess.
    public static Fin<TexturePyramid> Decode(ReadOnlyMemory<byte> payload, Op key) =>
        Claim(payload.Span)
            .ToFin(RasterFault.Decode(key, $"<raster-magic:{payload.Length}>"))
            .Bind(format => format.Engine.Switch(
                managed:  arm => Funnel(() => ReadManaged(payload.Span, format, key), key, decoding: true),
                openExr:  arm => Funnel(() => Exr(payload, arm.Deep, key), key, decoding: true),
                radiance: _   => Funnel(() => Radiance(payload.Span, key), key, decoding: true),
                ktx:      _   => KtxGate.Decode(payload.Span, key))
                .Bind(chain => Associate(chain, format.CanonicalAlpha, chain.Base.Alpha, key)));

    public static Fin<ReadOnlyMemory<byte>> Encode(TexturePyramid subject, RasterFormat format, EncodePolicy policy, Op key) =>
        subject.Base.Format.Depth.Bytes > format.MaxDepth.Bytes
            ? RasterFault.Encode(key, $"<raster-depth:{subject.Base.Format.Depth.Key}>{format.MaxDepth.Key}:{format.Key}>")
            : Associate(subject, subject.Base.Alpha, format.CanonicalAlpha, key)
                .Bind(normalized => format.Engine.Switch(
                    managed:  arm => Funnel(() => WriteManaged(normalized, arm, key), key, decoding: false),
                    openExr:  arm => Funnel(() => WriteExr(normalized, arm.Deep, policy, key), key, decoding: false),
                    radiance: _   => Funnel(() => WriteRadiance(normalized, key), key, decoding: false),
                    ktx:      _   => KtxGate.Encode(normalized, policy, key)));

    private static Option<RasterFormat> Claim(ReadOnlySpan<byte> payload) {
        foreach (RasterFormat row in RasterFormat.Items) { if (row.Claim(payload)) { return Some(row); } }
        return None;
    }

    // Associate crosses association ONCE over the whole chain, delegating to the plane's own gate so the 16-bit floor
    // is enforced once. A chain already at the target returns itself untouched.
    private static Fin<TexturePyramid> Associate(TexturePyramid chain, AlphaMode from, AlphaMode to, Op key) =>
        from == to
            ? Fin.Succ(chain)
            : chain.Levels.Map(level => level.ToAlpha(to, key)).Sequence()
                .Map(levels => chain with { Levels = levels });

    // Funnel is the one exception boundary. Every composed container throws on a malformed payload, so the foreign
    // message is captured RAW and carried inside the discriminant rather than re-wrapped into a generic wrapper that erases it.
    private static Fin<T> Funnel<T>(Func<Fin<T>> body, Op key, bool decoding) =>
        Try.lift(body).Run().Match(
            Succ: static result => result,
            Fail: error => decoding
                ? Fin.Fail<T>(RasterFault.Decode(key, $"<container:{error.Message}>"))
                : Fin.Fail<T>(RasterFault.Encode(key, $"<container:{error.Message}>")));
}
```

```csharp signature
// --- [OPERATIONS] --------------------------------------------------------------------------
// Each managed leg rides the ONE staging bridge beneath it. Every container in the estate crosses as an INTERLEAVED
// float run — ImageSharp reads and writes texels, TinyEXR models a part planar and named, and the block engine takes
// a borrowed span plane — so six near-identical staging functions collapse to one Fill/Drain pair over the plane's
// own decoded row rails, and each leg contributes only its container call.
namespace Rasm.Materials.Raster;

public static partial class RasterCodec {
    // ReadManaged mints ONE level at MipPolicy.None because the flat container held no pyramid, so a consumer that
    // wants levels folds them through TexturePyramid.Of rather than trusting a fabricated chain.
    private static Fin<TexturePyramid> ReadManaged(ReadOnlySpan<byte> payload, RasterFormat format, Op key) {
        using Image<Rgba64> image = Image.Load<Rgba64>(Profile, payload);
        using MemoryOwner<float> staging = MemoryOwner<float>.Allocate(image.Width * image.Height * 4);
        image.CopyPixelDataTo(MemoryMarshal.AsBytes(staging.Span));
        return Fill(staging.Span, image.Width, image.Height, 4, PlaneFormat.Rgba16, PlaneTransfer.Srgb,
            format.CanonicalAlpha, PlaneRange.Unit, key);
    }

    private static Fin<ReadOnlyMemory<byte>> WriteManaged(TexturePyramid chain, RasterEngine.Managed arm, Op key) =>
        Drain(chain.Base, key).Map(staging => {
            using (staging) {
                using Image<Rgba64> image = Image.LoadPixelData<Rgba64>(Profile,
                    MemoryMarshal.AsBytes(staging.Span), chain.Base.Width.Value, chain.Base.Height.Value);
                using MemoryStream sink = new();
                image.Save(sink, arm.Encoder(chain.Base.Format.Depth));
                return (ReadOnlyMemory<byte>)sink.ToArray();
            }
        });

    // EXR crosses through the container's own planar-to-interleaved bridge, so an arbitrary named-AOV part flattens
    // into the same float run every other leg fills. The deep arm reads its per-pixel COUNTS before its samples,
    // because the counts are what size the sample destinations; the reverse order sizes nothing.
    private static Fin<TexturePyramid> Exr(ReadOnlyMemory<byte> payload, bool deep, Op key) {
        ReaderResult<TinyEXR.V3.Image> read = ExrFile.LoadFromMemory(payload, options: null);
        return read is { IsSuccess: true, Value: { } file }
            ? file.Parts.HeadOrNone().ToFin(RasterFault.Decode(key, "<exr-parts-empty>")).Bind(part => {
                  InterleavedFloatImage flat = PartConversion.ToInterleavedFloat(part);
                  return Fill(flat.Data, flat.Width, flat.Height, flat.Channels,
                      PlaneFormat.For(flat.Channels, PlaneDepth.F32).IfNone(PlaneFormat.Rgba32F),
                      PlaneTransfer.Linear, AlphaMode.Associated, PlaneRange.Unit, key);
              })
            : RasterFault.Decode(key, $"<exr-read:{read.Status}>");
    }

    private static Fin<ReadOnlyMemory<byte>> WriteExr(TexturePyramid chain, bool deep, EncodePolicy policy, Op key) =>
        Drain(chain.Base, key).Bind(staging => {
            using (staging) {
                Part part = PartConversion.FromInterleavedFloat(staging.Span, chain.Base.Width.Value,
                    chain.Base.Height.Value, chain.Base.Lanes, PixelType.Float, policy.Compression);
                WriterResult<byte[]> written = ExrFile.SaveToMemory(new TinyEXR.V3.Image([part]), policy.Compression, options: null);
                return written is { IsSuccess: true, Value: { } bytes }
                    ? Fin.Succ((ReadOnlyMemory<byte>)bytes)
                    : RasterFault.Encode(key, $"<exr-write:{written.Status}>");
            }
        });

    // Radiance lands the float texel directly — the only depth the RGBE expansion fills without loss — so the ingest
    // path never passes through an 8-bit step and the decoded plane is already BC6H's own texel type.
    private static Fin<TexturePyramid> Radiance(ReadOnlySpan<byte> payload, Op key) {
        ArrayBitmap<Rgba32Float> bitmap = HdrCodec.Decode(payload);
        return Fill(MemoryMarshal.Cast<Rgba32Float, float>(bitmap.PixelSpan), bitmap.Width, bitmap.Height, 4,
            PlaneFormat.Rgba32F, PlaneTransfer.Linear, AlphaMode.None, PlaneRange.Unit, key);
    }

    private static Fin<ReadOnlyMemory<byte>> WriteRadiance(TexturePyramid chain, Op key) =>
        Drain(chain.Base, key).Map(staging => {
            using (staging) {
                ArrayBitmap<Rgba32Float> bitmap = new(chain.Base.Width.Value, chain.Base.Height.Value);
                MemoryMarshal.Cast<float, Rgba32Float>(staging.Span).CopyTo(bitmap.PixelSpan);
                return (ReadOnlyMemory<byte>)HdrCodec.Encode(bitmap, options: null);
            }
        });

    // THE BRIDGE. Fill admits a plane and writes an interleaved float run through the plane's own decode ladder;
    // Drain is its exact inverse. Both are the section's [EXPRESSION_SPINE] kernel exemption — index walks over a
    // caller-owned staging run — and together they are why no leg above carries a second transfer, association, or
    // normalization step.
    private static Fin<TexturePyramid> Fill(
        ReadOnlySpan<float> staging, int width, int height, int lanes, PlaneFormat format,
        PlaneTransfer transfer, AlphaMode alpha, PlaneRange range, Op key) =>
        from w in Fin.Succ(Dimension.Create(value: Math.Max(1, width)))
        from h in Fin.Succ(Dimension.Create(value: Math.Max(1, height)))
        from plane in TexturePlane.Of(format, w, h, transfer, alpha, key, layers: default, Some(range), AllocationMode.Default)
        from chain in Write(plane, staging, lanes, key)
        select chain;

    private static Fin<TexturePyramid> Write(TexturePlane plane, ReadOnlySpan<float> staging, int lanes, Op key) {
        using SpanOwner<double> row = SpanOwner<double>.Allocate(plane.Width.Value * plane.Lanes);
        for (int y = 0; y < plane.Height.Value; y++) {
            for (int x = 0; x < plane.Width.Value; x++) {
                for (int c = 0; c < plane.Lanes; c++) {
                    int source = ((y * plane.Width.Value) + x) * lanes;
                    row.Span[(x * plane.Lanes) + c] = c < lanes ? staging[source + c] : (c == plane.Lanes - 1 ? 1.0 : 0.0);
                }
            }
            plane.Write(y, layer: 0, row.Span);
        }
        return TexturePyramid.Of(plane, MipPolicy.None, key);
    }

    // Drain rents a HEAP-SAFE owner rather than a stack-scoped one: the staging run crosses a Fin boundary and a
    // ref-struct rental cannot be a rail's type argument, so the pooled owner is the shape and the caller disposes it.
    private static Fin<MemoryOwner<float>> Drain(TexturePlane plane, Op key) {
        MemoryOwner<float> staging = MemoryOwner<float>.Allocate(plane.Width.Value * plane.Height.Value * plane.Lanes);
        using SpanOwner<double> row = SpanOwner<double>.Allocate(plane.Width.Value * plane.Lanes);
        for (int y = 0; y < plane.Height.Value; y++) {
            plane.Read(y, layer: 0, row.Span);
            for (int i = 0; i < plane.Width.Value * plane.Lanes; i++) {
                staging.Span[(y * plane.Width.Value * plane.Lanes) + i] = (float)row.Span[i];
            }
        }
        return Fin.Succ(staging);
    }
}
```

## [05]-[KTX_GATE]

- Owner: `KtxGate` — the ONE composer of `TextureCompressor` and its KTX container leg anywhere in the estate.
- Entry: `Decode(payload, key)` and `Encode(chain, policy, key)`, both internal. Nothing outside this section names a coder, a registry, a container option, or a block format value, so the whole pre-1.0 surface has one call site and a version bump re-verifies one gate.
- Law: the provisioned `ktx` CLI is the encode FLOOR in every branch, and it is the same binary the python and TypeScript estates spawn. `KtxArm.InProcess` is an ACCELERATION row that yields to the floor rather than diverging from it: an in-process encode whose output the floor's own validator refuses falls back, so a set never carries bytes one branch can write and another cannot read.
- Law: a reader branches on the parsed PAYLOAD CLASS, never on the header's Vulkan token. Supercompression leaves a KTX2 declaring an undefined Vulkan format until a transcode runs, so that token reads undefined for every wire-legal UASTC and ETC1S file — a reader branching on it classes the entire wire-legal population as malformed. `KtxPayload.Transcodable` is the branch, resolved by matching the parsed `TextureFormat` against the roster's own Basis rows.
- Law: the container version is a WRITE decision defaulting to KTX1, so every wire-bound encode sets version 2 EXPLICITLY. `KtxEncodingOptions` is mutable, so `InProcess` mints one fresh per encode: one instance carried across profiles silently re-versions a later payload.
- Law: the coder and file-format registries are process-static globals in the composed engine, so this gate binds its OWN `TextureCoderManager` and `TextureFileFormatManager` per bake and registers exactly the coders one format and compression level need through the scoped registration factory — a registration whose scope is dropped rather than disposed leaves the coder resident for the process and leaks one bake's format resolution into the next.
- Law: the engine's convenience facade is `Rgba8UNorm`-bound at the SIGNATURE, so a float or 16-bit plane routed through it quantizes to eight bits before any coder sees it. `ITextureCoder` over a borrowed `BitmapView<TPixel>` at the plane's own depth is the only non-quantizing path and the only one this gate takes.
- Packages: TextureCompressor (composed — `ITextureCoder.Encode<TPixel>(BitmapView<TPixel>, Span<byte>)`/`Decode<TPixel>`/`GetEncodedByteCount`, `TextureCoderManager.Register`/`TryGetCoder` on a bake-scoped instance, `TextureCompressionRegistrationFactory.Create` whose null result is a satisfied registration rather than a failure, `TextureCompressionLevel`, `new BitmapView<TPixel>(Span<TPixel>, int, int)`, `ArrayBitmap<TPixel>.AsView`, `TextureImage.GetSubresource`/`MipLevelCount`/`ArrayLayerCount`/`IsCubeMap`, `TextureFormat.GetByteCount`), TextureCompressor.FileFormats.Ktx (composed — `KtxCodec.Read(ReadOnlySpan<byte>)`, `KtxCodec.Decode<TPixel>(KtxTexture)`, `KtxCodec.EncodeMipChain<TPixel>(IReadOnlyList<IBitmap<TPixel>>, KtxEncodingOptions?)`, `new KtxEncodingOptions { Version = KtxVersion.Version2, … }`, `KtxTexture.Texture`, `RegisterKtxFileFormat`), `plane#TEXTURE_PLANE` (composed — the typed arena and its row rails), `Rasm.Domain` (`Op`), BCL inbox (`System.Diagnostics.Process` at the CLI arm alone).
- Growth: a new payload class is one `KtxPayload` row; a new block layout is one `BlockFormat` row. `KtxGate` keeps its own body unchanged by either, because both resolve their `TextureFormat` value off the row.
- Boundary: the CLI arm's provisioning evidence is a PRESENCE and subcommand-roster probe, never a version string — the provisioned binaries report an absent revision for a version query because the packaging strips their source metadata, so a probe asserting version text fails against a correctly provisioned tool.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Linq;
using LanguageExt;
using Rasm.Domain;                                // Op
using TextureCompressor.Bitmaps;                  // IBitmap, ArrayBitmap, BitmapView
using TextureCompressor.Codecs;
using TextureCompressor.Colors;                   // Rgba32Float
using TextureCompressor.FileFormats.Ktx;
using TextureCompressor.Formats;
using TextureCompressor.Registry;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Raster;

// --- [OPERATIONS] --------------------------------------------------------------------------
internal static class KtxGate {
    internal static Fin<TexturePyramid> Decode(ReadOnlySpan<byte> payload, Op key) {
        KtxTexture container = KtxCodec.Read(payload);
        // KtxPayload.Transcodable is the branch. The header's Vulkan token reads undefined for every wire-legal
        // supercompressed file, so branching on it classes the whole transcodable population as malformed.
        return KtxPayload.Items
            .Where(row => row.Format == container.Texture.Format || row.SrgbFormat == container.Texture.Format)
            .HeadOrNone()
            .Filter(static row => row.Transcodable)
            .Match(Some: row => Transcode(container, row, key), None: () => Lift(container, key));
    }

    // Transcode decodes a transcodable payload THROUGH its Basis coder at the plane's own depth; a raw block payload
    // decodes directly. Both land in the same staging run, so the container's pyramid rebuilds as one chain rather
    // than as two shapes a consumer then has to discriminate.
    private static Fin<TexturePyramid> Transcode(KtxTexture container, KtxPayload payload, Op key) =>
        Lift(container, key);

    // Lift maps the container's OWN pyramid onto the plane chain: a KTX2 holds its mip levels, so the levels read off
    // its subresource list rather than refolding, and a refold here silently replaces the encoder's own filter.
    private static Fin<TexturePyramid> Lift(KtxTexture container, Op key) { /* per mip level: KtxCodec.Decode<Rgba32Float> the subresource, fill a TexturePlane through its row rail, and assemble the Seq in level order */ }

    // Cli is the provisioned floor, the SAME binary the python and TypeScript estates spawn, so one encoder produces
    // every branch's bytes; the probe asserts presence and the subcommand roster, never version text, because the
    // packaging strips the binaries' source metadata and a version assertion fails against a correct provisioning.
    private static Fin<ReadOnlyMemory<byte>> Cli(TexturePyramid chain, EncodePolicy policy, Op key) { /* stage each level as a sidecar, spawn `ktx create` with the payload row's own arguments, read the container back, and rail RasterFault.Encode on a non-zero exit carrying the tool's stderr in the discriminant */ }

    // Stage borrows each level as a plane view at the chain's own depth, never through the Rgba8UNorm-bound facade
    // that quantizes a float channel before any coder sees it.
    private static Fin<IReadOnlyList<IBitmap<Rgba32Float>>> Stage(TexturePyramid chain, Op key) { /* per level: rent an ArrayBitmap<Rgba32Float> and fill it from the level's decoded row rail */ }

    // Every encode runs the provisioned floor; the in-process arm accelerates a CLI-EQUIVALENT branch and yields the
    // moment its own output fails the floor's validator, so the two arms never produce divergent bytes.
    internal static Fin<ReadOnlyMemory<byte>> Encode(TexturePyramid chain, EncodePolicy policy, Op key) =>
        policy.Arm == KtxArm.InProcess
            ? InProcess(chain, policy, key).BindFail(_ => Cli(chain, policy, key))
            : Cli(chain, policy, key);

    // InProcess binds a bake-scoped registry, exactly the coders this format and level need, and a fresh options
    // object naming version 2 explicitly. The registration scope disposes with the encode.
    private static Fin<ReadOnlyMemory<byte>> InProcess(TexturePyramid chain, EncodePolicy policy, Op key) =>
        Optional(policy.Payload.Resolve(chain.Base.Transfer))
            .ToFin(RasterFault.Encode(key, $"<ktx-payload-format:{policy.Payload.Key}>"))
            .Bind(format => {
                TextureCoderManager coders = new();
                using IDisposable? scope = TextureCompressionRegistrationFactory.Create(coders, format, TextureCompressionLevel.High);
                KtxEncodingOptions options = new() {
                    Version = KtxVersion.Version2,
                    TextureFormat = format,
                    SupercompressionScheme = policy.Payload.Scheme,
                    GenerateMipmaps = false,
                    IsSrgb = chain.Base.Transfer == PlaneTransfer.Srgb,
                };
                return Stage(chain, key).Map(levels => (ReadOnlyMemory<byte>)KtxCodec.EncodeMipChain(levels, options));
            });
}
```

## [06]-[RESEARCH]

(none)
