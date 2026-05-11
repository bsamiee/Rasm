using Grasshopper2.Components;
using Grasshopper2.Parameters.Standard;
using Grasshopper2.Types.Shapes;
using LanguageExt;
using NUnit.Framework;
using Radyab.Components;
using Rasm.Domain;
using Rasm.Grasshopper;
using Rhino.Testing.Fixtures;
using static LanguageExt.Prelude;

namespace Runtime.Rhino.Tests.Grasshopper;

// --- [RUNTIME SPECS] ---------------------------------------------------------------------------

[TestFixture]
[RhinoTestFixture]
public sealed class ComponentRuntimeSpec {
    private static readonly string[] ExtractCurveCodes = ["AC", "CS", "CF", "S", "SCV", "BC", "NO", "NI", "IE", "NE", "OL", "IL", "U", "V", "SC", "DC", "IC"];
    private static readonly string[] ExtractPointCodes = ["EM", "SC", "BC", "V", "CP", "B", "Q", "K"];
    private static readonly string[] ExtractSurfaceCodes = ["AS", "SI", "TS", "BS", "IS", "UV", "FC", "FN", "FI", "CI", "UD"];

    [Test]
    public void ConstructsExtractionComponentsFromDeclaredContracts() =>
        Assert.Multiple(static () => {
            AssertComponent(component: new ExtractCurves(), inputs: ExtractCurves.Spec.Inputs, outputs: ExtractCurves.Spec.Outputs);
            AssertComponent(component: new ExtractPoints(), inputs: ExtractPoints.Spec.Inputs, outputs: ExtractPoints.Spec.Outputs);
            AssertComponent(component: new ExtractSurfaces(), inputs: ExtractSurfaces.Spec.Inputs, outputs: ExtractSurfaces.Spec.Outputs);
        });

    [Test]
    public void UsesSingleGenericGeometryInputContract() =>
        Assert.Multiple(static () => {
            Assert.That(actual: new ExtractCurves().Parameters.Input(index: 0), expression: Is.TypeOf<GenericParameter>());
            Assert.That(actual: new ExtractPoints().Parameters.Input(index: 0), expression: Is.TypeOf<GenericParameter>());
            Assert.That(actual: new ExtractSurfaces().Parameters.Input(index: 0), expression: Is.TypeOf<GenericParameter>());
        });

    [Test]
    public void AppliesNativeParameterPolicies() {
        Component component = new ExtractCurves();
        IntegerParameter index = (IntegerParameter)component.Parameters.Input(index: 1);
        VectorParameter direction = (VectorParameter)component.Parameters.Input(index: 2);
        AngleParameter draft = (AngleParameter)component.Parameters.Input(index: 3);

        Assert.Multiple(() => {
            Assert.That(actual: index.IsIndex, expression: Is.True);
            Assert.That(actual: index.Indexing, expression: Is.EqualTo(expected: IndexModifier.Clip));
            Assert.That(actual: direction.UnitiseVectors, expression: Is.True);
            Assert.That(actual: direction.ReverseVectors, expression: Is.False);
            Assert.That(actual: draft.ReduceAngles, expression: Is.True);
        });
    }

    [Test]
    public void PreservesExtractionOutputOrdering() =>
        Assert.Multiple(static () => {
            Assert.That(actual: Codes(outputs: ExtractCurves.Spec.Outputs), expression: Is.EqualTo(expected: ExtractCurveCodes));
            Assert.That(actual: Codes(outputs: ExtractPoints.Spec.Outputs), expression: Is.EqualTo(expected: ExtractPointCodes));
            Assert.That(actual: Codes(outputs: ExtractSurfaces.Spec.Outputs), expression: Is.EqualTo(expected: ExtractSurfaceCodes));
        });

    [Test]
    public void OutputCodesAreUniqueWithinEachComponent() =>
        Assert.Multiple(static () => {
            Assert.That(actual: Codes(outputs: ExtractCurves.Spec.Outputs), expression: Is.Unique);
            Assert.That(actual: Codes(outputs: ExtractPoints.Spec.Outputs), expression: Is.Unique);
            Assert.That(actual: Codes(outputs: ExtractSurfaces.Spec.Outputs), expression: Is.Unique);
        });

    [Test]
    public void MapsDefaultPortKindsAndEnumKinds() =>
        Assert.Multiple(static () => {
            Assert.That(actual: PortKind.From(type: typeof(int)).IfNone(PortKind.Generic), expression: Is.SameAs(expected: PortKind.Integer));
            Assert.That(actual: PortKind.From(type: typeof(double)).IfNone(PortKind.Generic), expression: Is.SameAs(expected: PortKind.Number));
            Assert.That(actual: PortKind.From(type: typeof(string)).IfNone(PortKind.Generic), expression: Is.SameAs(expected: PortKind.Text));
            Assert.That(actual: PortKind.From(type: typeof(CurveLocus)).IfNone(PortKind.Generic), expression: Is.SameAs(expected: PortKind.CurveLocus));
            Assert.That(actual: PortKind.From(type: typeof(SurfaceLocus)).IfNone(PortKind.Generic), expression: Is.SameAs(expected: PortKind.SurfaceLocus));
            Assert.That(actual: PortKind.From(type: typeof(GeometryKind)).IfNone(PortKind.Generic), expression: Is.SameAs(expected: PortKind.Generic));
            Assert.That(actual: PortKind.Enum(initial: GeometryKind.Unknown).Type, expression: Is.EqualTo(expected: typeof(GeometryKind)));
        });

    [Test]
    public void BindsNativeLocusParameterKinds() {
        LocusComponent component = new();

        Assert.Multiple(() => {
            Assert.That(actual: component.Parameters.Input(index: 0), expression: Is.TypeOf<CurveLocusParameter>());
            Assert.That(actual: component.Parameters.Input(index: 1), expression: Is.TypeOf<SurfaceLocusParameter>());
        });
    }

    private static void AssertComponent(Component component, Seq<IPort> inputs, Seq<IOutputGroup> outputs) =>
        Assert.Multiple(() => {
            Assert.That(actual: component.Parameters.InputCount, expression: Is.EqualTo(expected: inputs.Count));
            Assert.That(actual: component.Parameters.OutputCount, expression: Is.EqualTo(expected: outputs.Bind(static group => group.Ports).Count));
        });

    private static string[] Codes(Seq<IOutputGroup> outputs) =>
        [.. outputs.Bind(static group => group.Ports).Map(static port => port.Code)];

    public sealed class LocusSpec : IComponentSpec {
        public static Seq<IPort> Inputs { get; } = Seq<IPort>(
            Port.Required<CurveLocus>(name: "Curve Locus", code: "CL", info: "Native GH2 curve locus."),
            Port.Required<SurfaceLocus>(name: "Surface Locus", code: "SL", info: "Native GH2 surface locus."));
        public static Seq<IOutputGroup> Outputs { get; } = Seq<IOutputGroup>();
    }

    private sealed class LocusComponent : Rasm.Grasshopper.Component<LocusSpec> {
        public LocusComponent() : base(nomen: new Grasshopper2.UI.Nomen(name: "Locus", info: "Locus")) { }
    }
}
