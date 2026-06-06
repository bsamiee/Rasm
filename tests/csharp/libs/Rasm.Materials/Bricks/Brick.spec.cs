using Rasm.Materials.Bricks;
using Rasm.TestKit;

namespace Rasm.Materials.Tests.Bricks;

// --- [MODELS] ----------------------------------------------------------------------------
internal static class BrickCatalogGens {
    public static readonly (BrickRegion Region, int Count)[] RegionCounts =
        [(BrickRegion.Us, 15), (BrickRegion.Uk, 1), (BrickRegion.Din, 4), (BrickRegion.Au, 1), (BrickRegion.Is, 2)];
    public static readonly Gen<double> PositiveFinite = Gen.Double[start: 0.01, finish: 10_000.0];
    public static readonly Gen<double> NonPositiveOrNonFinite = Gen.OneOf(Gens.NonPositive, Gens.NonFinite);
    public static string NoteOf(BrickSource source) =>
        source switch {
            BrickSource.BiaTn10Table1 s => s.Note,
            BrickSource.BiaTn10Table2 s => s.Note,
            BrickSource.BsEn771Part1 s => s.Note,
            BrickSource.DinEn771Part1 s => s.Note,
            BrickSource.AsNzs4455Part1 s => s.Note,
            BrickSource.Is1077 s => s.Note,
            _ => string.Empty,
        };
}

// --- [OPERATIONS] ----------------------------------------------------------------------------
public sealed class BrickSmartEnumCatalogLaws {
    [Fact]
    public void ClosedCatalogKeysAreUniqueAcrossEveryVocabulary() {
        Spec.SmartEnumKeysUnique(items: CoringClass.Items, key: static item => item.Key);
        Spec.SmartEnumKeysUnique(items: BondKind.Items, key: static item => item.Key);
        Spec.SmartEnumKeysUnique(items: CopingProfile.Items, key: static item => item.Key);
        Spec.SmartEnumKeysUnique(items: PlinthKind.Items, key: static item => item.Key);
        Spec.SmartEnumKeysUnique(items: BrickType.Items, key: static item => item.Key);
        Spec.SmartEnumKeysUnique(items: Coring.Items, key: static item => item.Key);
        Spec.SmartEnumKeysUnique(items: JointProfile.Items, key: static item => item.Key);
        Spec.SmartEnumKeysUnique(items: ArchProfile.Items, key: static item => item.Key);
        Spec.SmartEnumKeysUnique(items: BrickRegion.Items, key: static item => item.Key);
        Spec.SmartEnumKeysUnique(items: BondName.Items, key: static item => item.Key);
        Spec.SmartEnumKeysUnique(items: BrickDesignation.Items, key: static item => item.Key);
    }

    [Fact]
    public void RegionUnitMembershipMatchesThePublishedCatalogPartition() =>
        _ = BrickCatalogGens.RegionCounts.AsIterable().Iter(static row => {
            Seq<BrickDesignation> units = row.Region.Units();
            Assert.Equal(expected: row.Count, actual: units.Count);
            Assert.True(condition: units.ForAll(unit => unit.Region == row.Region));
        });

    [Fact]
    public void CoringClassificationAndVoidFractionsStayWithinPublishedBands() =>
        Spec.Cases(items: Coring.Items, key: static item => item.Key, law: static item => {
            Assert.InRange(actual: item.VoidFraction, low: 0.0, high: 0.60);
            Assert.True(condition: item.Classification switch {
                CoringClass c when c == CoringClass.Solid => item.VoidFraction <= 0.18,
                CoringClass c when c == CoringClass.Cored => item.VoidFraction is >= 0.20 and <= 0.24,
                CoringClass c when c == CoringClass.Perforated => item.VoidFraction is >= 0.30 and <= 0.50,
                CoringClass c when c == CoringClass.Hollow => item.VoidFraction is >= 0.50 and <= 0.60,
                _ => false,
            });
        });

    [Fact]
    public void RegionalPoliciesCarryPositiveScalarRulesWithoutForkingTheBaseline() =>
        Spec.Cases(items: BrickRegion.Items, key: static region => region.Key, law: static region => {
            Assert.True(condition: region.StandardJointThicknessMm > 0.0);
            Assert.True(condition: region.Policy.Movement.MoistureCoefficient > 0.0);
            Assert.True(condition: region.Policy.Movement.ThermalPerCelsius > 0.0);
            Assert.True(condition: region.Policy.Expansion.Spacing(hasOpenings: true) < region.Policy.Expansion.Spacing(hasOpenings: false));
            Assert.True(condition: region.Policy.Expansion.WidthMinMm > 0.0 && region.Policy.Expansion.WidthMinMm < region.Policy.Expansion.WidthMaxMm);
            Assert.True(condition: region.Policy.WeepHole.WickMaxMm < region.Policy.WeepHole.OpenHeadMaxMm && region.Policy.WeepHole.OpenHeadMaxMm < region.Policy.WeepHole.IbcMaxMm);
            Assert.True(condition: region.Policy.Ties.Enhanced.MaxSpacingMm < region.Policy.Ties.Basic.MaxSpacingMm);
            Assert.True(condition: region.Policy.Ties.Enhanced.AreaPerTieMm2 < region.Policy.Ties.Basic.AreaPerTieMm2);
            Assert.True(condition: region.Policy.Ties.TypicalHorizontalMm > 0.0 && region.Policy.Ties.TypicalVerticalMm > 0.0);
            Assert.False(condition: string.IsNullOrWhiteSpace(value: region.Policy.Authority));
        });
}

public sealed class BrickDesignationLaws {
    [Fact]
    public void DesignationsCarryPositiveDimensionsCoursingAndSourceNotes() =>
        Spec.Cases(items: BrickDesignation.Items, key: static item => item.Key, law: static item => {
            Assert.True(condition: item.Specified.WidthMm > 0.0);
            Assert.True(condition: item.Specified.HeightMm > 0.0);
            Assert.True(condition: item.Specified.LengthMm > 0.0);
            Assert.True(condition: item.Coursing.CoursesPerModule > 0);
            Assert.True(condition: item.Coursing.CourseHeightMm > 0.0);
            Spec.Some(Some(item.Source), source => Assert.False(condition: string.IsNullOrWhiteSpace(value: BrickCatalogGens.NoteOf(source))));
            _ = item.Nominal.IfSome(nominal => {
                Assert.True(condition: nominal.WidthMm >= item.Specified.WidthMm);
                Assert.True(condition: nominal.HeightMm >= item.Specified.HeightMm);
                Assert.True(condition: nominal.LengthMm >= item.Specified.LengthMm);
            });
        });

    [Fact]
    public void AspectLimitedBondsAcceptAndRejectByLengthWidthRatio() =>
        Spec.Cases(items: BondName.Items.Where(static bond => bond.Aspect.IsSome).ToArray(), key: static bond => bond.Key, law: static bond => {
            AspectRatio ratio = bond.Aspect.IfNone(AspectRatio.Create(lengthOverWidth: 1.0));
            Brick exact = BrickDesignation.UsModular.Unit with {
                Specified = Dim3.Create(widthMm: 100.0, heightMm: 60.0, lengthMm: ratio.LengthOverWidth * 100.0),
            };
            Brick tooLong = exact with {
                Specified = Dim3.Create(widthMm: 100.0, heightMm: 60.0, lengthMm: (ratio.LengthOverWidth * 100.0) + (AspectRatio.MatchTolerance * ratio.LengthOverWidth * 100.0 * 2.0)),
            };
            Assert.True(condition: bond.Fits(brick: exact));
            Assert.False(condition: bond.Fits(brick: tooLong));
        });

    [Fact]
    public void TemplateCoursesWrapModuloWhileGeneratedBondsStayClosed() {
        CourseTemplate first = BondName.English.Course(index: 0).IfNone(() => throw new InvalidOperationException(message: "missing first course"));
        CourseTemplate wrappedNegative = BondName.English.Course(index: -2).IfNone(() => throw new InvalidOperationException(message: "missing wrapped course"));
        CourseTemplate wrappedPositive = BondName.English.Course(index: 2).IfNone(() => throw new InvalidOperationException(message: "missing wrapped course"));

        Assert.Equal(expected: first, actual: wrappedNegative);
        Assert.Equal(expected: first, actual: wrappedPositive);
        Spec.None(BondName.Herringbone45.Course(index: 0));
        Spec.None(BondName.OpusReticulatum.Course(index: 10));
    }
}

public sealed class BrickValueObjectLaws {
    [Fact]
    public void DimensionValueObjectsRejectNonPositiveAndNonFiniteInputs() =>
        Spec.ForAll(BrickCatalogGens.NonPositiveOrNonFinite, static value => {
            Exception dim = Assert.ThrowsAny<Exception>(testCode: () => { _ = Dim3.Create(widthMm: value, heightMm: 1.0, lengthMm: 1.0); });
            Exception aspect = Assert.ThrowsAny<Exception>(testCode: () => { _ = AspectRatio.Create(lengthOverWidth: value); });
            Exception coursing = Assert.ThrowsAny<Exception>(testCode: () => { _ = VerticalCoursing.Create(coursesPerModule: 1, moduleHeightMm: value); });
            Assert.Contains(expectedSubstring: "validation failed", actualString: dim.Message, comparisonType: StringComparison.OrdinalIgnoreCase);
            Assert.Contains(expectedSubstring: "validation failed", actualString: aspect.Message, comparisonType: StringComparison.OrdinalIgnoreCase);
            Assert.Contains(expectedSubstring: "validation failed", actualString: coursing.Message, comparisonType: StringComparison.OrdinalIgnoreCase);
        });

    [Fact]
    public void DimensionValueObjectsRejectFiniteDimensionsWhoseVolumeOverflows() {
        Exception dim = Assert.ThrowsAny<Exception>(testCode: static () => {
            _ = Dim3.Create(widthMm: double.MaxValue, heightMm: 2.0, lengthMm: 2.0);
        });
        Assert.Contains(expectedSubstring: "VolumeMm3 must be finite", actualString: dim.Message, comparisonType: StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidDimensionObjectsExposeClosedFormDerivedValues() =>
        Spec.ForAll(BrickCatalogGens.PositiveFinite.Select(BrickCatalogGens.PositiveFinite, BrickCatalogGens.PositiveFinite), static tuple => {
            Dim3 dims = Dim3.Create(widthMm: tuple.Item1, heightMm: tuple.Item2, lengthMm: tuple.Item3);
            Spec.Equal(left: dims.VolumeMm3, right: tuple.Item1 * tuple.Item2 * tuple.Item3, tolerance: 1.0e-8, what: "volume");
            VerticalCoursing coursing = VerticalCoursing.Create(coursesPerModule: 4, moduleHeightMm: tuple.Item2);
            Spec.Equal(left: coursing.CourseHeightMm, right: tuple.Item2 / 4.0, tolerance: 1.0e-12, what: "course height");
        });
}

public sealed class BrickSpecialShapeLaws {
    [Fact]
    public void SpecialShapeCatalogCoversEveryShapeFamilyWithPositivePayloads() {
        Assert.Equal(expected: 32, actual: SpecialShape.Catalog.Count);
        Assert.Equal(expected: 14, actual: SpecialShape.Catalog.Select(static shape => shape.GetType()).Distinct().Count());
        Assert.True(condition: Enumerable.All(SpecialShape.Catalog, static shape => shape switch {
            SpecialShape.SingleBullnose s => s.RadiusMm > 0.0,
            SpecialShape.DoubleBullnose s => s.RadiusMm > 0.0,
            SpecialShape.Cownose s => s.RadiusMm > 0.0,
            SpecialShape.Plinth s => s.BevelDegrees > 0.0 && s.ProjectionMm > 0.0,
            SpecialShape.Coping s => s.ThroatMm > 0.0,
            SpecialShape.Cant s => s.Faces > 0 && s.BevelDegrees > 0.0,
            SpecialShape.Squint s => s.AngleDegrees > 0.0,
            SpecialShape.Voussoir s => s.AngleDegrees > 0.0 && s.DepthMm > 0.0,
            SpecialShape.AirVent s => s.OpenAreaFraction is > 0.0 and < 1.0,
            SpecialShape.Soap s => s.ThicknessMm > 0.0,
            SpecialShape.BrickOnEdgeSill s => s.SlopeDegrees > 0.0 && s.KerfMm > 0.0,
            SpecialShape.HalfBrick or SpecialShape.ThreeQuarterBrick or SpecialShape.QuoinBrick => true,
            _ => false,
        }));
    }
}
