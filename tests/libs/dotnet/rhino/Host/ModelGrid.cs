using CsCheck;
using Rasm.Rhino.Document.Units;
using Rhino;

namespace Rasm.Rhino.Tests.Host;

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class ModelGrid {

    // --- [TOLERANCES]

    public static Tolerances Model { get; } = Tolerances.Create(RhinoMath.DefaultDistanceToleranceMillimeters, 0.01, RhinoMath.DefaultAngleTolerance);

    // --- [GENERATORS]

    public static Gen<double> Coordinate { get; } = Gen.Int[-100_000, 100_000].Select(static units => units * Model.Absolute);

    public static Gen<Point3d> Point { get; } = Coordinate.Select(Coordinate, Coordinate, static (x, y, z) => new Point3d(x, y, z));

    public static Gen<Vector3d> Offset { get; } = Coordinate.Select(Coordinate, Coordinate, static (x, y, z) => new Vector3d(x, y, z));

    public static Gen<double> Size { get; } = Gen.Int[1_000, 50_000].Select(static units => units * Model.Absolute);

    public static Gen<double> Fraction { get; } = Gen.Int[0, 1000].Select(static step => step / 1000d);

    public static Gen<(Vector3d Axis, double Angle)> Turn { get; } =
        from x in Gen.Int[-1000, 1000]
        from y in Gen.Int[-1000, 1000]
        from z in Gen.Int[-1000, 1000]
        from steps in Gen.Int[-179_999, 179_999]
        let axis = new Vector3d(x, y, z)
        where axis.Length > 100d
        select (Axis: axis, Angle: steps * RhinoMath.DefaultAngleTolerance / 1000d);
}
