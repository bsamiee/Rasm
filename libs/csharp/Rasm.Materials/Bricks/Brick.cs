using System.Globalization;
using System.Runtime.InteropServices;
using Foundation.CSharp.Analyzers.Contracts;

namespace Rasm.Materials.Bricks;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class CoringClass {
    public static readonly CoringClass Solid = new(key: "solid");
    public static readonly CoringClass Cored = new(key: "cored");
    public static readonly CoringClass Perforated = new(key: "perforated");
    public static readonly CoringClass Hollow = new(key: "hollow");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class BondCategory {
    public static readonly BondCategory Discrete = new(key: "discrete");
    public static readonly BondCategory Parametric = new(key: "parametric");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class CopingProfile {
    public static readonly CopingProfile Saddle = new(key: "saddle");
    public static readonly CopingProfile HalfRound = new(key: "half-round");
    public static readonly CopingProfile Splayed = new(key: "splayed");
    public static readonly CopingProfile Throated = new(key: "throated");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class PlinthKind {
    public static readonly PlinthKind Header = new(key: "header");
    public static readonly PlinthKind Stretcher = new(key: "stretcher");
    public static readonly PlinthKind ExternalReturn = new(key: "external-return");
    public static readonly PlinthKind InternalReturn = new(key: "internal-return");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class BrickType {
    public static readonly BrickType Fbx = new(key: "FBX", dimensionalToleranceMm: 3.2, warpageToleranceMm: 2.4, chippage: Some(new ChippageRule(MinPercent: 95.0, MaxPercent: 100.0, WithinMm: 3.2)));
    public static readonly BrickType Fbs = new(key: "FBS", dimensionalToleranceMm: 4.8, warpageToleranceMm: 3.2, chippage: Some(new ChippageRule(MinPercent: 90.0, MaxPercent: 100.0, WithinMm: 4.8)));
    public static readonly BrickType Fba = new(key: "FBA", dimensionalToleranceMm: 6.4, warpageToleranceMm: 4.8, chippage: Option<ChippageRule>.None);
    public static readonly BrickType Hbx = new(key: "HBX", dimensionalToleranceMm: 3.2, warpageToleranceMm: 2.4, chippage: Some(new ChippageRule(MinPercent: 95.0, MaxPercent: 100.0, WithinMm: 3.2)));
    public static readonly BrickType Hbs = new(key: "HBS", dimensionalToleranceMm: 4.8, warpageToleranceMm: 3.2, chippage: Some(new ChippageRule(MinPercent: 90.0, MaxPercent: 100.0, WithinMm: 4.8)));
    public static readonly BrickType Hba = new(key: "HBA", dimensionalToleranceMm: 7.9, warpageToleranceMm: 6.4, chippage: Option<ChippageRule>.None);
    public double DimensionalToleranceMm { get; }
    public double WarpageToleranceMm { get; }
    public Option<ChippageRule> Chippage { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class Coring {
    public static readonly Coring None = new(key: "none", voidFraction: 0.00, classification: CoringClass.Solid);
    public static readonly Coring SingleFrog = new(key: "single-frog", voidFraction: 0.10, classification: CoringClass.Solid);
    public static readonly Coring DoubleFrog = new(key: "double-frog", voidFraction: 0.18, classification: CoringClass.Solid);
    public static readonly Coring Cored3Hole = new(key: "cored-3-hole", voidFraction: 0.20, classification: CoringClass.Cored);
    public static readonly Coring Cored5Hole = new(key: "cored-5-hole", voidFraction: 0.22, classification: CoringClass.Cored);
    public static readonly Coring Cored10Hole = new(key: "cored-10-hole", voidFraction: 0.24, classification: CoringClass.Cored);
    public static readonly Coring Perforated5Cell = new(key: "perforated-5-cell", voidFraction: 0.30, classification: CoringClass.Perforated);
    public static readonly Coring Perforated10Cell = new(key: "perforated-10-cell", voidFraction: 0.42, classification: CoringClass.Perforated);
    public static readonly Coring Perforated16Cell = new(key: "perforated-16-cell", voidFraction: 0.50, classification: CoringClass.Perforated);
    public static readonly Coring Hollow2Cell = new(key: "hollow-2-cell", voidFraction: 0.50, classification: CoringClass.Hollow);
    public static readonly Coring Hollow3Cell = new(key: "hollow-3-cell", voidFraction: 0.55, classification: CoringClass.Hollow);
    public static readonly Coring Hollow4Cell = new(key: "hollow-4-cell", voidFraction: 0.60, classification: CoringClass.Hollow);
    public static readonly Coring Slotted = new(key: "slotted", voidFraction: 0.50, classification: CoringClass.Perforated);
    public double VoidFraction { get; }
    public CoringClass Classification { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class JointProfile {
    public static readonly JointProfile Concave = new(key: "concave", defaultThicknessMm: 9.525, description: "Compacted with curved jointer; default US exterior.");
    public static readonly JointProfile V = new(key: "v-joint", defaultThicknessMm: 9.525, description: "Crisp shadow line; Craftsman / Tudor.");
    public static readonly JointProfile Weathered = new(key: "weathered", defaultThicknessMm: 9.525, description: "Struck-down; sloped to shed water.");
    public static readonly JointProfile Struck = new(key: "struck", defaultThicknessMm: 9.525, description: "Struck-up; interior only.");
    public static readonly JointProfile Flush = new(key: "flush", defaultThicknessMm: 9.525, description: "Smooth modern look; good for paint or plaster.");
    public static readonly JointProfile Raked = new(key: "raked", defaultThicknessMm: 9.525, description: "Deep recess for dramatic shadow; demands dense brick.");
    public static readonly JointProfile Beaded = new(key: "beaded", defaultThicknessMm: 9.525, description: "Decorative bead; historic.");
    public static readonly JointProfile Extruded = new(key: "extruded", defaultThicknessMm: 9.525, description: "Squeezed-out rustic profile; interior only.");
    public static readonly JointProfile Grapevine = new(key: "grapevine", defaultThicknessMm: 9.525, description: "Colonial revival raised-ridge profile.");
    public double DefaultThicknessMm { get; }
    public string Description { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class ArchProfile {
    public static readonly ArchProfile Jack = new(key: "jack", rule: ArchRule.Bia31Universal);
    public static readonly ArchProfile Segmental = new(key: "segmental", rule: ArchRule.Bia31Universal);
    public static readonly ArchProfile Semicircular = new(key: "semicircular", rule: ArchRule.Bia31Universal);
    public static readonly ArchProfile Multicentered = new(key: "multicentered", rule: ArchRule.Bia31Universal);
    public static readonly ArchProfile Gothic = new(key: "gothic", rule: ArchRule.Bia31Universal);
    public static readonly ArchProfile Bullseye = new(key: "bullseye", rule: ArchRule.Bia31Universal);
    public static readonly ArchProfile Relieving = new(key: "relieving", rule: ArchRule.Bia31Universal);
    public ArchRule Rule { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class BrickRegion {
    public static readonly BrickRegion Us = new(key: "us", standardJointThicknessMm: 9.525, policy: RegionalMasonryPolicy.UsAuthoritative);
    public static readonly BrickRegion Uk = new(key: "uk", standardJointThicknessMm: 10.0, policy: RegionalMasonryPolicy.UsAuthoritative);
    public static readonly BrickRegion Din = new(key: "din", standardJointThicknessMm: 12.0, policy: RegionalMasonryPolicy.UsAuthoritative);
    public static readonly BrickRegion Au = new(key: "au", standardJointThicknessMm: 10.0, policy: RegionalMasonryPolicy.UsAuthoritative);
    public static readonly BrickRegion Is = new(key: "is", standardJointThicknessMm: 10.0, policy: RegionalMasonryPolicy.UsAuthoritative);
    public double StandardJointThicknessMm { get; }
    public RegionalMasonryPolicy Policy { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class BondName {
    private static readonly Seq<Orientation> AllStretcher = Seq<Orientation>(new Orientation.Stretcher());
    private static readonly Seq<Orientation> AllHeader = Seq<Orientation>(new Orientation.Header());
    private static readonly Seq<Orientation> AllSoldier = Seq<Orientation>(new Orientation.Soldier());
    private static readonly Option<AspectRatio> Aspect2 = Some(AspectRatio.Create(lengthOverWidth: 2.0));
    private static readonly Option<AspectRatio> Aspect3 = Some(AspectRatio.Create(lengthOverWidth: 3.0));
    private static CourseTemplate StretcherAt(double offset) => new(Sequence: AllStretcher, OffsetFraction: offset);
    private static CourseTemplate HeaderAt(double offset) => new(Sequence: AllHeader, OffsetFraction: offset);
    private static CourseTemplate SoldierAt(double offset) => new(Sequence: AllSoldier, OffsetFraction: offset);
    private static CourseTemplate Mixed(double offset, params Orientation[] sequence) => new(Sequence: toSeq(sequence), OffsetFraction: offset);
    private static BondName Simple(string key, params CourseTemplate[] courses) =>
        new(key: key, category: BondCategory.Discrete, closure: new ClosureRule.None(), requiredAspect: Option<AspectRatio>.None, correctionFactor: 0.0, courses: toSeq(courses));
    private static BondName Closer(string key, double correctionFactor, params CourseTemplate[] courses) =>
        new(key: key, category: BondCategory.Discrete, closure: new ClosureRule.QueenCloser(InsetCourses: 1), requiredAspect: Aspect2, correctionFactor: correctionFactor, courses: toSeq(courses));
    private static BondName Aspected(string key, Option<AspectRatio> aspect, params CourseTemplate[] courses) =>
        new(key: key, category: BondCategory.Discrete, closure: new ClosureRule.None(), requiredAspect: aspect, correctionFactor: 0.0, courses: toSeq(courses));
    private static BondName ParametricBond(string key, ClosureRule closure, Option<AspectRatio> aspect, double correctionFactor, params CourseTemplate[] courses) =>
        new(key: key, category: BondCategory.Parametric, closure: closure, requiredAspect: aspect, correctionFactor: correctionFactor, courses: toSeq(courses));

    public static readonly BondName Running = Simple("running", StretcherAt(0.0), StretcherAt(0.5));
    public static readonly BondName ThirdRunning = Aspected("third-running", Aspect3, StretcherAt(0.0), StretcherAt(1.0 / 3.0), StretcherAt(2.0 / 3.0));
    public static readonly BondName Common = new(key: "common", category: BondCategory.Discrete, closure: new ClosureRule.None(), requiredAspect: Aspect2, correctionFactor: 1.0 / 6.0,
        courses: Seq(StretcherAt(0.0), StretcherAt(0.5), StretcherAt(0.0), StretcherAt(0.5), StretcherAt(0.0), HeaderAt(0.5)));
    public static readonly BondName English = Closer("english", 0.5, StretcherAt(0.0), HeaderAt(0.5));
    public static readonly BondName Flemish = Closer("flemish", 1.0 / 3.0, Mixed(0.0, new Orientation.Stretcher(), new Orientation.Header()), Mixed(0.5, new Orientation.Header(), new Orientation.Stretcher()));
    public static readonly BondName Stack = Simple("stack", StretcherAt(0.0));
    public static readonly BondName EnglishCross = Closer("english-cross", 0.5, StretcherAt(0.0), HeaderAt(0.25), StretcherAt(0.5), HeaderAt(0.25));
    public static readonly BondName FlemishCross = Closer("flemish-cross", 1.0 / 3.0,
        Mixed(0.0, new Orientation.Stretcher(), new Orientation.Header()),
        StretcherAt(0.5),
        Mixed(0.0, new Orientation.Header(), new Orientation.Stretcher()),
        StretcherAt(0.0));
    public static readonly BondName Sussex = Aspected("sussex", Aspect2,
        Mixed(0.0, new Orientation.Stretcher(), new Orientation.Stretcher(), new Orientation.Stretcher(), new Orientation.Header()),
        Mixed(0.5, new Orientation.Stretcher(), new Orientation.Stretcher(), new Orientation.Stretcher(), new Orientation.Header()));
    public static readonly BondName Scotch = Aspected("scotch", Aspect2, StretcherAt(0.0), StretcherAt(0.5), StretcherAt(0.0), HeaderAt(0.5));
    public static readonly BondName Header = Simple("header", HeaderAt(0.0), HeaderAt(0.5));
    public static readonly BondName Monk = Aspected("monk", Aspect2,
        Mixed(0.0, new Orientation.Stretcher(), new Orientation.Stretcher(), new Orientation.Header()),
        Mixed(1.0 / 3.0, new Orientation.Stretcher(), new Orientation.Stretcher(), new Orientation.Header()));
    public static readonly BondName RatTrap = Simple("rat-trap",
        Mixed(0.0, new Orientation.Shiner(), new Orientation.Rowlock()),
        Mixed(0.5, new Orientation.Rowlock(), new Orientation.Shiner()));
    public static readonly BondName RunningRotated = ParametricBond("running-rotated", new ClosureRule.None(), Option<AspectRatio>.None, 0.0, StretcherAt(0.0), StretcherAt(0.5));
    public static readonly BondName Herringbone45 = ParametricBond("herringbone-45", new ClosureRule.None(), Option<AspectRatio>.None, 0.0, StretcherAt(0.0));
    public static readonly BondName BasketWeave = Simple("basket-weave",
        Mixed(0.0, new Orientation.Stretcher(), new Orientation.Soldier(), new Orientation.Soldier()),
        Mixed(0.0, new Orientation.Soldier(), new Orientation.Soldier(), new Orientation.Stretcher()));
    public static readonly BondName Soldier = Simple("soldier-course", SoldierAt(0.0));
    public static readonly BondName SingleFlemish = Closer("single-flemish", 1.0 / 3.0,
        Mixed(0.0, new Orientation.Stretcher(), new Orientation.Header()),
        Mixed(0.5, new Orientation.Header(), new Orientation.Stretcher()));
    public static readonly BondName DoubleFlemish = Closer("double-flemish", 1.0 / 3.0,
        Mixed(0.0, new Orientation.Stretcher(), new Orientation.Header()),
        Mixed(0.5, new Orientation.Header(), new Orientation.Stretcher()));
    public static readonly BondName Quetta = Closer("quetta", 1.0 / 3.0,
        Mixed(0.0, new Orientation.Stretcher(), new Orientation.Header()),
        Mixed(0.5, new Orientation.Header(), new Orientation.Stretcher()));
    public static readonly BondName DoubleStretcherGardenWall = Aspected("double-stretcher-garden-wall", Aspect2, StretcherAt(0.0), StretcherAt(0.5), HeaderAt(0.5));
    public static readonly BondName FlemishStretcher = Closer("flemish-stretcher", 1.0 / 12.0, StretcherAt(0.0), StretcherAt(0.5), StretcherAt(0.0),
        Mixed(0.5, new Orientation.Stretcher(), new Orientation.Header()));
    public static readonly BondName MixedGardenWall = Aspected("mixed-garden-wall", Aspect2,
        Mixed(0.0, new Orientation.Stretcher(), new Orientation.Stretcher(), new Orientation.Stretcher(), new Orientation.Header()),
        StretcherAt(0.5));
    public static readonly BondName Diaper = ParametricBond("diaper", new ClosureRule.QueenCloser(InsetCourses: 1), Aspect2, 0.5, StretcherAt(0.0), HeaderAt(0.25), StretcherAt(0.5), HeaderAt(0.25));
    public static readonly BondName FlemishDiagonal = ParametricBond("flemish-diagonal", new ClosureRule.QueenCloser(InsetCourses: 1), Aspect2, 1.0 / 3.0,
        Mixed(0.0, new Orientation.Stretcher(), new Orientation.Header()),
        Mixed(0.5, new Orientation.Header(), new Orientation.Stretcher()));
    public static readonly BondName FlemishSpiral = ParametricBond("flemish-spiral", new ClosureRule.QueenCloser(InsetCourses: 1), Aspect2, 1.0 / 3.0,
        Mixed(0.0, new Orientation.Stretcher(), new Orientation.Header()),
        Mixed(0.5, new Orientation.Header(), new Orientation.Stretcher()));
    public static readonly BondName Pinwheel = ParametricBond("pinwheel", new ClosureRule.HalfBat(), Aspect2, 0.25,
        Mixed(0.0, new Orientation.Stretcher(), new Orientation.Stretcher(), new Orientation.Stretcher(), new Orientation.Stretcher()));
    public static readonly BondName DellaRobbiaWeave = Simple("della-robbia-weave",
        Mixed(0.0, new Orientation.Stretcher(), new Orientation.Soldier()),
        Mixed(0.5, new Orientation.Soldier(), new Orientation.Stretcher()));
    public static readonly BondName TudorCrossHatch = ParametricBond("tudor-cross-hatch", new ClosureRule.QueenCloser(InsetCourses: 1), Aspect2, 0.5,
        StretcherAt(0.0), HeaderAt(0.25), StretcherAt(0.5), HeaderAt(0.25));
    public static readonly BondName AppareilALaFrancaise = Closer("appareil-a-la-francaise", 2.0 / 3.0,
        Mixed(0.0, new Orientation.Stretcher(), new Orientation.Header(), new Orientation.Header()),
        Mixed(0.5, new Orientation.Header(), new Orientation.Header(), new Orientation.Stretcher()));
    public static readonly BondName DutchMulti = Closer("dutch-multi", 0.75, HeaderAt(0.0), HeaderAt(0.5), HeaderAt(0.0),
        Mixed(0.25, new Orientation.Stretcher(), new Orientation.Header()));
    public static readonly BondName Wild = ParametricBond("wild", new ClosureRule.None(), Aspect2, 0.25, StretcherAt(0.0));
    public static readonly BondName OpusReticulatum = ParametricBond("opus-reticulatum", new ClosureRule.None(), Option<AspectRatio>.None, 0.0, StretcherAt(0.0));
    public static readonly BondName Staffel = Simple("staffel", StretcherAt(0.0), StretcherAt(0.25), StretcherAt(0.5), StretcherAt(0.75));

    public BondCategory Category { get; }
    public ClosureRule Closure { get; }
    public Option<AspectRatio> RequiredAspect { get; }
    public double CorrectionFactor { get; }
    public Seq<CourseTemplate> Courses { get; }

    [BoundaryAdapter]
    public bool AcceptsAspect(Brick brick) {
        ArgumentNullException.ThrowIfNull(argument: brick);
        return RequiredAspect switch {
            { IsSome: true, Case: AspectRatio r } => r.Accepts(actualLengthOverWidth: brick.Specified.LengthMm / brick.Specified.WidthMm),
            _ => true,
        };
    }
    public CourseTemplate CourseAt(int courseIndex) => Courses[courseIndex % Courses.Count];
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class BrickDesignation {
    public static readonly BrickDesignation UsModular = new(key: "us.modular",
        spec: new Brick(Region: BrickRegion.Us,
            Specified: Dim3.Create(widthMm: 92.075, heightMm: 57.15, lengthMm: 193.675),
            Nominal: Some(Dim3.Create(widthMm: 101.6, heightMm: 67.7333, lengthMm: 203.2)),
            Coursing: VerticalCoursing.Create(coursesPerModule: 3, moduleHeightMm: 203.2),
            Coring: Coring.Cored3Hole, Type: BrickType.Fbs,
            Source: new BrickSource.BiaTn10Table1(Note: "modular, 3/8\" joint; coring per BIA TN 9A typical.")));
    public static readonly BrickDesignation UsEngineerModular = new(key: "us.engineer-modular",
        spec: new Brick(Region: BrickRegion.Us,
            Specified: Dim3.Create(widthMm: 92.075, heightMm: 71.4375, lengthMm: 193.675),
            Nominal: Some(Dim3.Create(widthMm: 101.6, heightMm: 81.28, lengthMm: 203.2)),
            Coursing: VerticalCoursing.Create(coursesPerModule: 5, moduleHeightMm: 406.4),
            Coring: Coring.Cored3Hole, Type: BrickType.Fbs,
            Source: new BrickSource.BiaTn10Table1(Note: "engineer modular, 3/8\" joint; coring per BIA TN 9A typical.")));
    public static readonly BrickDesignation UsClosureModular = new(key: "us.closure-modular",
        spec: new Brick(Region: BrickRegion.Us,
            Specified: Dim3.Create(widthMm: 92.075, heightMm: 92.075, lengthMm: 193.675),
            Nominal: Some(Dim3.Create(widthMm: 101.6, heightMm: 101.6, lengthMm: 203.2)),
            Coursing: VerticalCoursing.Create(coursesPerModule: 1, moduleHeightMm: 101.6),
            Coring: Coring.Cored3Hole, Type: BrickType.Fbs,
            Source: new BrickSource.BiaTn10Table1(Note: "closure modular, 3/8\" joint; coring per BIA TN 9A typical.")));
    public static readonly BrickDesignation UsRoman = new(key: "us.roman",
        spec: new Brick(Region: BrickRegion.Us,
            Specified: Dim3.Create(widthMm: 92.075, heightMm: 41.275, lengthMm: 295.275),
            Nominal: Some(Dim3.Create(widthMm: 101.6, heightMm: 50.8, lengthMm: 304.8)),
            Coursing: VerticalCoursing.Create(coursesPerModule: 2, moduleHeightMm: 101.6),
            Coring: Coring.SingleFrog, Type: BrickType.Fbs,
            Source: new BrickSource.BiaTn10Table1(Note: "Roman, 3/8\" joint; thin units typically frogged not cored.")));
    public static readonly BrickDesignation UsNorman = new(key: "us.norman",
        spec: new Brick(Region: BrickRegion.Us,
            Specified: Dim3.Create(widthMm: 92.075, heightMm: 57.15, lengthMm: 295.275),
            Nominal: Some(Dim3.Create(widthMm: 101.6, heightMm: 67.7333, lengthMm: 304.8)),
            Coursing: VerticalCoursing.Create(coursesPerModule: 3, moduleHeightMm: 203.2),
            Coring: Coring.Cored3Hole, Type: BrickType.Fbs,
            Source: new BrickSource.BiaTn10Table1(Note: "Norman, 3/8\" joint; coring per BIA TN 9A typical.")));
    public static readonly BrickDesignation UsEngineerNorman = new(key: "us.engineer-norman",
        spec: new Brick(Region: BrickRegion.Us,
            Specified: Dim3.Create(widthMm: 92.075, heightMm: 71.4375, lengthMm: 295.275),
            Nominal: Some(Dim3.Create(widthMm: 101.6, heightMm: 81.28, lengthMm: 304.8)),
            Coursing: VerticalCoursing.Create(coursesPerModule: 5, moduleHeightMm: 406.4),
            Coring: Coring.Cored3Hole, Type: BrickType.Fbs,
            Source: new BrickSource.BiaTn10Table1(Note: "engineer Norman, 3/8\" joint; coring per BIA TN 9A typical.")));
    public static readonly BrickDesignation UsUtility = new(key: "us.utility",
        spec: new Brick(Region: BrickRegion.Us,
            Specified: Dim3.Create(widthMm: 92.075, heightMm: 92.075, lengthMm: 295.275),
            Nominal: Some(Dim3.Create(widthMm: 101.6, heightMm: 101.6, lengthMm: 304.8)),
            Coursing: VerticalCoursing.Create(coursesPerModule: 1, moduleHeightMm: 101.6),
            Coring: Coring.Cored10Hole, Type: BrickType.Fbs,
            Source: new BrickSource.BiaTn10Table1(Note: "utility, 3/8\" joint; larger unit typically multi-cored.")));
    public static readonly BrickDesignation UsMeridian = new(key: "us.meridian",
        spec: new Brick(Region: BrickRegion.Us,
            Specified: Dim3.Create(widthMm: 92.075, heightMm: 92.075, lengthMm: 396.875),
            Nominal: Some(Dim3.Create(widthMm: 101.6, heightMm: 101.6, lengthMm: 406.4)),
            Coursing: VerticalCoursing.Create(coursesPerModule: 1, moduleHeightMm: 101.6),
            Coring: Coring.Cored10Hole, Type: BrickType.Fbs,
            Source: new BrickSource.BiaTn10Table1(Note: "Meridian, 3/8\" joint; larger unit typically multi-cored.")));
    public static readonly BrickDesignation UsDoubleMeridian = new(key: "us.double-meridian",
        spec: new Brick(Region: BrickRegion.Us,
            Specified: Dim3.Create(widthMm: 92.075, heightMm: 193.675, lengthMm: 396.875),
            Nominal: Some(Dim3.Create(widthMm: 101.6, heightMm: 203.2, lengthMm: 406.4)),
            Coursing: VerticalCoursing.Create(coursesPerModule: 1, moduleHeightMm: 203.2),
            Coring: Coring.Cored10Hole, Type: BrickType.Fbs,
            Source: new BrickSource.BiaTn10Table1(Note: "double Meridian, 3/8\" joint; larger unit typically multi-cored.")));
    public static readonly BrickDesignation UsThroughWallMeridian = new(key: "us.through-wall-meridian",
        spec: new Brick(Region: BrickRegion.Us,
            Specified: Dim3.Create(widthMm: 193.675, heightMm: 92.075, lengthMm: 396.875),
            Nominal: Some(Dim3.Create(widthMm: 203.2, heightMm: 101.6, lengthMm: 406.4)),
            Coursing: VerticalCoursing.Create(coursesPerModule: 1, moduleHeightMm: 101.6),
            Coring: Coring.Hollow2Cell, Type: BrickType.Hbs,
            Source: new BrickSource.BiaTn10Table1(Note: "8-in. through-wall Meridian, 3/8\" joint; through-wall depth implies ASTM C652 hollow construction.")));
    public static readonly BrickDesignation UsQueen = new(key: "us.queen",
        spec: new Brick(Region: BrickRegion.Us,
            Specified: Dim3.Create(widthMm: 73.025, heightMm: 69.85, lengthMm: 196.85),
            Nominal: Option<Dim3>.None,
            Coursing: VerticalCoursing.Create(coursesPerModule: 5, moduleHeightMm: 406.4),
            Coring: Coring.None, Type: BrickType.Fbs,
            Source: new BrickSource.BiaTn10Table2(Note: "Queen, non-modular; midpoint of published range; thin non-modular typically solid.")));
    public static readonly BrickDesignation UsKing = new(key: "us.king",
        spec: new Brick(Region: BrickRegion.Us,
            Specified: Dim3.Create(widthMm: 73.025, heightMm: 68.2625, lengthMm: 246.0625),
            Nominal: Option<Dim3>.None,
            Coursing: VerticalCoursing.Create(coursesPerModule: 5, moduleHeightMm: 406.4),
            Coring: Coring.None, Type: BrickType.Fbs,
            Source: new BrickSource.BiaTn10Table2(Note: "King, non-modular; midpoint of published range; thin non-modular typically solid.")));
    public static readonly BrickDesignation UsStandard = new(key: "us.standard",
        spec: new Brick(Region: BrickRegion.Us,
            Specified: Dim3.Create(widthMm: 92.075, heightMm: 57.15, lengthMm: 203.2),
            Nominal: Option<Dim3>.None,
            Coursing: VerticalCoursing.Create(coursesPerModule: 3, moduleHeightMm: 203.2),
            Coring: Coring.Cored3Hole, Type: BrickType.Fbs,
            Source: new BrickSource.BiaTn10Table2(Note: "Standard, non-modular; coring per BIA TN 9A typical.")));
    public static readonly BrickDesignation UsEngineerStandard = new(key: "us.engineer-standard",
        spec: new Brick(Region: BrickRegion.Us,
            Specified: Dim3.Create(widthMm: 92.075, heightMm: 71.4375, lengthMm: 203.2),
            Nominal: Option<Dim3>.None,
            Coursing: VerticalCoursing.Create(coursesPerModule: 5, moduleHeightMm: 406.4),
            Coring: Coring.Cored3Hole, Type: BrickType.Fbs,
            Source: new BrickSource.BiaTn10Table2(Note: "engineer Standard, non-modular; coring per BIA TN 9A typical.")));
    public static readonly BrickDesignation UsClosureStandard = new(key: "us.closure-standard",
        spec: new Brick(Region: BrickRegion.Us,
            Specified: Dim3.Create(widthMm: 92.075, heightMm: 92.075, lengthMm: 203.2),
            Nominal: Option<Dim3>.None,
            Coursing: VerticalCoursing.Create(coursesPerModule: 1, moduleHeightMm: 101.6),
            Coring: Coring.Cored3Hole, Type: BrickType.Fbs,
            Source: new BrickSource.BiaTn10Table2(Note: "closure Standard, non-modular; coring per BIA TN 9A typical.")));
    public static readonly BrickDesignation UkStandard = new(key: "uk.standard",
        spec: new Brick(Region: BrickRegion.Uk,
            Specified: Dim3.Create(widthMm: 102.5, heightMm: 65.0, lengthMm: 215.0),
            Nominal: Some(Dim3.Create(widthMm: 112.5, heightMm: 75.0, lengthMm: 225.0)),
            Coursing: VerticalCoursing.Create(coursesPerModule: 4, moduleHeightMm: 300.0),
            Coring: Coring.SingleFrog, Type: BrickType.Fbs,
            Source: new BrickSource.BsEn771Part1(Note: "UK work size + 10mm joint coordinating; UK traditional frogged.")));
    public static readonly BrickDesignation DinNf = new(key: "din.nf",
        spec: new Brick(Region: BrickRegion.Din,
            Specified: Dim3.Create(widthMm: 115.0, heightMm: 71.0, lengthMm: 240.0),
            Nominal: Option<Dim3>.None,
            Coursing: VerticalCoursing.Create(coursesPerModule: 1, moduleHeightMm: 83.0),
            Coring: Coring.None, Type: BrickType.Fbs,
            Source: new BrickSource.Din105(Note: "Normalformat (NF Vollziegel, 12mm joint); solid clay brick.")));
    public static readonly BrickDesignation DinDf = new(key: "din.df",
        spec: new Brick(Region: BrickRegion.Din,
            Specified: Dim3.Create(widthMm: 115.0, heightMm: 52.0, lengthMm: 240.0),
            Nominal: Option<Dim3>.None,
            Coursing: VerticalCoursing.Create(coursesPerModule: 1, moduleHeightMm: 64.0),
            Coring: Coring.None, Type: BrickType.Fbs,
            Source: new BrickSource.Din105(Note: "Dünnformat (DF Vollziegel, 12mm joint); thin solid clay brick.")));
    public static readonly BrickDesignation DinRf = new(key: "din.rf",
        spec: new Brick(Region: BrickRegion.Din,
            Specified: Dim3.Create(widthMm: 115.0, heightMm: 62.0, lengthMm: 240.0),
            Nominal: Option<Dim3>.None,
            Coursing: VerticalCoursing.Create(coursesPerModule: 1, moduleHeightMm: 74.0),
            Coring: Coring.None, Type: BrickType.Fbs,
            Source: new BrickSource.Din105(Note: "Reichsformat (RF Vollziegel, 12mm joint); solid clay brick.")));
    public static readonly BrickDesignation Din2Df = new(key: "din.2df",
        spec: new Brick(Region: BrickRegion.Din,
            Specified: Dim3.Create(widthMm: 115.0, heightMm: 113.0, lengthMm: 240.0),
            Nominal: Option<Dim3>.None,
            Coursing: VerticalCoursing.Create(coursesPerModule: 1, moduleHeightMm: 125.0),
            Coring: Coring.Perforated10Cell, Type: BrickType.Fbs,
            Source: new BrickSource.Din105(Note: "Doppel-Dünnformat (2DF Hochlochziegel, octametric, 12mm joint); thicker DIN unit typically perforated.")));
    public static readonly BrickDesignation AuStandard = new(key: "au.standard",
        spec: new Brick(Region: BrickRegion.Au,
            Specified: Dim3.Create(widthMm: 110.0, heightMm: 76.0, lengthMm: 230.0),
            Nominal: Option<Dim3>.None,
            Coursing: VerticalCoursing.Create(coursesPerModule: 7, moduleHeightMm: 600.0),
            Coring: Coring.Cored3Hole, Type: BrickType.Fbs,
            Source: new BrickSource.AsNzs4455Part1(Note: "Australian standard, 10mm joint; cored facing typical.")));
    public static readonly BrickDesignation IsModular = new(key: "is.modular",
        spec: new Brick(Region: BrickRegion.Is,
            Specified: Dim3.Create(widthMm: 90.0, heightMm: 90.0, lengthMm: 190.0),
            Nominal: Some(Dim3.Create(widthMm: 100.0, heightMm: 100.0, lengthMm: 200.0)),
            Coursing: VerticalCoursing.Create(coursesPerModule: 1, moduleHeightMm: 100.0),
            Coring: Coring.SingleFrog, Type: BrickType.Fbs,
            Source: new BrickSource.Is1077(Note: "Indian modular, 10mm joint; IS practice typically frogged.")));
    public static readonly BrickDesignation IsHalfBrick = new(key: "is.half-brick",
        spec: new Brick(Region: BrickRegion.Is,
            Specified: Dim3.Create(widthMm: 90.0, heightMm: 40.0, lengthMm: 190.0),
            Nominal: Some(Dim3.Create(widthMm: 100.0, heightMm: 50.0, lengthMm: 200.0)),
            Coursing: VerticalCoursing.Create(coursesPerModule: 1, moduleHeightMm: 50.0),
            Coring: Coring.None, Type: BrickType.Fbs,
            Source: new BrickSource.Is1077(Note: "Indian half-height modular, 10mm joint; thin unit typically solid.")));

    public Brick Spec { get; }
    public BrickRegion Region => Spec.Region;
    public Dim3 Specified => Spec.Specified;
    public Option<Dim3> Nominal => Spec.Nominal;
    public VerticalCoursing Coursing => Spec.Coursing;
    public Coring Coring => Spec.Coring;
    public BrickType Type => Spec.Type;
    public BrickSource Source => Spec.Source;
}

[Union]
public abstract partial record Orientation {
    private Orientation() { }
    public sealed record Stretcher : Orientation;
    public sealed record Header : Orientation;
    public sealed record Soldier : Orientation;
    public sealed record Sailor : Orientation;
    public sealed record Rowlock : Orientation;
    public sealed record Shiner : Orientation;
}

[Union]
public abstract partial record Cut {
    private Cut() { }
    public sealed record None : Cut;
    public sealed record HalfBat : Cut;
    public sealed record ThreeQuarterBat : Cut;
    public sealed record QuarterBat : Cut;
    public sealed record QueenCloser : Cut;
    public sealed record KingCloser : Cut;
    public sealed record Bevelled : Cut;
    public sealed record Mitred(double AngleDegrees) : Cut;
    public sealed record Custom(string Label) : Cut;
}

[Union]
public abstract partial record ClosureRule {
    private ClosureRule() { }
    public sealed record None : ClosureRule;
    public sealed record HalfBat : ClosureRule;
    public sealed record ThreeQuarterBat : ClosureRule;
    public sealed record QueenCloser(int InsetCourses) : ClosureRule;
    public sealed record KingCloser : ClosureRule;
}

[Union]
public abstract partial record SpecialShape {
    private SpecialShape() { }
    public sealed record SingleBullnose(double RadiusMm) : SpecialShape;
    public sealed record DoubleBullnose(double RadiusMm) : SpecialShape;
    public sealed record Cownose(double RadiusMm) : SpecialShape;
    public sealed record Plinth(PlinthKind Kind, double BevelDegrees, double ProjectionMm) : SpecialShape;
    public sealed record Coping(CopingProfile Profile, double PitchDegrees, double RadiusMm, double SplayDegrees, double ThroatMm) : SpecialShape;
    public sealed record Cant(int Faces, double BevelDegrees) : SpecialShape;
    public sealed record Squint(double AngleDegrees) : SpecialShape;
    public sealed record Voussoir(double AngleDegrees, double DepthMm) : SpecialShape;
    public sealed record AirVent(double OpenAreaFraction) : SpecialShape;
    public sealed record Soap(double ThicknessMm) : SpecialShape;
    public sealed record HalfBrick : SpecialShape;
    public sealed record ThreeQuarterBrick : SpecialShape;
    public sealed record QuoinBrick : SpecialShape;
    public sealed record BrickOnEdgeSill(double SlopeDegrees, double KerfMm) : SpecialShape;

    public static readonly FrozenSet<SpecialShape> Catalog = new SpecialShape[] {
        new SingleBullnose(RadiusMm: 15.0),
        new SingleBullnose(RadiusMm: 20.0),
        new SingleBullnose(RadiusMm: 15.875),
        new SingleBullnose(RadiusMm: 19.05),
        new SingleBullnose(RadiusMm: 25.4),
        new DoubleBullnose(RadiusMm: 15.0),
        new DoubleBullnose(RadiusMm: 20.0),
        new DoubleBullnose(RadiusMm: 15.875),
        new DoubleBullnose(RadiusMm: 19.05),
        new DoubleBullnose(RadiusMm: 25.4),
        new Cownose(RadiusMm: 51.25),
        new Cownose(RadiusMm: 46.04),
        new Plinth(Kind: PlinthKind.Header, BevelDegrees: 45.0, ProjectionMm: 23.0),
        new Plinth(Kind: PlinthKind.Stretcher, BevelDegrees: 45.0, ProjectionMm: 23.0),
        new Plinth(Kind: PlinthKind.ExternalReturn, BevelDegrees: 45.0, ProjectionMm: 23.0),
        new Plinth(Kind: PlinthKind.InternalReturn, BevelDegrees: 45.0, ProjectionMm: 23.0),
        new Coping(Profile: CopingProfile.Saddle, PitchDegrees: 45.0, RadiusMm: 0.0, SplayDegrees: 0.0, ThroatMm: 12.0),
        new Coping(Profile: CopingProfile.HalfRound, PitchDegrees: 0.0, RadiusMm: 50.0, SplayDegrees: 0.0, ThroatMm: 12.0),
        new Coping(Profile: CopingProfile.Splayed, PitchDegrees: 0.0, RadiusMm: 0.0, SplayDegrees: 15.0, ThroatMm: 12.0),
        new Coping(Profile: CopingProfile.Throated, PitchDegrees: 0.0, RadiusMm: 0.0, SplayDegrees: 0.0, ThroatMm: 18.0),
        new Cant(Faces: 1, BevelDegrees: 30.0),
        new Cant(Faces: 2, BevelDegrees: 45.0),
        new Squint(AngleDegrees: 30.0),
        new Squint(AngleDegrees: 45.0),
        new Squint(AngleDegrees: 60.0),
        new Voussoir(AngleDegrees: 15.0, DepthMm: 92.075),
        new AirVent(OpenAreaFraction: 0.25),
        new Soap(ThicknessMm: 46.0),
        new HalfBrick(),
        new ThreeQuarterBrick(),
        new QuoinBrick(),
        new BrickOnEdgeSill(SlopeDegrees: 15.0, KerfMm: 4.0),
    }.ToFrozenSet();
}

[Union]
public abstract partial record BrickSource {
    private BrickSource() { }
    public sealed record BiaTn10Table1(string Note) : BrickSource;
    public sealed record BiaTn10Table2(string Note) : BrickSource;
    public sealed record BsEn771Part1(string Note) : BrickSource;
    public sealed record Din105(string Note) : BrickSource;
    public sealed record AsNzs4455Part1(string Note) : BrickSource;
    public sealed record Is1077(string Note) : BrickSource;
}

// --- [MODELS] -----------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError<BrickFault>]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct Dim3 {
    public double WidthMm { get; }
    public double HeightMm { get; }
    public double LengthMm { get; }
    public double VolumeMm3 => WidthMm * HeightMm * LengthMm;
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref BrickFault? validationError, ref double widthMm, ref double heightMm, ref double lengthMm) =>
        validationError = (widthMm > 0.0 && double.IsFinite(widthMm), heightMm > 0.0 && double.IsFinite(heightMm), lengthMm > 0.0 && double.IsFinite(lengthMm)) switch {
            (false, _, _) => new BrickFault.InvalidDimension(Detail: string.Create(provider: CultureInfo.InvariantCulture, $"WidthMm must be positive and finite (got {widthMm:R}).")),
            (_, false, _) => new BrickFault.InvalidDimension(Detail: string.Create(provider: CultureInfo.InvariantCulture, $"HeightMm must be positive and finite (got {heightMm:R}).")),
            (_, _, false) => new BrickFault.InvalidDimension(Detail: string.Create(provider: CultureInfo.InvariantCulture, $"LengthMm must be positive and finite (got {lengthMm:R}).")),
            _ => null,
        };
}

[ValueObject<double>(KeyMemberName = "LengthOverWidth", KeyMemberAccessModifier = AccessModifier.Public)]
[ValidationError<BrickFault>]
public readonly partial struct AspectRatio {
    public const double MatchTolerance = 0.15;
    public bool Accepts(double actualLengthOverWidth) =>
        Math.Abs(actualLengthOverWidth - LengthOverWidth) <= MatchTolerance * LengthOverWidth;
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref BrickFault? validationError, ref double lengthOverWidth) =>
        validationError = double.IsFinite(lengthOverWidth) && lengthOverWidth > 0.0
            ? null
            : new BrickFault.InvalidDimension(Detail: string.Create(provider: CultureInfo.InvariantCulture, $"AspectRatio must be positive and finite (got {lengthOverWidth:R})."));
}

[ComplexValueObject]
[ValidationError<BrickFault>]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct VerticalCoursing {
    public int CoursesPerModule { get; }
    public double ModuleHeightMm { get; }
    public double CourseHeightMm => ModuleHeightMm / CoursesPerModule;
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref BrickFault? validationError, ref int coursesPerModule, ref double moduleHeightMm) =>
        validationError = (coursesPerModule > 0, moduleHeightMm > 0.0 && double.IsFinite(moduleHeightMm)) switch {
            (false, _) => new BrickFault.InvalidDimension(Detail: string.Create(provider: CultureInfo.InvariantCulture, $"CoursesPerModule must be positive (got {coursesPerModule}).")),
            (_, false) => new BrickFault.InvalidDimension(Detail: string.Create(provider: CultureInfo.InvariantCulture, $"ModuleHeightMm must be positive and finite (got {moduleHeightMm:R}).")),
            _ => null,
        };
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct ChippageRule(double MinPercent, double MaxPercent, double WithinMm);

[StructLayout(LayoutKind.Auto)]
public readonly record struct MovementCoefficients(double MoistureCoefficient, double ThermalPerCelsius);

[StructLayout(LayoutKind.Auto)]
public readonly record struct ExpansionJointSpacing(double NoOpeningsMm, double WithOpeningsMm, double WidthMinMm, double WidthMaxMm) {
    public double SpacingFor(bool hasOpenings) => hasOpenings ? WithOpeningsMm : NoOpeningsMm;
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct WeepHolePolicy(double IbcMaxMm, double OpenHeadMaxMm, double WickMaxMm);

[StructLayout(LayoutKind.Auto)]
public readonly record struct TieSpacing(double HorizontalMm, double VerticalMm, double AreaPerTiePrescriptiveMm2, double AreaPerTieSeismicMm2);

public sealed record ArchRule(double JointThicknessMinMm, double JointThicknessMaxMm, double MinDepthPerFootMm, double KeystoneMaxFraction) {
    public static readonly ArchRule Bia31Universal = new(JointThicknessMinMm: 3.175, JointThicknessMaxMm: 19.05, MinDepthPerFootMm: 25.4, KeystoneMaxFraction: 1.0 / 3.0);
}

public sealed record RegionalMasonryPolicy(
    MovementCoefficients Movement,
    ExpansionJointSpacing Expansion,
    WeepHolePolicy WeepHole,
    TieSpacing Ties) {
    public static readonly RegionalMasonryPolicy UsAuthoritative = new(
        Movement: new MovementCoefficients(MoistureCoefficient: 0.0003, ThermalPerCelsius: 7.2e-6),
        Expansion: new ExpansionJointSpacing(NoOpeningsMm: 7620.0, WithOpeningsMm: 6096.0, WidthMinMm: 9.525, WidthMaxMm: 12.7),
        WeepHole: new WeepHolePolicy(IbcMaxMm: 838.0, OpenHeadMaxMm: 610.0, WickMaxMm: 406.0),
        Ties: new TieSpacing(HorizontalMm: 812.8, VerticalMm: 635.0, AreaPerTiePrescriptiveMm2: 248000.0, AreaPerTieSeismicMm2: 186000.0));
}

public sealed record CourseTemplate(Seq<Orientation> Sequence, double OffsetFraction);

public sealed record Brick(
    BrickRegion Region,
    Dim3 Specified,
    Option<Dim3> Nominal,
    VerticalCoursing Coursing,
    Coring Coring,
    BrickType Type,
    BrickSource Source);

// --- [ERRORS] -----------------------------------------------------------------------------
[Union]
public abstract partial record BrickFault : Error, IValidationError<BrickFault> {
    private BrickFault() : base() { }
    public override bool IsExpected => true;
    public override bool IsExceptional => false;
    public override ErrorException ToErrorException() => new WrappedErrorExpectedException(this);
    public static BrickFault Create(string message) => new InvalidDimension(Detail: message);
    public sealed record InvalidDimension(string Detail) : BrickFault {
        public override string Message => $"Dimension validation failed: {Detail}";
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class BrickRegionExtensions {
    public static Seq<BrickDesignation> Designations(this BrickRegion region) =>
        toSeq(BrickDesignation.Items.Where(d => d.Region == region));
}
