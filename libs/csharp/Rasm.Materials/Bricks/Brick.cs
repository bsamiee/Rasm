using System.Runtime.InteropServices;

namespace Rasm.Materials.Bricks;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class BrickRegion {
    public static readonly BrickRegion Us = new(key: "us");
    public static readonly BrickRegion Uk = new(key: "uk");
    public static readonly BrickRegion Din = new(key: "din");
    public static readonly BrickRegion Au = new(key: "au");
    public static readonly BrickRegion Is = new(key: "is");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class BrickDesignation {
    public static readonly BrickDesignation UsModular = new(key: "us.modular");
    public static readonly BrickDesignation UsEngineerModular = new(key: "us.engineer-modular");
    public static readonly BrickDesignation UsClosureModular = new(key: "us.closure-modular");
    public static readonly BrickDesignation UsRoman = new(key: "us.roman");
    public static readonly BrickDesignation UsNorman = new(key: "us.norman");
    public static readonly BrickDesignation UsEngineerNorman = new(key: "us.engineer-norman");
    public static readonly BrickDesignation UsUtility = new(key: "us.utility");
    public static readonly BrickDesignation UsMeridian = new(key: "us.meridian");
    public static readonly BrickDesignation UsDoubleMeridian = new(key: "us.double-meridian");
    public static readonly BrickDesignation UsThroughWallMeridian = new(key: "us.through-wall-meridian");
    public static readonly BrickDesignation UsQueen = new(key: "us.queen");
    public static readonly BrickDesignation UsKing = new(key: "us.king");
    public static readonly BrickDesignation UsStandard = new(key: "us.standard");
    public static readonly BrickDesignation UsEngineerStandard = new(key: "us.engineer-standard");
    public static readonly BrickDesignation UsClosureStandard = new(key: "us.closure-standard");
    public static readonly BrickDesignation UkStandard = new(key: "uk.standard");
    public static readonly BrickDesignation DinNf = new(key: "din.nf");
    public static readonly BrickDesignation DinDf = new(key: "din.df");
    public static readonly BrickDesignation DinRf = new(key: "din.rf");
    public static readonly BrickDesignation Din2Df = new(key: "din.2df");
    public static readonly BrickDesignation AuStandard = new(key: "au.standard");
    public static readonly BrickDesignation IsModular = new(key: "is.modular");
    public static readonly BrickDesignation IsHalfBrick = new(key: "is.half-brick");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class BrickGrade {
    public static readonly BrickGrade Sw = new(key: "SW");
    public static readonly BrickGrade Mw = new(key: "MW");
    public static readonly BrickGrade Nw = new(key: "NW");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class BrickType {
    public static readonly BrickType Fbx = new(key: "FBX");
    public static readonly BrickType Fbs = new(key: "FBS");
    public static readonly BrickType Fba = new(key: "FBA");
    public static readonly BrickType Hbx = new(key: "HBX");
    public static readonly BrickType Hbs = new(key: "HBS");
    public static readonly BrickType Hbb = new(key: "HBB");
    public static readonly BrickType Hba = new(key: "HBA");
}

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
    public static readonly Coring Hollow2Cell = new(key: "hollow-2-cell", voidFraction: 0.38, classification: CoringClass.Hollow);
    public static readonly Coring Hollow3Cell = new(key: "hollow-3-cell", voidFraction: 0.43, classification: CoringClass.Hollow);
    public static readonly Coring Hollow4Cell = new(key: "hollow-4-cell", voidFraction: 0.48, classification: CoringClass.Hollow);
    public static readonly Coring Slotted = new(key: "slotted", voidFraction: 0.50, classification: CoringClass.Perforated);
    public double VoidFraction { get; }
    public CoringClass Classification { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class JointProfile {
    public static readonly JointProfile Concave = new(key: "concave");
    public static readonly JointProfile V = new(key: "v-joint");
    public static readonly JointProfile Weathered = new(key: "weathered");
    public static readonly JointProfile Struck = new(key: "struck");
    public static readonly JointProfile Flush = new(key: "flush");
    public static readonly JointProfile Raked = new(key: "raked");
    public static readonly JointProfile Beaded = new(key: "beaded");
    public static readonly JointProfile Extruded = new(key: "extruded");
    public static readonly JointProfile Grapevine = new(key: "grapevine");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class BondName {
    public static readonly BondName Running = new(key: "running");
    public static readonly BondName ThirdRunning = new(key: "third-running");
    public static readonly BondName Common = new(key: "common");
    public static readonly BondName English = new(key: "english");
    public static readonly BondName Flemish = new(key: "flemish");
    public static readonly BondName Stack = new(key: "stack");
    public static readonly BondName EnglishCross = new(key: "english-cross");
    public static readonly BondName FlemishCross = new(key: "flemish-cross");
    public static readonly BondName Sussex = new(key: "sussex");
    public static readonly BondName Scotch = new(key: "scotch");
    public static readonly BondName Header = new(key: "header");
    public static readonly BondName Monk = new(key: "monk");
    public static readonly BondName RatTrap = new(key: "rat-trap");
    public static readonly BondName HerringboneFlat = new(key: "herringbone-flat");
    public static readonly BondName Herringbone45 = new(key: "herringbone-45");
    public static readonly BondName BasketWeave = new(key: "basket-weave");
    public static readonly BondName Soldier = new(key: "soldier-course");
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
public abstract partial record SpecialShape {
    private SpecialShape() { }
    public sealed record SingleBullnose(double RadiusMm) : SpecialShape;
    public sealed record DoubleBullnose(double RadiusMm) : SpecialShape;
    public sealed record Cownose(double RadiusMm) : SpecialShape;
    public sealed record PlinthHeader : SpecialShape;
    public sealed record PlinthStretcher : SpecialShape;
    public sealed record PlinthExternalReturn : SpecialShape;
    public sealed record PlinthInternalReturn : SpecialShape;
    public sealed record Coping(CopingProfile Profile) : SpecialShape;
    public sealed record Cant(int Faces) : SpecialShape;
    public sealed record Squint(double AngleDegrees) : SpecialShape;
    public sealed record Voussoir(double AngleDegrees, double DepthMm) : SpecialShape;
    public sealed record AirVent(double OpenAreaFraction) : SpecialShape;
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

// --- [MODELS] -----------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct Dim3(double WidthMm, double HeightMm, double LengthMm) {
    public double VolumeMm3 => WidthMm * HeightMm * LengthMm;
}

public sealed record VerticalCoursing(int CoursesPerModule, double ModuleHeightMm) {
    public double CourseHeightMm => ModuleHeightMm / CoursesPerModule;
}

public sealed record Brick(
    BrickDesignation Designation,
    BrickRegion Region,
    Dim3 Specified,
    Option<Dim3> Nominal,
    VerticalCoursing Coursing,
    string SourceStandard);

public sealed record CourseTemplate(Seq<Orientation> Sequence, double OffsetFraction);

public sealed record Bond(BondName Name, Seq<CourseTemplate> Courses, string Description) {
    public int RepeatPeriod => Courses.Count;
}

public sealed record JointSpec(
    JointProfile Profile,
    double ThicknessMm,
    Option<double> ReinforcementWireGaugeMm,
    string Description);

// --- [ERRORS] -----------------------------------------------------------------------------
[Union]
public abstract partial record BrickError : Error {
    private BrickError() : base() { }
    public override bool IsExpected => true;
    public override bool IsExceptional => false;
    public override ErrorException ToErrorException() => new WrappedErrorExpectedException(this);
    public sealed record DesignationNotFound(BrickDesignation Designation) : BrickError {
        public override string Message => $"Brick designation '{Designation}' not found in catalogue.";
    }
    public sealed record BondNotFound(BondName Bond) : BrickError {
        public override string Message => $"Bond '{Bond}' not found in catalogue.";
    }
    public sealed record JointNotFound(JointProfile Profile) : BrickError {
        public override string Message => $"Joint profile '{Profile}' not found in catalogue.";
    }
}
