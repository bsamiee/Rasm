using System.Runtime.InteropServices;

namespace Rasm.Materials.Bricks;

// --- [TYPES] ------------------------------------------------------------------------------
[Union]
public abstract partial record BrickPath {
    private BrickPath() { }
    public sealed record Line(double LengthMm) : BrickPath;
    public sealed record Arc(double RadiusMm, double SweepDegrees) : BrickPath;
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record BrickRun(
    BrickDesignation Unit,
    BondName Bond,
    BrickPath Path,
    double HeightMm,
    JointProfile Joint,
    Option<double> HeadJointMm,
    Option<double> BedJointMm);

public sealed record BrickAssembly(
    BrickRun Run,
    double LengthMm,
    int CourseCount,
    double HeadJointMm,
    double BedJointMm,
    Seq<BrickPlacement> Placements) {
    public static Fin<BrickAssembly> Layout(BrickRun run) =>
        from active in Optional(run).ToFin(Fail: new BrickFault.InvalidLayout(Detail: "BrickRun is required."))
        from length in LengthOf(path: active.Path)
        from _ in Validate(run: active, lengthMm: length)
        from joints in JointsOf(run: active)
        from __ in ValidateJoints(headJointMm: joints.Head, bedJointMm: joints.Bed)
        let courseCount = Math.Max(val1: 1, val2: (int)Math.Ceiling(a: active.HeightMm / active.Unit.Coursing.CourseHeightMm))
        from placements in Courses(run: active, lengthMm: length, courseCount: courseCount, headJointMm: joints.Head)
        select new BrickAssembly(Run: active, LengthMm: length, CourseCount: courseCount, HeadJointMm: joints.Head, BedJointMm: joints.Bed, Placements: placements);

    private static Fin<Unit> Validate(BrickRun run, double lengthMm) =>
        Positive(value: run.HeightMm, name: nameof(run.HeightMm))
            .Bind(_ => Positive(value: lengthMm, name: nameof(LengthMm)))
            .Bind(_ => Optional(run.Unit).ToFin(Fail: new BrickFault.InvalidLayout(Detail: "BrickDesignation is required.")).Map(_ => unit))
            .Bind(_ => Optional(run.Bond).ToFin(Fail: new BrickFault.InvalidLayout(Detail: "BondName is required.")).Map(_ => unit))
            .Bind(_ => Optional(run.Joint).ToFin(Fail: new BrickFault.InvalidLayout(Detail: "JointProfile is required.")).Map(_ => unit))
            .Bind(_ => Optional(run.Path).ToFin(Fail: new BrickFault.InvalidLayout(Detail: "BrickPath is required.")).Map(_ => unit))
            .Bind(_ => guard(run.Bond.Kind == BondKind.Template, () => (LanguageExt.Common.Error)new BrickFault.UnsupportedLayout(Detail: $"Bond '{run.Bond.Key}' is generated; layout requires a template bond.")).ToFin())
            .Bind(_ => guard(run.Bond.Fits(brick: run.Unit.Unit), () => (LanguageExt.Common.Error)new BrickFault.IncompatibleBond(BondKey: run.Bond.Key, UnitKey: run.Unit.Key)).ToFin());

    private static Fin<Unit> ValidateJoints(double headJointMm, double bedJointMm) =>
        Positive(value: headJointMm, name: nameof(HeadJointMm))
            .Bind(_ => Positive(value: bedJointMm, name: nameof(BedJointMm)).Map(_ => unit));

    private static Fin<(double Head, double Bed)> JointsOf(BrickRun run) =>
        Fin.Succ(value: (
            Head: run.HeadJointMm.IfNone(run.Unit.Region.StandardJointThicknessMm),
            Bed: run.BedJointMm.IfNone(run.Joint.DefaultThicknessMm)));

    private static Fin<double> LengthOf(BrickPath path) =>
        Optional(path).ToFin(Fail: new BrickFault.InvalidLayout(Detail: "BrickPath is required.")).Bind(active => active switch {
            BrickPath.Line line => Positive(value: line.LengthMm, name: nameof(BrickPath.Line.LengthMm)),
            BrickPath.Arc arc => Positive(value: arc.RadiusMm, name: nameof(BrickPath.Arc.RadiusMm))
                .Bind(radius => Positive(value: Math.Abs(value: arc.SweepDegrees), name: nameof(BrickPath.Arc.SweepDegrees))
                    .Map(sweep => radius * (Math.PI / 180.0) * sweep)),
            _ => Fin.Fail<double>(error: new BrickFault.UnsupportedLayout(Detail: $"Unsupported path '{active.GetType().Name}'.")),
        });

    private static Fin<double> Positive(double value, string name) =>
        from _ in guard(double.IsFinite(value) && value > 0.0, () => (LanguageExt.Common.Error)new BrickFault.InvalidLayout(Detail: $"{name} must be positive and finite (got {value:R}).")).ToFin()
        select value;

    private static Fin<Seq<BrickPlacement>> Courses(BrickRun run, double lengthMm, int courseCount, double headJointMm) =>
        toSeq(Enumerable.Range(start: 0, count: courseCount)).Fold(
            initialState: Fin.Succ(value: Seq<BrickPlacement>()),
            f: (acc, course) => acc.Bind(placements =>
                run.Bond.Course(index: course)
                    .ToFin(Fail: new BrickFault.UnsupportedLayout(Detail: $"Bond '{run.Bond.Key}' has no template course at index {course}."))
                    .Bind(template => Course(run: run, template: template, course: course, lengthMm: lengthMm, headJointMm: headJointMm)
                        .Map(coursePlacements => placements + coursePlacements))));

    private static Fin<Seq<BrickPlacement>> Course(BrickRun run, CourseTemplate template, int course, double lengthMm, double headJointMm) =>
        from _ in guard(template.Sequence.Count > 0, () => (LanguageExt.Common.Error)new BrickFault.InvalidLayout(Detail: $"Bond '{run.Bond.Key}' course {course} has no orientation sequence.")).ToFin()
        select Repeat(run: run, template: template, course: course, lengthMm: lengthMm, headJointMm: headJointMm);

    private static Seq<BrickPlacement> Repeat(BrickRun run, CourseTemplate template, int course, double lengthMm, double headJointMm) {
        Orientation first = template.Sequence[0];
        Footprint firstFootprint = FootprintOf(unit: run.Unit.Unit, orientation: first);
        double minStep = MinStep(unit: run.Unit.Unit, headJointMm: headJointMm);
        int count = Math.Max(val1: 1, val2: (int)Math.Ceiling(a: (lengthMm + firstFootprint.RunMm + run.Unit.Specified.LengthMm) / minStep) + template.Sequence.Count + 4);
        CourseState state = toSeq(Enumerable.Range(start: 0, count: count)).Fold(
            initialState: new CourseState(CursorMm: -template.OffsetFraction * firstFootprint.RunMm, Sequence: 0, Placements: Seq<BrickPlacement>()),
            f: (acc, _) => Place(run: run, template: template, course: course, lengthMm: lengthMm, headJointMm: headJointMm, state: acc));
        return state.Placements;
    }

    private static CourseState Place(BrickRun run, CourseTemplate template, int course, double lengthMm, double headJointMm, CourseState state) {
        Orientation orientation = template.Sequence[state.Sequence % template.Sequence.Count];
        Footprint footprint = FootprintOf(unit: run.Unit.Unit, orientation: orientation);
        double stationMm = state.CursorMm;
        double centerMm = stationMm + (footprint.RunMm / 2.0);
        Seq<BrickPlacement> placements = (stationMm < lengthMm && stationMm + footprint.RunMm > 0.0) switch {
            true => state.Placements.Add(new BrickPlacement(
                Course: course,
                Sequence: state.Sequence,
                StationMm: stationMm,
                ElevationMm: course * run.Unit.Coursing.CourseHeightMm,
                RunMm: footprint.RunMm,
                RiseMm: footprint.RiseMm,
                PathAngleDegrees: AngleAt(path: run.Path, stationMm: centerMm),
                Orientation: orientation,
                Cut: new Cut.None())),
            false => state.Placements,
        };
        return new CourseState(CursorMm: state.CursorMm + footprint.RunMm + headJointMm, Sequence: state.Sequence + 1, Placements: placements);
    }

    private static Footprint FootprintOf(Brick unit, Orientation orientation) =>
        orientation switch {
            Orientation.Stretcher => new Footprint(RunMm: unit.Specified.LengthMm, RiseMm: unit.Specified.HeightMm),
            Orientation.Header => new Footprint(RunMm: unit.Specified.WidthMm, RiseMm: unit.Specified.HeightMm),
            Orientation.Soldier => new Footprint(RunMm: unit.Specified.WidthMm, RiseMm: unit.Specified.LengthMm),
            Orientation.Sailor => new Footprint(RunMm: unit.Specified.HeightMm, RiseMm: unit.Specified.LengthMm),
            Orientation.Rowlock => new Footprint(RunMm: unit.Specified.LengthMm, RiseMm: unit.Specified.WidthMm),
            Orientation.Shiner => new Footprint(RunMm: unit.Specified.LengthMm, RiseMm: unit.Specified.WidthMm),
            _ => new Footprint(RunMm: unit.Specified.LengthMm, RiseMm: unit.Specified.HeightMm),
        };

    private static double AngleAt(BrickPath path, double stationMm) =>
        path switch {
            BrickPath.Line => 0.0,
            BrickPath.Arc arc => Math.Sign(value: arc.SweepDegrees) * stationMm / arc.RadiusMm * 180.0 / Math.PI,
            _ => 0.0,
        };

    private static double MinStep(Brick unit, double headJointMm) =>
        Seq(
            unit.Specified.LengthMm,
            unit.Specified.WidthMm,
            unit.Specified.HeightMm)
        .Map(value => value + headJointMm)
        .Fold(initialState: double.PositiveInfinity, f: static (acc, value) => Math.Min(val1: acc, val2: value));

    [StructLayout(LayoutKind.Auto)]
    private readonly record struct Footprint(double RunMm, double RiseMm);

    [StructLayout(LayoutKind.Auto)]
    private readonly record struct CourseState(double CursorMm, int Sequence, Seq<BrickPlacement> Placements);
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct BrickPlacement(
    int Course,
    int Sequence,
    double StationMm,
    double ElevationMm,
    double RunMm,
    double RiseMm,
    double PathAngleDegrees,
    Orientation Orientation,
    Cut Cut);
