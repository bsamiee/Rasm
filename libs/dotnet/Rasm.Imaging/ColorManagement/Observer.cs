using System.Globalization;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Rasm.Imaging.ColorManagement;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class StandardObserver {
    private static readonly Memo<Arr<(float Wavelength, Vector3 Xyz)>> Rows = memo(static () => {
        using StreamReader reader = new(EmbeddedTables.Open("CIE_xyz_1931_2deg.csv"));
        return (Arr<(float Wavelength, Vector3 Xyz)>)[.. reader.ReadToEnd().Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(static line =>
            System.Array.ConvertAll(line.Split(','), static cell => float.Parse(cell, CultureInfo.InvariantCulture)) switch {
                var cells => (cells[0], new Vector3(cells[1], cells[2], cells[3])),
            })];
    });

    public static Arr<(float Wavelength, Vector3 Matching)> Matching(Gamut working) {
        Matrix<double> matrix = working.FromXyz;
        return Rows.Value.Map(row => matrix.Multiply(new DenseVector([row.Xyz.X, row.Xyz.Y, row.Xyz.Z])) switch {
            var rgb => (row.Wavelength, new Vector3((float)rgb[0], (float)rgb[1], (float)rgb[2])),
        });
    }
}
