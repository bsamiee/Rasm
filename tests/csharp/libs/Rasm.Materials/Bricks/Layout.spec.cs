using Rasm.Materials.Bricks;
using Rasm.TestKit;

namespace Rasm.Materials.Tests.Bricks;

// --- [CONSTANTS] --------------------------------------------------------------------------
internal static class BrickGens {
    public static readonly BondName[] TemplateBonds = [BondName.Running, BondName.Stack, BondName.Header, BondName.English];
    public static readonly Gen<double> Length = Gen.Double[start: 250.0, finish: 5000.0];
    public static readonly Gen<double> Height = Gen.Double[start: 70.0, finish: 1200.0];
    public static readonly Gen<BrickRun> Run = Length.Select(Height, static (double length, double height) => new BrickRun(
        Unit: BrickDesignation.UsModular,
        Bond: BondName.Running,
        Path: new BrickPath.Line(LengthMm: length),
        HeightMm: height,
        Joint: JointProfile.Concave,
        HeadJointMm: Option<double>.None,
        BedJointMm: Option<double>.None));
}

// --- [ALGEBRAIC] --------------------------------------------------------------------------
public sealed class BrickCatalogLaws {
    [Fact]
    public void CatalogKeysAreDistinctAndTemplateBondsExposeCourses() =>
        Spec.Cases(items: BrickGens.TemplateBonds, key: static bond => bond.Key, law: static bond => {
            Assert.Equal(expected: BondKind.Template, actual: bond.Kind);
            Spec.Some(bond.Course(index: -1));
            Spec.Some(bond.Course(index: 0));
            Spec.Some(bond.Course(index: 1));
        });

    [Fact]
    public void GeneratedBondsDoNotExposeTemplateCourses() =>
        Spec.None(BondName.Herringbone45.Course(index: 0));
}

public sealed class BrickLayoutLaws {
    [Fact]
    public void LineLayoutProducesPositiveCoursesAndPlacements() =>
        Spec.ForAll(BrickGens.Run, static run => Spec.Succ(BrickAssembly.Layout(run: run), then: assembly => {
            Assert.Equal(expected: ((BrickPath.Line)run.Path).LengthMm, actual: assembly.LengthMm);
            Assert.True(condition: assembly.CourseCount >= 1);
            Assert.True(condition: assembly.Placements.Count >= assembly.CourseCount);
            _ = assembly.Placements.Iter(placement => {
                Assert.True(condition: placement.Course >= 0 && placement.Course < assembly.CourseCount);
                Assert.True(condition: placement.RunMm > 0.0);
                Assert.True(condition: placement.RiseMm > 0.0);
                Spec.Equal(left: placement.ElevationMm, right: placement.Course * run.Unit.Unit.Coursing.CourseHeightMm, tolerance: 1.0e-9, what: "elevation");
            });
        }));

    [Fact]
    public void ArcLengthMatchesClosedFormAndInvalidInputsFail() {
        BrickRun arc = new(
            Unit: BrickDesignation.UsModular,
            Bond: BondName.Running,
            Path: new BrickPath.Arc(RadiusMm: 1000.0, SweepDegrees: 90.0),
            HeightMm: 250.0,
            Joint: JointProfile.Concave,
            HeadJointMm: Option<double>.None,
            BedJointMm: Option<double>.None);
        Spec.Succ(BrickAssembly.Layout(run: arc), then: assembly =>
            Spec.Equal(left: assembly.LengthMm, right: Math.PI * 500.0, tolerance: 1.0e-9, what: "arc length"));
        Spec.Fail(BrickAssembly.Layout(run: arc with { HeightMm = 0.0 }));
        Spec.Fail(BrickAssembly.Layout(run: arc with { Bond = BondName.Herringbone45 }));
    }
}
