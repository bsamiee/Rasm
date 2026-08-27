# [MATERIALS_CODEC]

THE CONTAINER BOUNDARY AND THE BAND-2460 CHANNEL. One `RasterFormat` `[SmartEnum<string>]` roster closes the container family — each row carrying its extension, its magic claim, its canonical alpha association, the `CodecCapability` carriage its engine holds inside one file, the depth ceiling it writes, and the `RasterEngine` case that reads and writes it — so `RasterCodec.Decode` takes NO declared format and `RasterCodec.Encode` takes exactly one row. One `RasterFault` `[Union]` on the `FaultBand.Raster` ledger row carries every container, device, and synthesis failure the raster module produces, including the exact cause on documented provider refusals, and one internal `KtxGate` is the SOLE composer of the pre-1.0 block-compression engine, holding the provisioned `ktx` CLI as its encode floor beneath an in-process acceleration arm that yields rather than diverges.

Container choice is DATA, not dispatch: a new container is one `RasterFormat` row naming an existing engine, and only a genuinely new reader-writer is a `RasterEngine` case. That split is what keeps five packages behind one entry — `SixLabors.ImageSharp` owns PNG, TIFF, WebP, QOI, and the JPEG ingest row; `TinyEXR.NET` owns OpenEXR whole — flat, tiled-and-mip-levelled, and deep alike, because the held ImageSharp major carries no EXR codec at all; `TextureCompressor.FileFormats.Hdr` owns Radiance RGBE ingest; `TextureCompressor.FileFormats.Ktx` owns the KTX2 container behind the gate; and `Magick.NET` owns the INGEST-ONLY breadth tier — AVIF, HEIF, JPEG XL, and the DPX and Cineon motion-picture plates — reached only where every other probe declines and carrying no encoder in either direction. Format identity resolves by INSTANCE against each package's own singleton, never by comparing a format name string: the sniff returns the package's `IImageFormat` and the row holds it, so a caller cannot fork on a spelling the package chose (`"Webp"`, not `"WEBP"`) and a package rename breaks at compile time rather than at a decode that silently claims nothing. `RasterCodec` and `KtxGate` compose the `plane#TEXTURE_PLANE` typed arena with its decoded row accessors, its association gate, and its `PlanePrimaries` chromaticity axis, the `plane#TEXTURE_PYRAMID` chain every container-held pyramid maps onto, the kernel `Drawing/pack#ENCODING_CHANNEL` `ChannelDtype` depth roster, the kernel `Domain/results#FAULT_BAND` `FaultBand` ledger row, the kernel `Domain/validation#CAPABILITY` capability floor, the kernel `Op` fault key and `Op.Catch` boundary funnel, and the four container packages — re-minting no header writer, no block encoder, no RGBE expander, no depth vocabulary, and no supercompressor.

## [01]-[INDEX]

- [02]-[RASTER_FAULT]: `RasterFault` closes the compact direct family on `FaultBand.Raster`; `[FaultCase]` generates each numeric identity.
- [03]-[RASTER_FORMAT]: `CodecCapability` rosters the combinable columns, `ExrTopology` closes the three EXR part topologies, `RasterEngine` families the reader-writers, the `RasterFormat` roster closes the containers, and `BlockFormat`/`KtxPayload`/`KtxArm` with the `EncodePolicy` row and its `PreviewPolicy` column carry the payload and egress vocabularies.
- [04]-[RASTER_CODEC]: `RasterCodec` folds the magic claim, declares its untrusted-input caps, normalizes association, records declared primaries, dispatches the engine, and captures every container exception without reminting it.
- [05]-[KTX_GATE]: `KtxGate` composes TextureCompressor alone, floors encode on the CLI beneath the in-process arm, writes the layered and cube container, and branches the transcodable payload.

## [02]-[RASTER_FAULT]

- Owner: `RasterFault` is the closed direct family on `FaultBand.Raster`; its leaves own their typed evidence and rendered messages.
- Cases: `Decode`, `Encode`, `Device`, `Tile`, `Provision`, `DecodeProvider`, `EncodeProvider` — the caused pair preserves documented codec exceptions; `gpu#PRESS_DEVICE` is the sole `Device` producer and `tile#TILE_SYNTH` the sole `Tile` producer.
- Law: `FaultBand.Raster` is the sole band declaration and `[FaultCase]` the sole per-case identity declaration; each leaf converts directly onto the shared error channel.
- Law: the split against band 2450 is by CONCERN, not by location. `MaterialFault` fails every appearance-domain admission failure — a parameter out of range, a colour out of gamut, a graph that will not compile, a plane extent, chromaticity, or association a shape gate refuses — and `RasterFault` fails a raster-MECHANICAL failure at a container, a device, or a synthesizer. `plane#TEXTURE_PLANE` and `filter#PLANE_OP` therefore fail band 2450 for every shape gate and reach this band only through a codec, a device, or a tile boundary, so no shape refusal ever wears a raster code.
- Law: a typed leaf lifts bare onto `Fin<T>` and `Validation<Error,T>`; a wrapper factory or pre-wrapped generic failure is unnecessary.
- Packages: `Rasm.Domain` (`Fault`, `FaultBand.Raster`, `[FaultCase]`, `Op`), Thinktecture.Runtime.Extensions (`[Union]`), LanguageExt.Core.
- Growth: a new mechanical failure is one typed leaf with the next justified `[FaultCase]` ordinal.
- Boundary: `Detail` is an angle-bracketed `<kind:value>` discriminant owned by the producing site, never a sentence and never a foreign exception's text; documented return-contract refusals mint the uncaused direction leaf, documented codec throws mint the caused direction leaf, and an unknown package exception remains exact.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Raster;

// --- [TYPES] ---------------------------------------------------------------------------
// --- [ERRORS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RasterFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Raster;
    private RasterFault(string detail) { Key = key; Detail = detail; }
    public string Detail { get; }
    [FaultCase(0)] public sealed partial record Decode(string Detail) : RasterFault(Key, Detail) { public override string Message => $"Raster container decode failed under '{Key}': {Detail}."; }
    [FaultCase(1)] public sealed partial record Encode(string Detail) : RasterFault(Key, Detail) { public override string Message => $"Raster container encode failed under '{Key}': {Detail}."; }
    [FaultCase(2)] public sealed partial record Device(string Detail) : RasterFault(Key, Detail) { public override string Message => $"Bake device failed under '{Key}': {Detail}."; }
    [FaultCase(3)] public sealed partial record Tile(string Detail) : RasterFault(Key, Detail) { public override string Message => $"Tile synthesis failed under '{Key}': {Detail}."; }
    [FaultCase(4)] public sealed partial record Provision(string Detail) : RasterFault(Key, Detail) { public override string Message => $"Provisioned tool unavailable under '{Key}': {Detail}."; }
    [FaultCase(5)] public sealed partial record DecodeProvider(Error Cause)
        : RasterFault(Key, "<codec-provider-refused>"), ICausedFault {
        public override string Message => $"Raster container provider rejected decode under '{Key}'.";
    }
    [FaultCase(6)] public sealed partial record EncodeProvider(Error Cause)
        : RasterFault(Key, "<codec-provider-refused>"), ICausedFault {
        public override string Message => $"Raster container provider rejected encode under '{Key}'.";
    }
}
```

## [03]-[RASTER_FORMAT]

- Owner: `CodecCapability` the page's ONE combinable-column roster over the kernel `ICapability<TSelf>` floor; `ExrTopology` the three EXR part topologies with the container part type each authors; `RasterEngine` the reader-writer family; `RasterFormat` the container roster with its derived `Carriage` column; `BlockFormat` the block-layout roster over the composed `TextureFormat` values; `KtxPayload` the KTX2 payload-class roster carrying its trait set, its supercompression scheme, and its format pair; `KtxArm` the composition posture; `PreviewPolicy` the display-egress row; `EncodePolicy` the per-encode row.
- Cases: capability {`pyramid`, `layers`, `wire-legal`, `block-compressed`} · topology {`flat`, `tiled`, `deep`} · engine {`Managed`, `OpenExr`, `Radiance`, `Ktx`, `Breadth`} · format {`png16`, `tiff16`, `webp`, `qoi`, `jpeg`, `exr`, `exrDeep`, `exrTiled`, `hdr`, `ktx2`, `avif`, `heif`, `jxl`, `dpx`, `cineon`} · block {`bc1`…`bc7`, `bc6h`, `none`} · payload {`rawBcn`, `uastc`, `etc1s`, `astc`, `none`} · arm {`cli`, `inProcess`}.
- Law: the DEPTH AXIS is the kernel `Drawing/pack#ENCODING_CHANNEL` `ChannelDtype` roster, the same one `plane#PLANE_FORMAT` seats its storage rows onto, so a container's ceiling and a plane's storage speak one vocabulary and no comparison crosses two rosters. `MaxDepth` is a ROW FACT the encode gate reads through `Admits`, and it is the honest ceiling rather than the advertised one. `Admits` compares ROWS through the plane page's own `Normalizes` discriminant, never byte widths — `Unorm16` and `Float16` share a width, so a width compare alone would pass an `rgba16f` plane through the `png16` ceiling and silently narrow it through the integer texel; a normalizing-ceiling row therefore admits only normalizing depths at or below its width, and a float-ceiling row admits every depth its width holds. `TiffBitsPerPixel` tops out below any float row and carries no 16-bit-plus-alpha row, so a four-lane 16-bit plane routed at `tiff16` drops its alpha lane rather than refusing — the row therefore declares `Unorm16` as its ceiling and the encoder states `BitsPerPixel` explicitly, because an unset depth knob is an INFERENCE and inference is what quietly ships an 8-bit channel.
- Law: `qoi` is a fast lossless EIGHT-BIT row and `jpeg` a lossy eight-bit INGEST row. `qoi` admits for a preview or a thumbnail egress; `jpeg` is the container `Appearance/acquisition#ACQUISITION` and `set#SET_INGEST` receive a captured photograph on, `AlphaMode.None` its truth because JFIF carries no coverage. Neither reaches a channel plane, because an 8-bit intermediate on a texture path is a silent quantization no downstream consumer can recover: the `set#TEXTURE_SET` egress grammar admits only the frozen `<ext>` roster, which carries neither `qoi` nor `jpg`, so both rows are structurally unreachable from a set leaf and `Extension` stays the one `<ext>` source for every row the egress CAN reach.
- Law: EXR is `TinyEXR.NET`'s WHOLE — the held ImageSharp major ships no EXR codec, so `exr`, `exrTiled`, and `exrDeep` are three rows of one engine case discriminating on an `ExrTopology` row rather than three engines. The row carries its own `PartType`, so the topology a container DECLARES and the part type its writer AUTHORS are one fact; the deleted bool pair spelled a fourth corner nothing meant and left both part types as literals at the two writers. Its tiled row is the module's ONE float pyramid in a single file: `TileLevelMode.MipMap` holds every level under one header, so an HDR chain — the `environment#IBL_PREFILTER` specular level set, every solver-grade plane — takes a lossless `Compression.ZIP` home instead of the per-mip file series that was previously its only shape. Per-channel FILES stay the canonical cross-branch form; multipart, named-AOV, and tiled mip-levelled files are branch-local optimization, so no parity fixture depends on a leg one branch alone can write.
- Law: `Breadth` is an INGEST-ONLY TIER and its one-directionality is STRUCTURAL. The case carries an identity value and no encoder column at all, so `[04]`'s encode dispatch has nothing to call and fails by construction — a produced texture cannot leave the module through AVIF, HEIF, JPEG XL, DPX, or Cineon however a caller names the row, and every authored product still egresses a managed, EXR, or KTX2 leg. The five rows DECLARE LAST, so the ordered claim fold reaches the breadth probes only after every managed and container-native probe has declined: "reached only where no managed row matches" is roster ORDER rather than a guard each probe repeats. The tier is admitted on MEASURED delegate presence rather than on a package's advertised format list — the shipped osx-arm64 native links `heic`, `jxl`, `openexr`, `raw`, `webp`, and `jp2` statically, AVIF and JPEG XL round-trip and HEIC decodes read-only, and KTX has no coder in it at all, which is why the block container stays the KTX gate's and never crosses here.
- Law: `hdr` is an INGEST row. Radiance stores three 8-bit mantissas under one shared exponent, so the expansion to float does not recover the quantization and every PRODUCED environment product egresses `exr`, `exrTiled`, or `ktx2`.
- Law: `KtxPayload.Traits` `wire-legal` membership is the gate `set#TEXTURE_SET` reads at egress. `rawBcn` and `astc` are desktop payloads no Basis-transcoding consumer reads, so neither appears on a manifest-borne channel row; `uastc`, `etc1s`, and `none` are the three the web wire admits. Every payload row carries its supercompression scheme; the sRGB and linear `TextureFormat` pair is carried only where the PAYLOAD CLASS names the target — the two Basis rows and the ASTC LDR row — while `rawBcn` resolves its format off the `BlockFormat` row the policy names and `none` off the plane's own depth, so both carry the pair as absence and `Resolve` is a row read rather than a fallback ladder.
- Law: BLOCK COMPRESSION IS 8-BIT-INPUT-ONLY — the measured bound on both legs, carried as the `BlockCompressed` row fact. `ktx create --encode` admits only `R8*` formats and the in-process block coders refuse a deeper store, so a block encode STAGES its levels at `u8` in the encoded domain and a float or half plane — the HDR IBL pyramid, a solver-grade plane — takes `KtxPayload.None`: the uncompressed deep KTX2 whose `vk_format` is set, whose `needs_transcoding` is false, and whose depth ceiling is the container's own `Float32`. ASTC-HDR is unreachable on the provisioned toolchain, so `astc` composes only at LDR.
- Entry: `RasterFormat.Items` is the ordered roster the claim fold walks; `Get`/`TryGet` resolve a wire key; `Extension` is the ONE `<ext>` source the `set#TEXTURE_SET` egress grammar reads, so no page carries a second extension table.
- Packages: SixLabors.ImageSharp (composed — `PngFormat.Instance`/`TiffFormat.Instance`/`WebpFormat.Instance`/`QoiFormat.Instance`/`JpegFormat.Instance` the identity singletons each row holds, `PngEncoder`/`TiffEncoder`/`WebpEncoder`/`QoiEncoder`/`JpegEncoder` with `PngBitDepth`/`TiffBitsPerPixel` stated explicitly, `KnownQuantizers` and `IQuantizer` the preview row's palette value, `Size` the preview extent), TinyEXR.NET (composed — `ExrFile.IsExr` the magic probe, `Compression.ZIP` the durable row, `TileDescription`/`TileLevelMode.MipMap` the tiled row's own level mode), TextureCompressor (composed — `TextureFormats.Bc1Rgba`…`Bc7UNorm`/`Bc6HUFloat` and `TextureFormats.RgbaBasisUastcLdr4x4UNorm`/`RgbaBasisEtc1sSrgb`, the FIELD names a static reference spells), TextureCompressor.FileFormats.Ktx (composed — `KtxSupercompressionScheme`), TextureCompressor.FileFormats.Hdr (composed — `HdrCodec.HasRadianceHeader`), Magick.NET-Q16-HDRI-AnyCPU (composed — `MagickFormatInfo.Create(ReadOnlySpan<byte>) -> MagickFormatInfo` the span sniff every breadth probe reads, null the no-claim, `MagickFormatInfo.Format` its answer, `MagickFormat.Avif`/`Heic`/`Heif`/`Jxl`/`Dpx`/`Cin` the identity values the rows hold), `Rasm.Drawing` (composed — `ChannelDtype.Unorm8`/`Unorm16`/`Float32` the depth ceilings and their `Width` column), `plane#PLANE_VOCABULARY` (composed — `AlphaMode`/`PlaneFormat.Normalizes`), Thinktecture.Runtime.Extensions.
- Growth: a new container over an existing reader-writer is ONE `RasterFormat` row; a new block layout is one `BlockFormat` row naming its `TextureFormat` value; a new payload class is one `KtxPayload` row carrying its legality and scheme; a further ingest-only family the breadth native already links is one row at the roster's tail holding its `MagickFormat` value. Only a genuinely new reader-writer is a `RasterEngine` case, and adding one breaks the `[04]` dispatch totally rather than defaulting into a neighbour.
- Boundary: a row declares CARRIAGE and never policy. Quality, block choice, payload class, composition arm, and the preview egress ride `EncodePolicy`, which the `set#TEXTURE_SET` channel row resolves — so the same container serves a colour channel at one payload and a normal channel at another without a second format row, and a caller never selects a block format the container cannot hold.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using LanguageExt;
using Rasm.Domain;
using Rasm.Drawing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Qoi;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.Formats.Tiff.Constants;
using SixLabors.ImageSharp.Formats.Webp;
using ImageMagick;
using SixLabors.ImageSharp.Processing;
using TextureCompressor.FileFormats.Hdr;
using TextureCompressor.FileFormats.Ktx;
using TextureCompressor.Formats;
using Thinktecture;
using TinyEXR.V3;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Raster;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CodecCapability : ICapability<CodecCapability> {
    public static readonly CodecCapability Pyramid = new("pyramid");
    public static readonly CodecCapability Layers = new("layers");
    public static readonly CodecCapability WireLegal = new("wire-legal");
    public static readonly CodecCapability BlockCompressed = new("block-compressed");

    private CodecCapability(string key, int rank) : this(key) => Rank = rank;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ExrTopology {
    public static readonly ExrTopology Flat = new("flat", PartType.ScanLine);
    public static readonly ExrTopology Tiled = new("tiled", PartType.Tiled);
    public static readonly ExrTopology Deep = new("deep", PartType.DeepScanline);

    public PartType Part { get; }
    private ExrTopology(string key, PartType part) : this(key) => Part = part;
}

[Union]
public abstract partial record RasterEngine {
    private RasterEngine() { }

    public sealed record Managed(IImageFormat Identity, Func<ChannelDtype, EncodePolicy, IImageEncoder> Encoder) : RasterEngine;
    public sealed record OpenExr(ExrTopology Topology) : RasterEngine;
    public sealed record Radiance : RasterEngine;
    public sealed record Ktx : RasterEngine;
    public sealed record Breadth(MagickFormat Identity) : RasterEngine;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RasterFormat {
    public static readonly RasterFormat Png16 = new("png16", "png", AlphaMode.Straight, ChannelDtype.Unorm16,
        new RasterEngine.Managed(PngFormat.Instance, static (depth, _) => new PngEncoder {
            BitDepth = depth == ChannelDtype.Unorm8 ? PngBitDepth.Bit8 : PngBitDepth.Bit16, ColorType = PngColorType.RgbWithAlpha }),
        static payload => Claims(payload, PngFormat.Instance));

    public static readonly RasterFormat Tiff16 = new("tiff16", "tif", AlphaMode.Straight, ChannelDtype.Unorm16,
        new RasterEngine.Managed(TiffFormat.Instance, static (_, _) => new TiffEncoder {
            BitsPerPixel = TiffBitsPerPixel.Bit48, PhotometricInterpretation = TiffPhotometricInterpretation.Rgb }),
        static payload => Claims(payload, TiffFormat.Instance));

    public static readonly RasterFormat WebP = new("webp", "webp", AlphaMode.Straight, ChannelDtype.Unorm8,
        new RasterEngine.Managed(WebpFormat.Instance, static (_, _) => new WebpEncoder { FileFormat = WebpFileFormatType.Lossless }),
        static payload => Claims(payload, WebpFormat.Instance));

    public static readonly RasterFormat Qoi = new("qoi", "qoi", AlphaMode.Straight, ChannelDtype.Unorm8,
        new RasterEngine.Managed(QoiFormat.Instance, static (_, _) => new QoiEncoder()),
        static payload => Claims(payload, QoiFormat.Instance));

    public static readonly RasterFormat Jpeg = new("jpeg", "jpg", AlphaMode.None, ChannelDtype.Unorm8,
        new RasterEngine.Managed(JpegFormat.Instance,
            static (_, policy) => new JpegEncoder { Quality = Math.Clamp(policy.Quality, JpegFloor, JpegCeiling) }),
        static payload => Claims(payload, JpegFormat.Instance));

    private const int JpegFloor = 1;
    private const int JpegCeiling = 100;

    private static Option<IImageFormat> Sniff(ReadOnlySpan<byte> payload) {
        try { return Some(Image.DetectFormat(payload)); }
        catch (UnknownImageFormatException) { return None; }
    }

    private static bool Claims(ReadOnlySpan<byte> payload, IImageFormat identity) =>
        Sniff(payload).Exists(format => ReferenceEquals(format, identity));

    public static readonly RasterFormat ExrDeep = new("exrDeep", "exr", AlphaMode.Associated, ChannelDtype.Float32,
        new RasterEngine.OpenExr(ExrTopology.Deep),
        static payload => ExrFile.IsExr(payload) && (SniffHeader(payload, "deepscanline"u8) || SniffHeader(payload, "deeptile"u8)));

    public static readonly RasterFormat ExrTiled = new("exrTiled", "exr", AlphaMode.Associated, ChannelDtype.Float32,
        new RasterEngine.OpenExr(ExrTopology.Tiled),
        static payload => ExrFile.IsExr(payload) && SniffHeader(payload, "tiledesc"u8));

    public static readonly RasterFormat Exr = new("exr", "exr", AlphaMode.Associated, ChannelDtype.Float32,
        new RasterEngine.OpenExr(ExrTopology.Flat), static payload => ExrFile.IsExr(payload));

    private static bool SniffHeader(ReadOnlySpan<byte> payload, ReadOnlySpan<byte> token) =>
        payload[..Math.Min(payload.Length, 4096)].IndexOf(token) >= 0;

    public static readonly RasterFormat Hdr = new("hdr", "hdr", AlphaMode.None, ChannelDtype.Float32,
        new RasterEngine.Radiance(), static payload => HdrCodec.HasRadianceHeader(payload));

    public static readonly RasterFormat Ktx2 = new("ktx2", "ktx2", AlphaMode.Straight, ChannelDtype.Float32,
        new RasterEngine.Ktx(), static payload => payload.StartsWith(Ktx2Magic));

    private static ReadOnlySpan<byte> Ktx2Magic => [0xAB, 0x4B, 0x54, 0x58, 0x20, 0x32, 0x30, 0xBB, 0x0D, 0x0A, 0x1A, 0x0A];

    // --- [BREADTH_ROWS]
    public static readonly RasterFormat Avif = new("avif", "avif", AlphaMode.Straight, ChannelDtype.Unorm16,
        new RasterEngine.Breadth(MagickFormat.Avif), static payload => Brands(payload, MagickFormat.Avif));

    public static readonly RasterFormat Heif = new("heif", "heif", AlphaMode.Straight, ChannelDtype.Unorm16,
        new RasterEngine.Breadth(MagickFormat.Heic), static payload => Brands(payload, MagickFormat.Heic, MagickFormat.Heif));

    public static readonly RasterFormat Jxl = new("jxl", "jxl", AlphaMode.Straight, ChannelDtype.Float32,
        new RasterEngine.Breadth(MagickFormat.Jxl), static payload => Brands(payload, MagickFormat.Jxl));

    public static readonly RasterFormat Dpx = new("dpx", "dpx", AlphaMode.None, ChannelDtype.Unorm16,
        new RasterEngine.Breadth(MagickFormat.Dpx), static payload => Brands(payload, MagickFormat.Dpx));

    public static readonly RasterFormat Cineon = new("cineon", "cin", AlphaMode.None, ChannelDtype.Unorm16,
        new RasterEngine.Breadth(MagickFormat.Cin), static payload => Brands(payload, MagickFormat.Cin));

    private static Option<MagickFormat> Breadth(ReadOnlySpan<byte> payload) =>
        Optional(MagickFormatInfo.Create(payload)?.Format);

    private static bool Brands(ReadOnlySpan<byte> payload, params ReadOnlySpan<MagickFormat> identities) {
        Option<MagickFormat> claimed = Breadth(payload);
        foreach (MagickFormat identity in identities) { if (claimed == Some(identity)) { return true; } }
        return false;
    }

    public string Extension { get; }
    public AlphaMode CanonicalAlpha { get; }
    public ChannelDtype MaxDepth { get; }
    public RasterEngine Engine { get; }
    public ClaimProbe Claim { get; }
    public CapabilitySet<CodecCapability> Carriage =>
        Engine is RasterEngine.Ktx ? ContainerChain
        : Engine is RasterEngine.OpenExr exr && exr.Topology == ExrTopology.Tiled ? ChainOnly
        : CapabilitySet<CodecCapability>.None;

    private static readonly CapabilitySet<CodecCapability> ContainerChain =
        CapabilitySet<CodecCapability>.Of(CodecCapability.Pyramid, CodecCapability.Layers);
    private static readonly CapabilitySet<CodecCapability> ChainOnly =
        CapabilitySet<CodecCapability>.Of(CodecCapability.Pyramid);

    public static readonly TileDescription MipTiles = new(64u, 64u, TileLevelMode.MipMap, TileRoundingMode.RoundDown);

    public Option<TileDescription> Tiles => Engine is RasterEngine.OpenExr exr && exr.Topology == ExrTopology.Tiled ? Some(MipTiles) : None;

    public bool Admits(ChannelDtype depth) =>
        depth.Width <= MaxDepth.Width && (!PlaneFormat.Normalizes(MaxDepth) || PlaneFormat.Normalizes(depth));

    private RasterFormat(string key, string extension, AlphaMode alpha, ChannelDtype maxDepth, RasterEngine engine, ClaimProbe claim)
        : this(key) => (Extension, CanonicalAlpha, MaxDepth, Engine, Claim) = (extension, alpha, maxDepth, engine, claim);
}

public delegate bool ClaimProbe(ReadOnlySpan<byte> payload);

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

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class KtxPayload {
    public static readonly KtxPayload RawBcn = new("rawBcn", CapabilitySet<CodecCapability>.Of(CodecCapability.BlockCompressed),
        KtxSupercompressionScheme.Zstandard, format: null, srgbFormat: null);
    public static readonly KtxPayload Uastc = new("uastc", CapabilitySet<CodecCapability>.Of(CodecCapability.WireLegal, CodecCapability.BlockCompressed),
        KtxSupercompressionScheme.Zstandard, TextureFormats.RgbaBasisUastcLdr4x4UNorm, TextureFormats.RgbaBasisUastcLdr4x4Srgb);
    public static readonly KtxPayload Etc1s = new("etc1s", CapabilitySet<CodecCapability>.Of(CodecCapability.WireLegal, CodecCapability.BlockCompressed),
        KtxSupercompressionScheme.BasisLz, TextureFormats.RgbaBasisEtc1sUNorm, TextureFormats.RgbaBasisEtc1sSrgb);
    public static readonly KtxPayload Astc = new("astc", CapabilitySet<CodecCapability>.Of(CodecCapability.BlockCompressed),
        KtxSupercompressionScheme.None, TextureFormats.RgbaAstc4x4UNorm, TextureFormats.RgbaAstc4x4Srgb);
    public static readonly KtxPayload None = new("none", CapabilitySet<CodecCapability>.Of(CodecCapability.WireLegal),
        KtxSupercompressionScheme.None, format: null, srgbFormat: null);

    public CapabilitySet<CodecCapability> Traits { get; }
    public KtxSupercompressionScheme Scheme { get; }
    public TextureFormat? Format { get; }
    public TextureFormat? SrgbFormat { get; }
    public bool Transcodable => Scheme != KtxSupercompressionScheme.None;
    public TextureFormat? Resolve(PlaneTransfer transfer) => transfer == PlaneTransfer.Srgb ? SrgbFormat : Format;

    private KtxPayload(string key, CapabilitySet<CodecCapability> traits, KtxSupercompressionScheme scheme,
        TextureFormat? format, TextureFormat? srgbFormat) : this(key) =>
        (Traits, Scheme, Format, SrgbFormat) = (traits, scheme, format, srgbFormat);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class KtxArm {
    public static readonly KtxArm Cli = new("cli");
    public static readonly KtxArm InProcess = new("inProcess");
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record PreviewPolicy(Size Size, IQuantizer Palette) {
    public static readonly PreviewPolicy Thumbnail = new(new Size(512, 512), KnownQuantizers.Wu);
}

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
- Entry: `Decode(ReadOnlyMemory<byte> payload)` takes NO declared format and returns the chain the container held; `Encode(TexturePyramid subject, RasterFormat format, EncodePolicy policy)` takes one row and one policy. Arity is discriminated by the SUBJECT: a flat container writes the chain's base level and a pyramid-holding container writes every level, so no `EncodeLevel`/`EncodeChain` pair exists and no boolean selects between them.
- Law: the claim is a FOLD over `RasterFormat.Items` reading each row's own probe, first match wins, and an unclaimed payload fails `Decode`. `Decode` takes no declared format: a caller who must name the container has already read the magic bytes, and a caller who names the wrong one gets a misparse rather than a refusal.
- Law: PROBES answer ABSENCE and DECODES answer a typed fault, and neither substitutes for the other. `Sniff` over the managed package and `Breadth` over the ingest tier both return `Option`: a package refusing to recognize bytes states a NO-CLAIM about ONE row and says nothing about the payload, so a fault minted there refuses the first row's decline as the whole payload's refusal before the EXR, Radiance, and KTX2 rows ever probe. Each capture NAMES that exemption at its own site, and one typed refusal downstream bounds it — `Claim` folds every row and `Decode` lifts a total decline to `RasterFault.Decode` carrying the payload length, so an undecodable input reaches the band-2460 channel exactly once, at the level that separates "no row claimed this" from "this row declined". Inverting either end erases that distinction: a probe minting a fault refuses too early, a decode answering absence hands a caller no reason at all.
- Law: `Decode` is the ONE path a THIRD-PARTY file crosses — `set#SET_INGEST` classifies a vendor library and `environment#ENVIRONMENT_MAP` admits a downloaded dome — so the reader's caps are DECLARED here rather than inherited: the package's defaults bound a trusted producer, and a header claiming a two-billion-texel edge must refuse before any rental. Writer caps bound what this module itself emits, which is the strictly narrower posture. Every cap states the module fact it derives from, so a raised bake extent moves one declaration rather than a scatter of literals.
- Law: decode TAGS what the file DECLARED and converts nothing. Canonical association rides the row — EXR is associated, PNG, TIFF, WebP, QOI, JPEG, and KTX2 are straight, Radiance carries none — and the declared CHROMATICITY rides the container's own metadata: an EXR header's `Chromaticities` attribute and an ImageSharp `CicpProfile`'s primaries each resolve to a `PlanePrimaries` row or to `unknown`, and a container that declares neither yields `unknown` rather than the working space. A row DECLARED without a reconciliation endpoint is a third reachable state, not a gap: a P3-D65 file tags `p3d65` and keeps its own label through the key and the container write while `ToPrimaries` off it refuses, where resolving it to `unknown` discarded a declaration the file genuinely made. Its DECLARING consumer then normalizes to the plane's declared `AlphaMode` through the `plane#TEXTURE_PLANE` `ToAlpha` gate and to its working gamut through `ToPrimaries`, which is where the frozen decode-normalizes direction lives: `Decode(payload)` carries no declaration, so the canonical tag is the honest intermediate and `set#SET_INGEST`'s per-role declaration is the one normalization site. Encode CONVERTS the plane's association into the format's canonical one through the same gate, so the 16-bit floor on a premultiply-state crossing is enforced once for the whole module. Neither direction is a caller knob; the bridge itself moves ENCODED STORAGE lanes and never premultiplies, decodes, unpacks, or rebases, because those are plane declarations applied at `Read` and a bridge running them would double-apply every curve the file already carries.
- Law: the MANAGED leg is asymmetric in ARITY and the asymmetry is the package's own. Decode lands four lanes on every managed row because the pixel type the package decodes into is four-lane and the arena's texel structs are not ImageSharp pixel types — so a greyscale plate replicates across RGB and an alpha-less container reads opaque, both from the package's expansion rather than from a fabrication here. Encode is arity-honest in the other direction: `WriteManaged` seats the plane's own lane stride, so a one-lane height plane egresses grey rather than reading past its rental. Narrowing a widened decode is `set#SET_INGEST`'s, because the ROLE decides which lanes carry meaning and a codec has no role to read. The EXR and KTX legs carry the container's real arity, so this widening is the managed leg's alone and never a page-wide law.
- Law: encode REFUSES a plane whose depth the row's `Admits` denies rather than narrowing it. `Admits` exists to name exactly that silent narrow — a float plane on a normalizing-ceiling row included — and the caller either states an admitted plane or picks a row that holds the depth.
- Law: encode REFUSES a LAYERED chain at a row whose `Carriage` admits no `layers`. Every leg below reads `chain.Base` as the whole subject and drains `Width × Height × Lanes`, while a layered plane's store carries `Height × Layers` rows — so the block container is the one row that may receive a cube or an array, and every other row states that in its carriage rather than in a reviewer's memory.
- Law: every composed container throw crosses `Op.Catch`; ImageSharp format exceptions, TinyEXR limit failures, and the measured Magick missing-delegate refusal become direction-specific cause-bearing leaves, while every unknown throw remains its exact exceptional `Error`. TinyEXR result errors carry that same exact cause instead of collapsing to their status; a result without an error remains the boundary's uncaused direction refusal. `RasterFault.Decode` and `RasterFault.Encode` represent only return-contract refusals the container boundary itself owns. No category row, message remint, or boolean factory switch remains.
- Law: a declining PROBE is not a fault and never mints one. `Sniff` catches only ImageSharp's documented `UnknownImageFormatException`; `Breadth` folds Magick's documented null. A malformed claim or exceptional provider failure crosses the caught claim and retains its cause, while genuine absence lets the fold reach the next row. `Claims` and `Brands` are the two membership reads every row takes, so no row spells the absence check itself and the `Heif` row states the brand SET it answers to.
- Law: the ImageSharp leg crosses through ONE staging copy each way and the pixel-type conversion rides the package's own BULK converter. `CopyPixelDataTo` drains the decoded image into the pooled pixel run, `PixelOperations<Rgba64>.Instance.ToVector4`/`FromVector4Destructive` convert the whole run under `PixelConversionModifiers.None`, and `LoadPixelData<TPixel>(Configuration, ReadOnlySpan<TPixel>, int, int)` mints the encode image — so the module's only managed decode and encode pay the package's vectorized path rather than a per-texel scalar round trip. This modifier is stated because `Rgba64`'s scaled projection IS its plain vector projection: `None` is byte-identical to the per-texel spelling it replaces, while `Scale` or `SRgbCompand` would rescale or linearize what the arena already holds encoded. Zero-copy `WrapMemory` binding is unreachable because the arena's texel structs are not ImageSharp pixel types, and a `MemoryMarshal` reinterpretation across that boundary is exactly the stride fork the bridge law forbids.
- Law: a `Preview` column is the ONE Processing site in this module. It runs on bytes the encode already staged as an `Image`, it is legal only on a row whose `MaxDepth` is `Unorm8`, and it never reaches a chain a `set#TEXTURE_SET` leaf addresses — the egress grammar carries no `qoi`, no `webp`, and no `jpg` extension, which is what keeps a resampled and palette-collapsed plane structurally unreachable from a keyed set. Its caller is `press#PRESS_PRODUCT`'s `Preview` arm, whose planes carry no set and therefore no key, which is exactly the subject an 8-bit display egress may take.
- Packages: SixLabors.ImageSharp (composed — `Image.DetectFormat(ReadOnlySpan<byte>)` the throw-only sniff each row's probe reads through the one caught fold, `Image.Load<TPixel>(DecoderOptions, ReadOnlySpan<byte>)` naming the demanded depth so the file never decides the arena — the `Configuration` threads through `DecoderOptions`, no bare `(Configuration, span)` overload exists, `Image<TPixel>.CopyPixelDataTo(Span<TPixel>)` the decode drain, `PixelOperations<Rgba64>.Instance.ToVector4`/`FromVector4Destructive(Configuration, …, PixelConversionModifiers)` the bulk pixel-conversion entry, `Image.LoadPixelData<TPixel>(Configuration, ReadOnlySpan<TPixel>, int, int)` the encode mint, `Image.Mutate(Action<IImageProcessingContext>)` with `Resize(ResizeOptions)` + `Quantize(IQuantizer)` and `KnownResamplers.Lanczos3` at the preview site alone, `ImageMetadata.CicpProfile`/`CicpProfile.ColorPrimaries` the declared-primaries read, `Image.Save(Stream, IImageEncoder)`), TinyEXR.NET (composed — `ExrFile.LoadFromMemory`/`SaveToMemory` over `ReaderResult<Image>`/`WriterResult<byte[]>` under declared `ReaderOptions`/`WriterOptions` and their `ReaderLimits`/`WriterLimits`, `Image.Parts`/`Part.Levels` the part-level walk, `Header.Chromaticities` the declared-primaries read, `PartConversion.IsLuminanceChroma`/`LuminanceChromaToRgbaFloat`/`ToInterleavedFloat`/`FromInterleavedFloat` the planar-interleaved bridge and its luminance-chroma arm, `ExrWriter.OpenSink`/`AddPart`/`Begin`/`WriteTile`/`End` over `StreamDataSink` the tiled mip writer, `Header(PartType, Box2i, IEnumerable<Channel>, Compression, …)` with its `tiles` tail + `DeepLevel(int, int, Box2i, ReadOnlySpan<int>, IEnumerable<ChannelBuffer>)` + `ChannelBuffer(string, PixelType, ReadOnlySpan<byte>)` the deep and tiled authoring surface, `DeepLevel.SampleCounts` the deep read fold, `PartLevel.Channels`/`Region` the per-level interleave, `ReaderResult<T>.Status`/`WriterResult<T>.Status` the reason a refusal spells), TextureCompressor.FileFormats.Hdr (composed — `HdrCodec.Decode(ReadOnlySpan<byte>)`, `HdrCodec.Encode<TPixel>(IBitmap<TPixel>, HdrEncodingOptions?) -> byte[]` the byte-returning overload the decompile grounds), Magick.NET-Q16-HDRI-AnyCPU (composed — `new MagickImage(ReadOnlySpan<byte>)` the span-admitting decode, `MagickImage.Width`/`Height`/`ChannelCount`/`Depth` as `uint` and `HasAlpha`/`ColorSpace` the container's own declarations, `IPixelCollection<float> MagickImage.GetPixels()` — the HDRI build's quantum is FLOAT managed-side, so no integer round trip exists — `IPixelCollection<float>.GetReadOnlyArea(int, int, uint, uint) -> ReadOnlySpan<float>` the zero-copy row window, `Quantum.Max` the float quantum ceiling the unit normalization divides by), `plane#TEXTURE_PLANE` (composed — `TexturePlane.Of` over both modalities, `Read`/`Write`/`ToAlpha`/`ToPrimaries`, `PlanePrimaries.Of`, `TexturePyramid.Of`/`Levels`/`Base`), `Rasm.Domain` (`Op`, the `Op.Catch` boundary funnel), `Rasm.Numerics` (`Dimension`), LanguageExt.Core.
- Growth: a container added as a `RasterFormat` row reaches decode and encode with zero edits here, because both fold over the row's engine. `RasterEngine` grows by one case, which adds one arm to each of the two `Switch` folds and breaks the generated dispatch totally until both land.
- Boundary: this page owns CONTAINERS and never pixels. Transfer, primaries, association, range, and the decode ladder are `plane#PLANE_VOCABULARY`'s, resampling and derivation are `filter#PLANE_OP`'s, and channel semantics are `set#TEXTURE_CHANNEL`'s — so a codec never decides what a plane MEANS and never applies a colour transform a decode did not carry. Every EXR read on this page is a WHOLE-MEMORY read — `Decode` receives the complete payload and hands it to `LoadFromMemory` — so the container's incremental protocol never engages and no resumable state is reachable here. A non-success is terminal: its `Error` remains the caused provider refusal when present, otherwise `Status` is the uncaused discriminant, because this page has no byte window to feed back and no partial read to continue.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.IO;
using System.Linq;
using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using ImageMagick;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rasm.Drawing;
using Rasm.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using TextureCompressor.Bitmaps;
using TextureCompressor.Colors;
using TextureCompressor.FileFormats.Hdr;
using TinyEXR.V3;
using TinyEXR.V3.IO;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Raster;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static partial class RasterCodec {
    private static Option<RasterFault.DecodeProvider> DecodeProvider(Error cause) =>
        cause.Exception.Case is ImageFormatException or MagickMissingDelegateErrorException or ReaderLimitExceededException
            ? Some(new RasterFault.DecodeProvider(cause))
            : None;

    private static Option<RasterFault.EncodeProvider> EncodeProvider(Error cause) =>
        cause.Exception.Case is ImageFormatException or WriterLimitExceededException
            ? Some(new RasterFault.EncodeProvider(cause))
            : None;

    private static readonly Configuration Profile = Configured();

    private static Configuration Configured() {
        Configuration profile = Configuration.Default.Clone();
        profile.PreferContiguousImageBuffers = true;
        return profile;
    }

    private static readonly ReaderOptions Ingest = new(new ReaderLimits(
        maximumParts: 64,
        maximumDimension: 1 << 16,
        maximumMaterializedByteCount: (long)Array.MaxLength * sizeof(float),
        maximumDeepSampleCount: Array.MaxLength));

    private static readonly WriterOptions Emit = new(new WriterLimits(
        maximumParts: 64,
        maximumDimension: 1 << 16,
        maximumDeepSampleCount: Array.MaxLength));

    public static Fin<TexturePyramid> Decode(ReadOnlyMemory<byte> payload) =>
        payload.IsEmpty
            ? new RasterFault.Decode("<raster-magic:0>")
            : Try.lift(() => Claim(payload.Span).ToFin(new RasterFault.Decode($"<raster-magic:{payload.Length}>"))).Run().Bind(static inner => inner)
            .Bind(format => format.Engine.Switch(
                managed:  arm => Try.lift(() => ReadManaged(payload.Span, format)).Run().Bind(static inner => inner),
                openExr:  arm => Try.lift(() => Exr(payload, arm)).Run().Bind(static inner => inner),
                radiance: _   => Try.lift(() => Radiance(payload.Span)).Run().Bind(static inner => inner),
                ktx:      _   => Try.lift(() => KtxGate.Decode(payload)).Run().Bind(static inner => inner),
                breadth:  _   => Try.lift(() => ReadBreadth(payload.Span)).Run().Bind(static inner => inner)));

    public static Fin<ReadOnlyMemory<byte>> Encode(TexturePyramid subject, RasterFormat format, EncodePolicy policy) =>
        !format.Admits(subject.Base.Format.Depth)
            ? new RasterFault.Encode($"<raster-depth:{subject.Base.Format.Depth.Key}:{format.MaxDepth.Key}@{format.Key}>")
            : subject.Base.Layers.Value > 1 && !format.Carriage.Admits(CodecCapability.Layers)
            ? new RasterFault.Encode($"<raster-layers:{subject.Base.Layers.Value}@{format.Key}>")
            : Associate(subject, subject.Base.Alpha, format.CanonicalAlpha)
                .Bind(normalized => Bracketed(subject, normalized, () => format.Engine.Switch(
                    managed:  arm => Try.lift(() => WriteManaged(normalized, arm, policy)).Run().Bind(static inner => inner),
                    openExr:  arm => Try.lift(() => WriteExr(normalized, arm, format, policy)).Run().Bind(static inner => inner),
                    radiance: _   => Try.lift(() => WriteRadiance(normalized)).Run().Bind(static inner => inner),
                    ktx:      _   => Try.lift(() => KtxGate.Encode(normalized, policy)).Run().Bind(static inner => inner),
                    breadth:  arm => Fin.Fail<ReadOnlyMemory<byte>>(
                                 new RasterFault.Encode($"<raster-ingest-only:{format.Key}:{arm.Identity}>")))));

    private static Fin<T> Bracketed<T>(TexturePyramid subject, TexturePyramid converted, Func<Fin<T>> write) {
        if (ReferenceEquals(subject, converted)) { return write(); }
        return Custody.Bracket(write, converted);
    }

    private static Option<RasterFormat> Claim(ReadOnlySpan<byte> payload) {
        foreach (RasterFormat row in RasterFormat.Items) { if (row.Claim(payload)) { return Some(row); } }
        return None;
    }

    private static Fin<TexturePyramid> Associate(TexturePyramid chain, AlphaMode from, AlphaMode to) =>
        from == to
            ? Fin.Succ(chain)
            : chain.Levels
                .FoldM(Seq<TexturePlane>(), (converted, level) => level.ToAlpha(to)
                    .Map(converted.Add)
                    .Rollback([.. converted]))
                .As()
                .Map(levels => chain with { Levels = levels });

}
```

```csharp
// --- [OPERATIONS] ----------------------------------------------------------------------
namespace Rasm.Materials.Raster;

public static partial class RasterCodec {
    private static Fin<TexturePyramid> ReadManaged(ReadOnlySpan<byte> payload, RasterFormat format) {
        using Image<Rgba64> image = Image.Load<Rgba64>(new DecoderOptions { Configuration = Profile }, payload);
        using MemoryOwner<Rgba64> pixels = MemoryOwner<Rgba64>.Allocate(image.Width * image.Height);
        using MemoryOwner<Vector4> vectors = MemoryOwner<Vector4>.Allocate(image.Width * image.Height);
        image.CopyPixelDataTo(pixels.Span);
        PixelOperations<Rgba64>.Instance.ToVector4(Profile, pixels.Span, vectors.Span, PixelConversionModifiers.None);
        return Fill(MemoryMarshal.Cast<Vector4, float>(vectors.Span), image.Width, image.Height, 4,
            format.MaxDepth == ChannelDtype.Unorm8 ? PlaneFormat.Rgba8 : PlaneFormat.Rgba16, PlaneTransfer.Srgb,
            Declared(image.Metadata), format.CanonicalAlpha, PlaneRange.Unit);
    }

    private static Fin<TexturePyramid> ReadBreadth(ReadOnlySpan<byte> payload) {
        using MagickImage image = new(payload);
        int width = checked((int)image.Width), height = checked((int)image.Height), lanes = checked((int)image.ChannelCount);
        using MemoryOwner<float> staging = MemoryOwner<float>.Allocate(width * height * lanes);
        IPixelCollection<float> pixels = image.GetPixels();
        for (int y = 0; y < height; y++) {
            ReadOnlySpan<float> row = pixels.GetReadOnlyArea(0, y, image.Width, 1);
            TensorPrimitives.Divide(row[..(width * lanes)], Quantum.Max, staging.Span.Slice(y * width * lanes, width * lanes));
        }
        return Fill(staging.Span, width, height, lanes,
            PlaneFormat.For(lanes, BreadthDepth(image.Depth)).IfNone(PlaneFormat.Rgba32F),
            image.ColorSpace == ImageMagick.ColorSpace.sRGB ? PlaneTransfer.Srgb : PlaneTransfer.Linear,
            PlanePrimaries.Unknown, image.HasAlpha ? AlphaMode.Straight : AlphaMode.None, PlaneRange.Unit);
    }

    private static ChannelDtype BreadthDepth(uint declared) =>
        declared <= 8 ? ChannelDtype.Unorm8 : declared <= 16 ? ChannelDtype.Unorm16 : ChannelDtype.Float32;

    private static PlanePrimaries Declared(ImageMetadata metadata) =>
        metadata.CicpProfile is { } cicp ? PlanePrimaries.Of((int)cicp.ColorPrimaries) : PlanePrimaries.Unknown;

    private static Fin<ReadOnlyMemory<byte>> WriteManaged(TexturePyramid chain, RasterEngine.Managed arm, EncodePolicy policy) {
        TexturePlane plane = chain.Base;
        int lanes = plane.Lanes, colour = plane.Alpha.ColourLanes(lanes), alpha = plane.Alpha.AlphaLane(lanes);
        using MemoryOwner<float> staging = Drain(plane);
        using MemoryOwner<Vector4> vectors = MemoryOwner<Vector4>.Allocate(plane.Width.Value * plane.Height.Value);
        using MemoryOwner<Rgba64> pixels = MemoryOwner<Rgba64>.Allocate(vectors.Length);
        for (int i = 0; i < vectors.Length; i++) {
            int at = i * lanes;
            vectors.Span[i] = new Vector4(
                staging.Span[at],
                colour > 1 ? staging.Span[at + 1] : staging.Span[at],
                colour > 2 ? staging.Span[at + 2] : (colour == 2 ? 0f : staging.Span[at]),
                alpha >= 0 ? staging.Span[at + alpha] : 1f);
        }
        PixelOperations<Rgba64>.Instance.FromVector4Destructive(Profile, vectors.Span, pixels.Span, PixelConversionModifiers.None);
        using Image<Rgba64> image = Image.LoadPixelData<Rgba64>(Profile, pixels.Span,
            plane.Width.Value, plane.Height.Value);
        policy.Preview.Iter(preview => image.Mutate(context => context
            .Resize(new ResizeOptions { Size = preview.Size, Sampler = KnownResamplers.Lanczos3 })
            .Quantize(preview.Palette)));
        using MemoryStream sink = new();
        image.Save(sink, arm.Encoder(plane.Format.Depth, policy));
        return Fin.Succ((ReadOnlyMemory<byte>)sink.ToArray());
    }

    private static Fin<TexturePyramid> Exr(ReadOnlyMemory<byte> payload, RasterEngine.OpenExr arm) {
        ReaderResult<TinyEXR.V3.Image> read = ExrFile.LoadFromMemory(payload, Ingest);
        return read is { IsSuccess: true, Value: { } file }
            ? toSeq(file.Parts).Head.ToFin(new RasterFault.Decode("<exr-parts-empty>")).Bind(part =>
                  arm.Topology == ExrTopology.Deep && part.Levels.Count > 0 && part.Levels[0] is DeepLevel deepLevel
                      ? DeepFront(deepLevel, part)
                      : arm.Topology == ExrTopology.Tiled ? Tiled(part) : Flat(part))
            : read.Error is Exception cause
            ? new RasterFault.DecodeProvider(Error.New(cause.Message, cause))
            : new RasterFault.Decode($"<exr-read:{read.Status}>");
    }

    private static Fin<TexturePyramid> Flat(Part part) {
        InterleavedFloatImage flat = PartConversion.IsLuminanceChroma(part)
            ? PartConversion.LuminanceChromaToRgbaFloat(part)
            : PartConversion.ToInterleavedFloat(part);
        return Fill(flat.Data, flat.Width, flat.Height, flat.Channels,
            PlaneFormat.For(flat.Channels, ChannelDtype.Float32).IfNone(PlaneFormat.Rgba32F),
            PlaneTransfer.Linear, Declared(part.Header), flat.Channels is 4 ? AlphaMode.Associated : AlphaMode.None,
            PlaneRange.Unit);
    }

    private static Fin<TexturePyramid> Tiled(Part part) =>
        toSeq(part.Levels)
            .TraverseM(level => Level(level, part.Header)).As()
            .Map(levels => new TexturePyramid(levels, levels.Count > 1 ? MipPolicy.Box : MipPolicy.None, Coupled: false));

    private static Fin<TexturePlane> Level(PartLevel level, Header header) {
        int width = checked((int)level.Width), height = checked((int)level.Height);
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
            PlaneRange.Unit).Map(static chain => chain.Base);
    }

    private static PlanePrimaries Declared(Header header) => PlanePrimaries.Of(Optional(header.Chromaticities));

    private static Fin<TexturePyramid> DeepFront(DeepLevel level, Part part) {
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
            PlaneRange.Unit);
    }

    private static Fin<ReadOnlyMemory<byte>> WriteExr(
        TexturePyramid chain, RasterEngine.OpenExr arm, RasterFormat format, EncodePolicy policy) =>
        arm.Topology == ExrTopology.Deep ? WriteDeep(chain, arm.Topology, policy)
        : arm.Topology == ExrTopology.Tiled ? WriteTiled(chain, arm.Topology, format, policy)
        : WriteFlat(chain, policy);

    private static Fin<ReadOnlyMemory<byte>> WriteFlat(TexturePyramid chain, EncodePolicy policy) {
        TexturePlane plane = chain.Base;
        using MemoryOwner<float> staging = Drain(plane);
        Part part = PartConversion.FromInterleavedFloat(staging.Span, plane.Width.Value,
            plane.Height.Value, plane.Lanes, PixelType.Float, policy.Compression);
        WriterResult<byte[]> written = ExrFile.SaveToMemory(new TinyEXR.V3.Image([part]), policy.Compression, Emit);
        return written is { IsSuccess: true, Value: { } bytes }
            ? Fin.Succ((ReadOnlyMemory<byte>)bytes)
            : written.Error is Exception cause
            ? new RasterFault.EncodeProvider(Error.New(cause.Message, cause))
            : new RasterFault.Encode($"<exr-write:{written.Status}>");
    }

    private static Fin<ReadOnlyMemory<byte>> WriteDeep(TexturePyramid chain, ExrTopology topology, EncodePolicy policy) {
        TexturePlane plane = chain.Base;
        using MemoryOwner<float> staging = Drain(plane);
        int width = plane.Width.Value, height = plane.Height.Value, lanes = plane.Lanes;
        Box2i window = new(0, 0, width - 1, height - 1);
        using MemoryOwner<int> counts = MemoryOwner<int>.Allocate(width * height);
        counts.Span.Fill(1);
        string[] names = Names(plane.Format);
        using MemoryOwner<float> lane = MemoryOwner<float>.Allocate(width * height);
        Seq<ChannelBuffer> buffers = toSeq(Enumerable.Range(0, Math.Min(lanes, names.Length)).Select(c => {
            for (int texel = 0; texel < width * height; texel++) { lane.Span[texel] = staging.Span[(texel * lanes) + c]; }
            return new ChannelBuffer(names[c], PixelType.Float, MemoryMarshal.AsBytes(lane.Span[..(width * height)]));
        }).ToList());
        Header header = new(topology.Part, window,
            buffers.Map(static buffer => new Channel(buffer.Name, PixelType.Float)), policy.Compression);
        DeepLevel deepLevel = new(0, 0, window, counts.Span, buffers);
        WriterResult<byte[]> written = ExrFile.SaveToMemory(
            new TinyEXR.V3.Image([new Part(header, [deepLevel])]), policy.Compression, Emit);
        return written is { IsSuccess: true, Value: { } bytes }
            ? Fin.Succ((ReadOnlyMemory<byte>)bytes)
            : written.Error is Exception cause
            ? new RasterFault.EncodeProvider(Error.New(cause.Message, cause))
            : new RasterFault.Encode($"<exr-deep-write:{written.Status}>");
    }

    private static Fin<ReadOnlyMemory<byte>> WriteTiled(TexturePyramid chain, ExrTopology topology, RasterFormat format, EncodePolicy policy) {
        TexturePlane basePlane = chain.Base;
        int lanes = basePlane.Lanes;
        string[] names = Names(basePlane.Format);
        using MemoryStream sink = new();
        using ExrWriter writer = ExrWriter.OpenSink(new StreamDataSink(sink), Emit);
        Header header = new(topology.Part, new Box2i(0, 0, basePlane.Width.Value - 1, basePlane.Height.Value - 1),
            toSeq(names).Take(Math.Min(lanes, names.Length)).Map(static name => new Channel(name, PixelType.Float)),
            policy.Compression, tiles: format.Tiles.IfNone(RasterFormat.MipTiles));
        int part = writer.AddPart(header);
        return Sealed(writer.Begin(), "exr-tiled-begin")
            .Bind(_ => toSeq(chain.Levels.Select((level, index) => (Level: level, Index: index)))
                .FoldM(unit, (_, slot) => WriteLevelTiles(writer, part, slot.Index, slot.Level, format, names))
                .As())
            .Bind(_ => Sealed(writer.End(), "exr-tiled-end"))
            .Map(_ => (ReadOnlyMemory<byte>)sink.ToArray());
    }

    private static Fin<Unit> WriteLevelTiles(
        ExrWriter writer, int part, int index, TexturePlane level, RasterFormat format, string[] names) {
        TileDescription tiles = format.Tiles.IfNone(RasterFormat.MipTiles);
        int tw = checked((int)tiles.TileSizeX), th = checked((int)tiles.TileSizeY);
        int channels = Math.Min(level.Lanes, names.Length);
        int across = ((level.Width.Value + tw) - 1) / tw, down = ((level.Height.Value + th) - 1) / th;
        using MemoryOwner<float> staging = Drain(level);
        using MemoryOwner<float> tile = MemoryOwner<float>.Allocate(tw * th * channels);
        using MemoryOwner<float> planar = MemoryOwner<float>.Allocate(tw * th);
        return toSeq(Enumerable.Range(0, across * down))
            .FoldM(unit, (_, slot) => {
                (int tx, int ty) = (slot % across, slot / across);
                int spanX = Math.Min(tw, level.Width.Value - (tx * tw));
                int spanY = Math.Min(th, level.Height.Value - (ty * th));
                for (int y = 0; y < spanY; y++) {
                    for (int x = 0; x < spanX; x++) {
                        int source = ((((ty * th) + y) * level.Width.Value) + (tx * tw) + x) * level.Lanes;
                        for (int c = 0; c < channels; c++) { tile.Span[(((y * spanX) + x) * channels) + c] = staging.Span[source + c]; }
                    }
                }
                int texels = spanX * spanY;
                Seq<ChannelBuffer> buffers = toSeq(Enumerable.Range(0, channels).Select(c => {
                    for (int texel = 0; texel < texels; texel++) { planar.Span[texel] = tile.Span[(texel * channels) + c]; }
                    return new ChannelBuffer(names[c], PixelType.Float, MemoryMarshal.AsBytes(planar.Span[..texels]));
                }).ToList());
                return Sealed(writer.WriteTile(part, tx, ty, index, index, buffers), $"exr-tile:{index}:{tx}:{ty}");
            })
            .As();
    }

    private static Fin<Unit> Sealed(WriterResult result, string at) =>
        result.IsSuccess
            ? Fin.Succ(unit)
            : result.Error is Exception cause
            ? new RasterFault.EncodeProvider(Error.New(cause.Message, cause))
            : new RasterFault.Encode($"<{at}:{result.Status}>");

    private static string[] Names(PlaneFormat format) {
        int colour = format.Alpha.ColourLanes(format.Components), alpha = format.Alpha.AlphaLane(format.Components);
        string[] names = new string[format.Components];
        if (colour is 1) { names[0] = "Y"; } else { Rgb.AsSpan(0, colour).CopyTo(names); }
        if (alpha >= 0) { names[alpha] = "A"; }
        return names;
    }

    private static readonly string[] Rgb = ["R", "G", "B"];

    private static Fin<TexturePyramid> Radiance(ReadOnlySpan<byte> payload) {
        ArrayBitmap<Rgba32Float> bitmap = HdrCodec.Decode(payload);
        return Fill(MemoryMarshal.Cast<Rgba32Float, float>(bitmap.PixelSpan), bitmap.Width, bitmap.Height, 4,
            PlaneFormat.Rgba32F, PlaneTransfer.Linear, PlanePrimaries.Unknown, AlphaMode.None, PlaneRange.Unit);
    }

    private static Fin<ReadOnlyMemory<byte>> WriteRadiance(TexturePyramid chain) {
        using MemoryOwner<float> staging = Drain(chain.Base);
        ArrayBitmap<Rgba32Float> bitmap = new(chain.Base.Width.Value, chain.Base.Height.Value);
        MemoryMarshal.Cast<float, Rgba32Float>(staging.Span).CopyTo(bitmap.PixelSpan);
        return Fin.Succ((ReadOnlyMemory<byte>)HdrCodec.Encode(bitmap, options: null));
    }

    internal static Fin<TexturePyramid> Fill(
        ReadOnlySpan<float> staging, int width, int height, int lanes, PlaneFormat format,
        PlaneTransfer transfer, PlanePrimaries primaries, AlphaMode alpha, PlaneRange range) =>
        width <= 0 || height <= 0
            ? new RasterFault.Decode($"<raster-extent:{width}x{height}>")
            : TexturePlane.Of(format, Dimension.Create(width), Dimension.Create(height), transfer, alpha,
                    range: Some(range), primaries: Some(primaries), mode: AllocationMode.Default)
                .Map(plane => {
                    plane.Store.Accept<ComposeRows, Unit>(new ComposeRows(staging, lanes));
                    return plane;
                })
                .Bind(plane => TexturePyramid.Of(plane, MipPolicy.None));

    internal static MemoryOwner<float> Drain(TexturePlane plane) {
        MemoryOwner<float> staging = MemoryOwner<float>.Allocate(plane.Width.Value * plane.Height.Value * plane.Lanes);
        plane.Store.Accept<ProjectRows, Unit>(new ProjectRows(staging.Span, plane.Lanes));
        return staging;
    }
}

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

- Owner: `KtxGate` — the ONE composer of `TextureCompressor` and its KTX container leg anywhere in the repo.
- Entry: `Decode(payload, key)` and `Encode(chain, policy, key)`, both internal. Nothing outside this section names a coder, a registry, a container option, a subresource, or a block format value, so the whole pre-1.0 surface has one call site and a version bump re-verifies one gate.
- Law: the provisioned `ktx` CLI is the encode FLOOR in every branch: the python module spawns the same binary, and the TypeScript module CONSUMES the produced bytes and spawns nothing — two spawning branches, one consuming. `KtxArm.InProcess` is an ACCELERATION row that yields to the floor rather than diverging from it: every in-process encode runs the floor's own `ktx validate --format mini-json` over its bytes before yielding — the verdict reads the report's `valid` field, never the process status, and `--gltf-basisu` spells exactly where the payload transcodes — and a refused validation falls to the CLI arm where the payload row has one, so a set never carries bytes one branch can write and another cannot read. `astc` and `rawBcn` have NO CLI arm — `ktx create --encode` speaks only the Basis pair — so their validation refusal fails rather than falling back, which is exactly the branch-local posture their wire illegality declares. Every create spells the frozen color-assignment pair — `--assign-tf` with `--assign-primaries` — under `--fail-on-color-conversions`, so a relabel is total and an implicit conversion is a tool refusal rather than a silently re-tagged plane. `none` is a real `--assign-primaries` enumerator beside the named gamuts, which is what lets an undeclared plane write its own absence rather than borrow a label; it is also the one value under which `--convert-primaries` is refused outright, so absence forecloses conversion at the tool exactly as `PlanePrimaries.Matrix` forecloses it in process.
- Law: the layered and cube container is the block row's OWN shape, so the block row's `Carriage` admits `layers` and its encoder writes them; `[04]` Encode reads that column, and a layered chain routed at any other row refuses there rather than draining one layer's staging against a store holding every layer's rows. Every layered chain drains per `(level, layer)` through the same staging every flat level takes, encodes into a `TextureFormat.GetByteCount`-sized window through the bake-scoped coder, and mints one `TextureSubresource` per slot; the fold then hands one `KtxTexture` carrying the whole subresource list to `KtxCodec.Write`, which resolves NO coder and therefore never reaches the process-global registry the mip-chain facade does. Face count reads the chain's own layer census against the six-face cube law, so a six-face dome is one blob a `wgpu` upload binds directly rather than a six-file fan whose faces carry six content addresses and no container relation. Its CLI arm carries no layer leg — `ktx create` takes a per-layer file fan this module does not stage — so a layered chain that falls out of the in-process arm fails rather than silently writing layer zero under a whole-chain name.
- Law: a reader branches on the parsed PAYLOAD CLASS, never on the header's Vulkan token. Supercompression leaves a KTX2 declaring an undefined Vulkan format until a transcode runs, so that token reads undefined for every wire-legal UASTC and ETC1S file — a reader branching on it classes the entire wire-legal population as malformed. `KtxPayload.Transcodable` is the branch: a container whose parse resolves a coder decodes per level in process, and one whose supercompressed payload no in-process coder serves crosses the CLI's own `ktx transcode` into a coder-servable file first — the tool's default target family, so no unverified flag spelling rides the spawn.
- Law: the FLOOR COSTS A PROCESS and the cost is bounded PER CONTAINER, never per level or per texel. One encoded blob spawns `ktx` exactly once on the CLI arm (`create`) and exactly once on the in-process arm (`validate`), twice only where a validation refusal falls back — so an N-channel set pays N to 2N spawns and a fifteen-level chain pays the same as a single level, because every level rides one invocation as a sidecar fan. That bound is what makes the floor affordable at set scale and it is also the whole ceiling on the acceleration arm's advantage: `InProcess` buys the encode and still pays the spawn to prove it, so the row earns its place on encode throughput alone. A decode spawns nothing unless the payload is supercompressed past every in-process coder, which the hop budget caps at one.
- Law: the container version is a WRITE decision defaulting to KTX1, so every wire-bound encode sets version 2 EXPLICITLY. `KtxEncodingOptions` is mutable, so `InProcess` mints one fresh per encode: one instance carried across profiles silently re-versions a later payload.
- Law: the coder registry is a process-static global in the composed engine — `TextureCoderManager.Global` — so this gate binds its OWN `TextureCoderManager` per bake: `TryGetCoder` creates the built-in coder for a standard format lazily ON THE INSTANCE, so the bake-scoped manager needs no registration call for decode, and an encode wanting a compression level registers the family coder itself with `TextureCompressionOptions` — the engine's own registration factory is INTERNAL and unreachable, so the per-family coder constructors are the public spelling of the same ladder.
- Law: the engine's convenience facade is `Rgba8UNorm`-bound at the SIGNATURE, and the measured block-encode bound makes that the CORRECT staging depth for every block class: a block encode stages `Unorm8` in the ENCODED domain — the quantization is the payload class's own, stated at the stage rather than smuggled by a facade — while the `none` deep store and the BC6H raw row stage `Rgba32Float` at the plane's own depth.
- Packages: TextureCompressor (composed — `ITextureCoder.Encode<TPixel>(BitmapView<TPixel>, Span<byte>)`/`Decode<TPixel>`/`GetEncodedByteCount`, `TextureCoderManager.TryGetCoder`/`GetCoder` on a bake-scoped instance with lazy built-in coder creation, `TextureCoderManager.Register(TextureFormat, ITextureCoder)` for the options-bearing encode coders, `TextureCompressionOptions.CompressionMode` + `TextureCompressionLevel`, `AstcTextureCoder`/`BptcTextureCoder`/`S3tcTextureCoder`/`RgtcLatcTextureCoder` the public family ctors over `(TextureFormat, TextureCompressionOptions)`, `TextureFormat.GetByteCount(int, int)` the payload sizing, `ArrayBitmap<TPixel>.AsView`, `new TextureSubresource(int, int, int, int, int, byte[])` the mip-layer-face slot, `TextureImage.Format`/`GetSubresource(int, int, int)`/`MipLevelCount`/`ArrayLayerCount`/`FaceCount`/`Subresources`, `TextureSubresource.Width`/`Height`/`Payload`), TextureCompressor.FileFormats.Ktx (composed — `KtxCodec.Read(ReadOnlySpan<byte>)`, `KtxCodec.EncodeMipChain<TPixel>(IReadOnlyList<IBitmap<TPixel>>, KtxEncodingOptions?)`, `KtxCodec.Write(KtxTexture, KtxEncodingOptions?) -> byte[]`, `new KtxTexture(TextureFormat, IReadOnlyList<TextureSubresource>, int, int)`, `new KtxEncodingOptions { Version = KtxVersion.Version2, … }`, `KtxTexture.Texture`), `plane#TEXTURE_PLANE` (composed — the typed arena, its row accessors, and `PlanePrimaries.Assign`), `Rasm.Domain` (`Op`), BCL inbox (`System.Diagnostics.Process` + `ProcessStartInfo.ArgumentList` at the CLI arm alone).
- Growth: a new payload class is one `KtxPayload` row; a new block layout is one `BlockFormat` row. `KtxGate` keeps its own body unchanged by either, because both resolve their `TextureFormat` value off the row.
- Boundary: the CLI arm's provisioning evidence is a PRESENCE and subcommand-roster probe, never a version string — the provisioned binaries report an absent revision for a version query because the packaging strips their source metadata, so a probe asserting version text fails against a correctly provisioned tool.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance.Buffers;
using LanguageExt;
using Rasm.Domain;
using Rasm.Drawing;
using TextureCompressor.Bitmaps;
using TextureCompressor.Codecs;
using TextureCompressor.Colors;
using TextureCompressor.FileFormats.Ktx;
using TextureCompressor.Formats;
using TextureCompressor.Registry;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Raster;

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class KtxGate {
    internal static Fin<TexturePyramid> Decode(ReadOnlyMemory<byte> payload) => Read(payload, TranscodeHops);

    private const int TranscodeHops = 1;

    private static Fin<TexturePyramid> Read(ReadOnlyMemory<byte> payload, int hops) {
        KtxTexture container = KtxCodec.Read(payload.Span);
        TextureCoderManager coders = new();
        return coders.TryGetCoder(container.Texture.Format, out ITextureCoder? coder) && coder is not null
            ? Materialize(container, coder)
            : hops > 0
                ? Transcoded(payload, hops - 1)
                : new RasterFault.Decode($"<ktx-unservable-payload:{container.Texture.Format}>");
    }

    private static Fin<TexturePyramid> Materialize(KtxTexture container, ITextureCoder coder) {
        TextureFormat declared = container.Texture.Format;
        PlaneTransfer transfer = SrgbDeclared(declared) ? PlaneTransfer.Srgb : PlaneTransfer.Linear;
        PlaneFormat storage = FloatDeclared(declared) ? PlaneFormat.Rgba32F : PlaneFormat.Rgba16F;
        return toSeq(Enumerable.Range(0, container.Texture.MipLevelCount))
            .TraverseM(level => Level(container, coder, level, storage, transfer)).As()
            .Map(levels => new TexturePyramid(levels,
                container.Texture.MipLevelCount > 1 ? MipPolicy.Box : MipPolicy.None, Coupled: false));
    }

    private static bool SrgbDeclared(TextureFormat declared) =>
        KtxPayload.Items.Any(row => row.SrgbFormat == declared) || BlockFormat.Items.Any(row => row.SrgbFormat == declared)
        || declared == TextureFormats.Rgba8Srgb;
    private static bool FloatDeclared(TextureFormat declared) =>
        declared == TextureFormats.Rgba32Float || declared == TextureFormats.Rgba16Float
        || declared == TextureFormats.Bc6HUFloat || declared == TextureFormats.Bc6HSFloat;

    private static Fin<TexturePlane> Level(
        KtxTexture container, ITextureCoder coder, int level, PlaneFormat storage, PlaneTransfer transfer) {
        TextureSubresource subresource = container.Texture.GetSubresource(level, arrayLayer: 0);
        ArrayBitmap<Rgba32Float> bitmap = new(subresource.Width, subresource.Height);
        coder.Decode(subresource.Payload, bitmap.AsView());
        return RasterCodec.Fill(MemoryMarshal.Cast<Rgba32Float, float>(bitmap.PixelSpan), subresource.Width,
                subresource.Height, 4, storage, transfer, PlanePrimaries.Unknown, AlphaMode.Straight, PlaneRange.Unit)
            .Map(static chain => chain.Base);
    }

    private static Fin<TexturePyramid> Transcoded(ReadOnlyMemory<byte> payload, int hops) =>
        Staged("transcode", stage => {
            string source = Path.Combine(stage, "in.ktx2"), sink = Path.Combine(stage, "out.ktx2");
            File.WriteAllBytes(source, payload.Span);
            return Run(["transcode", source, sink])
                .Bind(_ => Read(File.ReadAllBytes(sink), hops));
        });

    internal static Fin<ReadOnlyMemory<byte>> Encode(TexturePyramid chain, EncodePolicy policy) =>
        chain.Base.Layers.Value is not 1
            ? Layered(chain, policy).Bind(bytes => Validated(bytes, policy))
            : policy.Arm == KtxArm.InProcess
                ? InProcess(chain, policy)
                    .Bind(bytes => Validated(bytes, policy)
                        .BindFail(_ => policy.Payload == KtxPayload.Astc || policy.Payload == KtxPayload.RawBcn
                            ? Fin.Fail<ReadOnlyMemory<byte>>(new RasterFault.Encode($"<ktx-validate:{policy.Payload.Key}>"))
                            : Cli(chain, policy)))
                : policy.Payload == KtxPayload.Astc || policy.Payload == KtxPayload.RawBcn
                    ? new RasterFault.Encode($"<ktx-cli-arm:{policy.Payload.Key}>")
                    : Cli(chain, policy);

    private static Fin<ReadOnlyMemory<byte>> Layered(TexturePyramid chain, EncodePolicy policy) {
        PlaneTransfer transfer = chain.Base.Transfer;
        int layers = chain.Base.Layers.Value;
        int faces = layers is 6 ? 6 : 1;
        return Resolve(chain, policy, transfer).Bind(format => {
            TextureCoderManager coders = new();
            return coders.TryGetCoder(format, out ITextureCoder? coder) && coder is not null
                ? Slots(chain, coder, format, layers, faces).Map(slots =>
                    (ReadOnlyMemory<byte>)KtxCodec.Write(
                        new KtxTexture(format, slots.ToList(), faces is 6 ? 1 : layers, faces), Options(format, policy, transfer)))
                : Fin.Fail<ReadOnlyMemory<byte>>(new RasterFault.Encode($"<ktx-layer-coder:{format}>"));
        });
    }

    private static Fin<Seq<TextureSubresource>> Slots(
        TexturePyramid chain, ITextureCoder coder, TextureFormat format, int layers, int faces) =>
        toSeq(chain.Levels.Select((level, index) => (Level: level, Index: index)))
            .FoldM(Seq<TextureSubresource>(), (built, slot) =>
                toSeq(Enumerable.Range(0, layers)).FoldM(built, (rows, layer) =>
                    slot.Level.Layer(layer).Map(face => {
                        using MemoryOwner<byte> payload =
                            MemoryOwner<byte>.Allocate(format.GetByteCount(face.Width.Value, face.Height.Value));
                        coder.Encode(StageBlock(face).AsView(), payload.Span);
                        return rows.Add(new TextureSubresource(slot.Index,
                            faces is 6 ? 0 : layer, faces is 6 ? layer : 0,
                            face.Width.Value, face.Height.Value, payload.Span.ToArray()));
                    })).As()).As();

    private static Fin<ReadOnlyMemory<byte>> InProcess(TexturePyramid chain, EncodePolicy policy) {
        PlaneTransfer transfer = chain.Base.Transfer;
        return Resolve(chain, policy, transfer).Map(format => {
            KtxEncodingOptions options = Options(format, policy, transfer);
            bool ldr = policy.Payload.Traits.Admits(CodecCapability.BlockCompressed)
                && format != TextureFormats.Bc6HUFloat && format != TextureFormats.Bc6HSFloat;
            return ldr
                ? (ReadOnlyMemory<byte>)KtxCodec.EncodeMipChain(StageLdr(chain), options)
                : (ReadOnlyMemory<byte>)KtxCodec.EncodeMipChain(StageFloat(chain), options);
        });
    }

    private static Fin<TextureFormat> Resolve(TexturePyramid chain, EncodePolicy policy, PlaneTransfer transfer) =>
        policy.Payload == KtxPayload.RawBcn
            ? Optional(policy.Block.Resolve(transfer)).ToFin(new RasterFault.Encode($"<ktx-block-format:{policy.Block.Key}>"))
            : policy.Payload == KtxPayload.None
                ? Fin.Succ(chain.Base.Format.Depth == ChannelDtype.Float16 ? TextureFormats.Rgba16Float : TextureFormats.Rgba32Float)
                : Optional(policy.Payload.Resolve(transfer)).ToFin(new RasterFault.Encode($"<ktx-payload-format:{policy.Payload.Key}>"));

    private static KtxEncodingOptions Options(TextureFormat format, EncodePolicy policy, PlaneTransfer transfer) => new() {
        Version = KtxVersion.Version2,
        TextureFormat = format,
        SupercompressionScheme = policy.Payload.Scheme,
        GenerateMipmaps = false,
        IsSrgb = transfer == PlaneTransfer.Srgb,
    };

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
        int lanes = level.Lanes, colour = level.Alpha.ColourLanes(lanes), alpha = level.Alpha.AlphaLane(lanes);
        float[] texel = new float[4];
        for (int i = 0; i < pixels.Length; i++) {
            int at = i * lanes;
            texel[0] = staging[at];
            texel[1] = colour > 1 ? staging[at + 1] : staging[at];
            texel[2] = colour > 2 ? staging[at + 2] : (colour == 2 ? 0f : staging[at]);
            texel[3] = alpha >= 0 ? staging[at + alpha] : 1f;
            pixels[i] = compose(texel);
        }
    }

    private static Fin<ReadOnlyMemory<byte>> Validated(ReadOnlyMemory<byte> bytes, EncodePolicy policy) =>
        Staged("validate", stage => {
            string candidate = Path.Combine(stage, "candidate.ktx2");
            File.WriteAllBytes(candidate, bytes.Span);
            return Spawn(Seq("validate", "--format", "mini-json")
                    + (policy.Payload.Transcodable ? Seq("--gltf-basisu") : Seq<string>())
                    + Seq(candidate))
                .Bind(run => Try.lift(() => {
                    using JsonDocument report = JsonDocument.Parse(run.Stdout);
                    return report.RootElement.GetProperty("valid").GetBoolean()
                        ? Fin.Succ(bytes)
                        : Fin.Fail<ReadOnlyMemory<byte>>(new RasterFault.Encode($"<ktx-validate:{policy.Payload.Key}:{run.Stdout}>"));
                }).Run().Bind(static inner => inner));
        });

    private static Fin<ReadOnlyMemory<byte>> Cli(TexturePyramid chain, EncodePolicy policy) =>
        Staged("create", stage => {
            string sink = Path.Combine(stage, "out.ktx2");
            return toSeq(chain.Levels.Select((level, index) => (Level: level, Index: index)))
                .TraverseM(slot =>
                    RasterCodec.Encode(new TexturePyramid(Seq(slot.Level), MipPolicy.None, Coupled: false),
                            RasterFormat.Exr, policy)
                        .Map(bytes => {
                            string leaf = Path.Combine(stage, $"level{slot.Index:D2}.exr");
                            File.WriteAllBytes(leaf, bytes.Span);
                            return leaf;
                        })).As()
                .Bind(leaves => Run(CreateArgs(chain, policy, leaves, sink)))
                .Map(_ => (ReadOnlyMemory<byte>)File.ReadAllBytes(sink));
        });

    private static Fin<T> Staged<T>(string purpose, Func<string, Fin<T>> body) =>
        Try.lift(() => {
                string stage = Path.Combine(Path.GetTempPath(), $"rasm-ktx-{purpose}-{Guid.NewGuid():N}");
                Directory.CreateDirectory(stage);
                return Fin.Succ(stage);
            }).Run().Bind(static inner => inner)
            .Bind(stage => Try.lift(() => body(stage)).Run().Bind(static inner => inner)
                .Settled(() => {
                    Directory.Delete(stage, recursive: true);
                    return Fin.Succ(unit);
                }));

    private static Seq<string> CreateArgs(TexturePyramid chain, EncodePolicy policy, Seq<string> leaves, string sink) {
        CreateRow row = CreateRow.For(policy.Payload, chain.Base.Transfer, chain.Base.Format.Depth);
        return Seq("create")
            + Seq("--format", row.VkFormat)
            + row.Encode.Match(Some: arm => Seq("--encode", arm), None: static () => Seq<string>())
            + row.Quality.Match(Some: q => Seq("--qlevel", q(policy).ToString(CultureInfo.InvariantCulture)), None: static () => Seq<string>())
            + row.Zstd.Match(Some: static level => Seq("--zstd", level.Level.ToString(CultureInfo.InvariantCulture)), None: static () => Seq<string>())
            + (chain.Levels.Count > 1 ? Seq("--levels", chain.Levels.Count.ToString(CultureInfo.InvariantCulture)) : Seq<string>())
            + Seq("--assign-tf", row.AssignTf)
            + Seq("--assign-primaries", chain.Base.Primaries.Assign)
            + Seq("--fail-on-color-conversions")
            + leaves + Seq(sink);
    }

    // --- [CREATE_ROW]
    private readonly record struct ZstdLevel(int Level) {
        internal static readonly ZstdLevel Durable = new(18);
    }

    private readonly record struct CreateRow(
        string VkFormat, string AssignTf, Option<string> Encode, Option<Func<EncodePolicy, int>> Quality, Option<ZstdLevel> Zstd) {
        internal static CreateRow For(KtxPayload payload, PlaneTransfer transfer, ChannelDtype depth) {
            bool srgb = transfer == PlaneTransfer.Srgb;
            string block = srgb ? "R8G8B8A8_SRGB" : "R8G8B8A8_UNORM", tf = srgb ? "srgb" : "linear";
            return payload == KtxPayload.Etc1s
                ? new(block, tf, Some("basis-lz"),
                      Some((Func<EncodePolicy, int>)(static p => Math.Clamp(p.Quality, QualityFloor, QualityCeiling))), None)
                : payload == KtxPayload.Uastc
                    ? new(block, tf, Some("uastc"), None, Some(ZstdLevel.Durable))
                    : new(depth == ChannelDtype.Float16 ? "R16G16B16A16_SFLOAT" : "R32G32B32A32_SFLOAT", tf, None, None, None);
        }

        const int QualityFloor = 1;
        const int QualityCeiling = 255;
    }

    private sealed record KtxRun(int ExitCode, string Stdout, string Stderr);

    private static Fin<KtxRun> Spawn(Seq<string> args) {
        using System.Diagnostics.Process ktx = new() {
            StartInfo = new System.Diagnostics.ProcessStartInfo("ktx") {
                RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false },
        };
        args.Iter(arg => ktx.StartInfo.ArgumentList.Add(arg));
        return Try.lift(() => ktx.Start()
            ? Fin.Succ(Drain(ktx))
            : Fin.Fail<KtxRun>(new RasterFault.Provision($"<ktx-not-launched:{args.Head}>"))).Run().Bind(static inner => inner);
    }

    private static KtxRun Drain(System.Diagnostics.Process ktx) {
        Task<string> errors = ktx.StandardError.ReadToEndAsync();
        string stdout = ktx.StandardOutput.ReadToEnd();
        ktx.WaitForExit();
        return new KtxRun(ktx.ExitCode, stdout, errors.GetAwaiter().GetResult());
    }

    private static Fin<Unit> Run(Seq<string> args) =>
        Spawn(args).Bind(run => run.ExitCode is 0
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new RasterFault.Encode($"<ktx-{args.Head}:{run.ExitCode}:{run.Stderr}>")));
}
```

## [06]-[RESEARCH]

(none)
