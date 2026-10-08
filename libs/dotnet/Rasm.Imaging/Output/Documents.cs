using System.Buffers.Binary;
using System.Drawing;
using System.IO.Compression;
using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.InteropServices;
using System.Text;
using BitMiracle.LibTiff.Classic;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using CommunityToolkit.HighPerformance.Helpers;
using Rasm.Imaging.ColorManagement;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Quantization;

namespace Rasm.Imaging.Output;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DocumentLayer {
    private DocumentLayer(PassName name, Mix opacity, bool visible) => (Name, Opacity, Visible) = (name, opacity, visible);

    public PassName Name { get; }
    public Mix Opacity { get; }
    public bool Visible { get; }

    public sealed record Raster(PassName Name, PixelFrame Frame, BlendingMode Mode, Mix Opacity, bool Visible, Option<Coverage.Matte> Mask) : DocumentLayer(Name, Opacity, Visible);
    public sealed record Folder(PassName Name, Option<BlendingMode> Mode, Mix Opacity, bool Visible, bool Open, Seq<DocumentLayer> Children) : DocumentLayer(Name, Opacity, Visible);

    public static DocumentLayer Identities(PassName name, PixelFrame ids, Seq<IdentityLayer> entries) =>
        new Folder(name, None, Mix.Full, Visible: true, Open: false, entries.Map(entry => (DocumentLayer)new Raster(
            entry.Name,
            new PixelFrame(ids.Origin, ids.Size, ids.Extent, block =>
                IdentityLayer.Fill(Seq(entry))(MemoryMarshal.Cast<float, Vector4>(ids.Block.AsSpan()), MemoryMarshal.Cast<float, Vector4>(block.AsSpan()))),
            BlendingMode.Mix, Mix.Full, Visible: true, None)));
}

public sealed record LayeredDocument {
    private const TiffTag ImageSourceData = (TiffTag)37724;
    private static readonly uint Signature = BinaryPrimitives.ReadUInt32BigEndian("8BIM"u8);
    private static readonly uint Transparency = BinaryPrimitives.ReadUInt32BigEndian("MTrn"u8);
    private static readonly uint UnicodeName = BinaryPrimitives.ReadUInt32BigEndian("luni"u8);
    private static readonly uint SectionDivider = BinaryPrimitives.ReadUInt32BigEndian("lsct"u8);
    private static readonly uint PassThrough = BinaryPrimitives.ReadUInt32BigEndian("pass"u8);
    private static readonly Encoding MacRoman = CodePagesEncodingProvider.Instance.GetEncoding(10000)!;
    private static readonly Seq<TiffFieldInfo> SourceField = [
        new(ImageSourceData, TiffFieldInfo.Variable2, TiffFieldInfo.Variable2, TiffType.UNDEFINED, FieldBit.Custom, okToChange: true, passCount: true, nameof(ImageSourceData)),
    ];
    private static readonly Seq<(short Id, int Plane)> ColourChannels = [(-1, 3), (0, 0), (1, 1), (2, 2)];
    private static readonly (short Id, int Plane) MaskChannel = (-2, 4);
    private static readonly Seq<(short Id, ReadOnlyMemory<byte> Data)> GroupChannels = ColourChannels.Map(static channel => (channel.Id, new ReadOnlyMemory<byte>(new byte[sizeof(ushort)]))).Strict();
    private static readonly LayerRecord Divider = new(Rectangle.Empty, GroupChannels, PassThrough, Mix.Full, Visible: false, None, "</Layer group>", [3]);

    private LayeredDocument((TiffSample Tiff, uint Key) sample, ColorEncoding encoding, PixelFrame canvas, Seq<DocumentLayer> layers) => (Sample, Encoding, Canvas, Layers) = (sample, encoding, canvas, layers);

    public ColorEncoding Encoding { get; }
    public Seq<DocumentLayer> Layers { get; }
    private (TiffSample Tiff, uint Key) Sample { get; }
    private PixelFrame Canvas { get; }

    public static Seq<OutputDepth> Depths => toSeq(OutputDepth.Items).Filter(static depth => Table(depth).IsSome);

    private static ReadOnlySpan<byte> DataBlock => "Adobe Photoshop Document Data Block\0"u8;

    public static Validation<Error, LayeredDocument> From(OutputDepth depth, ColorEncoding encoding, Seq<DocumentLayer> layers) =>
        (Table(depth).ToValidation<Error>(new DepthRefused(depth, Depths)), CanvasOf(Rasters(layers)))
            .Apply((sample, canvas) => new LayeredDocument(sample, encoding, canvas, layers))
            .As();

    public PixelFrame Composite() => Isolated(Layers);

    internal IO<Unit> Write(FileMetadata metadata, FileStream stream) =>
        from block in IO.lift(SourceData)
        from written in StillWriter.Scanlines(
            stream,
            "w4",
            SourceField,
            StillWriter.Tagged(Canvas.Extent, Sample.Tiff, alpha: true, Encoding, metadata, [(ImageSourceData, [block.Length, block])]),
            Canvas.Extent.Height,
            Scanline(Composite()))
        select written;

    private static Option<(TiffSample Tiff, uint Key)> Table(OutputDepth depth) =>
        from key in depth.Map<Option<uint>>(
            uInt8: BinaryPrimitives.ReadUInt32BigEndian("Layr"u8),
            uInt10: None,
            uInt12: None,
            uInt14: None,
            uInt16: BinaryPrimitives.ReadUInt32BigEndian("Lr16"u8),
            float16: None,
            float32: BinaryPrimitives.ReadUInt32BigEndian("Lr32"u8))
        from tiff in StillFormat.TiffRaster.Table(depth)
        select (Tiff: tiff, Key: key);

    private static Seq<DocumentLayer.Raster> Rasters(Seq<DocumentLayer> layers) =>
        layers.Bind(static layer => layer.Switch(raster: static raster => Seq(raster), folder: static folder => Rasters(folder.Children)));

    private static Validation<Error, PixelFrame> CanvasOf(Seq<DocumentLayer.Raster> rasters) =>
        from canvas in rasters.Head.Map(static raster => raster.Frame).ToValidation<Error>(new DocumentEmpty())
        from placed in rasters.Map(static (raster, index) => raster.Frame.Cons(raster.Mask.Map(static mask => mask.Frame).ToSeq()).Map(frame => (Frame: frame, Index: index)))
            .Flatten()
            .Traverse(entry => entry.Frame.Window == canvas.Window && entry.Frame.Extent == canvas.Extent
                ? Validation.Success<Error, Unit>(unit)
                : Validation.Fail<Error, Unit>(new LayerWindowRefused(entry.Index, entry.Frame.Window, entry.Frame.Extent)))
            .As()
        select canvas;

    private PixelFrame Isolated(Seq<DocumentLayer> layers) =>
        Flatten(layers, new PixelFrame(Canvas.Origin, Canvas.Size, Canvas.Extent, static block => block.AsSpan().Clear())) switch {
            var flat => new PixelFrame(flat.Origin, flat.Size, flat.Extent, block => {
                ReadOnlySpan<Vector4> associated = MemoryMarshal.Cast<float, Vector4>(flat.Block.AsSpan());
                Span<Vector4> straight = MemoryMarshal.Cast<float, Vector4>(block.AsSpan());
                for (int i = 0; i < straight.Length; i++)
                    straight[i] = associated[i].W > 0f ? new Vector4(associated[i].AsVector3() / associated[i].W, associated[i].W) : Vector4.Zero;
            }),
        };

    private PixelFrame Flatten(Seq<DocumentLayer> layers, PixelFrame backdrop) =>
        layers.Filter(static layer => layer.Visible).Fold(backdrop, Over);

    private PixelFrame Over(PixelFrame under, DocumentLayer layer) =>
        layer.Switch(
            (Document: this, Under: under),
            raster: static (state, raster) => Blend.Composite(state.Under, raster.Frame, raster.Mode, raster.Opacity, raster.Mask.Map(static mask => new Mask(MaskOperation.Add, mask)).ToSeq()),
            folder: static (state, folder) => folder.Mode.Match(
                Some: mode => Blend.Composite(state.Under, state.Document.Isolated(folder.Children), mode, folder.Opacity, []),
                None: () => state.Document.Flatten(folder.Children, state.Under) switch {
                    var passed => new PixelFrame(passed.Origin, passed.Size, passed.Extent, block => TensorPrimitives.Lerp(state.Under.Block, passed.Block, (float)folder.Opacity, block)),
                }));

    private Fin<byte[]> SourceData() {
        Seq<LayerRecord> records = Records(Layers).Strict();
        using MemoryStream fields = new();
        using (BinaryWriter writer = new(fields, System.Text.Encoding.UTF8, leaveOpen: true))
            _ = records.Iter(record => record.Write(writer));
        long info = sizeof(short) + fields.Length + records.Fold(0L, static (sum, record) => record.Channels.Fold(sum, static (total, channel) => total + channel.Data.Length));
        long layered = info + (info & 1);
        long size = (DataBlock.Length + (6 * sizeof(uint)) + layered + 3) & ~3L;
        long bytes = size + ((long)Canvas.Extent.Height * Sample.Tiff.Depth.RowBytes(Canvas.Extent.Width, alpha: true));
        if (bytes > StillWriter.ClassicBytes || size > System.Array.MaxLength)
            return new TiffBytesRefused(bytes);
        byte[] block = new byte[size];
        using (BinaryWriter writer = new(new MemoryStream(block))) {
            writer.Write(DataBlock);
            writer.Write(Signature);
            writer.Write(Transparency);
            writer.Write(0u);
            writer.Write(Signature);
            writer.Write(Sample.Key);
            writer.Write(uint.CreateSaturating(layered));
            writer.Write((short)-records.Count);
            writer.Write(fields.GetBuffer(), 0, int.CreateSaturating(fields.Length));
            _ = records.Iter(record => record.Channels.Iter(channel => writer.Write(channel.Data.Span)));
        }
        return block;
    }

    private Seq<LayerRecord> Records(Seq<DocumentLayer> layers) =>
        layers.Bind(layer => layer.Switch(
            this,
            raster: static (document, raster) => Seq(new LayerRecord(
                document.Canvas.Window, document.Channels(raster), raster.Mode.BlendModeKey, raster.Opacity, raster.Visible, raster.Mask.Map(_ => document.Canvas.Window), raster.Name, [])),
            folder: static (document, folder) => folder.Mode.Map(static mode => mode.BlendModeKey).IfNone(PassThrough) switch {
                var key => Divider.Cons(document.Records(folder.Children)).Add(new LayerRecord(
                    Rectangle.Empty, GroupChannels, key, folder.Opacity, folder.Visible, None, folder.Name, [folder.Open ? 1u : 2u, Signature, key])),
            }));

    private Seq<(short Id, ReadOnlyMemory<byte> Data)> Channels(DocumentLayer.Raster raster) {
        Seq<(short Id, int Plane)> channels = ColourChannels + raster.Mask.Map(static _ => MaskChannel).ToSeq();
        int plane = Canvas.Size.Width * Canvas.Size.Height * Sample.Tiff.Depth.SampleBytes;
        using MemoryOwner<byte> planes = MemoryOwner<byte>.Allocate(channels.Count * plane);
        StillWriter.Planar(Sample.Tiff.Depth, Canvas.Size, ColourChannels.Count, (y, row) => raster.Frame.View.Span.GetRowSpan(y).CopyTo(row), planes.Span);
        _ = raster.Mask.Iter(mask => StillWriter.Planar(Sample.Tiff.Depth, Canvas.Size, 1, (y, row) => Weigh(mask, y, row), planes.Span[(MaskChannel.Plane * plane)..]));
        Sample.Tiff.Depth.ToBigEndian(planes.Span);
        byte[][] compressed = new byte[channels.Count][];
        ParallelHelper.For(0, channels.Count, new ChannelAction(planes.Memory, plane, compressed));
        return channels.Map(channel => (channel.Id, new ReadOnlyMemory<byte>(compressed[channel.Plane]))).Strict();
    }

    private void Weigh(Coverage.Matte mask, int y, Span<Vector4> row) {
        using SpanOwner<float> weights = SpanOwner<float>.Allocate(row.Length);
        mask.Weights(row, Canvas.Origin.X, Canvas.Line(y), Canvas.Extent, weights.Span);
        for (int x = 0; x < row.Length; x++)
            row[x] = new Vector4(weights.Span[x]);
    }

    private Action<int, Span<byte>> Scanline(PixelFrame composite) {
        Action<int, Span<byte>> rows = StillWriter.Rows(composite, Sample.Tiff.Depth, alpha: true);
        return (line, scanline) => {
            scanline.Clear();
            if (line >= Canvas.Window.Top && line < Canvas.Window.Bottom)
                rows(line - Canvas.Window.Top, scanline[Sample.Tiff.Depth.RowBytes(Canvas.Origin.X, alpha: true)..]);
        };
    }

    private readonly record struct LayerRecord(Rectangle Bounds, Seq<(short Id, ReadOnlyMemory<byte> Data)> Channels, uint Mode, Mix Opacity, bool Visible, Option<Rectangle> Mask, string Name, Seq<uint> Section) {
        internal void Write(BinaryWriter writer) {
            static void Edges(BinaryWriter into, Rectangle rectangle) {
                into.Write(rectangle.Top);
                into.Write(rectangle.Left);
                into.Write(rectangle.Bottom);
                into.Write(rectangle.Right);
            }
            byte[] pascal = MacRoman.GetBytes(Name);
            byte[] unicode = System.Text.Encoding.Unicode.GetBytes(Name);
            int named = int.Min(pascal.Length, byte.MaxValue);
            using MemoryStream extra = new();
            using (BinaryWriter fields = new(extra, System.Text.Encoding.UTF8, leaveOpen: true)) {
                fields.Write(Mask.IsSome ? 20u : 0u);
                _ = Mask.Iter(window => {
                    Edges(fields, window);
                    fields.Write(0u);
                });
                fields.Write(0u);
                fields.Write((byte)named);
                fields.Write(pascal, 0, named);
                fields.Write(new byte[(4 - ((1 + named) & 3)) & 3]);
                fields.Write(Signature);
                fields.Write(UnicodeName);
                fields.Write((uint)(sizeof(uint) + unicode.Length + sizeof(char)));
                fields.Write((uint)((unicode.Length / sizeof(char)) + 1));
                fields.Write(unicode);
                fields.Write((ushort)0);
                if (!Section.IsEmpty) {
                    fields.Write(Signature);
                    fields.Write(SectionDivider);
                    fields.Write((uint)(sizeof(uint) * Section.Count));
                    _ = Section.Iter(fields.Write);
                }
            }
            Edges(writer, Bounds);
            writer.Write((ushort)Channels.Count);
            _ = Channels.Iter(channel => {
                writer.Write(channel.Id);
                writer.Write((uint)channel.Data.Length);
            });
            writer.Write(Signature);
            writer.Write(Mode);
            writer.Write((byte)MathF.Round((float)Opacity * byte.MaxValue, MidpointRounding.ToEven));
            writer.Write((byte)0);
            writer.Write((byte)(Visible ? 8 : 10));
            writer.Write((byte)0);
            writer.Write(uint.CreateSaturating(extra.Length));
            writer.Write(extra.GetBuffer(), 0, int.CreateSaturating(extra.Length));
        }
    }

    private readonly struct ChannelAction(Memory<byte> planes, int length, byte[][] compressed) : IAction {
        public void Invoke(int i) {
            using MemoryStream output = new();
            output.Write([2, 0]);
            using (ZLibStream zlib = new(output, CompressionLevel.Optimal, leaveOpen: true))
                zlib.Write(planes.Span.Slice(i * length, length));
            compressed[i] = output.ToArray();
        }
    }
}
