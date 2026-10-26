using System.Globalization;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Wacton.Unicolour;

namespace Rasm.Imaging.Color;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class StandardObserver {
    private static readonly Memo<Arr<(float Wavelength, Vector3 Xyz)>> Rows = memo(static () => {
        using StreamReader reader = new(typeof(StandardObserver).Assembly.GetManifestResourceStream(typeof(StandardObserver), "Tables.CIE_xyz_1931_2deg.csv")!);
        return (Arr<(float Wavelength, Vector3 Xyz)>)[.. reader.ReadToEnd().Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(static line =>
            System.Array.ConvertAll(line.Split(','), static cell => float.Parse(cell, CultureInfo.InvariantCulture)) switch {
                var cells => (cells[0], new Vector3(cells[1], cells[2], cells[3])),
            })];
    });

    public static Arr<Vector2> Locus { get; } = [.. Cmf.RequiredWavelengths.Select(static wavelength =>
        new Unicolour(new Spd(wavelength, 1, 1d)).Chromaticity switch { var point => new Vector2((float)point.X, (float)point.Y) })];

    public static Arr<(float Wavelength, Vector3 Matching)> Matching(Gamut working) {
        Matrix<double> matrix = working.FromXyz;
        return Rows.Value.Map(row => matrix.Multiply(new DenseVector([row.Xyz.X, row.Xyz.Y, row.Xyz.Z])) switch {
            var rgb => (row.Wavelength, new Vector3((float)rgb[0], (float)rgb[1], (float)rgb[2])),
        });
    }
}
