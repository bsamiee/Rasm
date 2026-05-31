using Rasm.Domain;
using Rasm.TestKit;
using Rhino.Geometry;

namespace Rasm.Tests.Domain;

// --- [CONSTANTS] ----------------------------------------------------------------------------
internal static class ValidationGens {
    public static readonly Op Key = Op.Of(name: "validation-test");
    public static readonly Gen<string> Token = Gen.Int[start: 1, finish: 100_000].Select(static value => $"token-{value}");
    public static readonly (Error Error, string Category)[] FaultRows = [
        (new Fault.MissingOperation(), "Operation"),
        (Key.MissingContext(), "Operation"),
        (Key.InvalidInput(), "Input"),
        (Key.InvalidResult(), "Result"),
        (new Fault.Cancelled(), "Cancelled"),
        (Key.Unsupported(geometryType: typeof(Point3d), outputType: typeof(Plane)), "Unsupported"),
        (new Fault.ComputationFailed(Label: "probe"), "Computation"),
        (new Fault.MissingGeometry(), "Geometry"),
        (new Fault.OutOfRange(Label: "probe", Scalar: -1.0, Requirement: "positive"), "Tolerance"),
        (new Fault.InvalidUnitSystem(Units: Rhino.UnitSystem.None, Requirement: "model"), "Context"),
        (Key.Caution(concern: "recoverable"), "Caution"),
    ];
}

// --- [ALGEBRAIC] ----------------------------------------------------------------------------
public sealed class OpAcceptanceLaws {
    [Fact]
    public void AcceptValueAdmitsKnownValidDomainsAndRejectsKnownInvalidDomains() =>
        Spec.ForAll(Gens.Finite.Select(ValidationGens.Token), static tuple => {
            (double scalar, string text) = tuple;
            Spec.Succ(result: ValidationGens.Key.AcceptValue(value: scalar), then: accepted => Spec.Equal(left: accepted, right: scalar, tolerance: 0.0, what: "finite scalar"));
            Spec.Succ(result: ValidationGens.Key.AcceptText(value: $"  {text}  "), then: accepted => Assert.Equal(expected: text, actual: accepted));
            Spec.Succ(result: ValidationGens.Key.AcceptValue(value: true), then: accepted => Assert.True(condition: accepted));
            Spec.FailCategory(result: ValidationGens.Key.AcceptValue(value: double.NaN), category: "Result");
            Spec.FailCategory(result: ValidationGens.Key.AcceptValue(value: Guid.Empty), category: "Result");
            Spec.FailCategory(result: ValidationGens.Key.AcceptText(value: "   "), category: "Result");
        });

    [Fact]
    public void AcceptSequencesConserveAllAcceptedValuesAndRejectTheFirstInvalidCandidate() {
        Spec.Succ(result: ValidationGens.Key.Accept(values: [1.0, 2.0, 3.0]), then: values => Assert.Equal(expected: Seq(1.0, 2.0, 3.0), actual: values));
        Spec.FailCategory(result: ValidationGens.Key.Accept(values: [1.0, double.PositiveInfinity, 3.0]), category: "Result");
        Spec.FailUnsupportedFor(result: ValidationGens.Key.AcceptResults<double, int>(values: [1.0]), geometryType: typeof(double), outputType: typeof(int));
    }

    [Fact]
    public void NeedConfirmCatchAndOrDefaultPreserveRailCategories() {
        Spec.Succ(result: ValidationGens.Key.Need(value: Some("present")), then: value => Assert.Equal(expected: "present", actual: value));
        Spec.FailCategory(result: ValidationGens.Key.Need(value: Option<string>.None), category: "Input");
        Spec.Succ(result: ValidationGens.Key.Confirm(success: true));
        Spec.FailCategory(result: ValidationGens.Key.Confirm(success: false), category: "Result");
        Spec.FailCategory(result: ValidationGens.Key.Catch<int>(body: static () => throw new InvalidOperationException(message: "boom")), category: "Result");
        Assert.Equal(expected: Op.Of(name: "fallback"), actual: ((Op?)null).OrDefault(name: "fallback"));
    }
}

public sealed class FaultCategoryLaws {
    [Fact]
    public void FaultCategoriesAreStableAndUnsupportedCarriesTypePair() =>
        Spec.Cases(items: ValidationGens.FaultRows, key: static row => row.Error.GetType(), law: static row => {
            Assert.Equal(expected: row.Category, actual: row.Error.Category());
            _ = row.Error switch {
                Fault.Unsupported unsupported => AssertUnsupported(unsupported: unsupported),
                _ => unit,
            };
        });

    private static Unit AssertUnsupported(Fault.Unsupported unsupported) {
        Assert.Equal(expected: Fault.UnsupportedCode, actual: unsupported.Code);
        Assert.Equal(expected: typeof(Point3d), actual: unsupported.GeometryType);
        Assert.Equal(expected: typeof(Plane), actual: unsupported.OutputType);
        return unit;
    }
}

public sealed class RequirementLaws(ContextFixture fixture) {
    [Fact]
    public void EmptyRequirementAcceptsManagedValuesAndRejectsMissingGeometry() {
        Spec.Valid(result: Requirement.None.Apply(context: fixture.Default, value: 3.0, cancel: TestContext.Current.CancellationToken), then: accepted => Spec.Equal(left: accepted, right: 3.0, tolerance: 0.0, what: "managed scalar"));
        Spec.Invalid(result: Requirement.None.Apply<string>(context: fixture.Default, value: null!, cancel: TestContext.Current.CancellationToken), then: error => Assert.Equal(expected: "Geometry", actual: error.Category()));
    }

    [Fact]
    public void RequirementForKindMatchesTopologyOwnership() =>
        Spec.Cases(items: Kind.Items, key: static kind => kind.Key, law: static kind =>
            Assert.Equal(expected: ExpectedRequirement(kind: kind), actual: Requirement.ForKind(kind: kind)));

    private static Requirement ExpectedRequirement(Kind kind) =>
        kind.Topology == Topology.Curve ? Requirement.CurveLength
        : kind.Topology == Topology.Surface ? Requirement.SurfaceEvaluation
        : kind.Topology == Topology.Brep || kind.Topology == Topology.Extrusion ? Requirement.SolidTopology
        : kind.Topology == Topology.Mesh || kind.Topology == Topology.SubD ? Requirement.MeshCheck
        : kind.Topology == Topology.Point || kind.Topology == Topology.PointCloud || kind.Topology == Topology.Hatch ? Requirement.None
        : Requirement.Basic;
}
