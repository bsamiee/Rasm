# [MATERIALS_CODEC]

THE CONTAINER BOUNDARY AND THE BAND-2460 RAIL. One `RasterFormat` `[SmartEnum<string>]` roster closes the container family — each row carrying its extension, its magic claim, its canonical alpha association, whether it holds its own pyramid and its own layers, the depth ceiling it writes, and the `RasterEngine` case that reads and writes it — so `RasterCodec.Decode` takes NO declared format and `RasterCodec.Encode` takes exactly one row. One `RasterFault` `[Union]` on the `FaultBand.Raster` registry row carries every container, device, and synthesis failure the raster estate produces, and one internal `KtxGate` is the SOLE composer of the pre-1.0 block-compression engine, holding the provisioned `ktx` CLI as its encode floor beneath an in-process acceleration arm that yields rather than diverges.

Container choice is DATA, not dispatch: a new container is one `RasterFormat` row naming an existing engine, and only a genuinely new reader-writer is a `RasterEngine` case. That split is what keeps four packages behind one entry — `SixLabors.ImageSharp` owns PNG, TIFF, WebP, QOI, and the JPEG ingest row; `TinyEXR.NET` owns OpenEXR whole — flat, tiled-and-mip-levelled, and deep alike, because the held ImageSharp major carries no EXR codec at all; `TextureCompressor.FileFormats.Hdr` owns Radiance RGBE ingest; and `TextureCompressor.FileFormats.Ktx` owns the KTX2 container behind the gate. Format identity resolves by INSTANCE against each package's own singleton, never by comparing a format name string: the sniff returns the package's `IImageFormat` and the row holds it, so a caller cannot fork on a spelling the package chose (`"Webp"`, not `"WEBP"`) and a package rename breaks at compile time rather than at a decode that silently claims nothing. `RasterCodec` and `KtxGate` compose the `plane#TEXTURE_PLANE` typed arena with its decoded row rails, its association gate, and its `PlanePrimaries` chromaticity axis, the `plane#TEXTURE_PYRAMID` chain every container-held pyramid maps onto, the kernel `Drawing/pack#ENCODING_CHANNEL` `ChannelDtype` depth roster, the seam `Rasm.Element` `FaultBand` registry, the kernel `Op` fault key and `Try` boundary funnel, and the four container packages — re-minting no header writer, no block encoder, no RGBE expander, no depth vocabulary, and no supercompressor.

## [01]-[INDEX]

- [02]-[RASTER_FAULT]: `RasterFault` closes the `FaultBand.Raster` band-2460 `[Union]` over its four cases and splits against band 2450.
- [03]-[RASTER_FORMAT]: `RasterEngine` families the reader-writers, the ten-row `RasterFormat` roster closes the containers, and `BlockFormat`/`KtxPayload`/`KtxArm` with the `EncodePolicy` row and its `PreviewPolicy` column carry the payload and egress vocabularies.
- [04]-[RASTER_CODEC]: `RasterCodec` folds the magic claim, declares its untrusted-input caps, normalizes association, records declared primaries, dispatches the engine, and funnels every container exception.
- [05]-[KTX_GATE]: `KtxGate` composes TextureCompressor alone, floors encode on the CLI beneath the in-process arm, writes the layered and cube container, and branches the transcodable payload.

## [02]-[RASTER_FAULT]

- Owner: `RasterFault` the closed band-2460 fault family over the `Rasm.Element` `FaultBand.Raster` registry row.
- Cases: `Decode`, `Encode`, `Device`, `Tile` — this page mints the two container cases, `gpu#PRESS_DEVICE` is the sole `Device` producer, and `tile#TILE_SYNTH` the sole `Tile` producer; the family homes here because the band is one registry row however many pages mint its cases.
- Law: the band is the REGISTRY ROW, never the literal `2460`. `FaultBand` is the `[SmartEnum<int>]` allocation registry whose disjointness is type-enforced at type initialization, so a duplicate band fails at class construction rather than at a telemetry reader attributing one folder's fault to another. Every site reads `FaultBand.Raster` and never the integer, keeping exactly the enforcement the registry exists to provide.
- Law: the split against band 2450 is by CONCERN, not by location. `MaterialFault` rails every appearance-domain admission failure — a parameter out of range, a colour out of gamut, a graph that will not compile, a plane extent, chromaticity, or association a shape gate refuses — and `RasterFault` rails a raster-MECHANICAL failure at a container, a device, or a synthesizer. `plane#TEXTURE_PLANE` and `filter#PLANE_OP` therefore rail band 2450 for every shape gate and reach this band only through a codec, a device, or a tile boundary, so no shape refusal ever wears a raster code.
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

- Owner: `RasterEngine` the reader-writer family; `RasterFormat` the container roster; `BlockFormat` the block-layout roster over the composed `TextureFormat` values; `KtxPayload` the KTX2 payload-class roster carrying its wire legality, its supercompression scheme, and its 8-bit block-encode bound; `KtxArm` the composition posture; `PreviewPolicy` the display-egress row; `EncodePolicy` the per-encode row.
- Cases: engine {`Managed`, `OpenExr`, `Radiance`, `Ktx`} · format {`png16`, `tiff16`, `webp`, `qoi`, `jpeg`, `exr`, `exrDeep`, `exrTiled`, `hdr`, `ktx2`} · block {`bc1`…`bc7`, `bc6h`, `none`} · payload {`rawBcn`, `uastc`, `etc1s`, `astc`, `none`} · arm {`cli`, `inProcess`}.
- Law: the DEPTH AXIS is the kernel `Drawing/pack#ENCODING_CHANNEL` `ChannelDtype` roster, the same one `plane#PLANE_FORMAT` seats its storage rows onto, so a container's ceiling and a plane's storage speak one vocabulary and no comparison crosses two rosters. `MaxDepth` is a ROW FACT the encode gate reads through `Admits`, and it is the honest ceiling rather than the advertised one. `Admits` compares ROWS through the plane page's own `Normalizes` discriminant, never byte widths — `Unorm16` and `Float16` share a width, so a width compare alone would pass an `rgba16f` plane through the `png16` ceiling and silently narrow it through the integer texel; a normalizing-ceiling row therefore admits only normalizing depths at or below its width, and a float-ceiling row admits every depth its width holds. `TiffBitsPerPixel` tops out below any float row and carries no 16-bit-plus-alpha row, so a four-lane 16-bit plane routed at `tiff16` drops its alpha lane rather than refusing — the row therefore declares `Unorm16` as its ceiling and the encoder states `BitsPerPixel` explicitly, because an unset depth knob is an INFERENCE and inference is what quietly ships an 8-bit channel.
- Law: `qoi` is a fast lossless EIGHT-BIT row and `jpeg` a lossy eight-bit INGEST row. `qoi` admits for a preview or a thumbnail egress; `jpeg` is the container `Appearance/acquisition#ACQUISITION` and `set#SET_INGEST` receive a captured photograph on, `AlphaMode.None` its truth because JFIF carries no coverage. Neither reaches a channel plane, because an 8-bit intermediate on a texture path is a silent quantization no downstream consumer can recover: the `set#TEXTURE_SET` egress grammar admits only the frozen `<ext>` roster, which carries neither `qoi` nor `jpg`, so both rows are structurally unreachable from a set leaf and `Extension` stays the one `<ext>` source for every row the egress CAN reach.
- Law: EXR is `TinyEXR.NET`'s WHOLE — the held ImageSharp major ships no EXR codec, so `exr`, `exrTiled`, and `exrDeep` are three rows of one engine case discriminating on the part type rather than three engines. Its tiled row is the estate's ONE float pyramid in a single file: `TileLevelMode.MipMap` holds every level under one header, so an HDR chain — the `environment#IBL_PREFILTER` specular level set, every solver-grade plane — takes a lossless `Compression.ZIP` home instead of the per-mip file series that was previously its only shape. Per-channel FILES stay the canonical cross-branch form; multipart, named-AOV, and tiled mip-levelled files are branch-local optimization, so no parity fixture depends on a leg one branch alone can write.
- Law: `hdr` is an INGEST row. Radiance stores three 8-bit mantissas under one shared exponent, so the expansion to float does not recover the quantization and every PRODUCED environment product egresses `exr`, `exrTiled`, or `ktx2`.
- Law: `KtxPayload.WireLegal` is the gate `set#TEXTURE_SET` reads at egress. `rawBcn` and `astc` are desktop payloads no Basis-transcoding consumer reads, so neither appears on a manifest-borne channel row; `uastc`, `etc1s`, and `none` are the three the web wire admits, and each block row carries the supercompression scheme and the sRGB and linear `TextureFormat` values its encode names.
- Law: BLOCK COMPRESSION IS 8-BIT-INPUT-ONLY — the measured bound on both legs, carried as the `BlockCompressed` row fact. `ktx create --encode` admits only `R8*` formats and the in-process block coders refuse a deeper store, so a block encode STAGES its levels at `u8` in the encoded domain and a float or half plane — the HDR IBL pyramid, a solver-grade plane — takes `KtxPayload.None`: the uncompressed deep KTX2 whose `vk_format` is set, whose `needs_transcoding` is false, and whose depth ceiling is the container's own `Float32`. ASTC-HDR is unreachable on the provisioned toolchain, so `astc` composes only at LDR.
- Entry: `RasterFormat.Items` is the ordered roster the claim fold walks; `Get`/`TryGet` resolve a wire key; `Extension` is the ONE `<ext>` source the `set#TEXTURE_SET` egress grammar reads, so no page carries a second extension table.
- Packages: SixLabors.ImageSharp (composed — `PngFormat.Instance`/`TiffFormat.Instance`/`WebpFormat.Instance`/`QoiFormat.Instance`/`JpegFormat.Instance` the identity singletons each row holds, `PngEncoder`/`TiffEncoder`/`WebpEncoder`/`QoiEncoder`/`JpegEncoder` with `PngBitDepth`/`TiffBitsPerPixel` stated explicitly, `KnownQuantizers` and `IQuantizer` the preview row's palette value, `Size` the preview extent), TinyEXR.NET (composed — `ExrFile.IsExr` the magic probe, `Compression.ZIP` the durable row, `TileDescription`/`TileLevelMode.MipMap` the tiled row's own level mode), TextureCompressor (composed — `TextureFormats.Bc1Rgba`…`Bc7UNorm`/`Bc6HUFloat` and `TextureFormats.RgbaBasisUastcLdr4x4UNorm`/`RgbaBasisEtc1sSrgb`, the FIELD names a static reference spells), TextureCompressor.FileFormats.Ktx (composed — `KtxSupercompressionScheme`), TextureCompressor.FileFormats.Hdr (composed — `HdrCodec.HasRadianceHeader`), `Rasm.Drawing` (composed — `ChannelDtype.Unorm8`/`Unorm16`/`Float32` the depth ceilings and their `Width` column), `plane#PLANE_VOCABULARY` (composed — `AlphaMode`/`PlaneFormat.Normalizes`), Thinktecture.Runtime.Extensions.
- Growth: a new container over an existing reader-writer is ONE `RasterFormat` row; a new block layout is one `BlockFormat` row naming its `TextureFormat` value; a new payload class is one `KtxPayload` row carrying its legality and scheme. Only a genuinely new reader-writer is a `RasterEngine` case, and adding one breaks the `[04]` dispatch totally rather than defaulting into a neighbour.
- Boundary: a row declares CARRIAGE and never policy. Quality, block choice, payload class, composition arm, and the preview egress ride `EncodePolicy`, which the `set#TEXTURE_SET` channel row resolves — so the same container serves a colour channel at one payload and a normal channel at another without a second format row, and a caller never selects a block format the container cannot hold.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using LanguageExt;
using Rasm.Drawing;                               // ChannelDtype — the kernel storage-type roster
using SixLabors.ImageSharp;                       // IImageFormat, Size
using SixLabors.ImageSharp.Formats;               // IImageEncoder
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Qoi;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.Formats.Tiff.Constants;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;            // IQuantizer, KnownQuantizers — the preview row's palette value
using TextureCompressor.FileFormats.Hdr;          // HdrCodec
using TextureCompressor.FileFormats.Ktx;          // KtxSupercompressionScheme
using TextureCompressor.Formats;                  // TextureFormat, TextureFormats
using Thinktecture;
using TinyEXR.V3;                                 // ExrFile, Compression, TileDescription, TileLevelMode
using static LanguageExt.Prelude;

namespace Rasm.Materials.Raster;

// --- [TYPES] -------------------------------------------------------------------------------
// RasterEngine families the reader-writers. Five container rows share the Managed case because they share one
// package, one identity mechanism, and one encoder contract — so an added PNG-class container is a ROW and only a new
// package is a case. OpenExr carries TWO discriminants because one package owns three part topologies: Deep selects the
// per-texel sample model and Tiled the mip-in-one-file level mode.
[Union]
public abstract partial record RasterEngine {
    private RasterEngine() { }

    public sealed record Managed(IImageFormat Identity, Func<ChannelDtype, IImageEncoder> Encoder) : RasterEngine;
    public sealed record OpenExr(bool Deep, bool Tiled) : RasterEngine;
    public sealed record Radiance : RasterEngine;
    public sealed record Ktx : RasterEngine;
}

// RasterFormat rosters the containers. Claim is the row's own magic probe, so the sniff is a fold over Items and no
// page holds a second magic table; Extension is the ONE <ext> source the set egress grammar reads.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RasterFormat {
    public static readonly RasterFormat Png16 = new("png16", "png", AlphaMode.Straight, ChannelDtype.Unorm16,
        new RasterEngine.Managed(PngFormat.Instance, static depth => new PngEncoder {
            BitDepth = depth == ChannelDtype.Unorm8 ? PngBitDepth.Bit8 : PngBitDepth.Bit16, ColorType = PngColorType.RgbWithAlpha }),
        static payload => Sniff(payload) == PngFormat.Instance);

    public static readonly RasterFormat Tiff16 = new("tiff16", "tif", AlphaMode.Straight, ChannelDtype.Unorm16,
        new RasterEngine.Managed(TiffFormat.Instance, static _ => new TiffEncoder {
            BitsPerPixel = TiffBitsPerPixel.Bit48, PhotometricInterpretation = TiffPhotometricInterpretation.Rgb }),
        static payload => Sniff(payload) == TiffFormat.Instance);

    public static readonly RasterFormat WebP = new("webp", "webp", AlphaMode.Straight, ChannelDtype.Unorm8,
        new RasterEngine.Managed(WebpFormat.Instance, static _ => new WebpEncoder { FileFormat = WebpFileFormatType.Lossless }),
        static payload => Sniff(payload) == WebpFormat.Instance);

    public static readonly RasterFormat Qoi = new("qoi", "qoi", AlphaMode.Straight, ChannelDtype.Unorm8,
        new RasterEngine.Managed(QoiFormat.Instance, static _ => new QoiEncoder()),
        static payload => Sniff(payload) == QoiFormat.Instance);

    // Jpeg is the INGEST row the photo-to-PBR path arrives on: baseline and progressive, eight-bit, lossy. It never reaches a
    // set leaf — the egress grammar carries no `jpg` — so a captured photograph decodes here and every produced
    // channel egresses through a lossless row.
    public static readonly RasterFormat Jpeg = new("jpeg", "jpg", AlphaMode.None, ChannelDtype.Unorm8,
        new RasterEngine.Managed(JpegFormat.Instance, static _ => new JpegEncoder { Quality = 95 }),
        static payload => Sniff(payload) == JpegFormat.Instance);

    // ONE managed sniff shared by the five Managed probes. The held major's Image.DetectFormat THROWS
    // UnknownImageFormatException on foreign bytes, so a per-row bare call would tear the claim fold down on
    // every EXR, HDR, and KTX2 payload before those rows' own probes ever ran — this is the platform-forced
    // boundary capture, and null is the typed no-claim the probes compare against.
    private static IImageFormat? Sniff(ReadOnlySpan<byte> payload) {
        try { return Image.DetectFormat(payload); }
        catch (UnknownImageFormatException) { return null; }
        catch (InvalidImageContentException) { return null; }
    }

    // Three EXR rows DECLARE in refusal order so the ordered claim fold resolves the narrowest topology first: a
    // deep file claims before a tiled one (a deep tiled part is deep, and its sample model is what decides the read), a
    // tiled file before the flat row, and the flat row then claims every remaining EXR. Each probe is a header-window
    // byte scan for the attribute-type token the container spells verbatim.
    public static readonly RasterFormat ExrDeep = new("exrDeep", "exr", AlphaMode.Associated, ChannelDtype.Float32,
        new RasterEngine.OpenExr(Deep: true, Tiled: false),
        static payload => ExrFile.IsExr(payload) && (SniffHeader(payload, "deepscanline"u8) || SniffHeader(payload, "deeptile"u8)));

    public static readonly RasterFormat ExrTiled = new("exrTiled", "exr", AlphaMode.Associated, ChannelDtype.Float32,
        new RasterEngine.OpenExr(Deep: false, Tiled: true),
        static payload => ExrFile.IsExr(payload) && SniffHeader(payload, "tiledesc"u8));

    public static readonly RasterFormat Exr = new("exr", "exr", AlphaMode.Associated, ChannelDtype.Float32,
        new RasterEngine.OpenExr(Deep: false, Tiled: false), static payload => ExrFile.IsExr(payload));

    // ONE header-window scan serves all three EXR probes: the attribute type strings appear verbatim in the header
    // block, so the discriminant is a token search over the leading window rather than a full parse on the claim path.
    private static bool SniffHeader(ReadOnlySpan<byte> payload, ReadOnlySpan<byte> token) =>
        payload[..Math.Min(payload.Length, 4096)].IndexOf(token) >= 0;

    public static readonly RasterFormat Hdr = new("hdr", "hdr", AlphaMode.None, ChannelDtype.Float32,
        new RasterEngine.Radiance(), static payload => HdrCodec.HasRadianceHeader(payload));

    // Ktx2 ceilings at the DEEP store's Float32: KtxPayload.None carries float planes uncompressed, and the
    // block rows gate their OWN Unorm8 staging bound at the payload row rather than at the container.
    public static readonly RasterFormat Ktx2 = new("ktx2", "ktx2", AlphaMode.Straight, ChannelDtype.Float32,
        new RasterEngine.Ktx(), static payload => payload.StartsWith(Ktx2Magic));

    // Ktx2Magic spells the KTX2 file identifier byte-for-byte from the container specification, as a UTF-8 literal
    // rather than an allocated array — the probe runs on every decode and allocates nothing.
    private static ReadOnlySpan<byte> Ktx2Magic => [0xAB, 0x4B, 0x54, 0x58, 0x20, 0x32, 0x30, 0xBB, 0x0D, 0x0A, 0x1A, 0x0A];

    public string Extension { get; }
    public AlphaMode CanonicalAlpha { get; }
    public ChannelDtype MaxDepth { get; }
    public RasterEngine Engine { get; }
    public ClaimProbe Claim { get; }
    // Two containers hold their own pyramid — the block container and the tiled mip-levelled EXR — so a per-mip file
    // SERIES is the shape every other row takes and a mip variant on either leaf is refused. Layers stay the block
    // container's alone: its subresource list carries array layers and cube faces in one file.
    public bool HoldsPyramid => Engine is RasterEngine.Ktx or RasterEngine.OpenExr { Tiled: true };
    public bool HoldsLayers => Engine is RasterEngine.Ktx;

    // Tiles carries the tiled EXR level mode. Sixty-four-texel tiles are the container's own working granularity, and MipMap is
    // what makes one file hold the chain; the flat and deep rows carry no description at all.
    public Option<TileDescription> Tiles =>
        Engine is RasterEngine.OpenExr { Tiled: true }
            ? Some(new TileDescription(64u, 64u, TileLevelMode.MipMap, TileRoundingMode.RoundDown))
            : None;

    // Depth admission compares ROWS through the plane page's own normalization discriminant, never widths alone:
    // Unorm16 and Float16 share two bytes, so a width compare would narrow an rgba16f plane through the png16
    // integer texel silently. A normalizing ceiling admits normalizing depths at or below its width; a float
    // ceiling admits every depth its width holds.
    public bool Admits(ChannelDtype depth) =>
        depth.Width <= MaxDepth.Width && (!PlaneFormat.Normalizes(MaxDepth) || PlaneFormat.Normalizes(depth));

    private RasterFormat(string key, string extension, AlphaMode alpha, ChannelDtype maxDepth, RasterEngine engine, ClaimProbe claim)
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
// names; BlockCompressed carries the measured 8-bit staging bound; the two TextureFormat columns are the target rows an
// encode resolves — the Basis rows for the transcodable pair, the ASTC LDR row for the in-process block lane, and
// NULL on the two rows whose format resolves elsewhere (rawBcn off BlockFormat, none off the plane's own depth).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class KtxPayload {
    public static readonly KtxPayload RawBcn = new("rawBcn", wireLegal: false, blockCompressed: true,
        KtxSupercompressionScheme.Zstandard, format: null, srgbFormat: null);
    public static readonly KtxPayload Uastc = new("uastc", wireLegal: true, blockCompressed: true,
        KtxSupercompressionScheme.Zstandard, TextureFormats.RgbaBasisUastcLdr4x4UNorm, TextureFormats.RgbaBasisUastcLdr4x4Srgb);
    public static readonly KtxPayload Etc1s = new("etc1s", wireLegal: true, blockCompressed: true,
        KtxSupercompressionScheme.BasisLz, TextureFormats.RgbaBasisEtc1sUNorm, TextureFormats.RgbaBasisEtc1sSrgb);
    public static readonly KtxPayload Astc = new("astc", wireLegal: false, blockCompressed: true,
        KtxSupercompressionScheme.None, TextureFormats.RgbaAstc4x4UNorm, TextureFormats.RgbaAstc4x4Srgb);
    public static readonly KtxPayload None = new("none", wireLegal: true, blockCompressed: false,
        KtxSupercompressionScheme.None, format: null, srgbFormat: null);

    public bool WireLegal { get; }
    // BlockCompressed carries the measured bound: every block class stages Unorm8 in the encoded domain before its coder or the CLI's
    // --encode runs; the none row alone carries the plane's own depth up to the container's Float32 ceiling.
    public bool BlockCompressed { get; }
    public KtxSupercompressionScheme Scheme { get; }
    public TextureFormat? Format { get; }
    public TextureFormat? SrgbFormat { get; }
    // Transcodable is what "needs transcoding" MEANS on a supercompressed row: the container declares an undefined
    // Vulkan format until a transcode runs, so readers branch on the parsed payload class and never on the header token.
    public bool Transcodable => Scheme != KtxSupercompressionScheme.None;
    public TextureFormat? Resolve(PlaneTransfer transfer) => transfer == PlaneTransfer.Srgb ? SrgbFormat : Format;

    private KtxPayload(string key, bool wireLegal, bool blockCompressed, KtxSupercompressionScheme scheme,
        TextureFormat? format, TextureFormat? srgbFormat) : this(key) =>
        (WireLegal, BlockCompressed, Scheme, Format, SrgbFormat) = (wireLegal, blockCompressed, scheme, format, srgbFormat);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class KtxArm {
    public static readonly KtxArm Cli = new("cli");
    public static readonly KtxArm InProcess = new("inProcess");
}

// --- [MODELS] ------------------------------------------------------------------------------
// PreviewPolicy is the display-egress row and it carries its own behaviour: the extent a thumbnail lands at and the
// palette collapse it takes, as the composed quantizer VALUE rather than a mode token the encode re-resolves.
public sealed record PreviewPolicy(Size Size, IQuantizer Palette) {
    public static readonly PreviewPolicy Thumbnail = new(new Size(512, 512), KnownQuantizers.Wu);
}

// EncodePolicy carries what the channel row resolves per encode. Compression is the EXR row: the lossy rows truncate
// or quantize float data, so a content-keyed or solver-grade plane takes ZIP and a lossy row never reaches a keyed
// plane. The two HTJ2K rows are refused on EVERY float plane rather than only on a keyed one — their small-extent
// decode is NaN, not approximate, and a pyramid folding to 1x1 crosses that range on every set.
// Block resolves the raw-BCn arm's own TextureFormat; Quality is the etc1s CLI `--qlevel` scale — the
// in-process arm resolves its coders internally and carries no level knob, which is the acceleration row's stated
// trade. Preview is ABSENT on every keyed encode by construction: it is the one column that changes what the bytes
// depict rather than how they are stored.
public sealed record EncodePolicy(
    BlockFormat Block,
    KtxPayload Payload,
    KtxArm Arm,
    Compression Compression,
    int Quality,
    Option<PreviewPolicy> Preview = default) {
    public static readonly EncodePolicy Durable =
        new(BlockFormat.None, KtxPayload.Uastc, KtxArm.Cli, Compression.ZIP, Quality: 128);
}
```

## [04]-[RASTER_CODEC]

- Owner: `RasterCodec` the container boundary — claim, untrusted-input caps, decode, association normalization, declared-primaries capture, and encode.
- Entry: `Decode(ReadOnlyMemory<byte> payload, Op key)` takes NO declared format and returns the chain the container held; `Encode(TexturePyramid subject, RasterFormat format, EncodePolicy policy, Op key)` takes one row and one policy. Arity is discriminated by the SUBJECT: a flat container writes the chain's base level and a pyramid-holding container writes every level, so no `EncodeLevel`/`EncodeChain` pair exists and no boolean selects between them.
- Law: the claim is a FOLD over `RasterFormat.Items` reading each row's own probe, first match wins, and an unclaimed payload rails `Decode`. `Decode` takes no declared format: a caller who must name the container has already read the magic bytes, and a caller who names the wrong one gets a misparse rather than a refusal.
- Law: `Decode` is the ONE path a THIRD-PARTY file crosses — `set#SET_INGEST` classifies a vendor library and `environment#ENVIRONMENT_MAP` admits a downloaded dome — so the reader's caps are DECLARED here rather than inherited: the package's defaults bound a trusted producer, and a header claiming a two-billion-texel edge must refuse before any rental. Writer caps bound what this estate itself emits, which is the strictly narrower posture. Every cap states the estate fact it derives from, so a raised bake extent moves one declaration rather than a scatter of literals.
- Law: decode TAGS what the file DECLARED and converts nothing. Canonical association rides the row — EXR is associated, PNG, TIFF, WebP, QOI, JPEG, and KTX2 are straight, Radiance carries none — and the declared CHROMATICITY rides the container's own metadata: an EXR header's `Chromaticities` attribute and an ImageSharp `CicpProfile`'s primaries each resolve to a `PlanePrimaries` row or to `unknown`, and a container that declares neither yields `unknown` rather than the working space. A row DECLARED without a reconciliation endpoint is a third reachable state, not a gap: a P3-D65 file tags `p3d65` and keeps its own label through the key and the container write while `ToPrimaries` off it refuses, where resolving it to `unknown` discarded a declaration the file genuinely made. Its DECLARING consumer then normalizes to the plane's declared `AlphaMode` through the `plane#TEXTURE_PLANE` `ToAlpha` gate and to its working gamut through `ToPrimaries`, which is where the frozen decode-normalizes direction lives: `Decode(payload, key)` carries no declaration, so the canonical tag is the honest intermediate and `set#SET_INGEST`'s per-role declaration is the one normalization site. Encode CONVERTS the plane's association into the format's canonical one through the same gate, so the 16-bit floor on a premultiply-state crossing is enforced once for the whole estate. Neither direction is a caller knob; the bridge itself moves ENCODED STORAGE lanes and never premultiplies, decodes, unpacks, or rebases, because those are plane declarations applied at `Read` and a bridge running them would double-apply every curve the file already carries.
- Law: encode REFUSES a plane whose depth the row's `Admits` denies rather than narrowing it. `Admits` exists to name exactly that silent narrow — a float plane on a normalizing-ceiling row included — and the caller either states an admitted plane or picks a row that holds the depth.
- Law: every composed container throws on a malformed payload — `ImageFormatException`, `UnknownImageFormatException`, the reader's typed limit exceptions, and the block engine's own — so every package call crosses the `Try.lift(...).Run()` funnel and lowers with the foreign message preserved inside the `Detail` discriminant. `Funnel` catches every container exception at the boundary and carries the foreign message verbatim, so nothing escapes and no re-wrap erases it, and the declared caps therefore need no rail of their own.
- Law: the ImageSharp leg crosses through ONE staging copy each way and the pixel-type conversion rides the package's own BULK converter. `CopyPixelDataTo` drains the decoded image into the pooled pixel run, `PixelOperations<Rgba64>.Instance.ToVector4`/`FromVector4Destructive` convert the whole run under `PixelConversionModifiers.None`, and `LoadPixelData<TPixel>(Configuration, ReadOnlySpan<TPixel>, int, int)` mints the encode image — so the estate's only managed decode and encode pay the package's vectorized path rather than a per-texel scalar round trip. This modifier is stated because `Rgba64`'s scaled projection IS its plain vector projection: `None` is byte-identical to the per-texel spelling it replaces, while `Scale` or `SRgbCompand` would rescale or linearize what the arena already holds encoded. Zero-copy `WrapMemory` binding is unreachable because the arena's texel structs are not ImageSharp pixel types, and a `MemoryMarshal` reinterpretation across that seam is exactly the stride fork the bridge law forbids.
- Law: a `Preview` column is the ONE Processing site in this estate. It runs on bytes the encode already staged as an `Image`, it is legal only on a row whose `MaxDepth` is `Unorm8`, and it never reaches a chain a `set#TEXTURE_SET` leaf addresses — the egress grammar carries no `qoi`, no `webp`, and no `jpg` extension, which is what keeps a resampled and palette-collapsed plane structurally unreachable from a keyed set. Its caller is `press#PRESS_RECEIPT`'s `Preview` arm, whose planes carry no set and therefore no key, which is exactly the subject an 8-bit display egress may take.
- Packages: SixLabors.ImageSharp (composed — `Image.DetectFormat(ReadOnlySpan<byte>)` the throw-only sniff each row's probe reads through the one caught fold, `Image.Load<TPixel>(DecoderOptions, ReadOnlySpan<byte>)` naming the demanded depth so the file never decides the arena — the `Configuration` threads through `DecoderOptions`, no bare `(Configuration, span)` overload exists, `Image<TPixel>.CopyPixelDataTo(Span<TPixel>)` the decode drain, `PixelOperations<Rgba64>.Instance.ToVector4`/`FromVector4Destructive(Configuration, …, PixelConversionModifiers)` the bulk pixel-conversion rail, `Image.LoadPixelData<TPixel>(Configuration, ReadOnlySpan<TPixel>, int, int)` the encode mint, `Image.Mutate(Action<IImageProcessingContext>)` with `Resize(ResizeOptions)` + `Quantize(IQuantizer)` and `KnownResamplers.Lanczos3` at the preview site alone, `ImageMetadata.CicpProfile`/`CicpProfile.ColorPrimaries` the declared-primaries read, `Image.Save(Stream, IImageEncoder)`), TinyEXR.NET (composed — `ExrFile.LoadFromMemory`/`SaveToMemory` over `ReaderResult<Image>`/`WriterResult<byte[]>` under declared `ReaderOptions`/`WriterOptions` and their `ReaderLimits`/`WriterLimits`, `Image.Parts`/`Part.Levels` the part-level walk, `Header.Chromaticities` the declared-primaries read, `PartConversion.IsLuminanceChroma`/`LuminanceChromaToRgbaFloat`/`ToInterleavedFloat`/`FromInterleavedFloat` the planar-interleaved bridge and its luminance-chroma arm, `ExrWriter.OpenSink`/`AddPart`/`Begin`/`WriteTile`/`End` over `StreamDataSink` the tiled mip writer, `Header(PartType, Box2i, IEnumerable<Channel>, Compression, …)` with its `tiles` tail + `DeepLevel(int, int, Box2i, ReadOnlySpan<int>, IEnumerable<ChannelBuffer>)` + `ChannelBuffer(string, PixelType, ReadOnlySpan<byte>)` the deep and tiled authoring surface, `DeepLevel.SampleCounts` the deep read fold, `PartLevel.Channels`/`Region` the per-level interleave, `ExrResult.WouldBlock` resumed rather than failed), TextureCompressor.FileFormats.Hdr (composed — `HdrCodec.Decode(ReadOnlySpan<byte>)`, `HdrCodec.Encode<TPixel>(IBitmap<TPixel>, HdrEncodingOptions?) -> byte[]` the byte-returning overload the decompile grounds), `plane#TEXTURE_PLANE` (composed — `TexturePlane.Of` over both modalities, `Read`/`Write`/`ToAlpha`/`ToPrimaries`, `PlanePrimaries.Of`, `TexturePyramid.Of`/`Levels`/`Base`), `Rasm.Domain` (`Op`, the `Try` boundary funnel), `Rasm.Numerics` (`Dimension`), LanguageExt.Core.
- Growth: a container added as a `RasterFormat` row reaches decode and encode with zero edits here, because both fold over the row's engine. `RasterEngine` grows by one case, which adds one arm to each of the two `Switch` folds and breaks the generated dispatch totally until both land.
- Boundary: this page owns CONTAINERS and never pixels. Transfer, primaries, association, range, and the decode ladder are `plane#PLANE_VOCABULARY`'s, resampling and derivation are `filter#PLANE_OP`'s, and channel semantics are `set#TEXTURE_CHANNEL`'s — so a codec never decides what a plane MEANS and never applies a colour transform a decode did not carry. `ReaderResult`/`WriterResult` values carry `Status` beside `IsSuccess`, and `ExrResult.WouldBlock` is a PROTOCOL state naming the byte range the reader wants, resumed by feeding exactly that window; classing it as failure stalls a healthy incremental read.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.IO;
using System.Linq;                                // Enumerable.Range — the deep and tiled channel-buffer fans
using System.Numerics;                            // Vector4 — the pixel ladder's bulk projection
using System.Runtime.InteropServices;             // MemoryMarshal — the ONE reinterpretation at the staging bridge
using CommunityToolkit.HighPerformance;           // Memory2D — the storage folds' view
using CommunityToolkit.HighPerformance.Buffers;
using LanguageExt;
using Rasm.Domain;                                // Op
using Rasm.Numerics;                              // Dimension
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;          // Rgba64, PixelOperations, PixelConversionModifiers
using SixLabors.ImageSharp.Processing;            // Mutate, ResizeOptions, KnownResamplers — the preview site alone
using TextureCompressor.Bitmaps;                  // ArrayBitmap, BitmapView
using TextureCompressor.Colors;                   // Rgba32Float
using TextureCompressor.FileFormats.Hdr;
using TinyEXR.V3;
using TinyEXR.V3.IO;                              // StreamDataSink — the tiled writer's sink
using static LanguageExt.Prelude;

namespace Rasm.Materials.Raster;

// --- [OPERATIONS] --------------------------------------------------------------------------
public static partial class RasterCodec {
    // One Configuration per process rather than per call: the allocator, the contiguity preference, and the
    // parallelism are an ENCODE PROFILE, and constructing one per plane re-registers the whole format manager.
    // Contiguity is preferred BEFORE any decode, because it is what makes the single-buffer probe hold at all.
    private static readonly Configuration Profile = Configured();

    private static Configuration Configured() {
        Configuration profile = Configuration.Default.Clone();
        profile.PreferContiguousImageBuffers = true;
        return profile;
    }

    // THE UNTRUSTED-INPUT POSTURE. Decode is the one path an outside file crosses, so its caps are declared from the
    // estate's own facts rather than inherited from a trusted-producer default: the dimension cap is two doublings
    // past the 16k plane the arena law names as the working extent, so a legitimate asset admits and a hostile edge
    // refuses before a rental; the materialization cap is the widest float staging the typed arena's own element
    // ceiling can back, so a payload no plane could hold never allocates; the part cap is the estate's named-AOV
    // posture; and the deep cap follows the front-sample fold, which reads one sample per texel. Emit is strictly
    // narrower because it bounds what this estate itself writes.
    private static readonly ReaderOptions Ingest = new(new ReaderLimits {
        MaximumDimension = 1 << 16,
        MaximumMaterializedByteCount = (long)Array.MaxLength * sizeof(float),
        MaximumParts = 64,
        MaximumDeepSampleCount = Array.MaxLength,
    });

    private static readonly WriterOptions Emit = new(new WriterLimits {
        MaximumDimension = 1 << 16,
        MaximumParts = 64,
        MaximumDeepSampleCount = Array.MaxLength,
    });

    // No declared format: the row's own probe claims the bytes. First match wins over the ordered roster, and an
    // unclaimed payload is a typed refusal rather than a guess. Every engine arm — the block container included —
    // crosses the one Funnel, because KtxCodec.Read throws on a malformed container exactly as the managed
    // packages do and an unfunnelled arm is the one escape hatch off the rail.
    public static Fin<TexturePyramid> Decode(ReadOnlyMemory<byte> payload, Op key) =>
        Claim(payload.Span)
            .ToFin(RasterFault.Decode(key, $"<raster-magic:{payload.Length}>"))
            .Bind(format => format.Engine.Switch(
                managed:  arm => Funnel(() => ReadManaged(payload.Span, format, key), key, decoding: true),
                openExr:  arm => Funnel(() => Exr(payload, arm, key), key, decoding: true),
                radiance: _   => Funnel(() => Radiance(payload.Span, key), key, decoding: true),
                ktx:      _   => Funnel(() => KtxGate.Decode(payload, key), key, decoding: true)));

    public static Fin<ReadOnlyMemory<byte>> Encode(TexturePyramid subject, RasterFormat format, EncodePolicy policy, Op key) =>
        !format.Admits(subject.Base.Format.Depth)
            ? RasterFault.Encode(key, $"<raster-depth:{subject.Base.Format.Depth.Key}>{format.MaxDepth.Key}@{format.Key}>")
            : Associate(subject, subject.Base.Alpha, format.CanonicalAlpha, key)
                .Bind(normalized => format.Engine.Switch(
                    managed:  arm => Funnel(() => WriteManaged(normalized, arm, policy, key), key, decoding: false),
                    openExr:  arm => Funnel(() => WriteExr(normalized, arm, format, policy, key), key, decoding: false),
                    radiance: _   => Funnel(() => WriteRadiance(normalized, key), key, decoding: false),
                    ktx:      _   => Funnel(() => KtxGate.Encode(normalized, policy, key), key, decoding: false)));

    private static Option<RasterFormat> Claim(ReadOnlySpan<byte> payload) {
        foreach (RasterFormat row in RasterFormat.Items) { if (row.Claim(payload)) { return Some(row); } }
        return None;
    }

    // Associate crosses association ONCE over the whole chain, delegating to the plane's own gate so the 16-bit floor
    // is enforced once. A chain already at the target returns itself untouched; a mid-chain refusal disposes every
    // level already converted, because an orphaned rental on the failure arm is a pool leak the rail cannot see.
    private static Fin<TexturePyramid> Associate(TexturePyramid chain, AlphaMode from, AlphaMode to, Op key) =>
        from == to
            ? Fin.Succ(chain)
            : chain.Levels
                .Fold(Fin.Succ(Seq<TexturePlane>()), (state, level) => state.Bind(converted =>
                    level.ToAlpha(to, key)
                        .Map(converted.Add)
                        .MapFail(fault => { converted.Iter(static plane => plane.Dispose()); return fault; })))
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
// Each leg rides the ONE staging bridge beneath it, and the bridge carries ENCODED UNIT LANES — the domain the texel
// witnesses' Project/Compose already speak. A container stores encoded values and so does the arena, so the bridge
// runs NO transfer, NO unpack, NO rebase, and NO un-association in either direction: those are declarations the plane
// carries and applies at Read. Routing container texels through the decode ladder would double-encode every write and
// hand every read a curve the file already applied — the defect this bridge shape forecloses.
namespace Rasm.Materials.Raster;

public static partial class RasterCodec {
    // ReadManaged mints ONE level at MipPolicy.None because the flat container held no pyramid. The Rgba64 run
    // converts through the package's own BULK vector rail under PixelConversionModifiers.None — Rgba64's scaled
    // projection IS its plain vector projection, so the modifier is byte-identical to the per-texel spelling and an
    // 8-bit source expands losslessly because the unit ratio v/255 equals v'/65535, so the row's OWN depth
    // re-quantizes bit-faithful. The storage row follows the container ceiling: a Unorm8 row lands Rgba8 so its
    // round trip re-encodes, a Unorm16 row lands Rgba16. The srgb transfer is the integer colour container's
    // declared assumption and the CICP block is its declared chromaticity; set#SET_INGEST re-declares per role,
    // which re-keys nothing because identity is storage bytes alone.
    private static Fin<TexturePyramid> ReadManaged(ReadOnlySpan<byte> payload, RasterFormat format, Op key) {
        using Image<Rgba64> image = Image.Load<Rgba64>(new DecoderOptions { Configuration = Profile }, payload);
        using MemoryOwner<Rgba64> pixels = MemoryOwner<Rgba64>.Allocate(image.Width * image.Height);
        using MemoryOwner<Vector4> vectors = MemoryOwner<Vector4>.Allocate(image.Width * image.Height);
        using MemoryOwner<float> staging = MemoryOwner<float>.Allocate(image.Width * image.Height * 4);
        image.CopyPixelDataTo(pixels.Span);
        PixelOperations<Rgba64>.Instance.ToVector4(Profile, pixels.Span, vectors.Span, PixelConversionModifiers.None);
        for (int i = 0; i < vectors.Length; i++) {
            (staging.Span[i * 4], staging.Span[(i * 4) + 1], staging.Span[(i * 4) + 2], staging.Span[(i * 4) + 3]) =
                (vectors.Span[i].X, vectors.Span[i].Y, vectors.Span[i].Z, vectors.Span[i].W);
        }
        return Fill(staging.Span, image.Width, image.Height, 4,
            format.MaxDepth == ChannelDtype.Unorm8 ? PlaneFormat.Rgba8 : PlaneFormat.Rgba16, PlaneTransfer.Srgb,
            Declared(image.Metadata), format.CanonicalAlpha, PlaneRange.Unit, key);
    }

    // Declared reads the chromaticity off the container's own CICP block. Nothing is what an absent block declares, which is the
    // honest row — a file carrying no primaries statement is not a file carrying the working space.
    private static PlanePrimaries Declared(ImageMetadata metadata) =>
        metadata.CicpProfile is { } cicp ? PlanePrimaries.Of((int)cicp.ColorPrimaries) : PlanePrimaries.Unknown;

    // WriteManaged drains at the plane's OWN lane stride, seats the run through the one lane-to-register
    // correspondence in the ENCODED domain — a single lane replicates across RGB, a two-lane pair fills R and G
    // with B zero, and an absent coverage lane writes opaque — so a scalar r16 height plane egresses PNG16 as grey
    // rather than reading 2-4x past its own rental at a hardcoded stride, then converts the whole Vector4 run
    // through the package's bulk destructive rail. The preview column is the one Processing hop and it runs on an
    // Image the encode already minted, so no plane crosses into Image<TPixel> for a transform the arena owns.
    private static Fin<ReadOnlyMemory<byte>> WriteManaged(TexturePyramid chain, RasterEngine.Managed arm, EncodePolicy policy, Op key) {
        TexturePlane plane = chain.Base;
        int lanes = plane.Lanes, colour = plane.Alpha.Carries ? lanes - 1 : lanes;
        using MemoryOwner<float> staging = Drain(plane);
        using MemoryOwner<Vector4> vectors = MemoryOwner<Vector4>.Allocate(plane.Width.Value * plane.Height.Value);
        using MemoryOwner<Rgba64> pixels = MemoryOwner<Rgba64>.Allocate(vectors.Length);
        for (int i = 0; i < vectors.Length; i++) {
            int at = i * lanes;
            vectors.Span[i] = new Vector4(
                staging.Span[at],
                colour > 1 ? staging.Span[at + 1] : staging.Span[at],
                colour > 2 ? staging.Span[at + 2] : (colour == 2 ? 0f : staging.Span[at]),
                plane.Alpha.Carries ? staging.Span[at + lanes - 1] : 1f);
        }
        PixelOperations<Rgba64>.Instance.FromVector4Destructive(Profile, vectors.Span, pixels.Span, PixelConversionModifiers.None);
        using Image<Rgba64> image = Image.LoadPixelData<Rgba64>(Profile, pixels.Span,
            plane.Width.Value, plane.Height.Value);
        policy.Preview.Iter(preview => image.Mutate(context => context
            .Resize(new ResizeOptions { Size = preview.Size, Sampler = KnownResamplers.Lanczos3 })
            .Quantize(preview.Palette)));
        using MemoryStream sink = new();
        image.Save(sink, arm.Encoder(plane.Format.Depth));
        return Fin.Succ((ReadOnlyMemory<byte>)sink.ToArray());
    }

    // EXR crosses through the container's own bridges under the declared reader caps, so an arbitrary named-AOV part
    // flattens into the same encoded run every other leg fills — float storage IS its encoded form, so the lanes
    // cross verbatim. The three topologies split HERE: the deep arm reduces at the FRONT SAMPLE (a deep part's level
    // carries per-texel sample counts and per-channel contiguous sample runs, and a plane holds one value per texel,
    // so the first sample per texel is the one reduction a container boundary may take — compositing policy belongs
    // to a solver, never a codec), the tiled arm maps the part's own level roster onto the plane chain, and the flat
    // arm reads level zero alone.
    private static Fin<TexturePyramid> Exr(ReadOnlyMemory<byte> payload, RasterEngine.OpenExr arm, Op key) {
        ReaderResult<TinyEXR.V3.Image> read = ExrFile.LoadFromMemory(payload, Ingest);
        return read is { IsSuccess: true, Value: { } file }
            ? toSeq(file.Parts).Head.ToFin(RasterFault.Decode(key, "<exr-parts-empty>")).Bind(part =>
                  arm.Deep && part.Levels.Count > 0 && part.Levels[0] is DeepLevel deepLevel
                      ? DeepFront(deepLevel, part, key)
                      : arm.Tiled ? Tiled(part, key) : Flat(part, key))
            : RasterFault.Decode(key, $"<exr-read:{read.Status}>");
    }

    // Flat discriminates on the container's OWN luminance-chroma probe. Luminance-chroma parts store
    // Y/RY/BY at half chroma resolution, so the planar-to-interleaved bridge would hand three lanes of the WRONG
    // basis to a plane that declares RGB, and no downstream gate could see it — the container's own expander is the
    // one full-resolution reconstruction and IsLuminanceChroma is the one probe that selects it.
    private static Fin<TexturePyramid> Flat(Part part, Op key) {
        InterleavedFloatImage flat = PartConversion.IsLuminanceChroma(part)
            ? PartConversion.LuminanceChromaToRgbaFloat(part)
            : PartConversion.ToInterleavedFloat(part);
        return Fill(flat.Data, flat.Width, flat.Height, flat.Channels,
            PlaneFormat.For(flat.Channels, ChannelDtype.Float32).IfNone(PlaneFormat.Rgba32F),
            PlaneTransfer.Linear, Declared(part.Header), flat.Channels is 4 ? AlphaMode.Associated : AlphaMode.None,
            PlaneRange.Unit, key);
    }

    // Tiled maps the part's OWN level roster onto the plane chain in level order, so an encoder's filter
    // survives verbatim and no refold replaces it. Each level assembles from its planar channel buffers through the
    // one per-level interleave, and the recorded policy is the estate's box assumption over a FOREIGN fold — an
    // ingested chain's levels adopt as they are and steer only a later re-fold decision.
    private static Fin<TexturePyramid> Tiled(Part part, Op key) =>
        toSeq(part.Levels)
            .Fold(Fin.Succ(Seq<TexturePlane>()), (state, level) => state.Bind(levels =>
                Level(level, part.Header, key).Map(levels.Add)))
            .Map(levels => new TexturePyramid(levels, levels.Count > 1 ? MipPolicy.Box : MipPolicy.None, Coupled: false));

    // ONE per-level interleave over a part level's planar channel buffers — the shape the deep fold and the tiled
    // fold share, because PartConversion flattens a PART and a level is what a chain reads.
    private static Fin<TexturePlane> Level(PartLevel level, Header header, Op key) {
        int width = level.Region.MaxX - level.Region.MinX + 1, height = level.Region.MaxY - level.Region.MinY + 1;
        int channels = Math.Min(4, level.Channels.Count);
        using MemoryOwner<float> staging = MemoryOwner<float>.Allocate(width * height * channels, AllocationMode.Clear);
        for (int c = 0; c < channels; c++) {
            ChannelBuffer channel = level.Channels[c];
            for (int texel = 0; texel < width * height; texel++) {
                staging.Span[(texel * channels) + c] = channel.PixelType == PixelType.Half
                    ? (float)MemoryMarshal.Cast<byte, Half>(channel.Data)[texel]
                    : MemoryMarshal.Cast<byte, float>(channel.Data)[texel];
            }
        }
        return Fill(staging.Span, width, height, channels,
            PlaneFormat.For(channels, ChannelDtype.Float32).IfNone(PlaneFormat.Rgba32F),
            PlaneTransfer.Linear, Declared(header), channels is 4 ? AlphaMode.Associated : AlphaMode.None,
            PlaneRange.Unit, key).Map(static chain => chain.Base);
    }

    // Declared reads the chromaticity off the part header's own attribute. Absent means absent.
    private static PlanePrimaries Declared(Header header) => PlanePrimaries.Of(Optional(header.Chromaticities));

    // DeepFront reduces at the front sample: SampleCounts prefix-sums into each texel's first-sample index, and each channel
    // buffer reads its own PixelType at that index — an empty texel reads zero, the deep hole's typed neutral.
    private static Fin<TexturePyramid> DeepFront(DeepLevel level, Part part, Op key) {
        int width = checked((int)level.Width), height = checked((int)level.Height);
        int channels = Math.Min(4, level.Channels.Count);
        using MemoryOwner<float> staging = MemoryOwner<float>.Allocate(width * height * channels, AllocationMode.Clear);
        ReadOnlySpan<int> counts = level.SampleCounts;
        for (int texel = 0, first = 0; texel < width * height && texel < counts.Length; first += counts[texel], texel++) {
            if (counts[texel] is 0) { continue; }
            for (int c = 0; c < channels; c++) {
                ChannelBuffer channel = level.Channels[c];
                staging.Span[(texel * channels) + c] = channel.PixelType == PixelType.Half
                    ? (float)MemoryMarshal.Cast<byte, Half>(channel.Data)[first]
                    : MemoryMarshal.Cast<byte, float>(channel.Data)[first];
            }
        }
        return Fill(staging.Span, width, height, channels,
            PlaneFormat.For(channels, ChannelDtype.Float32).IfNone(PlaneFormat.Rgba32F),
            PlaneTransfer.Linear, Declared(part.Header), channels is 4 ? AlphaMode.Associated : AlphaMode.None,
            PlaneRange.Unit, key);
    }

    // WriteExr splits on the same three topologies the row declares: a flat arm rides the interleaved
    // bridge; the deep arm authors a genuine DeepScanline part at one sample per texel, which is what a plane can
    // state; the tiled arm is the estate's ONE float pyramid in a single file — it opens the incremental writer,
    // declares one MipMap-moded part, walks every level's tiles, and always reaches End, because a writer that
    // never seals leaves a headerless file.
    private static Fin<ReadOnlyMemory<byte>> WriteExr(
        TexturePyramid chain, RasterEngine.OpenExr arm, RasterFormat format, EncodePolicy policy, Op key) =>
        (arm.Deep, arm.Tiled) switch {
            (true, _) => WriteDeep(chain, policy, key),
            (_, true) => WriteTiled(chain, format, policy, key),
            _ => WriteFlat(chain, policy, key),
        };

    private static Fin<ReadOnlyMemory<byte>> WriteFlat(TexturePyramid chain, EncodePolicy policy, Op key) {
        TexturePlane plane = chain.Base;
        using MemoryOwner<float> staging = Drain(plane);
        Part part = PartConversion.FromInterleavedFloat(staging.Span, plane.Width.Value,
            plane.Height.Value, plane.Lanes, PixelType.Float, policy.Compression);
        WriterResult<byte[]> written = ExrFile.SaveToMemory(new TinyEXR.V3.Image([part]), policy.Compression, Emit);
        return written is { IsSuccess: true, Value: { } bytes }
            ? Fin.Succ((ReadOnlyMemory<byte>)bytes)
            : RasterFault.Encode(key, $"<exr-write:{written.Status}>");
    }

    private static Fin<ReadOnlyMemory<byte>> WriteDeep(TexturePyramid chain, EncodePolicy policy, Op key) {
        TexturePlane plane = chain.Base;
        using MemoryOwner<float> staging = Drain(plane);
        int width = plane.Width.Value, height = plane.Height.Value, lanes = plane.Lanes;
        Box2i window = new(0, 0, width - 1, height - 1);
        using MemoryOwner<int> counts = MemoryOwner<int>.Allocate(width * height);
        counts.Span.Fill(1);
        using MemoryOwner<float> lane = MemoryOwner<float>.Allocate(width * height);
        Seq<ChannelBuffer> buffers = toSeq(Enumerable.Range(0, Math.Min(lanes, Names(lanes).Length)).Select(c => {
            for (int texel = 0; texel < width * height; texel++) { lane.Span[texel] = staging.Span[(texel * lanes) + c]; }
            return new ChannelBuffer(Names(lanes)[c], PixelType.Float, MemoryMarshal.AsBytes(lane.Span[..(width * height)]));
        }).ToList());
        Header header = new(PartType.DeepScanline, window,
            buffers.Map(static buffer => new Channel(buffer.Name, PixelType.Float)), policy.Compression);
        DeepLevel deepLevel = new(0, 0, window, counts.Span, buffers);
        WriterResult<byte[]> written = ExrFile.SaveToMemory(
            new TinyEXR.V3.Image([new Part(header, [deepLevel])]), policy.Compression, Emit);
        return written is { IsSuccess: true, Value: { } bytes }
            ? Fin.Succ((ReadOnlyMemory<byte>)bytes)
            : RasterFault.Encode(key, $"<exr-deep-write:{written.Status}>");
    }

    // WriteTiled is the tiled mip writer. One part declares the row's own TileDescription, every level's tiles write at (levelX,
    // levelY) equal to the level index — the MipMap level mode's own square addressing — and End patches the offset
    // tables. A non-success at any phase rails immediately, because a partially written tiled file reads as a valid
    // header over missing levels.
    private static Fin<ReadOnlyMemory<byte>> WriteTiled(TexturePyramid chain, RasterFormat format, EncodePolicy policy, Op key) {
        TexturePlane basePlane = chain.Base;
        int lanes = basePlane.Lanes;
        string[] names = Names(lanes);
        using MemoryStream sink = new();
        using ExrWriter writer = ExrWriter.OpenSink(new StreamDataSink(sink), Emit);
        Header header = new(PartType.Tiled, new Box2i(0, 0, basePlane.Width.Value - 1, basePlane.Height.Value - 1),
            toSeq(names).Take(Math.Min(lanes, names.Length)).Map(static name => new Channel(name, PixelType.Float)),
            policy.Compression, tiles: format.Tiles.IfNone(() => new TileDescription(64u, 64u, TileLevelMode.MipMap, TileRoundingMode.RoundDown)));
        int part = writer.AddPart(header);
        return Sealed(writer.Begin(), key, "<exr-tiled-begin>")
            .Bind(_ => toSeq(chain.Levels.Select((level, index) => (Level: level, Index: index)))
                .Fold(Fin.Succ(unit), (state, slot) => state.Bind(_ =>
                    WriteLevelTiles(writer, part, slot.Index, slot.Level, format, names, key))))
            .Bind(_ => Sealed(writer.End(), key, "<exr-tiled-end>"))
            .Map(_ => (ReadOnlyMemory<byte>)sink.ToArray());
    }

    // One level's tile fan. Each tile drains its own window out of the level's staged run into per-channel planar
    // buffers, so the writer receives exactly the channel set the header declared and the tile grid derives from the
    // description rather than from a caller count.
    private static Fin<Unit> WriteLevelTiles(
        ExrWriter writer, int part, int index, TexturePlane level, RasterFormat format, string[] names, Op key) {
        TileDescription tiles = format.Tiles.IfNone(() => new TileDescription(64u, 64u, TileLevelMode.MipMap, TileRoundingMode.RoundDown));
        int tw = checked((int)tiles.TileSizeX), th = checked((int)tiles.TileSizeY);
        int channels = Math.Min(level.Lanes, names.Length);
        int across = ((level.Width.Value + tw) - 1) / tw, down = ((level.Height.Value + th) - 1) / th;
        using MemoryOwner<float> staging = Drain(level);
        using MemoryOwner<float> tile = MemoryOwner<float>.Allocate(tw * th * channels, AllocationMode.Clear);
        return toSeq(Enumerable.Range(0, across * down))
            .Fold(Fin.Succ(unit), (state, slot) => state.Bind(_ => {
                (int tx, int ty) = (slot % across, slot / across);
                tile.Span.Clear();
                for (int y = 0; y < th; y++) {
                    for (int x = 0; x < tw; x++) {
                        int sx = (tx * tw) + x, sy = (ty * th) + y;
                        if (sx >= level.Width.Value || sy >= level.Height.Value) { continue; }
                        for (int c = 0; c < channels; c++) {
                            tile.Span[(((y * tw) + x) * channels) + c] =
                                staging.Span[((((sy * level.Width.Value) + sx) * level.Lanes)) + c];
                        }
                    }
                }
                Seq<ChannelBuffer> buffers = toSeq(Enumerable.Range(0, channels).Select(c => {
                    float[] plane = new float[tw * th];
                    for (int texel = 0; texel < tw * th; texel++) { plane[texel] = tile.Span[(texel * channels) + c]; }
                    return new ChannelBuffer(names[c], PixelType.Float, MemoryMarshal.AsBytes(plane.AsSpan()));
                }).ToList());
                return Sealed(writer.WriteTile(part, tx, ty, index, index, buffers), key, $"<exr-tile:{index}:{tx}:{ty}>");
            }));
    }

    // Every writer phase carries its own result value rather than throwing, so one lowering serves Begin, WriteTile,
    // and End and a partial write never reads as a sealed file.
    private static Fin<Unit> Sealed(WriterResult result, Op key, string at) =>
        result.IsSuccess ? Fin.Succ(unit) : RasterFault.Encode(key, $"{at}:{result.Status}>");

    // Names rosters the channel names a lane count resolves to — one spelling for the deep and tiled authoring legs alike.
    private static string[] Names(int lanes) => lanes is 1 ? ["Y"] : lanes is 2 ? ["R", "G"] : ["R", "G", "B", "A"];

    // Radiance lands the float texel directly — the only depth the RGBE expansion fills without loss — so the ingest
    // path never passes through an 8-bit step and the decoded plane is already BC6H's own texel type. RGBE declares
    // no chromaticity this decoder reads, so the plane carries absence rather than an assumed primary set.
    private static Fin<TexturePyramid> Radiance(ReadOnlySpan<byte> payload, Op key) {
        ArrayBitmap<Rgba32Float> bitmap = HdrCodec.Decode(payload);
        return Fill(MemoryMarshal.Cast<Rgba32Float, float>(bitmap.PixelSpan), bitmap.Width, bitmap.Height, 4,
            PlaneFormat.Rgba32F, PlaneTransfer.Linear, PlanePrimaries.Unknown, AlphaMode.None, PlaneRange.Unit, key);
    }

    private static Fin<ReadOnlyMemory<byte>> WriteRadiance(TexturePyramid chain, Op key) {
        using MemoryOwner<float> staging = Drain(chain.Base);
        ArrayBitmap<Rgba32Float> bitmap = new(chain.Base.Width.Value, chain.Base.Height.Value);
        MemoryMarshal.Cast<float, Rgba32Float>(staging.Span).CopyTo(bitmap.PixelSpan);
        return Fin.Succ((ReadOnlyMemory<byte>)HdrCodec.Encode(bitmap, options: null));
    }

    // THE BRIDGE. Fill composes encoded unit lanes into STORAGE texels through the arena's own witnesses; Drain
    // projects them back. Both re-enter through the typed store's Accept, so the JIT specializes one body per texel
    // type and the codec never touches the decode ladder. A non-positive extent REFUSES — a malformed container
    // reporting a zero edge is a decode fault, never a fabricated 1x1 plane of pool residue — and the declared
    // primaries ride in as a tag the plane records rather than a conversion the bridge runs.
    internal static Fin<TexturePyramid> Fill(
        ReadOnlySpan<float> staging, int width, int height, int lanes, PlaneFormat format,
        PlaneTransfer transfer, PlanePrimaries primaries, AlphaMode alpha, PlaneRange range, Op key) =>
        width <= 0 || height <= 0
            ? RasterFault.Decode(key, $"<raster-extent:{width}x{height}>")
            : TexturePlane.Of(format, Dimension.Create(width), Dimension.Create(height), transfer, alpha, key,
                    range: Some(range), primaries: Some(primaries), mode: AllocationMode.Default)
                .Map(plane => {
                    plane.Store.Accept<ComposeRows, Unit>(new ComposeRows(staging, lanes));
                    return plane;
                })
                .Bind(plane => TexturePyramid.Of(plane, MipPolicy.None, key));

    // Drain rents a HEAP-SAFE owner rather than a stack-scoped one — the staging run outlives an expression scope
    // — and returns it BARE: a projection over an admitted plane has no failure arm, and a one-arm Fin is a rail
    // wearing a costume. The caller's using owns disposal.
    internal static MemoryOwner<float> Drain(TexturePlane plane) {
        MemoryOwner<float> staging = MemoryOwner<float>.Allocate(plane.Width.Value * plane.Height.Value * plane.Lanes);
        plane.Store.Accept<ProjectRows, Unit>(new ProjectRows(staging.Span, plane.Lanes));
        return staging;
    }
}

// Two storage folds close the bridge — the section's [EXPRESSION_SPINE] kernel exemption. ComposeRows pads an absent lane with
// zero and an absent alpha with one; ProjectRows is its exact inverse. Both speak encoded unit lanes, the witnesses'
// own domain, so no curve and no packing arithmetic exists on this page.
internal readonly ref struct ComposeRows(ReadOnlySpan<float> staging, int lanes) : IPlaneFold<Unit> {
    public Unit Fold<T>(Memory2D<T> view) where T : unmanaged, ITexel<T> {
        Span<double> texel = stackalloc double[4];
        for (int y = 0; y < view.Height; y++) {
            Span<T> row = view.Span.GetRowSpan(y);
            for (int x = 0; x < row.Length; x++) {
                int at = ((y * row.Length) + x) * lanes;
                for (int c = 0; c < T.Lanes; c++) {
                    texel[c] = c < lanes ? staging[at + c] : (c == T.Lanes - 1 ? 1.0 : 0.0);
                }
                row[x] = T.Compose(texel[..T.Lanes]);
            }
        }
        return Unit.Default;
    }
}

internal readonly ref struct ProjectRows(Span<float> staging, int lanes) : IPlaneFold<Unit> {
    public Unit Fold<T>(Memory2D<T> view) where T : unmanaged, ITexel<T> {
        Span<double> texel = stackalloc double[4];
        for (int y = 0; y < view.Height; y++) {
            ReadOnlySpan<T> row = view.Span.GetRowSpan(y);
            for (int x = 0; x < row.Length; x++) {
                T.Project(in row[x], texel[..T.Lanes]);
                int at = ((y * row.Length) + x) * lanes;
                for (int c = 0; c < lanes && c < T.Lanes; c++) { staging[at + c] = (float)texel[c]; }
            }
        }
        return Unit.Default;
    }
}
```

## [05]-[KTX_GATE]

- Owner: `KtxGate` — the ONE composer of `TextureCompressor` and its KTX container leg anywhere in the estate.
- Entry: `Decode(payload, key)` and `Encode(chain, policy, key)`, both internal. Nothing outside this section names a coder, a registry, a container option, a subresource, or a block format value, so the whole pre-1.0 surface has one call site and a version bump re-verifies one gate.
- Law: the provisioned `ktx` CLI is the encode FLOOR in every branch: the python estate spawns the same binary, and the TypeScript estate CONSUMES the produced bytes and spawns nothing — two spawning branches, one consuming. `KtxArm.InProcess` is an ACCELERATION row that yields to the floor rather than diverging from it: every in-process encode runs the floor's own `ktx validate --format mini-json` over its bytes before yielding — the verdict reads the report's `valid` field, never the process status, and `--gltf-basisu` spells exactly where the payload transcodes — and a refused validation falls to the CLI arm where the payload row has one, so a set never carries bytes one branch can write and another cannot read. `astc` and `rawBcn` have NO CLI arm — `ktx create --encode` speaks only the Basis pair — so their validation refusal rails rather than falling back, which is exactly the branch-local posture their wire illegality declares. Every create spells the frozen color-assignment pair — `--assign-tf` with `--assign-primaries` — under `--fail-on-color-conversions`, so a relabel is total and an implicit conversion is a tool refusal rather than a silently re-tagged plane.
- Law: the layered and cube container is the block row's OWN shape, so `HoldsLayers` is true at the row and true at the encoder. Every layered chain drains per `(level, layer)` through the same staging every flat level takes, encodes into a `TextureFormat.GetByteCount`-sized window through the bake-scoped coder, and mints one `TextureSubresource` per slot; the fold then hands one `KtxTexture` carrying the whole subresource list to `KtxCodec.Write`, which resolves NO coder and therefore never reaches the process-global registry the mip-chain facade does. Face count reads the chain's own layer census against the six-face cube law, so a six-face dome is one blob a `wgpu` upload binds directly rather than a six-file fan whose faces carry six content addresses and no container relation. Its CLI arm carries no layer leg — `ktx create` takes a per-layer file fan this estate does not stage — so a layered chain that falls out of the in-process arm rails rather than silently writing layer zero under a whole-chain name.
- Law: a reader branches on the parsed PAYLOAD CLASS, never on the header's Vulkan token. Supercompression leaves a KTX2 declaring an undefined Vulkan format until a transcode runs, so that token reads undefined for every wire-legal UASTC and ETC1S file — a reader branching on it classes the entire wire-legal population as malformed. `KtxPayload.Transcodable` is the branch: a container whose parse resolves a coder decodes per level in process, and one whose supercompressed payload no in-process coder serves crosses the CLI's own `ktx transcode` into a coder-servable file first — the tool's default target family, so no unverified flag spelling rides the spawn.
- Law: the container version is a WRITE decision defaulting to KTX1, so every wire-bound encode sets version 2 EXPLICITLY. `KtxEncodingOptions` is mutable, so `InProcess` mints one fresh per encode: one instance carried across profiles silently re-versions a later payload.
- Law: the coder registry is a process-static global in the composed engine — `TextureCoderManager.Global` — so this gate binds its OWN `TextureCoderManager` per bake: `TryGetCoder` creates the built-in coder for a standard format lazily ON THE INSTANCE, so the bake-scoped manager needs no registration call for decode, and an encode wanting a compression level registers the family coder itself with `TextureCompressionOptions` — the engine's own registration factory is INTERNAL and unreachable, so the per-family coder constructors are the public spelling of the same ladder.
- Law: the engine's convenience facade is `Rgba8UNorm`-bound at the SIGNATURE, and the measured block-encode bound makes that the CORRECT staging depth for every block class: a block encode stages `Unorm8` in the ENCODED domain — the quantization is the payload class's own, stated at the stage rather than smuggled by a facade — while the `none` deep store and the BC6H raw row stage `Rgba32Float` at the plane's own depth.
- Packages: TextureCompressor (composed — `ITextureCoder.Encode<TPixel>(BitmapView<TPixel>, Span<byte>)`/`Decode<TPixel>`/`GetEncodedByteCount`, `TextureCoderManager.TryGetCoder`/`GetCoder` on a bake-scoped instance with lazy built-in coder creation, `TextureCoderManager.Register(TextureFormat, ITextureCoder)` for the options-bearing encode coders, `TextureCompressionOptions.CompressionMode` + `TextureCompressionLevel`, `AstcTextureCoder`/`BptcTextureCoder`/`S3tcTextureCoder`/`RgtcLatcTextureCoder` the public family ctors over `(TextureFormat, TextureCompressionOptions)`, `TextureFormat.GetByteCount(int, int)` the payload sizing, `ArrayBitmap<TPixel>.AsView`, `new TextureSubresource(int, int, int, int, int, byte[])` the mip-layer-face slot, `TextureImage.Format`/`GetSubresource(int, int, int)`/`MipLevelCount`/`ArrayLayerCount`/`FaceCount`/`Subresources`, `TextureSubresource.Width`/`Height`/`Payload`), TextureCompressor.FileFormats.Ktx (composed — `KtxCodec.Read(ReadOnlySpan<byte>)`, `KtxCodec.EncodeMipChain<TPixel>(IReadOnlyList<IBitmap<TPixel>>, KtxEncodingOptions?)`, `KtxCodec.Write(KtxTexture, KtxEncodingOptions?) -> byte[]`, `new KtxTexture(TextureFormat, IReadOnlyList<TextureSubresource>, int, int)`, `new KtxEncodingOptions { Version = KtxVersion.Version2, … }`, `KtxTexture.Texture`), `plane#TEXTURE_PLANE` (composed — the typed arena, its row rails, and `PlanePrimaries.Assign`), `Rasm.Domain` (`Op`), BCL inbox (`System.Diagnostics.Process` + `ProcessStartInfo.ArgumentList` at the CLI arm alone).
- Growth: a new payload class is one `KtxPayload` row; a new block layout is one `BlockFormat` row. `KtxGate` keeps its own body unchanged by either, because both resolve their `TextureFormat` value off the row.
- Boundary: the CLI arm's provisioning evidence is a PRESENCE and subcommand-roster probe, never a version string — the provisioned binaries report an absent revision for a version query because the packaging strips their source metadata, so a probe asserting version text fails against a correctly provisioned tool.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Globalization;                       // CultureInfo — the CLI row table's numeric spelling
using System.IO;                                  // Path, File, Directory — the CLI arm's sidecar staging
using System.Linq;
using System.Runtime.InteropServices;             // MemoryMarshal — the Rgba32Float lane cast
using System.Text.Json;                           // JsonDocument — the validate verdict reads the mini-json `valid` field
using CommunityToolkit.HighPerformance.Buffers;   // MemoryOwner — the encoded staging rental
using LanguageExt;
using Rasm.Domain;                                // Op
using Rasm.Drawing;                               // ChannelDtype
using TextureCompressor.Bitmaps;                  // IBitmap, ArrayBitmap, BitmapView
using TextureCompressor.Codecs;                   // ITextureCoder
using TextureCompressor.Colors;                   // Rgba32Float, Rgba8UNorm
using TextureCompressor.FileFormats.Ktx;
using TextureCompressor.Formats;
using TextureCompressor.Registry;                 // TextureCoderManager
using static LanguageExt.Prelude;

namespace Rasm.Materials.Raster;

// --- [OPERATIONS] --------------------------------------------------------------------------
internal static class KtxGate {
    // Every wire-legal supercompressed file reads an undefined Vulkan token in its header, so the branch is the
    // PARSED payload's coder resolution: a format the bake-scoped manager serves decodes per level in process, and a
    // supercompressed payload no coder serves crosses the floor's own `ktx transcode` into a servable file first — the
    // tool's default target family, so no unverified flag spelling rides the spawn.
    internal static Fin<TexturePyramid> Decode(ReadOnlyMemory<byte> payload, Op key) {
        KtxTexture container = KtxCodec.Read(payload.Span);
        TextureCoderManager coders = new();
        return coders.TryGetCoder(container.Texture.Format, out ITextureCoder? coder) && coder is not null
            ? Materialize(container, coder, key)
            : Transcoded(payload, key);
    }

    // Materialize maps the container's OWN pyramid onto the plane chain: a KTX2 holds its mip levels, so each level decodes off
    // its subresource payload through the bake-scoped coder — TryGetCoder creates the built-in coder lazily on THIS
    // instance, so nothing leaks into the process-global registry — and the chain assembles in level order; a refold
    // here would silently replace the encoder's own filter. The recorded policy is the estate's box assumption over a
    // FOREIGN fold: an ingested chain's levels adopt verbatim and are never re-derived from the policy row, so the
    // assumption steers only a later re-fold decision. Rgba32Float staging is the non-quantizing decode path; the
    // srgb-ness of the declared format decides the transfer tag, the float rows land at the plane's own Float32, and the
    // primaries stay ABSENT because the container's parsed carrier publishes its format tokens and no data-format
    // descriptor read (see `[06]`).
    private static Fin<TexturePyramid> Materialize(KtxTexture container, ITextureCoder coder, Op key) {
        TextureFormat declared = container.Texture.Format;
        PlaneTransfer transfer = SrgbDeclared(declared) ? PlaneTransfer.Srgb : PlaneTransfer.Linear;
        PlaneFormat storage = FloatDeclared(declared) ? PlaneFormat.Rgba32F : PlaneFormat.Rgba16F;
        return toSeq(Enumerable.Range(0, container.Texture.MipLevelCount))
            .Fold(Fin.Succ(Seq<TexturePlane>()), (state, level) => state.Bind(levels =>
                Level(container, coder, level, storage, transfer, key).Map(levels.Add)))
            .Map(levels => new TexturePyramid(levels,
                container.Texture.MipLevelCount > 1 ? MipPolicy.Box : MipPolicy.None, Coupled: false));
    }

    // Two CLOSED comparisons against the roster's own format values — the engine exposes no value-kind read, so the
    // rows this estate writes are the rows this probe names.
    private static bool SrgbDeclared(TextureFormat declared) =>
        KtxPayload.Items.Any(row => row.SrgbFormat == declared) || BlockFormat.Items.Any(row => row.SrgbFormat == declared)
        || declared == TextureFormats.Rgba8Srgb;
    private static bool FloatDeclared(TextureFormat declared) =>
        declared == TextureFormats.Rgba32Float || declared == TextureFormats.Rgba16Float
        || declared == TextureFormats.Bc6HUFloat || declared == TextureFormats.Bc6HSFloat;

    // Payload is a byte[], not a memory carrier — the coder's ReadOnlySpan<byte> parameter takes the array
    // through its own implicit conversion, and a .Span projection on it does not compile.
    private static Fin<TexturePlane> Level(
        KtxTexture container, ITextureCoder coder, int level, PlaneFormat storage, PlaneTransfer transfer, Op key) {
        TextureSubresource subresource = container.Texture.GetSubresource(level, arrayLayer: 0);
        ArrayBitmap<Rgba32Float> bitmap = new(subresource.Width, subresource.Height);
        coder.Decode(subresource.Payload, bitmap.AsView());
        return RasterCodec.Fill(MemoryMarshal.Cast<Rgba32Float, float>(bitmap.PixelSpan), subresource.Width,
                subresource.Height, 4, storage, transfer, PlanePrimaries.Unknown, AlphaMode.Straight, PlaneRange.Unit, key)
            .Map(static chain => chain.Base);
    }

    // Transcoded is the supercompressed fallback: the floor's own `ktx transcode` at its DEFAULT target family lands a file the
    // in-process coders serve, and the re-read takes the ordinary path. Only a Basis payload reaches here, so the
    // default 8-bit target is depth-faithful to the LDR content it holds.
    private static Fin<TexturePyramid> Transcoded(ReadOnlyMemory<byte> payload, Op key) {
        string stage = Path.Combine(Path.GetTempPath(), $"rasm-ktx-{Guid.NewGuid():N}");
        Directory.CreateDirectory(stage);
        try {
            string source = Path.Combine(stage, "in.ktx2"), sink = Path.Combine(stage, "out.ktx2");
            File.WriteAllBytes(source, payload.Span);
            return Run(["transcode", source, sink], key)
                .Bind(_ => Decode(File.ReadAllBytes(sink), key));
        } finally { Directory.Delete(stage, recursive: true); }
    }

    // Encode splits on the SUBJECT, never on a flag: a layered chain is the container's own subresource shape and
    // takes the in-process layered writer, which resolves no coder inside the container codec and therefore never
    // touches the process-global registry; a single-layer chain takes the arm the policy names. Every in-process
    // arm VALIDATES its own bytes through the floor's `ktx validate` before yielding, and a refused validation
    // falls to the CLI arm where the payload row has one — the Basis pair and the deep store — while `astc` and
    // `rawBcn` rail, because `ktx create --encode` speaks only the Basis pair and a branch-local payload has no
    // cross-branch floor to agree with. A layered chain has no CLI leg at all, so its refusal is terminal rather
    // than a silent write of layer zero under a whole-chain name.
    internal static Fin<ReadOnlyMemory<byte>> Encode(TexturePyramid chain, EncodePolicy policy, Op key) =>
        chain.Base.Layers.Value is not 1
            ? Layered(chain, policy, key).Bind(bytes => Validated(bytes, policy, key))
            : policy.Arm == KtxArm.InProcess
                ? InProcess(chain, policy, key)
                    .Bind(bytes => Validated(bytes, policy, key)
                        .BindFail(_ => policy.Payload == KtxPayload.Astc || policy.Payload == KtxPayload.RawBcn
                            ? Fin.Fail<ReadOnlyMemory<byte>>(RasterFault.Encode(key, $"<ktx-validate:{policy.Payload.Key}>"))
                            : Cli(chain, policy, key)))
                : policy.Payload == KtxPayload.Astc || policy.Payload == KtxPayload.RawBcn
                    ? RasterFault.Encode(key, $"<ktx-cli-arm:{policy.Payload.Key}>")
                    : Cli(chain, policy, key);

    // Layered writes the container's own subresource shape. Every (level, layer) slot drains through the one staging seat, encodes into a
    // GetByteCount-sized window through the bake-scoped coder, and mints its own TextureSubresource; the whole list
    // then writes as one KtxTexture whose layer and face census the chain itself states — six layers ARE the cube,
    // which is what makes a dome one addressable blob. KtxCodec.Write resolves no coder, so this leg is the one
    // container write a bake-scoped manager genuinely serves.
    private static Fin<ReadOnlyMemory<byte>> Layered(TexturePyramid chain, EncodePolicy policy, Op key) {
        PlaneTransfer transfer = chain.Base.Transfer;
        int layers = chain.Base.Layers.Value;
        int faces = layers is 6 ? 6 : 1;
        return Resolve(chain, policy, transfer, key).Bind(format => {
            TextureCoderManager coders = new();
            return coders.TryGetCoder(format, out ITextureCoder? coder) && coder is not null
                ? Slots(chain, coder, format, layers, faces, key).Map(slots =>
                    (ReadOnlyMemory<byte>)KtxCodec.Write(
                        new KtxTexture(format, slots.ToList(), faces is 6 ? 1 : layers, faces), Options(format, policy, transfer)))
                : Fin.Fail<ReadOnlyMemory<byte>>(RasterFault.Encode(key, $"<ktx-layer-coder:{format}>"));
        });
    }

    private static Fin<Seq<TextureSubresource>> Slots(
        TexturePyramid chain, ITextureCoder coder, TextureFormat format, int layers, int faces, Op key) =>
        toSeq(chain.Levels.Select((level, index) => (Level: level, Index: index)))
            .Fold(Fin.Succ(Seq<TextureSubresource>()), (state, slot) => state.Bind(built =>
                toSeq(Enumerable.Range(0, layers)).Fold(Fin.Succ(built), (inner, layer) => inner.Bind(rows =>
                    slot.Level.Layer(layer, key).Map(face => {
                        using MemoryOwner<byte> payload =
                            MemoryOwner<byte>.Allocate(format.GetByteCount(face.Width.Value, face.Height.Value));
                        coder.Encode(StageBlock(face).AsView(), payload.Span);
                        return rows.Add(new TextureSubresource(slot.Index,
                            faces is 6 ? 0 : layer, faces is 6 ? layer : 0,
                            face.Width.Value, face.Height.Value, payload.Span.ToArray()));
                    }))));

    // InProcess mints a fresh options object naming version 2 explicitly — the engine defaults to KTX1 — and stages
    // per the payload row's OWN bound: a block class stages Unorm8 in the encoded domain (the measured 8-bit bound), the
    // deep store stages Rgba32Float at the plane's depth, and the raw-BCn row resolves its format off BlockFormat
    // with BC6H alone staying float. Quality is the CLI arm's knob: EncodeMipChain resolves its coders internally, so a
    // level-bearing in-process encode is not expressible and the acceleration row trades the knob for the spawn.
    private static Fin<ReadOnlyMemory<byte>> InProcess(TexturePyramid chain, EncodePolicy policy, Op key) {
        PlaneTransfer transfer = chain.Base.Transfer;
        return Resolve(chain, policy, transfer, key).Map(format => {
            KtxEncodingOptions options = Options(format, policy, transfer);
            bool ldr = policy.Payload.BlockCompressed && format != TextureFormats.Bc6HUFloat && format != TextureFormats.Bc6HSFloat;
            return ldr
                ? (ReadOnlyMemory<byte>)KtxCodec.EncodeMipChain(StageLdr(chain), options)
                : (ReadOnlyMemory<byte>)KtxCodec.EncodeMipChain(StageFloat(chain), options);
        });
    }

    // ONE format resolution and ONE options mint, so the flat, layered, and mip-chain legs never drift a version, a
    // scheme, or an sRGB flag between them.
    private static Fin<TextureFormat> Resolve(TexturePyramid chain, EncodePolicy policy, PlaneTransfer transfer, Op key) =>
        policy.Payload == KtxPayload.RawBcn
            ? Optional(policy.Block.Resolve(transfer)).ToFin(RasterFault.Encode(key, $"<ktx-block-format:{policy.Block.Key}>"))
            : policy.Payload == KtxPayload.None
                ? Fin.Succ(chain.Base.Format.Depth == ChannelDtype.Float16 ? TextureFormats.Rgba16Float : TextureFormats.Rgba32Float)
                : Optional(policy.Payload.Resolve(transfer)).ToFin(RasterFault.Encode(key, $"<ktx-payload-format:{policy.Payload.Key}>"));

    private static KtxEncodingOptions Options(TextureFormat format, EncodePolicy policy, PlaneTransfer transfer) => new() {
        Version = KtxVersion.Version2,
        TextureFormat = format,
        SupercompressionScheme = policy.Payload.Scheme,
        GenerateMipmaps = false,
        IsSrgb = transfer == PlaneTransfer.Srgb,
    };

    // Both stagings drain ENCODED unit lanes through the one codec bridge and seat them by the same lane-to-register
    // correspondence the managed leg uses — the block staging quantizes to Unorm8 HERE, stated at the stage rather
    // than smuggled by a facade, because the block coders admit no deeper store; the quantizer itself is the plane
    // owner's one U8.FromUnit, never a re-spelled clamp-round ladder.
    private static IReadOnlyList<IBitmap<Rgba32Float>> StageFloat(TexturePyramid chain) =>
        chain.Levels.Map(StageFloat).ToList();

    private static IBitmap<Rgba32Float> StageFloat(TexturePlane level) {
        using MemoryOwner<float> staging = RasterCodec.Drain(level);
        ArrayBitmap<Rgba32Float> bitmap = new(level.Width.Value, level.Height.Value);
        Seat(staging.Span, level, bitmap.PixelSpan, static lanes =>
            new Rgba32Float(lanes[0], lanes[1], lanes[2], lanes[3]));
        return bitmap;
    }

    private static IReadOnlyList<IBitmap<Rgba8UNorm>> StageLdr(TexturePyramid chain) =>
        chain.Levels.Map(StageBlock).ToList();

    private static ArrayBitmap<Rgba8UNorm> StageBlock(TexturePlane level) {
        using MemoryOwner<float> staging = RasterCodec.Drain(level);
        ArrayBitmap<Rgba8UNorm> bitmap = new(level.Width.Value, level.Height.Value);
        Seat(staging.Span, level, bitmap.PixelSpan, static lanes => new Rgba8UNorm(
            U8.FromUnit(lanes[0]),
            U8.FromUnit(lanes[1]),
            U8.FromUnit(lanes[2]),
            U8.FromUnit(lanes[3])));
        return bitmap;
    }

    private static void Seat<TPixel>(
        ReadOnlySpan<float> staging, TexturePlane level, Span<TPixel> pixels, Func<float[], TPixel> compose)
        where TPixel : unmanaged {
        int lanes = level.Lanes, colour = level.Alpha.Carries ? lanes - 1 : lanes;
        float[] texel = new float[4];
        for (int i = 0; i < pixels.Length; i++) {
            int at = i * lanes;
            texel[0] = staging[at];
            texel[1] = colour > 1 ? staging[at + 1] : staging[at];
            texel[2] = colour > 2 ? staging[at + 2] : (colour == 2 ? 0f : staging[at]);
            texel[3] = level.Alpha.Carries ? staging[at + lanes - 1] : 1f;
            pixels[i] = compose(texel);
        }
    }

    // Validated stages the bytes and runs the floor's own validator — the CLI-equivalence proof is the floor's own
    // verdict over the produced container, never a local re-parse that would prove only what this engine believes. The
    // verdict rides the mini-json report's `valid` field per the frozen conformance law, NEVER the process status —
    // and `--gltf-basisu` spells EXACTLY where the payload transcodes, because an RGBSDA deep container under that
    // flag fails error-6301 by design, so the flag reads the payload row and never rides unconditionally.
    private static Fin<ReadOnlyMemory<byte>> Validated(ReadOnlyMemory<byte> bytes, EncodePolicy policy, Op key) {
        string stage = Path.Combine(Path.GetTempPath(), $"rasm-ktx-{Guid.NewGuid():N}");
        Directory.CreateDirectory(stage);
        try {
            string candidate = Path.Combine(stage, "candidate.ktx2");
            File.WriteAllBytes(candidate, bytes.Span);
            KtxRun run = Spawn(Seq("validate", "--format", "mini-json")
                + (policy.Payload.Transcodable ? Seq1("--gltf-basisu") : Seq<string>())
                + Seq1(candidate));
            return Try.lift(() => {
                    using JsonDocument report = JsonDocument.Parse(run.Stdout);
                    return report.RootElement.GetProperty("valid").GetBoolean();
                }).Run().Match(
                    Succ: valid => valid
                        ? Fin.Succ(bytes)
                        : Fin.Fail<ReadOnlyMemory<byte>>(RasterFault.Encode(key, $"<ktx-validate:{policy.Payload.Key}:{run.Stdout}>")),
                    Fail: _ => Fin.Fail<ReadOnlyMemory<byte>>(RasterFault.Encode(key, $"<ktx-validate-report:{run.ExitCode}:{run.Stderr}>")));
        } finally { Directory.Delete(stage, recursive: true); }
    }

    // Cli is the provisioned floor, the SAME binary the python estate spawns, so one encoder produces both spawning
    // branches' bytes. Levels stage as per-level EXR sidecars — the float-faithful interchange the tool ingests —
    // and the argument set is a TYPED ROW TABLE over ArgumentList: `--assign-tf` labels the transfer without a
    // conversion pass (the sidecar bytes already carry the plane's encoding), the Basis rows carry their encode and
    // quality flags, and the deep store carries a float format with no encode flag and no supercompression. A
    // non-zero exit rails RasterFault.Encode carrying the tool's stderr in the discriminant.
    private static Fin<ReadOnlyMemory<byte>> Cli(TexturePyramid chain, EncodePolicy policy, Op key) {
        string stage = Path.Combine(Path.GetTempPath(), $"rasm-ktx-{Guid.NewGuid():N}");
        Directory.CreateDirectory(stage);
        try {
            string sink = Path.Combine(stage, "out.ktx2");
            return toSeq(chain.Levels.Select((level, index) => (Level: level, Index: index)))
                .Fold(Fin.Succ(Seq<string>()), (state, slot) => state.Bind(leaves =>
                    RasterCodec.Encode(new TexturePyramid(Seq1(slot.Level), MipPolicy.None, Coupled: false),
                            RasterFormat.Exr, policy, key)
                        .Map(bytes => {
                            string leaf = Path.Combine(stage, $"level{slot.Index:D2}.exr");
                            File.WriteAllBytes(leaf, bytes.Span);
                            return leaves.Add(leaf);
                        })))
                .Bind(leaves => Run(CreateArgs(chain, policy, leaves, sink), key))
                .Map(_ => (ReadOnlyMemory<byte>)File.ReadAllBytes(sink));
        } finally { Directory.Delete(stage, recursive: true); }
    }

    // CreateArgs is the `ktx create` row table. --raw is NOT taken: EXR sidecars are a supported input container, and `ktx create
    // --raw` demands one raw file per level under its own inflexible framing — the EXR leg carries the same per-level
    // fan with a self-describing container. Block rows name the measured R8 input family; the deep store names the
    // plane's own float family. The COLOR ASSIGNMENT pair spells on EVERY create per the frozen measured law —
    // `--assign-tf` and `--assign-primaries` RELABEL without touching a texel, and `--fail-on-color-conversions` then
    // turns any remaining implicit conversion into a tool refusal rather than a silently re-tagged plane; the
    // texcoord origin stays the frozen top-left storage default the tool already writes. Primaries read the PLANE's
    // own declared row rather than deriving from its transfer, so an ingested Rec.709 dome labels `bt709`, a
    // press-baked working-space plane labels `acescc`, and a plane whose container declared nothing labels `none` — the
    // honest absence a transfer-derived label would have overwritten. Raw rides `linear` on `--assign-tf` BY LAW: the
    // identity transfer is the only tf token a parameter plane can wear, the python leg's proven spelling.
    private static Seq<string> CreateArgs(TexturePyramid chain, EncodePolicy policy, Seq<string> leaves, string sink) {
        bool srgb = chain.Base.Transfer == PlaneTransfer.Srgb;
        Seq<string> format = policy.Payload.BlockCompressed
            ? Seq("--format", srgb ? "R8G8B8A8_SRGB" : "R8G8B8A8_UNORM")
            : Seq("--format", chain.Base.Format.Depth == ChannelDtype.Float16 ? "R16G16B16A16_SFLOAT" : "R32G32B32A32_SFLOAT");
        Seq<string> encode = policy.Payload == KtxPayload.Etc1s
            ? Seq("--encode", "basis-lz", "--qlevel", Math.Clamp(policy.Quality, 1, 255).ToString(CultureInfo.InvariantCulture))
            : policy.Payload == KtxPayload.Uastc
                ? Seq("--encode", "uastc", "--zstd", "18")
                : Seq<string>();
        Seq<string> levels = chain.Levels.Count > 1
            ? Seq("--levels", chain.Levels.Count.ToString(CultureInfo.InvariantCulture))
            : Seq<string>();
        return Seq1("create") + format + encode + levels
            + Seq("--assign-tf", srgb ? "srgb" : "linear")
            + Seq("--assign-primaries", chain.Base.Primaries.Assign)
            + Seq1("--fail-on-color-conversions")
            + leaves + Seq1(sink);
    }

    // ONE spawn shape for create, transcode, and validate: ArgumentList rows, both streams captured. Each verb then
    // applies its OWN verdict law — create and transcode rail on the exit status through Run, while validate reads the
    // mini-json `valid` field off Spawn's stdout because the frozen conformance law pins the report as the verdict,
    // never the process status. The provisioning probe asserts presence and the subcommand roster, never version
    // text, because the packaging strips the binaries' source metadata and a version assertion fails against a
    // correct provisioning.
    private sealed record KtxRun(int ExitCode, string Stdout, string Stderr);

    private static KtxRun Spawn(Seq<string> args) {
        using System.Diagnostics.Process ktx = new() {
            StartInfo = new System.Diagnostics.ProcessStartInfo("ktx") {
                RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false },
        };
        args.Iter(arg => ktx.StartInfo.ArgumentList.Add(arg));
        ktx.Start();
        string stdout = ktx.StandardOutput.ReadToEnd();
        string stderr = ktx.StandardError.ReadToEnd();
        ktx.WaitForExit();
        return new KtxRun(ktx.ExitCode, stdout, stderr);
    }

    private static Fin<Unit> Run(Seq<string> args, Op key) =>
        Spawn(args) switch {
            { ExitCode: 0 } => Fin.Succ(Unit.Default),
            var run => RasterFault.Encode(key, $"<ktx-{args.Head}:{run.ExitCode}:{run.Stderr}>"),
        };
}
```

## [06]-[RESEARCH]

- [EXR_TILE_BUFFER_EXTENT]-[OPEN]: does `ExrWriter.WriteTile` size each `ChannelBuffer` at the full `TileSizeX x TileSizeY` with the out-of-extent remainder zero-filled, as `[04]` writes it, or at the clipped region a border tile covers; route: `uv run python -m tools.assay api query TinyEXR.V3.ExrWriter --key TinyEXR.NET` beside `TinyEXR.V3.ChannelBuffer`, reading whether `SampleCount` validates against the tile description
- [KTX_DATA_FORMAT_PRIMARIES]-[OPEN]: does `KtxTexture` publish any read of the Khronos data-format descriptor a KTX2 declares its colour primaries in, given it exposes `VkFormat` and the four `Gl*` tokens alone and `[05]` `Materialize` therefore records `PlanePrimaries.Unknown`; route: `uv run python -m tools.assay api query TextureCompressor.FileFormats.Ktx.KtxTexture --key TextureCompressor.FileFormats.Ktx`, falling back to the CLI's own `ktx info --format json` report as the ingest read
