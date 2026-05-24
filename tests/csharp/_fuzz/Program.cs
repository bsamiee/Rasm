using System.Globalization;
using Rasm.Materials.Bricks;
using SharpFuzz;

namespace Rasm.Fuzz;

// --- [SERVICES] ---------------------------------------------------------------------------
internal static class Program {
    public static void Main() =>
        Fuzzer.OutOfProcess.Run(BrickLayoutScript, 4096);

    private static void BrickLayoutScript(string token) =>
        _ = token.Split(separator: ',', options: StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) switch {
            string[] cells => BrickAssembly.Layout(run: new BrickRun(
                Unit: UnitOf(cells: cells),
                Bond: BondOf(cells: cells),
                Path: PathOf(cells: cells),
                HeightMm: Scalar(cells: cells, index: 2, fallback: 240.0, min: 1.0, max: 2400.0),
                Joint: JointOf(cells: cells),
                HeadJointMm: JointOverride(cells: cells, index: 5),
                BedJointMm: JointOverride(cells: cells, index: 6))).Match(
                    Succ: static assembly => assembly.Placements.Count,
                    Fail: static fault => fault.ToString().Length),
        };

    private static BrickPath PathOf(string[] cells) =>
        Scalar(cells: cells, index: 3, fallback: 0.0, min: -360.0, max: 360.0) switch {
            double sweep when Math.Abs(value: sweep) > 1.0e-6 => new BrickPath.Arc(
                RadiusMm: Scalar(cells: cells, index: 1, fallback: 1000.0, min: 1.0, max: 10000.0),
                SweepDegrees: sweep),
            _ => new BrickPath.Line(LengthMm: Scalar(cells: cells, index: 1, fallback: 1000.0, min: 1.0, max: 10000.0)),
        };

    private static BondName BondOf(string[] cells) =>
        Text(cells: cells, index: 0).ToUpperInvariant() switch {
            "STACK" => BondName.Stack,
            "HEADER" => BondName.Header,
            "ENGLISH" => BondName.English,
            "HERRINGBONE" or "HERRINGBONE-45" => BondName.Herringbone45,
            _ => BondName.Running,
        };

    private static BrickDesignation UnitOf(string[] cells) =>
        Text(cells: cells, index: 4).ToUpperInvariant() switch {
            "ROMAN" => BrickDesignation.UsRoman,
            "UTILITY" => BrickDesignation.UsUtility,
            "MODULAR" or _ => BrickDesignation.UsModular,
        };

    private static JointProfile JointOf(string[] cells) =>
        Text(cells: cells, index: 7).ToUpperInvariant() switch {
            "FLUSH" => JointProfile.Flush,
            "RAKED" => JointProfile.Raked,
            "V" or "V-JOINT" => JointProfile.V,
            _ => JointProfile.Concave,
        };

    private static Option<double> JointOverride(string[] cells, int index) =>
        Scalar(cells: cells, index: index, fallback: 0.0, min: 0.0, max: 50.0) switch {
            > 0.0 and double value => Some(value),
            _ => Option<double>.None,
        };

    private static double Scalar(string[] cells, int index, double fallback, double min, double max) =>
        double.TryParse(
            s: Text(cells: cells, index: index),
            style: NumberStyles.Float,
            provider: CultureInfo.InvariantCulture,
            result: out double parsed) && double.IsFinite(parsed)
            ? Math.Clamp(value: Math.Abs(value: parsed), min: min, max: max)
            : fallback;

    private static string Text(string[] cells, int index) =>
        index >= 0 && index < cells.Length ? cells[index] : string.Empty;
}
