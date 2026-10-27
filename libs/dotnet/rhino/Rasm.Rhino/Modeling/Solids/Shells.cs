using Rasm.RhinoCommon.Document.Units;

namespace Rasm.RhinoCommon.Modeling.Solids;

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
    public static IO<OffsetResult> Offset(Tolerances tolerances, Brep brep, double distance, bool solid, bool extend, bool shrink) =>
        Brep.CreateOffsetBrep(brep, distance, solid, extend, shrink, tolerances.Absolute, out Brep[]? blends, out Brep[]? walls) switch {
            var offsets =>
                from offsetPieces in Products.Made(offsets)
                from blendPieces in Products.Made(blends)
                from wallPieces in Products.Made(walls)
                from filled in IO.lift(() => RhinoFault.Unless(!offsetPieces.IsEmpty))
                select offsetPieces.Count is 1 && blendPieces.IsEmpty && wallPieces.IsEmpty
                    ? (OffsetResult)new OffsetResult.Joined(offsetPieces[0])
                    : new OffsetResult.Pieces(offsetPieces, blendPieces, wallPieces),
        };

    // --- [PIPES]
    public static IO<Seq<Brep>> Pipe(Tolerances tolerances, Curve rail, PipeRadii radii, bool localBlending, PipeCapMode cap, bool fitRail) =>
        from piped in radii.Switch(
            (Rail: rail, LocalBlending: localBlending, Cap: cap, FitRail: fitRail, Tolerances: tolerances),
            singleWall: static (pipe, wall) => Products.Made(Brep.CreatePipe(pipe.Rail,
                from station in wall.Stations select station.NormalizedParameter,
                from station in wall.Stations select station.Radius,
                pipe.LocalBlending, pipe.Cap, pipe.FitRail, pipe.Tolerances.Absolute, pipe.Tolerances.Angle)),
            doubleWall: static (pipe, wall) => Products.Made(Brep.CreateThickPipe(pipe.Rail,
                from station in wall.Stations select station.NormalizedParameter,
                from station in wall.Stations select station.Radius0,
                from station in wall.Stations select station.Radius1,
                pipe.LocalBlending, pipe.Cap, pipe.FitRail, pipe.Tolerances.Absolute, pipe.Tolerances.Angle)))
        from filled in IO.lift(() => RhinoFault.Unless(!piped.IsEmpty))
        select piped;
}
