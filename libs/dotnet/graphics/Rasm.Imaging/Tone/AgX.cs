using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
using Rasm.Imaging.Color;

namespace Rasm.Imaging.Tone;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class AgX {
    public static LutTable Grey { get; } = (X: 0.2589235355689848d, Y: 0.6104985346066525d, Z: 0.13057792982436284d) switch {
        var weights => new LutTable.Affine(Matrix<double>.Build.Dense(3, 3, (_, column) => column switch { 0 => weights.X, 1 => weights.Y, _ => weights.Z })),
    };

    public static Action<Span<Vector4>> Formed(Gamut working, DisplayDevice device, LogSpace space, Option<Action<Span<Vector4>>> look, LutTable table, ColorEncoding signal) =>
        device.Formed(working, space.Gamut, Opened(space, look).Add(table.Apply), signal);

    internal static Seq<Action<Span<Vector4>>> Opened(LogSpace space, Option<Action<Span<Vector4>>> look) =>
        look.Match(
            Some: chosen => space.Into.Map<Action<Span<Vector4>>>(static step => step.Apply).Add(chosen) + space.Back.Map<Action<Span<Vector4>>>(static step => step.Apply),
            None: Seq<Action<Span<Vector4>>>())
        .Add(new LutTable.Log(space.Allocation).Apply);
}
