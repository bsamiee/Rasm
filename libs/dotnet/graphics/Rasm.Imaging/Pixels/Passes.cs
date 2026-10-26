using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Helpers;
using Rasm.Imaging.Color;

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

public readonly record struct Channels<T>(T Red, T Green, T Blue) where T : notnull {
    public Channels(T all) : this(all, all, all) { }

    public static Lens<Channels<T>, T> RedOf { get; } = Lens<Channels<T>, T>.New(static channels => channels.Red, static red => channels => channels with { Red = red });
    public static Lens<Channels<T>, T> GreenOf { get; } = Lens<Channels<T>, T>.New(static channels => channels.Green, static green => channels => channels with { Green = green });
    public static Lens<Channels<T>, T> BlueOf { get; } = Lens<Channels<T>, T>.New(static channels => channels.Blue, static blue => channels => channels with { Blue = blue });

    public Vector256<double> Lanes(Func<T, double> lane) => Vector256.Create(lane(Red), lane(Green), lane(Blue), 0d);

    public static Fin<Channels<T>> Validated(Vector3 stated, Func<float, Fin<T>> validated) =>
        (validated(stated.X), validated(stated.Y), validated(stated.Z)).Apply(static (red, green, blue) => new Channels<T>(red, green, blue)).As();
}

file readonly struct RowAction(PixelFrame frame, Memory2D<Vector4> rows, Action<Span<Vector4>, long, long> kernel, IProgress<int> progress, StrongBox<int> finished) : IAction {
    public static Unit For(PixelFrame frame, Action<Span<Vector4>, long, long> kernel, IProgress<int> progress) {
        ParallelHelper.For(0, frame.Size.Height, new RowAction(frame, frame.View, kernel, progress, new StrongBox<int>()));
        return unit;
    }

    public void Invoke(int i) {
        kernel(rows.Span.GetRowSpan(i), frame.Origin.X, frame.Line(i));
        progress.Report(Interlocked.Increment(ref finished.Value));
    }
}
