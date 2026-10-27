using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using CommunityToolkit.HighPerformance.Helpers;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Rasm.Imaging.Color;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rasm.Imaging.Pixels;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PixelPass {
    public abstract Fin<Unit> Run(PixelFrame frame, IProgress<int> progress);

    public sealed record Color : PixelPass {
        public Color(Action<Span<Vector4>> row) => (Row, Table) = (row, None);
        public Color(LutTable table) => (Row, Table) = (table.Apply, table);

        public Action<Span<Vector4>> Row { get; }
        public Option<LutTable> Table { get; }

        public override Fin<Unit> Run(PixelFrame frame, IProgress<int> progress) => RowAction.For(frame, (row, _, _) => Row(row), progress);
    }

    public sealed record Pointwise(Action<Span<Vector4>, long, long> Row) : PixelPass {
        public override Fin<Unit> Run(PixelFrame frame, IProgress<int> progress) => RowAction.For(frame, Row, progress);
    }

    public sealed record Frame(Func<PixelFrame, IProgress<int>, Fin<Unit>> Kernel) : PixelPass {
        public override Fin<Unit> Run(PixelFrame frame, IProgress<int> progress) => Kernel(frame, progress);
    }
}

public sealed record Channels<T>(T Red, T Green, T Blue) where T : notnull {
    public Channels(T all) : this(all, all, all) { }

    public static Fin<Channels<T>> Validated(Vector3 stated, Func<float, Fin<T>> validated) =>
        (validated(stated.X), validated(stated.Y), validated(stated.Z)).Apply(static (red, green, blue) => new Channels<T>(red, green, blue)).As();
}

file readonly struct RowAction(PixelFrame frame, Memory2D<Vector4> rows, Action<Span<Vector4>, long, long> kernel, IProgress<int> progress, StrongBox<int> finished) : IAction {
    internal static Unit For(PixelFrame frame, Action<Span<Vector4>, long, long> kernel, IProgress<int> progress) {
        ParallelHelper.For(0, frame.Size.Height, new RowAction(frame, frame.View, kernel, progress, new StrongBox<int>()));
        return unit;
    }

    public void Invoke(int i) {
        kernel(rows.Span.GetRowSpan(i), frame.Origin.X, frame.Line(i));
        progress.Report(Interlocked.Increment(ref finished.Value));
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static partial class PixelSampling {
    extension(PixelFrame frame) {
        internal Mat Premultiplied() {
            Mat associated = new(frame.Size.Height, frame.Size.Width, DepthType.Cv32F, 4);
            PixelOperations<RgbaVector>.Instance.ToVector4(Configuration.Default, frame.Block.Cast<float, RgbaVector>(), associated.GetSpan<Vector4>(), PixelConversionModifiers.Premultiply);
            return associated;
        }

        public unsafe Fin<Unit> Mutate(Action<Image<RgbaVector>> edit) {
            Span2D<float> samples = new((void*)frame.Address, frame.Size.Width * frame.Size.Height, 4, 0);
            using SpanOwner<float> alpha = SpanOwner<float>.Allocate(samples.Height);
            samples.GetColumn(3).CopyTo(alpha.Span);
            samples.GetColumn(3).Fill(1f);
            Fin<Unit> edited = Try.lift(() => {
                using Image<RgbaVector> image = Image.WrapMemory<RgbaVector>((void*)frame.Address, frame.Block.Length * sizeof(float), frame.Size.Width, frame.Size.Height);
                edit(image);
            }).Run();
            alpha.Span.CopyTo(samples.GetColumn(3));
            return edited;
        }
    }
}
