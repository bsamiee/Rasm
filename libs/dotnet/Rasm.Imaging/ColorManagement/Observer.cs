using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using TinyEXR;

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
        Vector3[] matching = [.. Rows.Value.Map(static row => row.Xyz)];
        Span<float> lanes = MemoryMarshal.Cast<Vector3, float>(matching.AsSpan());
        ImageProcessing.ApplyColorMatrix(lanes, lanes, 3, working.FromXyz);
        return [.. Rows.Value.Zip(matching, static (row, rgb) => (row.Wavelength, rgb))];
    }
}
