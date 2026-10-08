using Rasm.Rhino.Document.Notation;
using Rasm.Rhino.Document.Shapes;

namespace Rasm.Rhino.Modeling.Solids;

// --- [MODELS] --------------------------------------------------------------------------
[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record PipeRadii {
    public sealed record SingleWall(IterableNE<(double NormalizedParameter, double Radius)> Stations) : PipeRadii;
    public sealed record DoubleWall(IterableNE<(double NormalizedParameter, double Radius0, double Radius1)> Stations) : PipeRadii;
}

public sealed record OffsetResult(Seq<Brep> Offsets, Seq<Brep> Blends, Seq<Brep> Walls);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Shells {
    // --- [OFFSETS]
    public static IO<OffsetResult> Offset(Brep brep, double distance, bool solid, bool extend, bool shrink, Tolerances tolerances) =>
        Copies.Owned(
            IO.lift(() => (
                Offsets: Brep.CreateOffsetBrep(brep, distance, solid, extend, shrink, tolerances.Absolute, out Brep[]? blends, out Brep[]? walls),
                Blends: blends,
                Walls: walls)),
            static Fin<OffsetResult> (made) => made.Blends is null
                ? new Refused(nameof(Brep.CreateOffsetBrep))
                : (Measurements.Valid(toSeq<Brep?>(made.Offsets), nameof(Brep.CreateOffsetBrep)).ToValidation(),
                   Measurements.Valid(toSeq<Brep?>(made.Blends), nameof(OffsetResult.Blends)).ToValidation(),
                   Measurements.Valid(toSeq<Brep?>(made.Walls), nameof(OffsetResult.Walls)).ToValidation())
                    .Apply(static (offsets, blends, walls) => new OffsetResult(offsets, blends, walls)).As().ToFin(),
            static made => DisposalOps.Release(Conversions.Rows(made.Offsets) + Conversions.Rows(made.Blends) + Conversions.Rows(made.Walls)));

    // --- [PIPES]
    public static IO<Seq<Brep>> Pipe(Curve rail, PipeRadii radii, bool localBlending, PipeCapMode cap, bool fitRail, Tolerances tolerances) =>
        radii.Switch(
            (Rail: rail, LocalBlending: localBlending, Cap: cap, FitRail: fitRail, Tolerances: tolerances),
            singleWall: static (pipe, wall) => Copies.AcquireNonEmpty(
                () => Brep.CreatePipe(pipe.Rail,
                    from station in wall.Stations select station.NormalizedParameter,
                    from station in wall.Stations select station.Radius,
                    pipe.LocalBlending, pipe.Cap, pipe.FitRail, pipe.Tolerances.Absolute, pipe.Tolerances.Angle),
                nameof(Brep.CreatePipe)),
            doubleWall: static (pipe, wall) => Copies.AcquireNonEmpty(
                () => Brep.CreateThickPipe(pipe.Rail,
                    from station in wall.Stations select station.NormalizedParameter,
                    from station in wall.Stations select station.Radius0,
                    from station in wall.Stations select station.Radius1,
                    pipe.LocalBlending, pipe.Cap, pipe.FitRail, pipe.Tolerances.Absolute, pipe.Tolerances.Angle),
                nameof(Brep.CreateThickPipe)));
}
