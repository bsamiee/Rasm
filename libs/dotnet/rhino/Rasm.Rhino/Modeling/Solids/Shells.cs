using Rasm.Rhino.Document.Units;

namespace Rasm.Rhino.Modeling.Solids;

// --- [MODELS] --------------------------------------------------------------------------
[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record PipeRadii {
    public sealed record SingleWall(IterableNE<(double NormalizedParameter, double Radius)> Stations) : PipeRadii;
    public sealed record DoubleWall(IterableNE<(double NormalizedParameter, double Radius0, double Radius1)> Stations) : PipeRadii;
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record OffsetResult {
    public sealed record Joined(Brep Offset) : OffsetResult;
    public sealed record Pieces(Seq<Brep> Offsets, Seq<Brep> Blends, Seq<Brep> Walls) : OffsetResult;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Shells {
    // --- [OFFSETS]
    public static IO<OffsetResult> Offset(Tolerances tolerances, Brep brep, double distance, bool solid, bool extend, bool shrink, Action<IDisposable> retain) =>
        Copies.Acquire(nameof(Brep.CreateOffsetBrep), _ =>
            Brep.CreateOffsetBrep(brep, distance, solid, extend, shrink, tolerances.Absolute, out Brep[]? blends, out Brep[]? walls) switch {
                var offsets => (Answer.NonEmpty(offsets), Answer.Many(blends), Answer.Many(walls))
                    .Apply(static OffsetResult (offsetPieces, blendPieces, wallPieces) => offsetPieces.Count is 1 && blendPieces.IsEmpty && wallPieces.IsEmpty
                        ? new OffsetResult.Joined(offsetPieces[0])
                        : new OffsetResult.Pieces(offsetPieces, blendPieces, wallPieces)).As(),
            }, retain);

    // --- [PIPES]
    public static IO<Seq<Brep>> Pipe(Tolerances tolerances, Curve rail, PipeRadii radii, bool localBlending, PipeCapMode cap, bool fitRail, Action<IDisposable> retain) =>
        radii.Switch(
            (Rail: rail, LocalBlending: localBlending, Cap: cap, FitRail: fitRail, Tolerances: tolerances, Retain: retain),
            singleWall: static (pipe, wall) => Copies.Acquire(nameof(Brep.CreatePipe), _ => Answer.NonEmpty(Brep.CreatePipe(pipe.Rail,
                from station in wall.Stations select station.NormalizedParameter,
                from station in wall.Stations select station.Radius,
                pipe.LocalBlending, pipe.Cap, pipe.FitRail, pipe.Tolerances.Absolute, pipe.Tolerances.Angle)), pipe.Retain),
            doubleWall: static (pipe, wall) => Copies.Acquire(nameof(Brep.CreateThickPipe), _ => Answer.NonEmpty(Brep.CreateThickPipe(pipe.Rail,
                from station in wall.Stations select station.NormalizedParameter,
                from station in wall.Stations select station.Radius0,
                from station in wall.Stations select station.Radius1,
                pipe.LocalBlending, pipe.Cap, pipe.FitRail, pipe.Tolerances.Absolute, pipe.Tolerances.Angle)), pipe.Retain));
}
