using System.Buffers.Binary;
using System.Collections.Generic;
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
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Quantization;
using SixLabors.ImageSharp.PixelFormats;

namespace Rasm.Imaging.Files;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DocumentLayer {
    private DocumentLayer(string name, float opacity, bool visible) => (Name, Opacity, Visible) = (name, opacity, visible);

    public string Name { get; }
    public float Opacity { get; }
    public bool Visible { get; }

    public sealed record Raster(string Name, PixelFrame Frame, BlendingMode Mode, float Opacity, bool Visible) : DocumentLayer(Name, Opacity, Visible) {
        public Option<Mask> Mask { get; init; }
    }

    public sealed record Folder(string Name, Option<BlendingMode> Mode, float Opacity, bool Visible, bool Open, Seq<DocumentLayer> Children) : DocumentLayer(Name, Opacity, Visible);
}

public abstract partial record ImageFile {
    public sealed record Document : ImageFile {
        private const TiffTag ImageSourceData = (TiffTag)37724;
        private static readonly uint Signature = BinaryPrimitives.ReadUInt32BigEndian("8BIM"u8);
        private static readonly uint PassThrough = BinaryPrimitives.ReadUInt32BigEndian("pass"u8);
        private static readonly Encoding MacRoman = CodePagesEncodingProvider.Instance.GetEncoding(10000)!;
        private static readonly FrozenDictionary<OutputDepth, uint> Keys = new Dictionary<OutputDepth, uint> {
            [OutputDepth.UInt8] = BinaryPrimitives.ReadUInt32BigEndian("Layr"u8),
            [OutputDepth.UInt16] = BinaryPrimitives.ReadUInt32BigEndian("Lr16"u8),
            [OutputDepth.Float32] = BinaryPrimitives.ReadUInt32BigEndian("Lr32"u8),
        }.ToFrozenDictionary();
        private static readonly Seq<TiffFieldInfo> SourceField = [
            new(ImageSourceData, TiffFieldInfo.Variable2, TiffFieldInfo.Variable2, TiffType.UNDEFINED, FieldBit.Custom, okToChange: true, passCount: true, "ImageSourceData"),
        ];
        private static readonly Seq<(short Id, int Plane)> ColorChannels = [(-1, 3), (0, 0), (1, 1), (2, 2)];
        private static readonly Seq<(short Id, ReadOnlyMemory<byte> Data)> GroupChannels = ColorChannels.Map(static channel => (channel.Id, new ReadOnlyMemory<byte>(new byte[sizeof(ushort)]))).Strict();

        private readonly PixelFrame canvas;

        private Document(OutputDepth depth, Option<ColorEncoding> encoding, Seq<DocumentLayer> layers, PixelFrame canvas) =>
            (Depth, Encoding, Layers, this.canvas) = (depth, encoding, layers, canvas);

        public OutputDepth Depth { get; }
        public Option<ColorEncoding> Encoding { get; }
        public Seq<DocumentLayer> Layers { get; }

        public static Seq<OutputDepth> Depths => toSeq(OutputDepth.Items).Filter(Keys.ContainsKey);

        private static ReadOnlySpan<byte> DataBlock => "Adobe Photoshop Document Data Block\0"u8;

        public static Fin<Document> From(OutputDepth depth, Option<ColorEncoding> encoding, Seq<DocumentLayer> layers) =>
            from known in ImagingFault.Unless(Keys.ContainsKey(depth), ImagingReason.Unsupported, depth)
            from canvas in Canvas(Rasters(layers).Bind(static raster => raster.Frame.Cons(raster.Mask.Bind(static mask => mask.Matte).ToSeq())), windowed: true)
            select new Document(depth, encoding, layers, canvas);

        public PixelFrame Composite() => Isolated(Layers);

        public override IO<Unit> Write(FileMetadata metadata, Stream stream) =>
            from block in IO.lift(SourceData)
            from written in Scanlines(stream, Composite(), Depth, alpha: true, metadata, [.. Rgb(Depth, Encoding, metadata), (ImageSourceData, [block.Length, block])], SourceField)
            select written;

        private static Seq<DocumentLayer.Raster> Rasters(Seq<DocumentLayer> layers) =>
            layers.Bind(static layer => layer.Switch(raster: static raster => Seq(raster), folder: static folder => Rasters(folder.Children)));

        private PixelFrame Isolated(Seq<DocumentLayer> layers) => Flatten(layers, new PixelFrame(canvas.Origin, canvas.Size, canvas.Extent, static cleared => cleared.Clear()));

        private PixelFrame Flatten(Seq<DocumentLayer> layers, PixelFrame backdrop) =>
            layers.Filter(static layer => layer.Visible && layer.Opacity is not 0f).Fold(backdrop, Over);

        private PixelFrame Over(PixelFrame under, DocumentLayer layer) =>
            layer.Switch(
                (Document: this, Under: under),
                raster: static (state, raster) => raster.Mode.Composite(state.Under, raster.Frame, raster.Opacity, raster.Mask.ToSeq(), PixelAlphaRepresentation.Unassociated),
                folder: static (state, folder) => folder.Mode.Match(
                    Some: mode => mode.Composite(state.Under, state.Document.Isolated(folder.Children), folder.Opacity, [], PixelAlphaRepresentation.Unassociated),
                    None: () => state.Document.Flatten(folder.Children, state.Under) switch {
                        var passed when folder.Opacity is 1f => passed,
                        var passed => new PixelFrame(passed.Origin, passed.Size, passed.Extent, block => {
                            Span<Vector4> mixed = MemoryMarshal.Cast<float, Vector4>(block);
                            using SpanOwner<Vector4> below = SpanOwner<Vector4>.Allocate(mixed.Length);
                            PixelFrame.Associated(MemoryMarshal.Cast<float, Vector4>(state.Under.Block), below.Span);
                            PixelFrame.Associated(MemoryMarshal.Cast<float, Vector4>(passed.Block), mixed);
                            TensorPrimitives.Lerp(MemoryMarshal.Cast<Vector4, float>(below.Span), block, folder.Opacity, block);
                            PixelFrame.Unassociated(mixed);
                        }),
                    }));

        private Fin<byte[]> SourceData() {
            Seq<(byte[] Fields, Seq<ReadOnlyMemory<byte>> Data)> records = Records(Layers).Strict();
            long info = records.Fold((long)sizeof(short), static (sum, record) => record.Data.Fold(sum + record.Fields.Length, static (total, data) => total + data.Length));
            long layered = info + (info & 1);
            long size = (DataBlock.Length + (6 * sizeof(uint)) + layered + 3) & ~3L;
            return (ImagingFault.Unless(records.Count <= -short.MinValue, ImagingReason.Oversize, records.Count), ImagingFault.Unless(size <= System.Array.MaxLength, ImagingReason.Oversize, size))
                .Apply((_, _) => {
                    byte[] block = new byte[size];
                    using BinaryWriter writer = new(new MemoryStream(block));
                    writer.Write(DataBlock);
                    Tag(writer, BinaryPrimitives.ReadUInt32BigEndian("MTrn"u8), 0);
                    Tag(writer, Keys[Depth], layered);
                    writer.Write((short)-records.Count);
                    foreach (byte[] fields in records.Map(static record => record.Fields))
                        writer.Write(fields);
                    foreach (ReadOnlyMemory<byte> channel in records.Bind(static record => record.Data))
                        writer.Write(channel.Span);
                    return block;
                })
                .As();
        }

        private Seq<(byte[] Fields, Seq<ReadOnlyMemory<byte>> Data)> Records(Seq<DocumentLayer> layers) =>
            layers.Bind(layer => layer.Switch(
                this,
                raster: static (document, raster) => Seq(Record(
                    document.canvas.Window, document.Channels(raster), raster.Mode.BlendModeKey, raster.Opacity, raster.Visible, raster.Name, [], raster.Mask.Map(_ => document.canvas.Window))),
                folder: static (document, folder) => folder.Mode.Map(static mode => mode.BlendModeKey).IfNone(PassThrough) switch {
                    var key => [
                        Record(Rectangle.Empty, GroupChannels, PassThrough, 1f, visible: false, "</Layer group>", [3], None),
                        .. document.Records(folder.Children),
                        Record(Rectangle.Empty, GroupChannels, key, folder.Opacity, folder.Visible, folder.Name, [folder.Open ? 1u : 2u, Signature, key], None),
                    ],
                }));

        private Seq<(short Id, ReadOnlyMemory<byte> Data)> Channels(DocumentLayer.Raster raster) {
            byte[][] planes = [.. Planar(raster.Frame, ColorChannels.Count), .. raster.Mask.Map(mask => Planar(Weighed(mask, raster.Frame), 1)[0]).ToSeq()];
            ParallelHelper.For(0, planes.Length, new ChannelAction(planes));
            return (ColorChannels + raster.Mask.Map(static _ => (Id: (short)-2, Plane: ColorChannels.Count)).ToSeq())
                .Map(channel => (channel.Id, new ReadOnlyMemory<byte>(planes[channel.Plane])))
                .Strict();
        }

        private byte[][] Planar(PixelFrame frame, int channels) {
            (int width, int height, int bytes) = (frame.Size.Width, frame.Size.Height, Depth.SampleBytes);
            byte[][] planes = [.. Enumerable.Range(0, channels).Select(_ => new byte[width * height * bytes])];
            using SpanOwner<byte> encoded = SpanOwner<byte>.Allocate((int)Depth.RowBytes(width, alpha: true));
            for (int y = 0; y < height; y++) {
                Depth.Encode(frame.View.Span.GetRowSpan(y), alpha: true, encoded.Span);
                Span2D<byte> samples = encoded.Span.AsSpan2D(width, 4 * bytes);
                for (int channel = 0; channel < channels; channel++)
                    samples.Slice(0, channel * bytes, width, bytes).CopyTo(planes[channel].AsSpan().AsSpan2D(height, width * bytes).GetRowSpan(y));
            }
            foreach (byte[] plane in planes)
                Depth.ToBigEndian(plane);
            return planes;
        }

        private static PixelFrame Weighed(Mask mask, PixelFrame frame) =>
            new(frame.Origin, frame.Size, frame.Extent, block => {
                Span2D<Vector4> weighed = MemoryMarshal.Cast<float, Vector4>(block).AsSpan2D(frame.Size.Height, frame.Size.Width);
                using SpanOwner<float> weights = SpanOwner<float>.Allocate(frame.Size.Width);
                for (int y = 0; y < weighed.Height; y++) {
                    Mask.Weights([mask], frame.View.Span.GetRowSpan(y), frame.Origin.X, frame.Origin.Y + y, weights.Span);
                    for (int x = 0; x < weighed.Width; x++)
                        weighed[y, x] = new Vector4(weights.Span[x]);
                }
            });

        private static (byte[] Fields, Seq<ReadOnlyMemory<byte>> Data) Record(
            Rectangle bounds, Seq<(short Id, ReadOnlyMemory<byte> Data)> channels, uint mode, float opacity, bool visible, string name, Seq<uint> section, Option<Rectangle> mask) {
            static void Edges(BinaryWriter into, Rectangle rectangle) => Seq(rectangle.Top, rectangle.Left, rectangle.Bottom, rectangle.Right).Iter(into.Write);
            byte[] pascal = MacRoman.GetBytes(name);
            int named = int.Min(pascal.Length, byte.MaxValue);
            byte[] extra = Written(fields => {
                fields.Write(mask.IsSome ? 20u : 0u);
                foreach (Rectangle window in mask.ToSeq()) {
                    Edges(fields, window);
                    fields.Write(0u);
                }
                fields.Write(0u);
                fields.Write((byte)named);
                fields.Write(pascal, 0, named);
                fields.Write(new byte[(4 - ((1 + named) & 3)) & 3]);
                Tag(fields, BinaryPrimitives.ReadUInt32BigEndian("luni"u8), sizeof(uint) + (sizeof(char) * (name.Length + 1)));
                fields.Write((uint)(name.Length + 1));
                fields.Write(System.Text.Encoding.Unicode.GetBytes(name));
                fields.Write((ushort)0);
                if (!section.IsEmpty)
                    Tag(fields, BinaryPrimitives.ReadUInt32BigEndian("lsct"u8), sizeof(uint) * section.Count);
                foreach (uint word in section)
                    fields.Write(word);
            });
            return (Written(writer => {
                Edges(writer, bounds);
                writer.Write((ushort)channels.Count);
                foreach ((short id, ReadOnlyMemory<byte> data) in channels) {
                    writer.Write(id);
                    writer.Write((uint)data.Length);
                }
                writer.Write(Signature);
                writer.Write(mode);
                writer.Write([(byte)float.Round(opacity * byte.MaxValue, MidpointRounding.ToEven), 0, (byte)(visible ? 8 : 10), 0]);
                writer.Write((uint)extra.Length);
                writer.Write(extra);
            }), channels.Map(static channel => channel.Data));
        }

        private static byte[] Written(Action<BinaryWriter> write) {
            using MemoryStream bytes = new();
            using (BinaryWriter writer = new(bytes, System.Text.Encoding.UTF8, leaveOpen: true))
                write(writer);
            return bytes.ToArray();
        }

        private static void Tag(BinaryWriter writer, uint key, long length) {
            writer.Write(Signature);
            writer.Write(key);
            writer.Write((uint)length);
        }

        private readonly struct ChannelAction(byte[][] planes) : IAction {
            public void Invoke(int i) {
                using MemoryStream output = new();
                output.Write([2, 0]);
                using (ZLibStream zlib = new(output, CompressionLevel.Optimal, leaveOpen: true))
                    zlib.Write(planes[i]);
                planes[i] = output.ToArray();
            }
        }
    }
}
