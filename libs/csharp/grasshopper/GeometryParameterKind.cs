using Grasshopper2.Components;
using Grasshopper2.Parameters;
using Grasshopper2.UI;
using Rhino.Geometry;
using Thinktecture;
namespace Grasshopper;

// --- [MODELS] ----------------------------------------------------------------------------------

[SmartEnum<string>]
public sealed partial class GeometryParameterKind {
    public static readonly GeometryParameterKind Point = new(
        key: nameof(Point),
        clrType: typeof(Point3d),
        addInput: static (InputAdder adder, string name, string code, string info, Access access, Requirement requirement) =>
            adder.AddPoint(name: name, code: code, info: info, access: access, requirement: requirement),
        addOutput: static (OutputAdder adder, string name, string code, string info, Access access) =>
            adder.AddPoint(name: name, code: code, info: info, access: access));
    public static readonly GeometryParameterKind Vector = new(
        key: nameof(Vector),
        clrType: typeof(Vector3d),
        addInput: static (InputAdder adder, string name, string code, string info, Access access, Requirement requirement) =>
            adder.AddVector(name: name, code: code, info: info, access: access, requirement: requirement),
        addOutput: static (OutputAdder adder, string name, string code, string info, Access access) =>
            adder.AddVector(name: name, code: code, info: info, access: access));
    public static readonly GeometryParameterKind Curve = new(
        key: nameof(Curve),
        clrType: typeof(Curve),
        addInput: static (InputAdder adder, string name, string code, string info, Access access, Requirement requirement) =>
            adder.AddCurve(name: name, code: code, info: info, access: access, requirement: requirement),
        addOutput: static (OutputAdder adder, string name, string code, string info, Access access) =>
            adder.AddCurve(name: name, code: code, info: info, access: access));
    public static readonly GeometryParameterKind Surface = new(
        key: nameof(Surface),
        clrType: typeof(Surface),
        addInput: static (InputAdder adder, string name, string code, string info, Access access, Requirement requirement) =>
            adder.AddSurface(name: name, code: code, info: info, access: access, requirement: requirement),
        addOutput: static (OutputAdder adder, string name, string code, string info, Access access) =>
            adder.AddSurface(name: name, code: code, info: info, access: access));
    public static readonly GeometryParameterKind Brep = new(
        key: nameof(Brep),
        clrType: typeof(Brep),
        addInput: static (InputAdder adder, string name, string code, string info, Access access, Requirement requirement) =>
            adder.AddSurface(name: name, code: code, info: info, access: access, requirement: requirement),
        addOutput: static (OutputAdder adder, string name, string code, string info, Access access) =>
            adder.AddSurface(name: name, code: code, info: info, access: access));
    public static readonly GeometryParameterKind Mesh = new(
        key: nameof(Mesh),
        clrType: typeof(Mesh),
        addInput: static (InputAdder adder, string name, string code, string info, Access access, Requirement requirement) =>
            adder.AddMesh(name: name, code: code, info: info, access: access, requirement: requirement),
        addOutput: static (OutputAdder adder, string name, string code, string info, Access access) =>
            adder.AddMesh(name: name, code: code, info: info, access: access));
    public static readonly GeometryParameterKind Box = new(
        key: nameof(Box),
        clrType: typeof(Box),
        addInput: static (InputAdder adder, string name, string code, string info, Access access, Requirement requirement) =>
            adder.AddBox(name: name, code: code, info: info, access: access, requirement: requirement),
        addOutput: static (OutputAdder adder, string name, string code, string info, Access access) =>
            adder.AddBox(name: name, code: code, info: info, access: access));
    public static readonly GeometryParameterKind Plane = new(
        key: nameof(Plane),
        clrType: typeof(Plane),
        addInput: static (InputAdder adder, string name, string code, string info, Access access, Requirement requirement) =>
            adder.AddPlane(name: name, code: code, info: info, access: access, requirement: requirement),
        addOutput: static (OutputAdder adder, string name, string code, string info, Access access) =>
            adder.AddPlane(name: name, code: code, info: info, access: access));
    public static readonly GeometryParameterKind Line = new(
        key: nameof(Line),
        clrType: typeof(Line),
        addInput: static (InputAdder adder, string name, string code, string info, Access access, Requirement requirement) =>
            adder.AddLine(name: name, code: code, info: info, access: access, requirement: requirement),
        addOutput: static (OutputAdder adder, string name, string code, string info, Access access) =>
            adder.AddLine(name: name, code: code, info: info, access: access));
    public static readonly GeometryParameterKind Circle = new(
        key: nameof(Circle),
        clrType: typeof(Circle),
        addInput: static (InputAdder adder, string name, string code, string info, Access access, Requirement requirement) =>
            adder.AddCircle(name: name, code: code, info: info, access: access, requirement: requirement),
        addOutput: static (OutputAdder adder, string name, string code, string info, Access access) =>
            adder.AddCircle(name: name, code: code, info: info, access: access));
    public static readonly GeometryParameterKind Arc = new(
        key: nameof(Arc),
        clrType: typeof(Arc),
        addInput: static (InputAdder adder, string name, string code, string info, Access access, Requirement requirement) =>
            adder.AddArc(name: name, code: code, info: info, access: access, requirement: requirement),
        addOutput: static (OutputAdder adder, string name, string code, string info, Access access) =>
            adder.AddArc(name: name, code: code, info: info, access: access));
    public static readonly GeometryParameterKind Sphere = new(
        key: nameof(Sphere),
        clrType: typeof(Sphere),
        addInput: static (InputAdder adder, string name, string code, string info, Access access, Requirement requirement) =>
            adder.AddSphere(name: name, code: code, info: info, access: access, requirement: requirement),
        addOutput: static (OutputAdder adder, string name, string code, string info, Access access) =>
            adder.AddSphere(name: name, code: code, info: info, access: access));
    public static readonly GeometryParameterKind SubD = new(
        key: nameof(SubD),
        clrType: typeof(SubD),
        addInput: static (InputAdder adder, string name, string code, string info, Access access, Requirement requirement) =>
            adder.AddGeneric(name: name, code: code, info: info, access: access, requirement: requirement),
        addOutput: static (OutputAdder adder, string name, string code, string info, Access access) =>
            adder.AddGeneric(name: name, code: code, info: info, access: access));
    public static readonly GeometryParameterKind Polyline = new(
        key: nameof(Polyline),
        clrType: typeof(Polyline),
        addInput: static (InputAdder adder, string name, string code, string info, Access access, Requirement requirement) =>
            adder.AddPolyline(name: name, code: code, info: info, access: access, requirement: requirement),
        addOutput: static (OutputAdder adder, string name, string code, string info, Access access) =>
            adder.AddPolyline(name: name, code: code, info: info, access: access));
    public Type ClrType { get; }
    private Action<InputAdder, string, string, string, Access, Requirement> AddInput { get; }
    private Action<OutputAdder, string, string, string, Access> AddOutput { get; }

    // --- [OPERATIONS] --------------------------------------------------------------------------

    public static Option<GeometryParameterKind> From(Type clrType) =>
        Optional(System.Linq.Enumerable.FirstOrDefault(
            source: Items,
            predicate: (GeometryParameterKind candidate) => candidate.ClrType.Equals(o: clrType)));
    public Unit Build(
        InputAdder adder,
        string name,
        string code,
        string info,
        Access access,
        Requirement requirement) {
        AddInput(arg1: adder, arg2: name, arg3: code, arg4: info, arg5: access, arg6: requirement);
        return Unit.Default;
    }
    public Unit Build(
        OutputAdder adder,
        string name,
        string code,
        string info,
        Access access) {
        AddOutput(arg1: adder, arg2: name, arg3: code, arg4: info, arg5: access);
        return Unit.Default;
    }
}
