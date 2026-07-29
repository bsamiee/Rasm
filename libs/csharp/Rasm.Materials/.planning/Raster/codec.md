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
- Cases: `Decode`, `Encode`, `Device`, `Tile` — this page mints the two container cases, `gpu#PRESS_DEVICE` is the sole `Device` producer, and `tile#TILE_SYNTH` the sole `Tile` producer; the family homes here because the band is one registry row however many pages mint its cases.
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

- Owner: `RasterEngine` the reader-writer family; `RasterFormat` the container roster; `BlockFormat` the block-layout roster over the composed `TextureFormat` values; `KtxPayload` the KTX2 payload-class roster carrying its wire legality, its supercompression scheme, and its 8-bit block-encode bound; `KtxArm` the composition posture; `EncodePolicy` the per-encode row.
- Cases: engine {`Managed`, `OpenExr`, `Radiance`, `Ktx`} · format {`png16`, `tiff16`, `webp`, `qoi`, `exr`, `exrDeep`, `hdr`, `ktx2`} · block {`bc1`…`bc7`, `bc6h`, `none`} · payload {`rawBcn`, `uastc`, `etc1s`, `astc`, `none`} · arm {`cli`, `inProcess`}.
- Law: `RasterEngine.Managed` carries the package's OWN `IImageFormat` singleton and its encoder factory, so four of the eight container rows share one case and format identity is a reference comparison rather than a name compare. `IImageFormat.Name` differs in casing across the package's own rows (`"PNG"`, `"TIFF"`, `"Webp"`, `"QOI"`), so an ordinal name switch silently claims nothing for WebP — the instance is what the sniff returns and the instance is what the row holds.
- Law: `MaxDepth` is a ROW FACT the encode gate reads through `Admits`, and it is the honest ceiling rather than the advertised one. `Admits` compares DEPTH ROWS, never byte widths — `u16` and `f16` share a byte width, so a byte compare would pass an `rgba16f` plane through the `png16` ceiling and silently narrow it through the integer texel; an integer-ceiling row therefore admits only integer depths at or below its ceiling, and a float-ceiling row admits every depth its width holds. `TiffBitsPerPixel` tops out below any float row and carries no 16-bit-plus-alpha row, so a four-lane 16-bit plane routed at `tiff16` drops its alpha lane rather than refusing — the row therefore declares `Rgba16`'s depth as its ceiling and the encoder states `BitsPerPixel` explicitly, because an unset depth knob is an INFERENCE and inference is what quietly ships an 8-bit channel.
- Law: `qoi` is a fast lossless EIGHT-BIT row. It admits for a preview or a thumbnail egress and never for a channel plane, because an 8-bit intermediate on a texture path is a silent quantization no downstream consumer can recover. The `set#TEXTURE_SET` egress grammar admits only the frozen `<ext>` roster, which carries no `qoi` — so the preview row is structurally unreachable from a set leaf and `Extension` stays the one `<ext>` source for every row the egress CAN reach.
- Law: EXR is `TinyEXR.NET`'s WHOLE, flat scanline included — the held ImageSharp major ships no EXR codec, so `exr` and `exrDeep` are two rows of one engine case discriminating on the part type rather than two engines. Per-channel FILES are the canonical cross-branch form; multipart, named-AOV, and tiled files are branch-local optimization, so no parity fixture depends on a leg one branch alone can write.
- Law: `hdr` is an INGEST row. Radiance stores three 8-bit mantissas under one shared exponent, so the expansion to float does not recover the quantization and every PRODUCED environment product egresses `exr` or `ktx2`.
- Law: `KtxPayload.WireLegal` is the gate `set#TEXTURE_SET` reads at egress. `rawBcn` and `astc` are desktop payloads no Basis-transcoding consumer reads, so neither appears on a manifest-borne channel row; `uastc`, `etc1s`, and `none` are the three the web wire admits, and each block row carries the supercompression scheme and the sRGB and linear `TextureFormat` values its encode names.
- Law: BLOCK COMPRESSION IS 8-BIT-INPUT-ONLY — the measured bound on both legs, carried as the `BlockCompressed` row fact. `ktx create --encode` admits only `R8*` formats and the in-process block coders refuse a deeper store, so a block encode STAGES its levels at `u8` in the encoded domain and a float or half plane — the HDR IBL pyramid, a solver-grade plane — takes `KtxPayload.None`: the uncompressed deep KTX2 whose `vk_format` is set, whose `needs_transcoding` is false, and whose depth ceiling is the container's own `f32`. ASTC-HDR is unreachable on the provisioned toolchain, so `astc` composes only at LDR.
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
        static payload => Sniff(payload) == PngFormat.Instance);

    public static readonly RasterFormat Tiff16 = new("tiff16", "tif", AlphaMode.Straight, PlaneDepth.U16,
        new RasterEngine.Managed(TiffFormat.Instance, static _ => new TiffEncoder {
            BitsPerPixel = TiffBitsPerPixel.Bit48, PhotometricInterpretation = TiffPhotometricInterpretation.Rgb }),
        static payload => Sniff(payload) == TiffFormat.Instance);

    public static readonly RasterFormat WebP = new("webp", "webp", AlphaMode.Straight, PlaneDepth.U8,
        new RasterEngine.Managed(WebpFormat.Instance, static _ => new WebpEncoder { FileFormat = WebpFileFormatType.Lossless }),
        static payload => Sniff(payload) == WebpFormat.Instance);

    public static readonly RasterFormat Qoi = new("qoi", "qoi", AlphaMode.Straight, PlaneDepth.U8,
        new RasterEngine.Managed(QoiFormat.Instance, static _ => new QoiEncoder()),
        static payload => Sniff(payload) == QoiFormat.Instance);

    // ONE managed sniff shared by the four Managed probes. The held major's Image.DetectFormat THROWS
    // UnknownImageFormatException on foreign bytes, so a per-row bare call would tear the claim fold down on
    // every EXR, HDR, and KTX2 payload before those rows' own probes ever ran — this is the platform-forced
    // boundary capture, and null is the typed no-claim the probes compare against.
    private static IImageFormat? Sniff(ReadOnlySpan<byte> payload) {
        try { return Image.DetectFormat(payload); }
        catch (UnknownImageFormatException) { return null; }
        catch (InvalidImageContentException) { return null; }
    }

    // ExrDeep DECLARES BEFORE Exr so the ordered claim fold resolves a deep file to the deep row first: the part
    // type strings "deepscanline"/"deeptile" appear verbatim in the header's type attribute, so the probe is a
    // header-window byte scan and the flat row then claims every remaining EXR.
    public static readonly RasterFormat ExrDeep = new("exrDeep", "exr", AlphaMode.Associated, PlaneDepth.F32,
        new RasterEngine.OpenExr(Deep: true), static payload => ExrFile.IsExr(payload) && SniffDeep(payload));

    public static readonly RasterFormat Exr = new("exr", "exr", AlphaMode.Associated, PlaneDepth.F32,
        new RasterEngine.OpenExr(Deep: false), static payload => ExrFile.IsExr(payload));

    private static bool SniffDeep(ReadOnlySpan<byte> payload) {
        ReadOnlySpan<byte> window = payload[..Math.Min(payload.Length, 4096)];
        return window.IndexOf("deepscanline"u8) >= 0 || window.IndexOf("deeptile"u8) >= 0;
    }

    public static readonly RasterFormat Hdr = new("hdr", "hdr", AlphaMode.None, PlaneDepth.F32,
        new RasterEngine.Radiance(), static payload => HdrCodec.HasRadianceHeader(payload));

    // The container ceiling is the DEEP store's f32: KtxPayload.None carries float planes uncompressed, and the
    // block rows gate their OWN u8 staging bound at the payload row rather than at the container.
    public static readonly RasterFormat Ktx2 = new("ktx2", "ktx2", AlphaMode.Straight, PlaneDepth.F32,
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

    // Depth admission compares ROWS, never byte widths: u16 and f16 share two bytes, so a byte compare would narrow
    // an rgba16f plane through the png16 integer texel silently. An integer ceiling admits integer depths at or
    // below it; a float ceiling admits every depth its width holds.
    public bool Admits(PlaneDepth depth) =>
        depth.Bytes <= MaxDepth.Bytes && (!MaxDepth.Integer || depth.Integer);

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
// names; BlockCompressed carries the measured 8-bit staging bound; the two TextureFormat columns are the target rows
// an encode resolves — the Basis rows for the transcodable pair, the ASTC LDR row for the in-process block lane, and
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
    // The measured bound: every block class stages u8 in the encoded domain before its coder or the CLI's --encode
    // runs; the none row alone carries the plane's own depth up to the container's f32 ceiling.
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
// EncodePolicy carries what the channel row resolves per encode. Compression is the EXR row: the lossy rows truncate
// or quantize float data, so a content-keyed or solver-grade plane takes ZIP and a lossy row never reaches a keyed
// plane. Block resolves the raw-BCn arm's own TextureFormat; Quality is the etc1s CLI `--qlevel` scale — the
// in-process arm resolves its coders internally and carries no level knob, which is the acceleration row's stated trade.
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
- Law: decode TAGS the file's canonical association onto the plane it mints — EXR is associated, PNG, TIFF, WebP, QOI, and KTX2 are straight, Radiance carries none — and the DECLARING consumer then normalizes to the plane's declared `AlphaMode` through the `plane#TEXTURE_PLANE` `ToAlpha` gate, which is where the frozen decode-normalizes direction lives: `Decode(payload, key)` carries no declaration, so the canonical tag is the honest intermediate and `set#SET_INGEST`'s per-role declaration is the one normalization site. Encode CONVERTS the plane's declaration into the format's canonical association through the same gate, so the 16-bit floor on a premultiply-state crossing is enforced once for the whole estate. Neither direction is a caller knob; the bridge itself moves ENCODED STORAGE lanes and never premultiplies, decodes, or unpacks, because those are plane declarations applied at `Read` and a bridge running them would double-apply every curve the file already carries.
- Law: encode REFUSES a plane whose depth the row's `Admits` denies rather than narrowing it. `Admits` exists to name exactly that silent narrow — a float plane on an integer-ceiling row included — and the caller either states an admitted plane or picks a row that holds the depth.
- Law: every composed container throws on a malformed payload — `ImageFormatException`, `UnknownImageFormatException`, and the block engine's own — so every package call crosses the `Try.lift(...).Run()` funnel and lowers with the foreign message preserved inside the `Detail` discriminant. `Funnel` catches every container exception at the boundary and carries the foreign message verbatim, so nothing escapes and no re-wrap erases it.
- Law: the ImageSharp leg crosses through ONE staging copy each way — `CopyPixelDataTo` drains the decoded image into the pooled pixel run, and `LoadPixelData<TPixel>(Configuration, ReadOnlySpan<TPixel>, int, int)` mints the encode image from it. The texel witnesses' `Compose`/`Project` speak encoded unit lanes and `Rgba64.ToScaledVector4`/`FromScaledVector4` speak the same domain, so the two copies carry no curve and no reinterpretation; a zero-copy `WrapMemory` bind is unreachable because the arena's texel structs are not ImageSharp pixel types, and a `MemoryMarshal` reinterpretation across that seam is exactly the stride fork the bridge law forbids.
- Packages: SixLabors.ImageSharp (composed — `Image.DetectFormat(ReadOnlySpan<byte>)` the throw-only sniff each row's probe reads through the one caught fold, `Image.Load<TPixel>(DecoderOptions, ReadOnlySpan<byte>)` naming the demanded depth so the file never decides the arena — the `Configuration` threads through `DecoderOptions`, no bare `(Configuration, span)` overload exists, `Image<TPixel>.CopyPixelDataTo(Span<TPixel>)` the decode drain, `Image.LoadPixelData<TPixel>(Configuration, ReadOnlySpan<TPixel>, int, int)` the encode mint, `Image.Save(Stream, IImageEncoder)`), TinyEXR.NET (composed — `ExrFile.LoadFromMemory`/`SaveToMemory` over `ReaderResult<Image>`/`WriterResult<byte[]>`, `Image.Parts`/`Part.Levels` the part-level walk, `PartConversion.ToInterleavedFloat`/`FromInterleavedFloat` the planar-interleaved bridge, `Header(PartType, Box2i, IEnumerable<Channel>, Compression, …)` + `DeepLevel(int, int, Box2i, ReadOnlySpan<int>, IEnumerable<ChannelBuffer>)` + `ChannelBuffer(string, PixelType, ReadOnlySpan<byte>)` the deep authoring surface, `DeepLevel.SampleCounts` the deep read fold, `ExrResult.WouldBlock` resumed rather than failed), TextureCompressor.FileFormats.Hdr (composed — `HdrCodec.Decode(ReadOnlySpan<byte>)`, `HdrCodec.Encode<TPixel>(IBitmap<TPixel>, HdrEncodingOptions?) -> byte[]` the byte-returning overload the decompile grounds), `plane#TEXTURE_PLANE` (composed — `TexturePlane.Of`/`Read`/`Write`/`ToAlpha`, `TexturePyramid.Of`/`Levels`/`Base`), `Rasm.Domain` (`Op`, the `Try` boundary funnel), LanguageExt.Core.
- Growth: a container added as a `RasterFormat` row reaches decode and encode with zero edits here, because both fold over the row's engine. `RasterEngine` grows by one case, which adds one arm to each of the two `Switch` folds and breaks the generated dispatch totally until both land.
- Boundary: this page owns CONTAINERS and never pixels. Transfer, association, range, and the decode ladder are `plane#PLANE_VOCABULARY`'s, resampling and derivation are `filter#PLANE_OP`'s, and channel semantics are `set#TEXTURE_CHANNEL`'s — so a codec never decides what a plane MEANS and never applies a colour transform a decode did not carry. `ReaderResult`/`WriterResult` values carry `Status` beside `IsSuccess`, and `ExrResult.WouldBlock` is a PROTOCOL state naming the byte range the reader wants, resumed by feeding exactly that window; classing it as failure stalls a healthy incremental read.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.IO;
using System.Linq;                                // Enumerable.Range — the deep channel-buffer fan
using System.Numerics;                            // Vector4 — the pixel ladder's scaled projection
using System.Runtime.InteropServices;             // MemoryMarshal — the ONE reinterpretation at the staging bridge
using CommunityToolkit.HighPerformance;           // Memory2D — the storage folds' view
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
    private static readonly Configuration Profile = Configured();

    private static Configuration Configured() {
        Configuration profile = Configuration.Default.Clone();
        profile.PreferContiguousImageBuffers = true;
        return profile;
    }

    // No declared format: the row's own probe claims the bytes. First match wins over the ordered roster, and an
    // unclaimed payload is a typed refusal rather than a guess. Every engine arm — the block container included —
    // crosses the one Funnel, because KtxCodec.Read throws on a malformed container exactly as the managed
    // packages do and an unfunnelled arm is the one escape hatch off the rail.
    public static Fin<TexturePyramid> Decode(ReadOnlyMemory<byte> payload, Op key) =>
        Claim(payload.Span)
            .ToFin(RasterFault.Decode(key, $"<raster-magic:{payload.Length}>"))
            .Bind(format => format.Engine.Switch(
                managed:  arm => Funnel(() => ReadManaged(payload.Span, format, key), key, decoding: true),
                openExr:  arm => Funnel(() => Exr(payload, arm.Deep, key), key, decoding: true),
                radiance: _   => Funnel(() => Radiance(payload.Span, key), key, decoding: true),
                ktx:      _   => Funnel(() => KtxGate.Decode(payload, key), key, decoding: true)));

    public static Fin<ReadOnlyMemory<byte>> Encode(TexturePyramid subject, RasterFormat format, EncodePolicy policy, Op key) =>
        !format.Admits(subject.Base.Format.Depth)
            ? RasterFault.Encode(key, $"<raster-depth:{subject.Base.Format.Depth.Key}>{format.MaxDepth.Key}@{format.Key}>")
            : Associate(subject, subject.Base.Alpha, format.CanonicalAlpha, key)
                .Bind(normalized => format.Engine.Switch(
                    managed:  arm => Funnel(() => WriteManaged(normalized, arm, key), key, decoding: false),
                    openExr:  arm => Funnel(() => WriteExr(normalized, arm.Deep, policy, key), key, decoding: false),
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
// Each leg rides the ONE staging bridge beneath it, and the bridge carries ENCODED UNIT LANES — the domain the
// texel witnesses' Project/Compose already speak. A container stores encoded values and so does the arena, so the
// bridge runs NO transfer, NO unpack, and NO un-association in either direction: those are declarations the plane
// carries and applies at Read. Routing container texels through the decode ladder would double-encode every write
// and hand every read a curve the file already applied — the defect this bridge shape forecloses.
namespace Rasm.Materials.Raster;

public static partial class RasterCodec {
    // ReadManaged mints ONE level at MipPolicy.None because the flat container held no pyramid. Rgba64 texels
    // project through ToScaledVector4 into encoded unit lanes — an 8-bit source expands losslessly and the unit
    // ratio v/255 equals v'/65535, so the row's OWN depth re-quantizes bit-faithful — and the storage row follows
    // the container ceiling: a u8 row lands Rgba8 so its round trip re-encodes, a u16 row lands Rgba16. The srgb
    // transfer is the integer colour container's declared assumption; set#SET_INGEST re-declares per role, which
    // re-keys nothing because identity is storage bytes alone.
    private static Fin<TexturePyramid> ReadManaged(ReadOnlySpan<byte> payload, RasterFormat format, Op key) {
        using Image<Rgba64> image = Image.Load<Rgba64>(new DecoderOptions { Configuration = Profile }, payload);
        using MemoryOwner<Rgba64> pixels = MemoryOwner<Rgba64>.Allocate(image.Width * image.Height);
        using MemoryOwner<float> staging = MemoryOwner<float>.Allocate(image.Width * image.Height * 4);
        image.CopyPixelDataTo(pixels.Span);
        for (int i = 0; i < pixels.Length; i++) {
            Vector4 texel = pixels.Span[i].ToScaledVector4();
            (staging.Span[i * 4], staging.Span[(i * 4) + 1], staging.Span[(i * 4) + 2], staging.Span[(i * 4) + 3]) =
                (texel.X, texel.Y, texel.Z, texel.W);
        }
        return Fill(staging.Span, image.Width, image.Height, 4,
            format.MaxDepth == PlaneDepth.U8 ? PlaneFormat.Rgba8 : PlaneFormat.Rgba16, PlaneTransfer.Srgb,
            format.CanonicalAlpha, PlaneRange.Unit, key);
    }

    // WriteManaged drains at the plane's OWN lane stride and seats the run through the one lane-to-register
    // correspondence in the ENCODED domain — a single lane replicates across RGB, a two-lane pair fills R and G
    // with B zero, and an absent coverage lane writes opaque — so a scalar r16 height plane egresses PNG16 as
    // grey rather than reading 2-4x past its own rental at a hardcoded stride.
    private static Fin<ReadOnlyMemory<byte>> WriteManaged(TexturePyramid chain, RasterEngine.Managed arm, Op key) {
        TexturePlane plane = chain.Base;
        int lanes = plane.Lanes, colour = plane.Alpha.Carries ? lanes - 1 : lanes;
        using MemoryOwner<float> staging = Drain(plane);
        using MemoryOwner<Rgba64> pixels = MemoryOwner<Rgba64>.Allocate(plane.Width.Value * plane.Height.Value);
        for (int i = 0; i < pixels.Length; i++) {
            int at = i * lanes;
            Rgba64 texel = default;
            texel.FromScaledVector4(new Vector4(
                staging.Span[at],
                colour > 1 ? staging.Span[at + 1] : staging.Span[at],
                colour > 2 ? staging.Span[at + 2] : (colour == 2 ? 0f : staging.Span[at]),
                plane.Alpha.Carries ? staging.Span[at + lanes - 1] : 1f));
            pixels.Span[i] = texel;
        }
        using Image<Rgba64> image = Image.LoadPixelData<Rgba64>(Profile, pixels.Span,
            plane.Width.Value, plane.Height.Value);
        using MemoryStream sink = new();
        image.Save(sink, arm.Encoder(plane.Format.Depth));
        return Fin.Succ((ReadOnlyMemory<byte>)sink.ToArray());
    }

    // EXR crosses through the container's own planar-to-interleaved bridge, so an arbitrary named-AOV part flattens
    // into the same encoded run every other leg fills — float storage IS its encoded form, so the lanes cross
    // verbatim. The deep arm reduces at the FRONT SAMPLE: a deep part's level carries per-texel sample counts and
    // per-channel contiguous sample runs, and a plane holds one value per texel, so the first sample per texel is
    // the one reduction a container boundary may take — compositing policy belongs to a solver, never a codec.
    private static Fin<TexturePyramid> Exr(ReadOnlyMemory<byte> payload, bool deep, Op key) {
        ReaderResult<TinyEXR.V3.Image> read = ExrFile.LoadFromMemory(payload, options: null);
        return read is { IsSuccess: true, Value: { } file }
            ? file.Parts.HeadOrNone().ToFin(RasterFault.Decode(key, "<exr-parts-empty>")).Bind(part =>
                  deep && part.Levels.Count > 0 && part.Levels[0] is DeepLevel deepLevel
                      ? DeepFront(deepLevel, key)
                      : Flat(part, key))
            : RasterFault.Decode(key, $"<exr-read:{read.Status}>");
    }

    private static Fin<TexturePyramid> Flat(Part part, Op key) {
        InterleavedFloatImage flat = PartConversion.ToInterleavedFloat(part);
        return Fill(flat.Data, flat.Width, flat.Height, flat.Channels,
            PlaneFormat.For(flat.Channels, PlaneDepth.F32).IfNone(PlaneFormat.Rgba32F),
            PlaneTransfer.Linear, flat.Channels is 4 ? AlphaMode.Associated : AlphaMode.None, PlaneRange.Unit, key);
    }

    // The deep front-sample fold: SampleCounts prefix-sums into each texel's first-sample index, and each channel
    // buffer reads its own PixelType at that index — an empty texel reads zero, the deep hole's typed neutral.
    private static Fin<TexturePyramid> DeepFront(DeepLevel level, Op key) {
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
            PlaneFormat.For(channels, PlaneDepth.F32).IfNone(PlaneFormat.Rgba32F),
            PlaneTransfer.Linear, channels is 4 ? AlphaMode.Associated : AlphaMode.None, PlaneRange.Unit, key);
    }

    // The deep WRITE authors a genuine DeepScanline part — one sample per texel, which is what a plane can state —
    // through the container's own public deep surface: Header(PartType.DeepScanline, …), per-channel ChannelBuffer
    // runs, and a DeepLevel whose sample counts are all one. The flat arm rides the interleaved bridge unchanged.
    private static Fin<ReadOnlyMemory<byte>> WriteExr(TexturePyramid chain, bool deep, EncodePolicy policy, Op key) {
        TexturePlane plane = chain.Base;
        using MemoryOwner<float> staging = Drain(plane);
        if (!deep) {
            Part part = PartConversion.FromInterleavedFloat(staging.Span, plane.Width.Value,
                plane.Height.Value, plane.Lanes, PixelType.Float, policy.Compression);
            WriterResult<byte[]> written = ExrFile.SaveToMemory(new TinyEXR.V3.Image([part]), policy.Compression, options: null);
            return written is { IsSuccess: true, Value: { } bytes }
                ? Fin.Succ((ReadOnlyMemory<byte>)bytes)
                : RasterFault.Encode(key, $"<exr-write:{written.Status}>");
        }
        int width = plane.Width.Value, height = plane.Height.Value, lanes = plane.Lanes;
        string[] names = lanes is 1 ? ["Y"] : lanes is 2 ? ["R", "G"] : ["R", "G", "B", "A"];
        Box2i window = new(0, 0, width - 1, height - 1);
        using MemoryOwner<int> counts = MemoryOwner<int>.Allocate(width * height);
        counts.Span.Fill(1);
        using MemoryOwner<float> lane = MemoryOwner<float>.Allocate(width * height);
        Seq<ChannelBuffer> buffers = toSeq(Enumerable.Range(0, Math.Min(lanes, names.Length)).Select(c => {
            for (int texel = 0; texel < width * height; texel++) { lane.Span[texel] = staging.Span[(texel * lanes) + c]; }
            return new ChannelBuffer(names[c], PixelType.Float, MemoryMarshal.AsBytes(lane.Span[..(width * height)]));
        }).ToList());
        Header header = new(PartType.DeepScanline, window,
            buffers.Map(static buffer => new Channel(buffer.Name, PixelType.Float)), policy.Compression);
        DeepLevel deepLevel = new(0, 0, window, counts.Span, buffers);
        WriterResult<byte[]> deepWritten = ExrFile.SaveToMemory(
            new TinyEXR.V3.Image([new Part(header, [deepLevel])]), policy.Compression, options: null);
        return deepWritten is { IsSuccess: true, Value: { } deepBytes }
            ? Fin.Succ((ReadOnlyMemory<byte>)deepBytes)
            : RasterFault.Encode(key, $"<exr-deep-write:{deepWritten.Status}>");
    }

    // Radiance lands the float texel directly — the only depth the RGBE expansion fills without loss — so the ingest
    // path never passes through an 8-bit step and the decoded plane is already BC6H's own texel type.
    private static Fin<TexturePyramid> Radiance(ReadOnlySpan<byte> payload, Op key) {
        ArrayBitmap<Rgba32Float> bitmap = HdrCodec.Decode(payload);
        return Fill(MemoryMarshal.Cast<Rgba32Float, float>(bitmap.PixelSpan), bitmap.Width, bitmap.Height, 4,
            PlaneFormat.Rgba32F, PlaneTransfer.Linear, AlphaMode.None, PlaneRange.Unit, key);
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
    // reporting a zero edge is a decode fault, never a fabricated 1x1 plane of pool residue.
    internal static Fin<TexturePyramid> Fill(
        ReadOnlySpan<float> staging, int width, int height, int lanes, PlaneFormat format,
        PlaneTransfer transfer, AlphaMode alpha, PlaneRange range, Op key) =>
        width <= 0 || height <= 0
            ? RasterFault.Decode(key, $"<raster-extent:{width}x{height}>")
            : TexturePlane.Of(format, Dimension.Create(width), Dimension.Create(height), transfer, alpha, key,
                    layers: default, Some(range), AllocationMode.Default)
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

// The two storage folds — the section's [EXPRESSION_SPINE] kernel exemption. ComposeRows pads an absent lane with
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
- Entry: `Decode(payload, key)` and `Encode(chain, policy, key)`, both internal. Nothing outside this section names a coder, a registry, a container option, or a block format value, so the whole pre-1.0 surface has one call site and a version bump re-verifies one gate.
- Law: the provisioned `ktx` CLI is the encode FLOOR in every branch: the python estate spawns the same binary, and the TypeScript estate CONSUMES the produced bytes and spawns nothing — two spawning branches, one consuming. `KtxArm.InProcess` is an ACCELERATION row that yields to the floor rather than diverging from it: every in-process encode runs the floor's own `ktx validate --format mini-json` over its bytes before yielding — the verdict reads the report's `valid` field, never the process status, and `--gltf-basisu` spells exactly where the payload transcodes — and a refused validation falls to the CLI arm where the payload row has one, so a set never carries bytes one branch can write and another cannot read. `astc` and `rawBcn` have NO CLI arm — `ktx create --encode` speaks only the Basis pair — so their validation refusal rails rather than falling back, which is exactly the branch-local posture their wire illegality declares. Every create spells the frozen color-assignment pair — `--assign-tf` with `--assign-primaries` (`acescc` scene-linear, `bt709` srgb, `none` raw) — under `--fail-on-color-conversions`, so a relabel is total and an implicit conversion is a tool refusal rather than a silently re-tagged plane.
- Law: a reader branches on the parsed PAYLOAD CLASS, never on the header's Vulkan token. Supercompression leaves a KTX2 declaring an undefined Vulkan format until a transcode runs, so that token reads undefined for every wire-legal UASTC and ETC1S file — a reader branching on it classes the entire wire-legal population as malformed. `KtxPayload.Transcodable` is the branch: a container whose parse resolves a coder decodes per level in process, and one whose supercompressed payload no in-process coder serves crosses the CLI's own `ktx transcode` into a coder-servable file first — the tool's default target family, so no unverified flag spelling rides the spawn.
- Law: the container version is a WRITE decision defaulting to KTX1, so every wire-bound encode sets version 2 EXPLICITLY. `KtxEncodingOptions` is mutable, so `InProcess` mints one fresh per encode: one instance carried across profiles silently re-versions a later payload.
- Law: the coder registry is a process-static global in the composed engine — `TextureCoderManager.Global` — so this gate binds its OWN `TextureCoderManager` per bake: `TryGetCoder` creates the built-in coder for a standard format lazily ON THE INSTANCE, so the bake-scoped manager needs no registration call for decode, and an encode wanting a compression level registers the family coder itself with `TextureCompressionOptions` — the engine's own registration factory is INTERNAL and unreachable, so the per-family coder constructors are the public spelling of the same ladder.
- Law: the engine's convenience facade is `Rgba8UNorm`-bound at the SIGNATURE, and the measured block-encode bound makes that the CORRECT staging depth for every block class: a block encode stages `u8` in the ENCODED domain — the quantization is the payload class's own, stated at the stage rather than smuggled by a facade — while the `none` deep store and the BC6H raw row stage `Rgba32Float` at the plane's own depth.
- Packages: TextureCompressor (composed — `ITextureCoder.Encode<TPixel>(BitmapView<TPixel>, Span<byte>)`/`Decode<TPixel>`/`GetEncodedByteCount`, `TextureCoderManager.TryGetCoder` on a bake-scoped instance with lazy built-in coder creation, `TextureCoderManager.Register(TextureFormat, ITextureCoder)` for the options-bearing encode coders, `TextureCompressionOptions.CompressionMode` + `TextureCompressionLevel`, `AstcTextureCoder`/`BptcTextureCoder`/`S3tcTextureCoder`/`RgtcLatcTextureCoder` the public family ctors over `(TextureFormat, TextureCompressionOptions)`, `ArrayBitmap<TPixel>.AsView`, `TextureImage.Format`/`GetSubresource(int, int, int)`/`MipLevelCount`/`Subresources`, `TextureSubresource.Width`/`Height`/`Payload`), TextureCompressor.FileFormats.Ktx (composed — `KtxCodec.Read(ReadOnlySpan<byte>)`, `KtxCodec.EncodeMipChain<TPixel>(IReadOnlyList<IBitmap<TPixel>>, KtxEncodingOptions?)`, `new KtxEncodingOptions { Version = KtxVersion.Version2, … }`, `KtxTexture.Texture`), `plane#TEXTURE_PLANE` (composed — the typed arena and its row rails), `Rasm.Domain` (`Op`), BCL inbox (`System.Diagnostics.Process` + `ProcessStartInfo.ArgumentList` at the CLI arm alone).
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
    // The header's Vulkan token reads undefined for every wire-legal supercompressed file, so the branch is the
    // PARSED payload's coder resolution: a format the bake-scoped manager serves decodes per level in process, and
    // a supercompressed payload no coder serves crosses the floor's own `ktx transcode` into a servable file first
    // — the tool's default target family, so no unverified flag spelling rides the spawn.
    internal static Fin<TexturePyramid> Decode(ReadOnlyMemory<byte> payload, Op key) {
        KtxTexture container = KtxCodec.Read(payload.Span);
        TextureCoderManager coders = new();
        return coders.TryGetCoder(container.Texture.Format, out ITextureCoder? coder) && coder is not null
            ? Materialize(container, coder, key)
            : Transcoded(payload, key);
    }

    // The container's OWN pyramid maps onto the plane chain: a KTX2 holds its mip levels, so each level decodes off
    // its subresource payload through the bake-scoped coder — TryGetCoder creates the built-in coder lazily on THIS
    // instance, so nothing leaks into the process-global registry — and the chain assembles in level order; a refold
    // here would silently replace the encoder's own filter. The recorded policy is the estate's box assumption over
    // a FOREIGN fold: an ingested chain's levels adopt verbatim and are never re-derived from the policy row, so the
    // assumption steers only a later re-fold decision. Rgba32Float staging is the non-quantizing decode path; the
    // srgb-ness of the declared format decides the transfer tag and the float rows land at the plane's own f32.
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
                subresource.Height, 4, storage, transfer, AlphaMode.Straight, PlaneRange.Unit, key)
            .Map(static chain => chain.Base);
    }

    // The supercompressed fallback: the floor's own `ktx transcode` at its DEFAULT target family lands a file the
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

    // Every encode runs against the provisioned floor: the in-process arm VALIDATES its own bytes through the
    // floor's `ktx validate` before yielding, and a refused validation falls to the CLI arm where the payload row
    // has one — the Basis pair and the deep store — while `astc` and `rawBcn` rail, because `ktx create --encode`
    // speaks only the Basis pair and a branch-local payload has no cross-branch floor to agree with. A layered
    // chain refuses on both arms: the per-layer file fan is the container's declared growth leg, and an encode that
    // silently wrote layer zero alone would wear a whole-chain name.
    internal static Fin<ReadOnlyMemory<byte>> Encode(TexturePyramid chain, EncodePolicy policy, Op key) =>
        chain.Base.Layers.Value is not 1
            ? RasterFault.Encode(key, $"<ktx-layered:{chain.Base.Layers.Value}>")
            : policy.Arm == KtxArm.InProcess
                ? InProcess(chain, policy, key)
                    .Bind(bytes => Validated(bytes, policy, key)
                        .BindFail(_ => policy.Payload == KtxPayload.Astc || policy.Payload == KtxPayload.RawBcn
                            ? Fin.Fail<ReadOnlyMemory<byte>>(RasterFault.Encode(key, $"<ktx-validate:{policy.Payload.Key}>"))
                            : Cli(chain, policy, key)))
                : policy.Payload == KtxPayload.Astc || policy.Payload == KtxPayload.RawBcn
                    ? RasterFault.Encode(key, $"<ktx-cli-arm:{policy.Payload.Key}>")
                    : Cli(chain, policy, key);

    // InProcess mints a fresh options object naming version 2 explicitly — the engine defaults to KTX1 — and stages
    // per the payload row's OWN bound: a block class stages u8 in the encoded domain (the measured 8-bit bound), the
    // deep store stages Rgba32Float at the plane's depth, and the raw-BCn row resolves its format off BlockFormat
    // with BC6H alone staying float. Quality is the CLI arm's knob: EncodeMipChain resolves its coders internally,
    // so a level-bearing in-process encode is not expressible and the acceleration row trades the knob for the spawn.
    private static Fin<ReadOnlyMemory<byte>> InProcess(TexturePyramid chain, EncodePolicy policy, Op key) {
        PlaneTransfer transfer = chain.Base.Transfer;
        Fin<TextureFormat> resolved = policy.Payload == KtxPayload.RawBcn
            ? Optional(policy.Block.Resolve(transfer)).ToFin(RasterFault.Encode(key, $"<ktx-block-format:{policy.Block.Key}>"))
            : policy.Payload == KtxPayload.None
                ? Fin.Succ(chain.Base.Format.Depth == PlaneDepth.F16 ? TextureFormats.Rgba16Float : TextureFormats.Rgba32Float)
                : Optional(policy.Payload.Resolve(transfer)).ToFin(RasterFault.Encode(key, $"<ktx-payload-format:{policy.Payload.Key}>"));
        return resolved.Map(format => {
            KtxEncodingOptions options = new() {
                Version = KtxVersion.Version2,
                TextureFormat = format,
                SupercompressionScheme = policy.Payload.Scheme,
                GenerateMipmaps = false,
                IsSrgb = transfer == PlaneTransfer.Srgb,
            };
            bool ldr = policy.Payload.BlockCompressed && format != TextureFormats.Bc6HUFloat && format != TextureFormats.Bc6HSFloat;
            return ldr
                ? (ReadOnlyMemory<byte>)KtxCodec.EncodeMipChain(StageLdr(chain), options)
                : (ReadOnlyMemory<byte>)KtxCodec.EncodeMipChain(StageFloat(chain), options);
        });
    }

    // Both stagings drain ENCODED unit lanes through the one codec bridge and seat them by the same lane-to-register
    // correspondence the managed leg uses — the block staging quantizes to u8 HERE, stated at the stage rather than
    // smuggled by a facade, because the block coders admit no deeper store; the quantizer itself is the plane
    // owner's one U8.FromUnit, never a re-spelled clamp-round ladder.
    private static IReadOnlyList<IBitmap<Rgba32Float>> StageFloat(TexturePyramid chain) =>
        chain.Levels.Map(level => {
            using MemoryOwner<float> staging = RasterCodec.Drain(level);
            ArrayBitmap<Rgba32Float> bitmap = new(level.Width.Value, level.Height.Value);
            Seat(staging.Span, level, bitmap.PixelSpan, static lanes =>
                new Rgba32Float(lanes[0], lanes[1], lanes[2], lanes[3]));
            return (IBitmap<Rgba32Float>)bitmap;
        }).ToList();

    private static IReadOnlyList<IBitmap<Rgba8UNorm>> StageLdr(TexturePyramid chain) =>
        chain.Levels.Map(level => {
            using MemoryOwner<float> staging = RasterCodec.Drain(level);
            ArrayBitmap<Rgba8UNorm> bitmap = new(level.Width.Value, level.Height.Value);
            Seat(staging.Span, level, bitmap.PixelSpan, static lanes => new Rgba8UNorm(
                U8.FromUnit(lanes[0]),
                U8.FromUnit(lanes[1]),
                U8.FromUnit(lanes[2]),
                U8.FromUnit(lanes[3])));
            return (IBitmap<Rgba8UNorm>)bitmap;
        }).ToList();

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
    // verdict over the produced container, never a local re-parse that would prove only what this engine believes.
    // The verdict rides the mini-json report's `valid` field per the frozen conformance law, NEVER the process
    // status — and `--gltf-basisu` spells EXACTLY where the payload transcodes, because an RGBSDA deep container
    // under that flag fails error-6301 by design, so the flag reads the payload row and never rides unconditionally.
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

    // The `ktx create` row table. --raw is NOT taken: EXR sidecars are a supported input container, and `ktx
    // create --raw` demands one raw file per level under its own inflexible framing — the EXR leg carries the same
    // per-level fan with a self-describing container. Block rows name the measured R8 input family; the deep store
    // names the plane's own float family. The COLOR ASSIGNMENT pair spells on EVERY create per the frozen measured
    // law — `--assign-tf` and `--assign-primaries` RELABEL without touching a texel, and `--fail-on-color-conversions`
    // then turns any remaining implicit conversion into a tool refusal rather than a silently re-tagged plane; the
    // texcoord origin stays the frozen top-left storage default the tool already writes. Primaries follow the frozen
    // triple: the scene-linear working space spells `acescc` (reads back KHR_DF_PRIMARIES_ACESCC/AP1), `srgb`
    // spells `bt709`, and a `raw` parameter plane spells `none` — a container defaulting its own primaries would
    // hand a reader foreign chromaticity under a correct transfer tag. Raw rides `linear` on `--assign-tf` BY LAW:
    // the identity transfer is the only tf token a parameter plane can wear, the python leg's proven spawn spelling.
    private static Seq<string> CreateArgs(TexturePyramid chain, EncodePolicy policy, Seq<string> leaves, string sink) {
        bool srgb = chain.Base.Transfer == PlaneTransfer.Srgb;
        Seq<string> format = policy.Payload.BlockCompressed
            ? Seq("--format", srgb ? "R8G8B8A8_SRGB" : "R8G8B8A8_UNORM")
            : Seq("--format", chain.Base.Format.Depth == PlaneDepth.F16 ? "R16G16B16A16_SFLOAT" : "R32G32B32A32_SFLOAT");
        Seq<string> encode = policy.Payload == KtxPayload.Etc1s
            ? Seq("--encode", "basis-lz", "--qlevel", Math.Clamp(policy.Quality, 1, 255).ToString(CultureInfo.InvariantCulture))
            : policy.Payload == KtxPayload.Uastc
                ? Seq("--encode", "uastc", "--zstd", "18")
                : Seq<string>();
        Seq<string> levels = chain.Levels.Count > 1
            ? Seq("--levels", chain.Levels.Count.ToString(CultureInfo.InvariantCulture))
            : Seq<string>();
        string primaries = chain.Base.Transfer == PlaneTransfer.Raw ? "none" : srgb ? "bt709" : "acescc";
        return Seq1("create") + format + encode + levels
            + Seq("--assign-tf", srgb ? "srgb" : "linear")
            + Seq("--assign-primaries", primaries)
            + Seq1("--fail-on-color-conversions")
            + leaves + Seq1(sink);
    }

    // ONE spawn shape for create, transcode, and validate: ArgumentList rows, both streams captured. Each verb then
    // applies its OWN verdict law — create and transcode rail on the exit status through Run, while validate reads
    // the mini-json `valid` field off Spawn's stdout because the frozen conformance law pins the report as the
    // verdict, never the process status. The provisioning probe asserts presence and the subcommand roster, never
    // version text, because the packaging strips the binaries' source metadata and a version assertion fails
    // against a correct provisioning.
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

(none)
