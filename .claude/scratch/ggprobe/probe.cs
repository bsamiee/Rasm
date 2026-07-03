#:property PublishAot=false
using System.Reflection;

var asm = Assembly.LoadFrom("/Users/bardiasamiee/.nuget/packages/geometrygymifc_core/25.7.30/lib/net8.0/GeometryGymIFCcore.dll");
string[] types = [
    "IfcStructuralLoadSingleForceWarping", "IfcStructuralLoadSingleDisplacementDistortion",
    "IfcStructuralLoadConfiguration", "IfcStructuralLoadCase", "IfcStructuralLoadOrResult",
    "IfcStructuralAction", "IfcStructuralCurveAction", "IfcStructuralSurfaceAction",
    "IfcStructuralReaction", "IfcStructuralCurveReaction", "IfcStructuralResultGroup",
    "IfcStructuralCurveMember", "IfcStructuralMember", "IfcStructuralPointConnection",
    "IfcStructuralCurveConnection", "IfcBoundaryFaceCondition", "IfcBoundaryNodeConditionWarping",
    "IfcDistributionCircuit", "IfcBuiltSystem", "IfcBuildingSystem", "IfcDistributionSystem",
    "IfcStructuralPointReaction", "IfcStructuralPointAction", "IfcStructuralLinearAction", "IfcStructuralPlanarAction",
];
foreach (var name in types) {
    var t = asm.GetType($"GeometryGym.Ifc.{name}");
    if (t is null) { Console.WriteLine($"{name}: ABSENT"); continue; }
    var props = t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
        .Select(p => $"{p.PropertyType.Name} {p.Name}");
    Console.WriteLine($"{name}: base={t.BaseType?.Name} abstract={t.IsAbstract} | {string.Join(", ", props)}");
}
string[] enums = ["IfcActionSourceTypeEnum", "IfcStructuralCurveActivityTypeEnum", "IfcStructuralSurfaceActivityTypeEnum", "IfcDistributionSystemEnum", "IfcDistributionPortTypeEnum", "IfcAnalysisTheoryTypeEnum", "IfcActionTypeEnum", "IfcProjectedOrTrueLengthEnum", "IfcLoadGroupTypeEnum", "IfcFlowDirectionEnum"];
foreach (var name in enums) {
    var t = asm.GetType($"GeometryGym.Ifc.{name}");
    Console.WriteLine(t is null ? $"{name}: ABSENT" : $"{name}: [{string.Join(",", Enum.GetNames(t))}]");
}
