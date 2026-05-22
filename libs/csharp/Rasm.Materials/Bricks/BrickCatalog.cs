namespace Rasm.Materials.Bricks;

// --- [CONSTANTS] --------------------------------------------------------------------------
public static partial class BrickCatalog {
    public static readonly FrozenDictionary<BrickDesignation, Brick> Bricks = new Dictionary<BrickDesignation, Brick>() {
        [BrickDesignation.UsModular] = new Brick(
            Designation: BrickDesignation.UsModular,
            Region: BrickRegion.Us,
            Specified: new Dim3(WidthMm: 92.075, HeightMm: 57.15, LengthMm: 193.675),
            Nominal: Some(new Dim3(WidthMm: 101.6, HeightMm: 67.7333, LengthMm: 203.2)),
            Coursing: new VerticalCoursing(CoursesPerModule: 3, ModuleHeightMm: 203.2),
            SourceStandard: "BIA TN 10 Table 1 (modular, 3/8\" joint)"),
        [BrickDesignation.UsEngineerModular] = new Brick(
            Designation: BrickDesignation.UsEngineerModular,
            Region: BrickRegion.Us,
            Specified: new Dim3(WidthMm: 92.075, HeightMm: 71.4375, LengthMm: 193.675),
            Nominal: Some(new Dim3(WidthMm: 101.6, HeightMm: 81.28, LengthMm: 203.2)),
            Coursing: new VerticalCoursing(CoursesPerModule: 5, ModuleHeightMm: 406.4),
            SourceStandard: "BIA TN 10 Table 1 (engineer modular, 3/8\" joint)"),
        [BrickDesignation.UsClosureModular] = new Brick(
            Designation: BrickDesignation.UsClosureModular,
            Region: BrickRegion.Us,
            Specified: new Dim3(WidthMm: 92.075, HeightMm: 92.075, LengthMm: 193.675),
            Nominal: Some(new Dim3(WidthMm: 101.6, HeightMm: 101.6, LengthMm: 203.2)),
            Coursing: new VerticalCoursing(CoursesPerModule: 1, ModuleHeightMm: 101.6),
            SourceStandard: "BIA TN 10 Table 1 (closure modular, 3/8\" joint)"),
        [BrickDesignation.UsRoman] = new Brick(
            Designation: BrickDesignation.UsRoman,
            Region: BrickRegion.Us,
            Specified: new Dim3(WidthMm: 92.075, HeightMm: 41.275, LengthMm: 295.275),
            Nominal: Some(new Dim3(WidthMm: 101.6, HeightMm: 50.8, LengthMm: 304.8)),
            Coursing: new VerticalCoursing(CoursesPerModule: 2, ModuleHeightMm: 101.6),
            SourceStandard: "BIA TN 10 Table 1 (Roman, 3/8\" joint)"),
        [BrickDesignation.UsNorman] = new Brick(
            Designation: BrickDesignation.UsNorman,
            Region: BrickRegion.Us,
            Specified: new Dim3(WidthMm: 92.075, HeightMm: 57.15, LengthMm: 295.275),
            Nominal: Some(new Dim3(WidthMm: 101.6, HeightMm: 67.7333, LengthMm: 304.8)),
            Coursing: new VerticalCoursing(CoursesPerModule: 3, ModuleHeightMm: 203.2),
            SourceStandard: "BIA TN 10 Table 1 (Norman, 3/8\" joint)"),
        [BrickDesignation.UsEngineerNorman] = new Brick(
            Designation: BrickDesignation.UsEngineerNorman,
            Region: BrickRegion.Us,
            Specified: new Dim3(WidthMm: 92.075, HeightMm: 71.4375, LengthMm: 295.275),
            Nominal: Some(new Dim3(WidthMm: 101.6, HeightMm: 81.28, LengthMm: 304.8)),
            Coursing: new VerticalCoursing(CoursesPerModule: 5, ModuleHeightMm: 406.4),
            SourceStandard: "BIA TN 10 Table 1 (engineer Norman, 3/8\" joint)"),
        [BrickDesignation.UsUtility] = new Brick(
            Designation: BrickDesignation.UsUtility,
            Region: BrickRegion.Us,
            Specified: new Dim3(WidthMm: 92.075, HeightMm: 92.075, LengthMm: 295.275),
            Nominal: Some(new Dim3(WidthMm: 101.6, HeightMm: 101.6, LengthMm: 304.8)),
            Coursing: new VerticalCoursing(CoursesPerModule: 1, ModuleHeightMm: 101.6),
            SourceStandard: "BIA TN 10 Table 1 (utility, 3/8\" joint)"),
        [BrickDesignation.UsMeridian] = new Brick(
            Designation: BrickDesignation.UsMeridian,
            Region: BrickRegion.Us,
            Specified: new Dim3(WidthMm: 92.075, HeightMm: 92.075, LengthMm: 396.875),
            Nominal: Some(new Dim3(WidthMm: 101.6, HeightMm: 101.6, LengthMm: 406.4)),
            Coursing: new VerticalCoursing(CoursesPerModule: 1, ModuleHeightMm: 101.6),
            SourceStandard: "BIA TN 10 Table 1 (Meridian, 3/8\" joint)"),
        [BrickDesignation.UsDoubleMeridian] = new Brick(
            Designation: BrickDesignation.UsDoubleMeridian,
            Region: BrickRegion.Us,
            Specified: new Dim3(WidthMm: 92.075, HeightMm: 193.675, LengthMm: 396.875),
            Nominal: Some(new Dim3(WidthMm: 101.6, HeightMm: 203.2, LengthMm: 406.4)),
            Coursing: new VerticalCoursing(CoursesPerModule: 1, ModuleHeightMm: 203.2),
            SourceStandard: "BIA TN 10 Table 1 (double Meridian, 3/8\" joint)"),
        [BrickDesignation.UsThroughWallMeridian] = new Brick(
            Designation: BrickDesignation.UsThroughWallMeridian,
            Region: BrickRegion.Us,
            Specified: new Dim3(WidthMm: 193.675, HeightMm: 92.075, LengthMm: 396.875),
            Nominal: Some(new Dim3(WidthMm: 203.2, HeightMm: 101.6, LengthMm: 406.4)),
            Coursing: new VerticalCoursing(CoursesPerModule: 1, ModuleHeightMm: 101.6),
            SourceStandard: "BIA TN 10 Table 1 (8-in. through-wall Meridian, 3/8\" joint)"),
        [BrickDesignation.UsQueen] = new Brick(
            Designation: BrickDesignation.UsQueen,
            Region: BrickRegion.Us,
            Specified: new Dim3(WidthMm: 73.025, HeightMm: 69.85, LengthMm: 196.85),
            Nominal: Option<Dim3>.None,
            Coursing: new VerticalCoursing(CoursesPerModule: 5, ModuleHeightMm: 406.4),
            SourceStandard: "BIA TN 10 Table 2 (Queen, non-modular; midpoint of published range)"),
        [BrickDesignation.UsKing] = new Brick(
            Designation: BrickDesignation.UsKing,
            Region: BrickRegion.Us,
            Specified: new Dim3(WidthMm: 73.025, HeightMm: 68.2625, LengthMm: 246.0625),
            Nominal: Option<Dim3>.None,
            Coursing: new VerticalCoursing(CoursesPerModule: 5, ModuleHeightMm: 406.4),
            SourceStandard: "BIA TN 10 Table 2 (King, non-modular; midpoint of published range)"),
        [BrickDesignation.UsStandard] = new Brick(
            Designation: BrickDesignation.UsStandard,
            Region: BrickRegion.Us,
            Specified: new Dim3(WidthMm: 92.075, HeightMm: 57.15, LengthMm: 203.2),
            Nominal: Option<Dim3>.None,
            Coursing: new VerticalCoursing(CoursesPerModule: 3, ModuleHeightMm: 203.2),
            SourceStandard: "BIA TN 10 Table 2 (Standard, non-modular)"),
        [BrickDesignation.UsEngineerStandard] = new Brick(
            Designation: BrickDesignation.UsEngineerStandard,
            Region: BrickRegion.Us,
            Specified: new Dim3(WidthMm: 92.075, HeightMm: 71.4375, LengthMm: 203.2),
            Nominal: Option<Dim3>.None,
            Coursing: new VerticalCoursing(CoursesPerModule: 5, ModuleHeightMm: 406.4),
            SourceStandard: "BIA TN 10 Table 2 (engineer Standard, non-modular)"),
        [BrickDesignation.UsClosureStandard] = new Brick(
            Designation: BrickDesignation.UsClosureStandard,
            Region: BrickRegion.Us,
            Specified: new Dim3(WidthMm: 92.075, HeightMm: 92.075, LengthMm: 203.2),
            Nominal: Option<Dim3>.None,
            Coursing: new VerticalCoursing(CoursesPerModule: 1, ModuleHeightMm: 101.6),
            SourceStandard: "BIA TN 10 Table 2 (closure Standard, non-modular)"),
        [BrickDesignation.UkStandard] = new Brick(
            Designation: BrickDesignation.UkStandard,
            Region: BrickRegion.Uk,
            Specified: new Dim3(WidthMm: 102.5, HeightMm: 65.0, LengthMm: 215.0),
            Nominal: Some(new Dim3(WidthMm: 112.5, HeightMm: 75.0, LengthMm: 225.0)),
            Coursing: new VerticalCoursing(CoursesPerModule: 4, ModuleHeightMm: 300.0),
            SourceStandard: "BS EN 771-1 (UK work size + 10mm joint coordinating)"),
        [BrickDesignation.DinNf] = new Brick(
            Designation: BrickDesignation.DinNf,
            Region: BrickRegion.Din,
            Specified: new Dim3(WidthMm: 115.0, HeightMm: 71.0, LengthMm: 240.0),
            Nominal: Option<Dim3>.None,
            Coursing: new VerticalCoursing(CoursesPerModule: 1, ModuleHeightMm: 83.0),
            SourceStandard: "DIN 105 Normalformat (NF, 12mm joint)"),
        [BrickDesignation.DinDf] = new Brick(
            Designation: BrickDesignation.DinDf,
            Region: BrickRegion.Din,
            Specified: new Dim3(WidthMm: 115.0, HeightMm: 52.0, LengthMm: 240.0),
            Nominal: Option<Dim3>.None,
            Coursing: new VerticalCoursing(CoursesPerModule: 1, ModuleHeightMm: 64.0),
            SourceStandard: "DIN 105 Dünnformat (DF, 12mm joint)"),
        [BrickDesignation.DinRf] = new Brick(
            Designation: BrickDesignation.DinRf,
            Region: BrickRegion.Din,
            Specified: new Dim3(WidthMm: 115.0, HeightMm: 62.0, LengthMm: 240.0),
            Nominal: Option<Dim3>.None,
            Coursing: new VerticalCoursing(CoursesPerModule: 1, ModuleHeightMm: 74.0),
            SourceStandard: "DIN 105 Reichsformat (RF, 12mm joint)"),
        [BrickDesignation.Din2Df] = new Brick(
            Designation: BrickDesignation.Din2Df,
            Region: BrickRegion.Din,
            Specified: new Dim3(WidthMm: 115.0, HeightMm: 113.0, LengthMm: 240.0),
            Nominal: Option<Dim3>.None,
            Coursing: new VerticalCoursing(CoursesPerModule: 1, ModuleHeightMm: 125.0),
            SourceStandard: "DIN 105 Doppel-Dünnformat (2DF, octametric, 12mm joint)"),
        [BrickDesignation.AuStandard] = new Brick(
            Designation: BrickDesignation.AuStandard,
            Region: BrickRegion.Au,
            Specified: new Dim3(WidthMm: 110.0, HeightMm: 76.0, LengthMm: 230.0),
            Nominal: Option<Dim3>.None,
            Coursing: new VerticalCoursing(CoursesPerModule: 7, ModuleHeightMm: 600.0),
            SourceStandard: "AS/NZS 4455.1 (Australian standard, 10mm joint)"),
        [BrickDesignation.IsModular] = new Brick(
            Designation: BrickDesignation.IsModular,
            Region: BrickRegion.Is,
            Specified: new Dim3(WidthMm: 90.0, HeightMm: 90.0, LengthMm: 190.0),
            Nominal: Some(new Dim3(WidthMm: 100.0, HeightMm: 100.0, LengthMm: 200.0)),
            Coursing: new VerticalCoursing(CoursesPerModule: 1, ModuleHeightMm: 100.0),
            SourceStandard: "IS 1077 (Indian modular, 10mm joint)"),
        [BrickDesignation.IsHalfBrick] = new Brick(
            Designation: BrickDesignation.IsHalfBrick,
            Region: BrickRegion.Is,
            Specified: new Dim3(WidthMm: 90.0, HeightMm: 40.0, LengthMm: 190.0),
            Nominal: Some(new Dim3(WidthMm: 100.0, HeightMm: 50.0, LengthMm: 200.0)),
            Coursing: new VerticalCoursing(CoursesPerModule: 1, ModuleHeightMm: 50.0),
            SourceStandard: "IS 1077 (Indian half-height modular, 10mm joint)"),
    }.ToFrozenDictionary();

    public static readonly FrozenDictionary<BondName, Bond> Bonds = new Dictionary<BondName, Bond>() {
        [BondName.Running] = new Bond(
            Name: BondName.Running,
            Courses: Seq(
                new CourseTemplate(Sequence: Seq<Orientation>(new Orientation.Stretcher()), OffsetFraction: 0.0),
                new CourseTemplate(Sequence: Seq<Orientation>(new Orientation.Stretcher()), OffsetFraction: 0.5)),
            Description: "Running (half-stretcher) bond: all stretchers, half-brick offset every course. BIA TN 30."),
        [BondName.ThirdRunning] = new Bond(
            Name: BondName.ThirdRunning,
            Courses: Seq(
                new CourseTemplate(Sequence: Seq<Orientation>(new Orientation.Stretcher()), OffsetFraction: 0.0),
                new CourseTemplate(Sequence: Seq<Orientation>(new Orientation.Stretcher()), OffsetFraction: 1.0 / 3.0),
                new CourseTemplate(Sequence: Seq<Orientation>(new Orientation.Stretcher()), OffsetFraction: 2.0 / 3.0)),
            Description: "One-third running bond: stretchers with third-brick offset; suits Utility/Norman bricks (L=3W). BIA TN 30."),
        [BondName.Common] = new Bond(
            Name: BondName.Common,
            Courses: Seq(
                new CourseTemplate(Sequence: Seq<Orientation>(new Orientation.Stretcher()), OffsetFraction: 0.0),
                new CourseTemplate(Sequence: Seq<Orientation>(new Orientation.Stretcher()), OffsetFraction: 0.5),
                new CourseTemplate(Sequence: Seq<Orientation>(new Orientation.Stretcher()), OffsetFraction: 0.0),
                new CourseTemplate(Sequence: Seq<Orientation>(new Orientation.Stretcher()), OffsetFraction: 0.5),
                new CourseTemplate(Sequence: Seq<Orientation>(new Orientation.Stretcher()), OffsetFraction: 0.0),
                new CourseTemplate(Sequence: Seq<Orientation>(new Orientation.Header()), OffsetFraction: 0.25)),
            Description: "Common (American) bond: five stretcher courses then one header course; correction factor +1/5 bricks. BIA TN 30."),
        [BondName.English] = new Bond(
            Name: BondName.English,
            Courses: Seq(
                new CourseTemplate(Sequence: Seq<Orientation>(new Orientation.Stretcher()), OffsetFraction: 0.0),
                new CourseTemplate(Sequence: Seq<Orientation>(new Orientation.Header()), OffsetFraction: 0.25)),
            Description: "English bond: alternating stretcher and header courses; queen closer 4 in. from corner in header course. BIA TN 30."),
        [BondName.Flemish] = new Bond(
            Name: BondName.Flemish,
            Courses: Seq(
                new CourseTemplate(Sequence: Seq<Orientation>(new Orientation.Stretcher(), new Orientation.Header()), OffsetFraction: 0.0),
                new CourseTemplate(Sequence: Seq<Orientation>(new Orientation.Header(), new Orientation.Stretcher()), OffsetFraction: 0.5)),
            Description: "Flemish bond: header + stretcher alternating in every course; queen closer or three-quarter bat at quoin. BIA TN 30."),
        [BondName.Stack] = new Bond(
            Name: BondName.Stack,
            Courses: Seq(new CourseTemplate(Sequence: Seq<Orientation>(new Orientation.Stretcher()), OffsetFraction: 0.0)),
            Description: "Stack bond: all stretchers, vertical joints aligned; decorative, requires ties. BIA TN 30."),
        [BondName.EnglishCross] = new Bond(
            Name: BondName.EnglishCross,
            Courses: Seq(
                new CourseTemplate(Sequence: Seq<Orientation>(new Orientation.Stretcher()), OffsetFraction: 0.0),
                new CourseTemplate(Sequence: Seq<Orientation>(new Orientation.Header()), OffsetFraction: 0.25),
                new CourseTemplate(Sequence: Seq<Orientation>(new Orientation.Stretcher()), OffsetFraction: 0.5),
                new CourseTemplate(Sequence: Seq<Orientation>(new Orientation.Header()), OffsetFraction: 0.25)),
            Description: "English cross (Dutch) bond: English variant with stretchers in alternate stretcher courses offset by half-brick. BIA TN 30."),
        [BondName.FlemishCross] = new Bond(
            Name: BondName.FlemishCross,
            Courses: Seq(
                new CourseTemplate(Sequence: Seq<Orientation>(new Orientation.Stretcher(), new Orientation.Header()), OffsetFraction: 0.0),
                new CourseTemplate(Sequence: Seq<Orientation>(new Orientation.Stretcher()), OffsetFraction: 0.5)),
            Description: "Flemish cross bond: Flemish course alternating with all-stretcher course. BIA TN 30."),
        [BondName.Sussex] = new Bond(
            Name: BondName.Sussex,
            Courses: Seq(new CourseTemplate(Sequence: Seq<Orientation>(new Orientation.Stretcher(), new Orientation.Stretcher(), new Orientation.Stretcher(), new Orientation.Header()), OffsetFraction: 0.0)),
            Description: "Sussex (Flemish garden wall) bond: three stretchers plus one header per course. BIA TN 30."),
        [BondName.Scotch] = new Bond(
            Name: BondName.Scotch,
            Courses: Seq(
                new CourseTemplate(Sequence: Seq<Orientation>(new Orientation.Stretcher()), OffsetFraction: 0.0),
                new CourseTemplate(Sequence: Seq<Orientation>(new Orientation.Stretcher()), OffsetFraction: 0.5),
                new CourseTemplate(Sequence: Seq<Orientation>(new Orientation.Stretcher()), OffsetFraction: 0.0),
                new CourseTemplate(Sequence: Seq<Orientation>(new Orientation.Header()), OffsetFraction: 0.25)),
            Description: "Scotch (English garden wall) bond: three stretcher courses plus one header course. BIA TN 30."),
        [BondName.Header] = new Bond(
            Name: BondName.Header,
            Courses: Seq(
                new CourseTemplate(Sequence: Seq<Orientation>(new Orientation.Header()), OffsetFraction: 0.0),
                new CourseTemplate(Sequence: Seq<Orientation>(new Orientation.Header()), OffsetFraction: 0.5)),
            Description: "Header (heading) bond: all headers; thick walls and curved walls. BIA TN 30."),
        [BondName.Monk] = new Bond(
            Name: BondName.Monk,
            Courses: Seq(
                new CourseTemplate(Sequence: Seq<Orientation>(new Orientation.Stretcher(), new Orientation.Stretcher(), new Orientation.Header()), OffsetFraction: 0.0),
                new CourseTemplate(Sequence: Seq<Orientation>(new Orientation.Stretcher(), new Orientation.Stretcher(), new Orientation.Header()), OffsetFraction: 1.0 / 3.0)),
            Description: "Monk bond: two stretchers plus one header per course, third-brick course offset. BIA TN 30."),
        [BondName.RatTrap] = new Bond(
            Name: BondName.RatTrap,
            Courses: Seq(
                new CourseTemplate(Sequence: Seq<Orientation>(new Orientation.Shiner(), new Orientation.Rowlock()), OffsetFraction: 0.0),
                new CourseTemplate(Sequence: Seq<Orientation>(new Orientation.Rowlock(), new Orientation.Shiner()), OffsetFraction: 0.5)),
            Description: "Rat-trap bond (Indian, ISI): bricks on edge (shiner + rowlock) with internal cavities; ~27% brick savings, ~30-55% mortar savings."),
        [BondName.HerringboneFlat] = new Bond(
            Name: BondName.HerringboneFlat,
            Courses: Seq(
                new CourseTemplate(Sequence: Seq<Orientation>(new Orientation.Stretcher()), OffsetFraction: 0.0),
                new CourseTemplate(Sequence: Seq<Orientation>(new Orientation.Stretcher()), OffsetFraction: 0.5)),
            Description: "Herringbone (flat) bond: 90°-rotated paired stretchers; pavement and decorative panel. Geometric rotation applied at layout time."),
        [BondName.Herringbone45] = new Bond(
            Name: BondName.Herringbone45,
            Courses: Seq(new CourseTemplate(Sequence: Seq<Orientation>(new Orientation.Stretcher()), OffsetFraction: 0.0)),
            Description: "Herringbone (45°) bond: stretchers in 45°-rotated zig-zag pairs; layout applies the continuous rotation parameter."),
        [BondName.BasketWeave] = new Bond(
            Name: BondName.BasketWeave,
            Courses: Seq(
                new CourseTemplate(Sequence: Seq<Orientation>(new Orientation.Stretcher(), new Orientation.Soldier(), new Orientation.Soldier()), OffsetFraction: 0.0),
                new CourseTemplate(Sequence: Seq<Orientation>(new Orientation.Soldier(), new Orientation.Soldier(), new Orientation.Stretcher()), OffsetFraction: 0.0)),
            Description: "Basket weave bond: paired vertical and horizontal stretchers alternating in chequerboard."),
        [BondName.Soldier] = new Bond(
            Name: BondName.Soldier,
            Courses: Seq(new CourseTemplate(Sequence: Seq<Orientation>(new Orientation.Soldier()), OffsetFraction: 0.0)),
            Description: "Soldier course: all vertical bricks, narrow face out; lintels and decorative bands."),
    }.ToFrozenDictionary();

    public static readonly Seq<SpecialShape> Shapes = Seq<SpecialShape>(
        new SpecialShape.SingleBullnose(RadiusMm: 15.0),
        new SpecialShape.SingleBullnose(RadiusMm: 20.0),
        new SpecialShape.DoubleBullnose(RadiusMm: 15.0),
        new SpecialShape.DoubleBullnose(RadiusMm: 20.0),
        new SpecialShape.Cownose(RadiusMm: 28.5),
        new SpecialShape.PlinthHeader(),
        new SpecialShape.PlinthStretcher(),
        new SpecialShape.PlinthExternalReturn(),
        new SpecialShape.PlinthInternalReturn(),
        new SpecialShape.Coping(Profile: CopingProfile.Saddle),
        new SpecialShape.Coping(Profile: CopingProfile.HalfRound),
        new SpecialShape.Coping(Profile: CopingProfile.Splayed),
        new SpecialShape.Coping(Profile: CopingProfile.Throated),
        new SpecialShape.Cant(Faces: 1),
        new SpecialShape.Cant(Faces: 2),
        new SpecialShape.Squint(AngleDegrees: 30.0),
        new SpecialShape.Squint(AngleDegrees: 45.0),
        new SpecialShape.Squint(AngleDegrees: 60.0),
        new SpecialShape.Voussoir(AngleDegrees: 15.0, DepthMm: 92.075),
        new SpecialShape.AirVent(OpenAreaFraction: 0.25));

    public static readonly FrozenDictionary<JointProfile, JointSpec> Joints = new Dictionary<JointProfile, JointSpec>() {
        [JointProfile.Concave] = new JointSpec(Profile: JointProfile.Concave, ThicknessMm: 9.525, ReinforcementWireGaugeMm: Option<double>.None, Description: "Concave joint: best water resistance; compacted with curved jointer; default US exterior."),
        [JointProfile.V] = new JointSpec(Profile: JointProfile.V, ThicknessMm: 9.525, ReinforcementWireGaugeMm: Option<double>.None, Description: "V-joint: good water resistance; crisp shadow line; Craftsman / Tudor."),
        [JointProfile.Weathered] = new JointSpec(Profile: JointProfile.Weathered, ThicknessMm: 9.525, ReinforcementWireGaugeMm: Option<double>.None, Description: "Weathered (struck-down) joint: good water resistance; sloped to shed water."),
        [JointProfile.Struck] = new JointSpec(Profile: JointProfile.Struck, ThicknessMm: 9.525, ReinforcementWireGaugeMm: Option<double>.None, Description: "Struck (struck-up) joint: poor water resistance; ledge holds water; interior only."),
        [JointProfile.Flush] = new JointSpec(Profile: JointProfile.Flush, ThicknessMm: 9.525, ReinforcementWireGaugeMm: Option<double>.None, Description: "Flush joint: poor water resistance; good for paint or plaster; smooth modern look."),
        [JointProfile.Raked] = new JointSpec(Profile: JointProfile.Raked, ThicknessMm: 9.525, ReinforcementWireGaugeMm: Option<double>.None, Description: "Raked joint: poor water resistance; deep recess for dramatic shadow; demands dense brick."),
        [JointProfile.Beaded] = new JointSpec(Profile: JointProfile.Beaded, ThicknessMm: 9.525, ReinforcementWireGaugeMm: Option<double>.None, Description: "Beaded joint: poor water resistance; decorative bead; historic."),
        [JointProfile.Extruded] = new JointSpec(Profile: JointProfile.Extruded, ThicknessMm: 9.525, ReinforcementWireGaugeMm: Option<double>.None, Description: "Extruded (squeezed) joint: worst water resistance; rustic; interior only."),
        [JointProfile.Grapevine] = new JointSpec(Profile: JointProfile.Grapevine, ThicknessMm: 9.525, ReinforcementWireGaugeMm: Option<double>.None, Description: "Grapevine joint: moderate water resistance; Colonial revival profile."),
    }.ToFrozenDictionary();

    // --- [OPERATIONS] -------------------------------------------------------------------------
    public static Fin<Brick> Find(BrickDesignation designation) =>
        Bricks.GetValueOrDefault(key: designation) is Brick brick
            ? Fin.Succ(value: brick)
            : Fin.Fail<Brick>(error: new BrickError.DesignationNotFound(Designation: designation));

    public static Fin<Bond> Find(BondName name) =>
        Bonds.GetValueOrDefault(key: name) is Bond bond
            ? Fin.Succ(value: bond)
            : Fin.Fail<Bond>(error: new BrickError.BondNotFound(Bond: name));

    public static Fin<JointSpec> Find(JointProfile profile) =>
        Joints.GetValueOrDefault(key: profile) is JointSpec joint
            ? Fin.Succ(value: joint)
            : Fin.Fail<JointSpec>(error: new BrickError.JointNotFound(Profile: profile));

    public static Seq<Brick> InRegion(BrickRegion region) =>
        toSeq(Bricks.Values.Where(b => b.Region == region));
}
