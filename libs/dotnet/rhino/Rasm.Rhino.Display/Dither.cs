using System.Numerics;
using Rasm.Rhino.Document;

namespace Rasm.Rhino.Display;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
[ValidationError<ValidationFailure>]
public sealed partial class Dither {
    public static readonly Dither Off = new(nameof(Off), Noised(static (_, _) => 0.5f));

    public static readonly Dither Triangular = new(nameof(Triangular), Noised(Hashed));

    public static readonly Dither BlueNoise = new(nameof(BlueNoise), Noised(BlueNoised));

    public static readonly Dither FloydSteinberg = new(nameof(FloydSteinberg), new PixelPass.Frame(Diffused));

    private const float MaxCode = byte.MaxValue;
    private const int Tile = 64;
    private static readonly float[] Ranks = VoidAndCluster();

    public PixelPass Pass { get; }

    private static PixelPass.Pointwise Noised(Func<int, int, float> uniform) =>
        new((pixel, x, y) => {
            float signed = (2f * uniform(x, y)) - 1f;
            return Vector4.Create(Level(Quantized((MaxCode * pixel.AsVector3()) + Vector3.Create(MathF.CopySign(1f - MathF.Sqrt(1f - MathF.Abs(signed)), signed)))), pixel.W);
        });

    private static float Hashed(int x, int y) {
        const uint iq = 1103515245u;
        return unchecked(iq * ((iq * (((uint)x >> 1) ^ (uint)y)) ^ ((iq * (((uint)y >> 1) ^ (uint)x)) >> 3))) * (1f / uint.MaxValue);
    }

    private static float BlueNoised(int x, int y) =>
        Ranks[(y % Tile * Tile) + (x % Tile)];

    private static void Diffused(Span<Vector4> pixels, int width) {
        Vector3[] carried = new Vector3[2 * width];
        for (int line = 0; line < pixels.Length / width; line++) {
            int direction = (line & 1) == 0 ? 1 : -1;
            Span<Vector3> current = carried.AsSpan((line & 1) * width, width);
            Span<Vector3> next = carried.AsSpan(((line + 1) & 1) * width, width);
            next.Clear();
            for (int step = 0; step < width; step++) {
                int x = direction > 0 ? step : width - 1 - step;
                ref Vector4 pixel = ref pixels[(line * width) + x];
                Vector3 held = (MaxCode * pixel.AsVector3()) + current[x];
                Vector3 code = Quantized(held);
                Vector3 residual = held - code;
                pixel = Vector4.Create(Level(code), pixel.W);
                Spread(current, x + direction, 7f / 16f * residual);
                Spread(next, x - direction, 3f / 16f * residual);
                Spread(next, x, 5f / 16f * residual);
                Spread(next, x + direction, 1f / 16f * residual);
            }
        }
    }

    private static Vector3 Quantized(Vector3 code) =>
        Vector3.Truncate(Vector3.Clamp(code + Vector3.Create(0.5f), Vector3.Zero, Vector3.Create(MaxCode)));

    private static Vector3 Level(Vector3 quantized) =>
        (quantized + Vector3.Create(0.5f)) / (MaxCode + 0.5f);

    private static void Spread(Span<Vector3> row, int x, Vector3 share) {
        if (x >= 0 && x < row.Length)
            row[x] += share;
    }

    private static float[] VoidAndCluster() {
        const int count = Tile * Tile;
        const double sigma = 1.5;
        const float density = 0.1f;
        static int Wrapped(int cell, int source) =>
            (((cell / Tile) - (source / Tile) + Tile) % Tile * Tile) + (((cell % Tile) - (source % Tile) + Tile) % Tile);
        double[] kernel = [.. Enumerable.Range(0, count).Select(static offset => {
            int dx = Math.Min(offset % Tile, Tile - (offset % Tile));
            int dy = Math.Min(offset / Tile, Tile - (offset / Tile));
            return Math.Exp(-((dx * dx) + (dy * dy)) / (2 * sigma * sigma));
        })];
        bool[] ones = new bool[count];
        double[] energy = new double[count];
        int[] ranks = new int[count];
        void Toggle(int pixel, bool on) {
            ones[pixel] = on;
            for (int cell = 0; cell < count; cell++)
                energy[cell] += (on ? 1 : -1) * kernel[Wrapped(cell, pixel)];
        }
        int Extreme(bool one) =>
            Enumerable.Range(0, count).Where(pixel => ones[pixel] == one).Aggregate((best, pixel) => (one ? energy[pixel] > energy[best] : energy[pixel] < energy[best]) ? pixel : best);
        for (int pixel = 0; pixel < count; pixel++) {
            if (Hashed(pixel % Tile, pixel / Tile) < density)
                Toggle(pixel, on: true);
        }
        int cluster = Extreme(one: true);
        Toggle(cluster, on: false);
        for (int gap = Extreme(one: false); gap != cluster; gap = Extreme(one: false)) {
            Toggle(gap, on: true);
            cluster = Extreme(one: true);
            Toggle(cluster, on: false);
        }
        Toggle(cluster, on: true);
        bool[] prototypeOnes = [.. ones];
        double[] prototypeEnergy = [.. energy];
        int seeded = ones.Count(static one => one);
        for (int rank = seeded - 1; rank >= 0; rank--) {
            int tightest = Extreme(one: true);
            Toggle(tightest, on: false);
            ranks[tightest] = rank;
        }
        prototypeOnes.CopyTo(ones, 0);
        prototypeEnergy.CopyTo(energy, 0);
        for (int rank = seeded; rank < count; rank++) {
            int largest = Extreme(one: false);
            Toggle(largest, on: true);
            ranks[largest] = rank;
        }
        return [.. ranks.Select(static rank => (rank + 0.5f) / count)];
    }
}
