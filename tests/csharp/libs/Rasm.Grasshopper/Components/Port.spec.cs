using Rasm.Grasshopper.Components;
using Rasm.TestKit;
using PortSide = Grasshopper2.Components.Side;
using Requirement = Grasshopper2.Parameters.Requirement;

namespace Rasm.Grasshopper.Tests.Components;

// --- [MODELS] ----------------------------------------------------------------------------
internal enum SampleEnum { First = 0, Second = 1 }
[Flags]
internal enum SampleFlagsEnum { First = 1, Second = 2, }
internal enum SampleByteEnum : byte { First = 1 }

public sealed class CapabilityCompatibilityLaws {
    public static TheoryData<Capability, PortKind, bool> CompatibilityCases => new() {
        { new Capability.Vector(Unitise: true, Reverse: true), PortKind.Vector, true },
        { new Capability.Vector(), PortKind.Point, false },
        { new Capability.Angle(Kind: AngleEnforcement.Degrees, Reduce: true), PortKind.Angle, true },
        { new Capability.Index(), PortKind.Integer, true },
        { new Capability.Index(), PortKind.Index, true },
        { new Capability.Surface(AcceptMeshes: true), PortKind.Surface, true },
        { new Capability.Surface(AcceptMeshes: true), PortKind.Brep, true },
        { new Capability.Curve(), PortKind.Curve, true },
        { new Capability.Preset(Choices: Seq((1, "one"), (2, "two"))), PortKind.Index, true },
        { new Capability.Preset(Choices: Seq((1, "one"), (2, "two"))), PortKind.Number, false },
        { new Capability.Elective(), PortKind.Mesh, true },
        { new Capability.Hidden(), PortKind.Text, true },
        { new Capability.Category(Name: "Optional"), PortKind.Generic, true },
        { new Capability.Many(Items: Seq<Capability>(new Capability.Vector(), new Capability.Angle())), PortKind.Vector, false },
    };

    [Theory]
    [MemberData(nameof(CompatibilityCases))]
    public void CapabilityMatrixMatchesHostParameterTargets(Capability capability, PortKind kind, bool compatible) {
        ArgumentNullException.ThrowIfNull(argument: capability);
        ArgumentNullException.ThrowIfNull(argument: kind);
        Assert.Equal(expected: compatible, actual: capability.CompatibleWith(kind: kind));
    }

    [Fact]
    public void ValidateCapabilityCarriesPredicateAndMessageWithoutRestrictingKind() {
        Capability capability = new Capability.Validate(Predicate: static value => value is int i && i > 0, Message: "positive");
        (Func<object, bool> predicate, string message) = capability.Validators.Head.IfNone((static _ => false, ""));

        Assert.True(condition: capability.CompatibleWith(kind: PortKind.Generic));
        Assert.True(condition: predicate(arg: 1));
        Assert.False(condition: predicate(arg: 0));
        Assert.Equal(expected: "positive", actual: message);
    }
}

public sealed class PortKindCollisionLaws {
    [Fact]
    public void PrimitiveTypeCollisionsResolveToIntentionalDefaults() {
        Spec.Some(PortKind.From(type: typeof(int)), static kind => Assert.Equal(expected: PortKind.Integer, actual: kind));
        Spec.Some(PortKind.From(type: typeof(string)), static kind => Assert.Equal(expected: PortKind.Text, actual: kind));
        Spec.Holds(condition: !PortKind.TypeCollisions.Exists(static item => item.Type == typeof(int) || item.Type == typeof(string)), label: "intentional primitive defaults leaked into collision set");
    }

    [Fact]
    public void SideSpecificTypeResolutionKeepsAmbiguousNativeTypesExplicit() {
        Spec.Some(PortKind.From(type: typeof(Guid), side: PortSide.Input), static kind => Assert.Same(expected: PortKind.Topological, actual: kind));
        Spec.Some(PortKind.From(type: typeof(Guid), side: PortSide.Output), static kind => Assert.Same(expected: PortKind.Guid, actual: kind));
        Spec.Some(PortKind.From(type: typeof(object), side: PortSide.Input), static kind => Assert.Same(expected: PortKind.Generic, actual: kind));
        Spec.Some(PortKind.From(type: typeof(Shape), side: PortSide.Input), static kind => Assert.Same(expected: PortKind.Generic, actual: kind));
        Assert.True(condition: PortKind.From(type: typeof(TextDot), side: PortSide.Input).IsNone);
        Spec.Some(PortKind.From(type: typeof(TextDot), side: PortSide.Output), static kind => Assert.Same(expected: PortKind.Dot, actual: kind));
    }

    [Fact]
    public void EnumPortsRequireIntBackedNonFlagsEnumsAndUseIntegerWire() {
        Spec.Some(PortKind.From(type: typeof(SampleEnum), side: PortSide.Input), static kind => {
            Assert.Equal(expected: typeof(SampleEnum), actual: kind.Type);
            Assert.Equal(expected: typeof(int), actual: kind.WireType);
            Assert.True(condition: kind.RequiresWire<SampleEnum>());
        });
        _ = Assert.Throws<ArgumentException>(testCode: static () => PortKind.Enum(initial: SampleFlagsEnum.First));
        _ = Assert.Throws<ArgumentException>(testCode: static () => PortKind.Enum(initial: SampleByteEnum.First));
    }
}

public sealed class PortFactoryLaws {
    private static Seq<Capability> Flatten(Capability capability) =>
        capability is Capability.Many many ? many.Items.Bind(Flatten) : Seq(capability);

    [Fact]
    public void OptionalPortsAddElectiveAndCategoryPolicyWithoutDroppingResolvedKind() {
        Port<int> port = Port.Of<int>(name: "Count", code: "C", info: "Count", requirement: Requirement.MayBeMissing);
        Seq<Capability> policies = Flatten(capability: port.Policy);

        Assert.Same(expected: PortKind.Integer, actual: port.Kind);
        Assert.Equal(expected: Requirement.MayBeMissing, actual: port.Requirement);
        Assert.Contains(collection: policies, filter: static capability => capability is Capability.Elective);
        Assert.Contains(collection: policies, filter: static capability => capability is Capability.Category { Name: "Optional" });
    }

    [Fact]
    public void ExplicitKindMustAcceptTheDeclaredTypeOnTheRequestedSide() {
        _ = Assert.Throws<ArgumentException>(
            testCode: static () => Port.Of<int>(name: "Vector", code: "V", info: "bad", kind: PortKind.Vector));
        Assert.Same(expected: PortKind.Generic, actual: Port.Of<int>(name: "Any", code: "A", info: "generic", kind: PortKind.Generic).Kind);
        Assert.Same(expected: PortKind.Generic, actual: Port.Shape().Kind);
    }
}
