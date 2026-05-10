using System.Drawing;
using Grasshopper2.Components;
using Grasshopper2.Parameters;
using Grasshopper2.Types.Numeric;
using Grasshopper2.UI;
using Rhino.Geometry;
using Thinktecture;
namespace Rasm.Grasshopper;

// --- [MODELS] --------------------------------------------------------------------------

[SmartEnum<string>]
public sealed partial class Param {
    public static readonly Param Point = Of<Point3d>(static (a, n, c, i, ac, r) => a.AddPoint(name: n, code: c, info: i, access: ac, requirement: r), static (a, n, c, i, ac) => a.AddPoint(name: n, code: c, info: i, access: ac));
    public static readonly Param UvPoint = Of<Point2d>(static (a, n, c, i, ac, r) => a.AddUvPoint(name: n, code: c, info: i, access: ac, requirement: r), static (a, n, c, i, ac) => a.AddUvPoint(name: n, code: c, info: i, access: ac));
    public static readonly Param Vector = Of<Vector3d>(static (a, n, c, i, ac, r) => a.AddVector(name: n, code: c, info: i, access: ac, requirement: r), static (a, n, c, i, ac) => a.AddVector(name: n, code: c, info: i, access: ac));
    public static readonly Param Curve = Of<Curve>(static (a, n, c, i, ac, r) => a.AddCurve(name: n, code: c, info: i, access: ac, requirement: r), static (a, n, c, i, ac) => a.AddCurve(name: n, code: c, info: i, access: ac));
    public static readonly Param Surface = Of<Surface>(static (a, n, c, i, ac, r) => a.AddSurface(name: n, code: c, info: i, access: ac, requirement: r), static (a, n, c, i, ac) => a.AddSurface(name: n, code: c, info: i, access: ac));
    public static readonly Param Brep = Of<Brep>(static (a, n, c, i, ac, r) => a.AddSurface(name: n, code: c, info: i, access: ac, requirement: r), static (a, n, c, i, ac) => a.AddSurface(name: n, code: c, info: i, access: ac));
    public static readonly Param Mesh = Of<Mesh>(static (a, n, c, i, ac, r) => a.AddMesh(name: n, code: c, info: i, access: ac, requirement: r), static (a, n, c, i, ac) => a.AddMesh(name: n, code: c, info: i, access: ac));
    public static readonly Param Box = Of<Box>(static (a, n, c, i, ac, r) => a.AddBox(name: n, code: c, info: i, access: ac, requirement: r), static (a, n, c, i, ac) => a.AddBox(name: n, code: c, info: i, access: ac));
    public static readonly Param Plane = Of<Plane>(static (a, n, c, i, ac, r) => a.AddPlane(name: n, code: c, info: i, access: ac, requirement: r), static (a, n, c, i, ac) => a.AddPlane(name: n, code: c, info: i, access: ac));
    public static readonly Param Line = Of<Line>(static (a, n, c, i, ac, r) => a.AddLine(name: n, code: c, info: i, access: ac, requirement: r), static (a, n, c, i, ac) => a.AddLine(name: n, code: c, info: i, access: ac));
    public static readonly Param Circle = Of<Circle>(static (a, n, c, i, ac, r) => a.AddCircle(name: n, code: c, info: i, access: ac, requirement: r), static (a, n, c, i, ac) => a.AddCircle(name: n, code: c, info: i, access: ac));
    public static readonly Param Arc = Of<Arc>(static (a, n, c, i, ac, r) => a.AddArc(name: n, code: c, info: i, access: ac, requirement: r), static (a, n, c, i, ac) => a.AddArc(name: n, code: c, info: i, access: ac));
    public static readonly Param Sphere = Of<Sphere>(static (a, n, c, i, ac, r) => a.AddSphere(name: n, code: c, info: i, access: ac, requirement: r), static (a, n, c, i, ac) => a.AddSphere(name: n, code: c, info: i, access: ac));
    public static readonly Param Polyline = Of<Polyline>(static (a, n, c, i, ac, r) => a.AddPolyline(name: n, code: c, info: i, access: ac, requirement: r), static (a, n, c, i, ac) => a.AddPolyline(name: n, code: c, info: i, access: ac));
    public static readonly Param SubD = Of<SubD>(static (a, n, c, i, ac, r) => a.AddSurface(name: n, code: c, info: i, access: ac, requirement: r), static (a, n, c, i, ac) => a.AddSurface(name: n, code: c, info: i, access: ac));
    public static readonly Param Index = Of<int>(key: nameof(Index), onInput: static (a, n, c, i, ac, r) => a.AddIndex(name: n, code: c, info: i, access: ac, requirement: r), onOutput: static (a, n, c, i, ac) => a.AddInteger(name: n, code: c, info: i, access: ac));
    public static readonly Param Integer = Of<int>(static (a, n, c, i, ac, r) => a.AddInteger(name: n, code: c, info: i, access: ac, requirement: r), static (a, n, c, i, ac) => a.AddInteger(name: n, code: c, info: i, access: ac));
    public static readonly Param Number = Of<double>(static (a, n, c, i, ac, r) => a.AddNumber(name: n, code: c, info: i, access: ac, requirement: r), static (a, n, c, i, ac) => a.AddNumber(name: n, code: c, info: i, access: ac));
    public static readonly Param Text = Of<string>(static (a, n, c, i, ac, r) => a.AddText(name: n, code: c, info: i, access: ac, requirement: r), static (a, n, c, i, ac) => a.AddText(name: n, code: c, info: i, access: ac));
    public static readonly Param Boolean = Of<bool>(static (a, n, c, i, ac, r) => a.AddBoolean(name: n, code: c, info: i, access: ac, requirement: r), static (a, n, c, i, ac) => a.AddBoolean(name: n, code: c, info: i, access: ac));
    public static readonly Param Colour = Of<Color>(static (a, n, c, i, ac, r) => a.AddColour(name: n, code: c, info: i, access: ac, requirement: r), static (a, n, c, i, ac) => a.AddColour(name: n, code: c, info: i, access: ac));
    public static readonly Param Interval = Of<Interval>(static (a, n, c, i, ac, r) => a.AddInterval(name: n, code: c, info: i, access: ac, requirement: r), static (a, n, c, i, ac) => a.AddInterval(name: n, code: c, info: i, access: ac));
    public static readonly Param GeometryKind = Enum(initial: Rasm.Analysis.GeometryKind.Unknown);
    public static readonly Param CurveFeature = Enum(initial: Rasm.Analysis.CurveFeature.Input);
    public static readonly Param Angle = Of<Angle>(static (a, n, c, i, ac, r) => a.AddAngle(name: n, code: c, info: i, access: ac, requirement: r), static (a, n, c, i, ac) => a.AddAngle(name: n, code: c, info: i, access: ac));
    public static readonly Param Transform = Of<Transform>(static (a, n, c, i, ac, r) => a.AddTransform(name: n, code: c, info: i, access: ac, requirement: r), static (a, n, c, i, ac) => a.AddTransform(name: n, code: c, info: i, access: ac));
    public static readonly Param Generic = Of<object>(static (a, n, c, i, ac, r) => a.AddGeneric(name: n, code: c, info: i, access: ac, requirement: r), static (a, n, c, i, ac) => a.AddGeneric(name: n, code: c, info: i, access: ac));

    public Type Type { get; }
    private Action<InputAdder, string, string, string, Access, Requirement> OnInput { get; }
    private Action<OutputAdder, string, string, string, Access> OnOutput { get; }

    private static Param Of<T>(
        Action<InputAdder, string, string, string, Access, Requirement> onInput,
        Action<OutputAdder, string, string, string, Access> onOutput) =>
        new(key: typeof(T).Name, type: typeof(T), onInput: onInput, onOutput: onOutput);
    private static Param Of<T>(
        string key,
        Action<InputAdder, string, string, string, Access, Requirement> onInput,
        Action<OutputAdder, string, string, string, Access> onOutput) =>
        new(key: key, type: typeof(T), onInput: onInput, onOutput: onOutput);
    private static Param Enum<T>(T initial) where T : struct, Enum =>
        Of<T>(
            key: typeof(T).Name,
            onInput: (a, n, c, i, ac, r) => a.AddEnum<T>(name: n, code: c, info: i, initial: initial, access: ac, requirement: r),
            onOutput: static (a, n, c, i, ac) => a.AddEnum<T>(name: n, code: c, info: i, access: ac));

    // --- [OPERATIONS] ------------------------------------------------------------------

    public static Option<Param> From(Type type) =>
        type == typeof(int)
            ? Some(Integer)
            : toSeq(Items).Find(predicate: p => p.Type.Equals(o: type));
    public Unit Bind(InputAdder adder, string name, string code, string info, Access access, Requirement requirement) {
        ArgumentNullException.ThrowIfNull(argument: adder);
        OnInput(arg1: adder, arg2: name, arg3: code, arg4: info, arg5: access, arg6: requirement);
        return Unit.Default;
    }
    public Unit Bind(OutputAdder adder, string name, string code, string info, Access access) {
        ArgumentNullException.ThrowIfNull(argument: adder);
        OnOutput(arg1: adder, arg2: name, arg3: code, arg4: info, arg5: access);
        return Unit.Default;
    }
}
