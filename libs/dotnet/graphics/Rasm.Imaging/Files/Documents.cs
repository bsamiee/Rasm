using System.Buffers.Binary;
using System.Globalization;
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
using Rasm.Imaging.Grade;
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

    public sealed record Raster(string Name, PixelFrame Frame, BlendingMode Mode, float Opacity, bool Visible, Option<Coverage.Matte> Mask) : DocumentLayer(Name, Opacity, Visible);
    public sealed record Folder(string Name, Option<BlendingMode> Mode, float Opacity, bool Visible, bool Open, Seq<DocumentLayer> Children) : DocumentLayer(Name, Opacity, Visible);

    public static DocumentLayer Identities(string name, PixelFrame ids, Seq<IdentityLayer> entries) =>
        new Folder(name, None, 1f, Visible: true, Open: false, entries.Map(entry => (DocumentLayer)new Raster(
            entry.Name,
            new PixelFrame(ids.Origin, ids.Size, ids.Extent, block =>
                IdentityLayer.Fill(Seq(entry))(MemoryMarshal.Cast<float, Vector4>(ids.Block), MemoryMarshal.Cast<float, Vector4>(block.AsSpan()))),
            BlendingMode.Mix, 1f, Visible: true, None)));
}

public sealed record LayeredDocument {
    private const TiffTag ImageSourceData = (TiffTag)37724;
    private static readonly uint Signature = BinaryPrimitives.ReadUInt32BigEndian("8BIM"u8);
    private static readonly uint PassThrough = BinaryPrimitives.ReadUInt32BigEndian("pass"u8);
    private static readonly Encoding MacRoman = CodePagesEncodingProvider.Instance.GetEncoding(10000)!;
    private static readonly Seq<TiffFieldInfo> SourceField = [
        new(ImageSourceData, TiffFieldInfo.Variable2, TiffFieldInfo.Variable2, TiffType.UNDEFINED, FieldBit.Custom, okToChange: true, passCount: true, "ImageSourceData"),
    ];
    private static readonly Seq<(short Id, int Plane)> ColorChannels = [(-1, 3), (0, 0), (1, 1), (2, 2)];
    private static readonly Seq<(short Id, ReadOnlyMemory<byte> Data)> GroupChannels = ColorChannels.Map(static channel => (channel.Id, new ReadOnlyMemory<byte>(new byte[sizeof(ushort)]))).Strict();
    private static readonly LayerRecord Divider = new((0L, 0L, 0L, 0L), GroupChannels, PassThrough, 1f, Visible: false, None, "</Layer group>", [3]);

    private LayeredDocument((TiffSample Tiff, uint Key) sample, Option<ColorEncoding> encoding, PixelFrame canvas, Seq<DocumentLayer> layers) => (Sample, Encoding, Canvas, Layers) = (sample, encoding, canvas, layers);

    public Option<ColorEncoding> Encoding { get; }
    public Seq<DocumentLayer> Layers { get; }
    private (TiffSample Tiff, uint Key) Sample { get; }
    private PixelFrame Canvas { get; }

    public static Seq<OutputDepth> Depths => OutputDepth.Where(Table);

    private static ReadOnlySpan<byte> DataBlock => "Adobe Photoshop Document Data Block\0"u8;

    public static Fin<LayeredDocument> From(OutputDepth depth, Option<ColorEncoding> encoding, Seq<DocumentLayer> layers) =>
        from accepted in (OutputDepth.Sampled("Layered document", depth, Table), CanvasOf(Rasters(layers))).Apply(static (sample, canvas) => (Sample: sample, Canvas: canvas)).As()
        from buffered in accepted.Sample.Tiff.Depth.Buffered("Layered document", accepted.Canvas.Extent.Width, alpha: true)
        select new LayeredDocument(accepted.Sample, encoding, accepted.Canvas, layers);

    public PixelFrame Composite() => Isolated(Layers);

    internal IO<Unit> Write(FileMetadata metadata, Stream stream) =>
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

    private static Fin<PixelFrame> CanvasOf(Seq<DocumentLayer.Raster> rasters) =>
        from canvas in rasters.Head.Map(static raster => raster.Frame).ToFin(Error.New("Layered document holds no raster layer"))
        from accepted in (
                canvas.Window.Right <= int.MaxValue && canvas.Window.Bottom <= int.MaxValue
                    ? Fin.Succ(unit)
                    : Error.New($"Canvas window {canvas.Window} exceeds TIFF coordinate range"),
                rasters.Map(static (raster, index) => raster.Frame.Cons(raster.Mask.Map(static mask => mask.Frame).ToSeq()).Map(frame => (Frame: frame, Index: index)))
                    .Flatten()
                    .Traverse(entry => entry.Frame.Window == canvas.Window && entry.Frame.Extent == canvas.Extent
                        ? Fin.Succ(unit)
                        : Error.New(string.Create(CultureInfo.InvariantCulture, $"Document layer {entry.Index} window {entry.Frame.Window} on {entry.Frame.Extent} differs from canvas")))
                    .As())
            .Apply(static (_, _) => unit).As()
        select canvas;

    private PixelFrame Isolated(Seq<DocumentLayer> layers) =>
        new(Canvas.Origin, Canvas.Size, Canvas.Extent, block => {
            ReadOnlySpan<Vector4> associated = MemoryMarshal.Cast<float, Vector4>(Flatten(layers, new PixelFrame(Canvas.Origin, Canvas.Size, Canvas.Extent, static cleared => cleared.AsSpan().Clear())).Block);
            Span<Vector4> straight = MemoryMarshal.Cast<float, Vector4>(block.AsSpan());
            for (int i = 0; i < straight.Length; i++)
                straight[i] = associated[i].W is 0f or 1f ? associated[i] : new Vector4(associated[i].AsVector3() / associated[i].W, associated[i].W);
        });

    private PixelFrame Flatten(Seq<DocumentLayer> layers, PixelFrame backdrop) =>
        layers.Filter(static layer => layer.Visible && layer.Opacity is not 0f).Fold(backdrop, Over);

    private PixelFrame Over(PixelFrame under, DocumentLayer layer) =>
        layer.Switch(
            (Document: this, Under: under),
            raster: static (state, raster) => Blend.Composite(state.Under, raster.Frame, raster.Mode, raster.Opacity, raster.Mask.Map(static mask => new Mask(MaskOperation.Add, mask)).ToSeq(), PixelAlphaRepresentation.Associated),
            folder: static (state, folder) => folder.Mode.Match(
                Some: mode => Blend.Composite(state.Under, state.Document.Isolated(folder.Children), mode, folder.Opacity, [], PixelAlphaRepresentation.Associated),
                None: () => state.Document.Flatten(folder.Children, state.Under) switch {
                    var passed => folder.Opacity is 1f ? passed : new PixelFrame(passed.Origin, passed.Size, passed.Extent, block => TensorPrimitives.Lerp(state.Under.Block, passed.Block, folder.Opacity, block)),
                }));

    private Fin<byte[]> SourceData() {
        Seq<LayerRecord> records = Records(Layers).Strict();
        if (records.Count > -short.MinValue)
            return Error.New(string.Create(CultureInfo.InvariantCulture, $"{records.Count} layer records exceed Photoshop maximum {-short.MinValue}"));
        using MemoryStream fields = new();
        using (BinaryWriter writer = new(fields, System.Text.Encoding.UTF8, leaveOpen: true))
            _ = records.Iter(record => record.Write(writer));
        long info = sizeof(short) + fields.Length + records.Fold(0L, static (sum, record) => record.Channels.Fold(sum, static (total, channel) => total + channel.Data.Length));
        long layered = info + (info & 1);
        long size = (DataBlock.Length + (6 * sizeof(uint)) + layered + 3) & ~3L;
        if (size > System.Array.MaxLength)
            return Error.New(string.Create(CultureInfo.InvariantCulture, $"Document data block of {size} bytes exceeds {System.Array.MaxLength} byte buffer"));
        byte[] block = new byte[(int)size];
        using (BinaryWriter writer = new(new MemoryStream(block))) {
            writer.Write(DataBlock);
            writer.Write(Signature);
            writer.Write(BinaryPrimitives.ReadUInt32BigEndian("MTrn"u8));
            writer.Write(0u);
            writer.Write(Signature);
            writer.Write(Sample.Key);
            writer.Write((uint)layered);
            writer.Write((short)-records.Count);
            writer.Write(fields.GetBuffer(), 0, (int)fields.Length);
            _ = records.Iter(record => record.Channels.Iter(channel => writer.Write(channel.Data.Span)));
        }
        return block;
    }

    private Seq<LayerRecord> Records(Seq<DocumentLayer> layers) =>
        layers.Bind(layer => layer.Switch(
            this,
            raster: static (document, raster) => Seq(new LayerRecord(
                document.Canvas.Window, document.Channels(raster), raster.Mode.BlendModeKey, raster.Opacity, raster.Visible, raster.Mask.Map(_ => document.Canvas.Window), raster.Name, [])),
            folder: static (document, folder) => folder.Mode.Match(Some: static mode => mode.BlendModeKey, None: PassThrough) switch {
                var key => [Divider, .. document.Records(folder.Children), new LayerRecord(
                    (0L, 0L, 0L, 0L), GroupChannels, key, folder.Opacity, folder.Visible, None, folder.Name, [folder.Open ? 1u : 2u, Signature, key])],
            }));

    private Seq<(short Id, ReadOnlyMemory<byte> Data)> Channels(DocumentLayer.Raster raster) {
        Seq<(short Id, int Plane)> channels = ColorChannels + raster.Mask.Map(static _ => (Id: (short)-2, Plane: ColorChannels.Count)).ToSeq();
        byte[][] planes = [
            .. StillWriter.Planar(Sample.Tiff.Depth, Canvas.Size, ColorChannels.Count, (y, row) => raster.Frame.View.Span.GetRowSpan(y).CopyTo(row)),
            .. raster.Mask.Map(mask => StillWriter.Planar(Sample.Tiff.Depth, Canvas.Size, 1, (y, row) => Weigh(mask, y, row))[0]).ToSeq(),
        ];
        foreach (byte[] plane in planes)
            Sample.Tiff.Depth.ToBigEndian(plane);
        byte[][] compressed = new byte[channels.Count][];
        ParallelHelper.For(0, channels.Count, new ChannelAction(planes, compressed));
        return channels.Map(channel => (channel.Id, new ReadOnlyMemory<byte>(compressed[channel.Plane]))).Strict();
    }

    private void Weigh(Coverage.Matte mask, int y, Span<Vector4> row) {
        using SpanOwner<float> weights = SpanOwner<float>.Allocate(row.Length);
        mask.Weights(row, Canvas.Origin.X, Canvas.Line(y), weights.Span);
        for (int x = 0; x < row.Length; x++)
            row[x] = new Vector4(weights.Span[x]);
    }

    private Action<int, Span<byte>> Scanline(PixelFrame composite) {
        long left = long.Max(composite.Window.Left, 0L);
        int width = (int)long.Max(long.Min(composite.Window.Right, composite.Extent.Width) - left, 0L);
        return (line, scanline) => {
            scanline.Clear();
            if (width > 0 && line >= composite.Window.Top && line < composite.Window.Bottom)
                Sample.Tiff.Depth.Encode(
                    composite.View.Span.GetRowSpan(line - composite.Origin.Y).Slice((int)(left - composite.Origin.X), width), alpha: true,
                    scanline.Slice((int)Sample.Tiff.Depth.RowBytes((int)left, alpha: true), (int)Sample.Tiff.Depth.RowBytes(width, alpha: true)));
        };
    }

    private readonly record struct LayerRecord((long Left, long Top, long Right, long Bottom) Bounds, Seq<(short Id, ReadOnlyMemory<byte> Data)> Channels, uint Mode, float Opacity, bool Visible, Option<(long Left, long Top, long Right, long Bottom)> Mask, string Name, Seq<uint> Section) {
        internal void Write(BinaryWriter writer) {
            static void Edges(BinaryWriter into, (long Left, long Top, long Right, long Bottom) rectangle) {
                into.Write((int)rectangle.Top);
                into.Write((int)rectangle.Left);
                into.Write((int)rectangle.Bottom);
                into.Write((int)rectangle.Right);
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
                fields.Write(BinaryPrimitives.ReadUInt32BigEndian("luni"u8));
                fields.Write((uint)(sizeof(uint) + unicode.Length + sizeof(char)));
                fields.Write((uint)((unicode.Length / sizeof(char)) + 1));
                fields.Write(unicode);
                fields.Write((ushort)0);
                if (!Section.IsEmpty) {
                    fields.Write(Signature);
                    fields.Write(BinaryPrimitives.ReadUInt32BigEndian("lsct"u8));
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
            writer.Write([(byte)MathF.Round(Opacity * byte.MaxValue, MidpointRounding.ToEven), 0, (byte)(Visible ? 8 : 10), 0]);
            writer.Write((uint)extra.Length);
            writer.Write(extra.GetBuffer(), 0, (int)extra.Length);
        }
    }

    private readonly struct ChannelAction(byte[][] planes, byte[][] compressed) : IAction {
        public void Invoke(int i) {
            using MemoryStream output = new();
            output.Write([2, 0]);
            using (ZLibStream zlib = new(output, CompressionLevel.Optimal, leaveOpen: true))
                zlib.Write(planes[i]);
            compressed[i] = output.ToArray();
        }
    }
}
