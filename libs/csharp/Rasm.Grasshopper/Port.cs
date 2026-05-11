using System.Drawing;
using Requirement = Grasshopper2.Parameters.Requirement;

namespace Rasm.Grasshopper;

public interface IPort {
    public string Name { get; }
    public string Code { get; }
    public string Info { get; }
    public Type Type { get; }
    public PortKind Kind { get; }
    public Access Access { get; }
    public Requirement Requirement { get; }
    public PortPolicy Policy { get; }
}
public readonly record struct PortPolicy(
    bool UnitiseVectors,
    bool ReverseVectors,
    int AngleKind,
    bool ReduceAngles,
    CurveParameter.NormalisationMethod CurveDomains,
    bool FlipCurves,
    bool AcceptMeshes,
    CurveParameter.NormalisationMethod SurfaceDomains,
    bool FlipSurfaces,
    bool IsIndex,
    IndexModifier Indexing) {
    public static PortPolicy Empty =>
        new(
            UnitiseVectors: false,
            ReverseVectors: false,
            AngleKind: 0,
            ReduceAngles: false,
            CurveDomains: CurveParameter.NormalisationMethod.None,
            FlipCurves: false,
            AcceptMeshes: false,
            SurfaceDomains: CurveParameter.NormalisationMethod.None,
            FlipSurfaces: false,
            IsIndex: false,
            Indexing: IndexModifier.None);
    public static PortPolicy Vector(bool unitise = false, bool reverse = false) =>
        Empty with { UnitiseVectors = unitise, ReverseVectors = reverse };
    public static PortPolicy Angle(int kind = 0, bool reduce = false) =>
        Empty with { AngleKind = kind, ReduceAngles = reduce };
    public static PortPolicy Curve(CurveParameter.NormalisationMethod domains = CurveParameter.NormalisationMethod.None, bool flip = false) =>
        Empty with { CurveDomains = domains, FlipCurves = flip };
    public static PortPolicy Surface(bool acceptMeshes = false, CurveParameter.NormalisationMethod domains = CurveParameter.NormalisationMethod.None, bool flip = false) =>
        Empty with { AcceptMeshes = acceptMeshes, SurfaceDomains = domains, FlipSurfaces = flip };
    public static PortPolicy Index(IndexModifier indexing = IndexModifier.Clip) =>
        Empty with { IsIndex = true, Indexing = indexing };
    public Unit Apply(object parameter) {
        ArgumentNullException.ThrowIfNull(argument: parameter);
        // BOUNDARY ADAPTER — GH2 parameter policies are mutable SDK configuration.
        switch (parameter) {
            case VectorParameter vector:
                vector.UnitiseVectors = UnitiseVectors;
                vector.ReverseVectors = ReverseVectors;
                break;
            case AngleParameter angle:
                angle.EnforceKind = AngleKind;
                angle.ReduceAngles = ReduceAngles;
                break;
            case CurveParameter curve:
                curve.NormaliseDomains = CurveDomains;
                curve.FlipCurves = FlipCurves;
                break;
            case SurfaceParameter surface:
                surface.AcceptMeshes = AcceptMeshes;
                surface.NormaliseDomains = SurfaceDomains;
                surface.FlipSurfaces = FlipSurfaces;
                break;
            case IntegerParameter integer:
                integer.IsIndex = IsIndex;
                integer.Indexing = Indexing;
                break;
        }
        return Unit.Default;
    }
}
[SmartEnum<string>]
public sealed partial class PortKind {
    public static readonly PortKind Point = Of<Point3d>(key: nameof(Point), onInput: static (adder, name, code, info, access, requirement) => adder.AddPoint(name: name, code: code, info: info, access: access, requirement: requirement), onOutput: static (adder, name, code, info, access) => adder.AddPoint(name: name, code: code, info: info, access: access));
    public static readonly PortKind Vector = Of<Vector3d>(key: nameof(Vector), onInput: static (adder, name, code, info, access, requirement) => adder.AddVector(name: name, code: code, info: info, access: access, requirement: requirement), onOutput: static (adder, name, code, info, access) => adder.AddVector(name: name, code: code, info: info, access: access));
    public static readonly PortKind Curve = Of<Curve>(key: nameof(Curve), onInput: static (adder, name, code, info, access, requirement) => adder.AddCurve(name: name, code: code, info: info, access: access, requirement: requirement), onOutput: static (adder, name, code, info, access) => adder.AddCurve(name: name, code: code, info: info, access: access));
    public static readonly PortKind CurveLocus = Of<CurveLocus>(key: nameof(CurveLocus), onInput: static (adder, name, code, info, access, requirement) => adder.AddCurveLocus(name: name, code: code, info: info, access: access, requirement: requirement), onOutput: static (adder, name, code, info, access) => adder.AddCurveLocus(name: name, code: code, info: info, access: access));
    public static readonly PortKind Brep = Of<Brep>(key: nameof(Brep), onInput: static (adder, name, code, info, access, requirement) => adder.AddSurface(name: name, code: code, info: info, access: access, requirement: requirement), onOutput: static (adder, name, code, info, access) => adder.AddSurface(name: name, code: code, info: info, access: access));
    public static readonly PortKind SurfaceLocus = Of<SurfaceLocus>(key: nameof(SurfaceLocus), onInput: static (adder, name, code, info, access, requirement) => adder.AddSurfaceLocus(name: name, code: code, info: info, access: access, requirement: requirement), onOutput: static (adder, name, code, info, access) => adder.AddSurfaceLocus(name: name, code: code, info: info, access: access));
    public static readonly PortKind Plane = Of<Plane>(key: nameof(Plane), onInput: static (adder, name, code, info, access, requirement) => adder.AddPlane(name: name, code: code, info: info, access: access, requirement: requirement), onOutput: static (adder, name, code, info, access) => adder.AddPlane(name: name, code: code, info: info, access: access));
    public static readonly PortKind Index = Of<int>(key: nameof(Index), onInput: static (adder, name, code, info, access, requirement) => adder.AddIndex(name: name, code: code, info: info, access: access, requirement: requirement), onOutput: static (adder, name, code, info, access) => adder.AddInteger(name: name, code: code, info: info, access: access));
    public static readonly PortKind Integer = Of<int>(key: nameof(Integer), onInput: static (adder, name, code, info, access, requirement) => adder.AddInteger(name: name, code: code, info: info, access: access, requirement: requirement), onOutput: static (adder, name, code, info, access) => adder.AddInteger(name: name, code: code, info: info, access: access));
    public static readonly PortKind Number = Of<double>(key: nameof(Number), onInput: static (adder, name, code, info, access, requirement) => adder.AddNumber(name: name, code: code, info: info, access: access, requirement: requirement), onOutput: static (adder, name, code, info, access) => adder.AddNumber(name: name, code: code, info: info, access: access));
    public static readonly PortKind Text = Of<string>(key: nameof(Text), onInput: static (adder, name, code, info, access, requirement) => adder.AddText(name: name, code: code, info: info, access: access, requirement: requirement), onOutput: static (adder, name, code, info, access) => adder.AddText(name: name, code: code, info: info, access: access));
    public static readonly PortKind Boolean = Of<bool>(key: nameof(Boolean), onInput: static (adder, name, code, info, access, requirement) => adder.AddBoolean(name: name, code: code, info: info, access: access, requirement: requirement), onOutput: static (adder, name, code, info, access) => adder.AddBoolean(name: name, code: code, info: info, access: access));
    public static readonly PortKind Colour = Of<Color>(key: nameof(Colour), onInput: static (adder, name, code, info, access, requirement) => adder.AddColour(name: name, code: code, info: info, access: access, requirement: requirement), onOutput: static (adder, name, code, info, access) => adder.AddColour(name: name, code: code, info: info, access: access));
    public static readonly PortKind Interval = Of<Interval>(key: nameof(Interval), onInput: static (adder, name, code, info, access, requirement) => adder.AddInterval(name: name, code: code, info: info, access: access, requirement: requirement), onOutput: static (adder, name, code, info, access) => adder.AddInterval(name: name, code: code, info: info, access: access));
    public static readonly PortKind Angle = Of<Angle>(key: nameof(Angle), onInput: static (adder, name, code, info, access, requirement) => adder.AddAngle(name: name, code: code, info: info, access: access, requirement: requirement), onOutput: static (adder, name, code, info, access) => adder.AddAngle(name: name, code: code, info: info, access: access));
    public static readonly PortKind Transform = Of<Transform>(key: nameof(Transform), onInput: static (adder, name, code, info, access, requirement) => adder.AddTransform(name: name, code: code, info: info, access: access, requirement: requirement), onOutput: static (adder, name, code, info, access) => adder.AddTransform(name: name, code: code, info: info, access: access));
    public static readonly PortKind Generic = Of<object>(key: nameof(Generic), onInput: static (adder, name, code, info, access, requirement) => adder.AddGeneric(name: name, code: code, info: info, access: access, requirement: requirement), onOutput: static (adder, name, code, info, access) => adder.AddGeneric(name: name, code: code, info: info, access: access));
    public Type Type { get; }
    private Func<InputAdder, string, string, string, Access, Requirement, object> AddInput { get; }
    private Func<OutputAdder, string, string, string, Access, object> AddOutput { get; }
    public static PortKind Enum<T>(T initial) where T : struct, Enum =>
        Of<T>(
            key: typeof(T).Name,
            onInput: (adder, name, code, info, access, requirement) => adder.AddEnum<T>(name: name, code: code, info: info, initial: initial, access: access, requirement: requirement),
            onOutput: static (adder, name, code, info, access) => adder.AddEnum<T>(name: name, code: code, info: info, access: access));
    public static Option<PortKind> From(Type type) {
        ArgumentNullException.ThrowIfNull(argument: type);
        return type.Equals(o: typeof(int)) ? Some(Integer) : toSeq(Items).Find(kind => kind.Type.Equals(o: type));
    }
    public Unit Bind(InputAdder adder, string name, string code, string info, Access access, Requirement requirement, PortPolicy policy) {
        ArgumentNullException.ThrowIfNull(argument: adder);
        return policy.Apply(parameter: AddInput(arg1: adder, arg2: name, arg3: code, arg4: info, arg5: access, arg6: requirement));
    }
    public Unit Bind(OutputAdder adder, string name, string code, string info, Access access, PortPolicy policy) {
        ArgumentNullException.ThrowIfNull(argument: adder);
        return policy.Apply(parameter: AddOutput(arg1: adder, arg2: name, arg3: code, arg4: info, arg5: access));
    }
    private static PortKind Of<T>(string key, Func<InputAdder, string, string, string, Access, Requirement, object> onInput, Func<OutputAdder, string, string, string, Access, object> onOutput) => new(key: key, type: typeof(T), addInput: onInput, addOutput: onOutput);
}
public readonly record struct PortValue<TVal>(
    TVal Value,
    MetaData Meta,
    bool IsNull,
    Option<int> Index,
    Coverage Coverage);
public readonly record struct PortData<TVal>(
    Access Access,
    Seq<PortValue<TVal>> Values,
    Option<Twig<TVal>> Twig,
    Option<Tree<TVal>> Tree,
    Coverage Coverage,
    bool Changed) {
    public Option<TVal> Value => Values.Find(static value => !value.IsNull).Map(static value => value.Value);
    public Seq<TVal> NonNullValues => Values.Filter(static value => !value.IsNull).Map(static value => value.Value);
}
public readonly record struct Port<TVal>(
    string Name,
    string Code,
    string Info,
    PortKind Kind,
    Access Access,
    Requirement Requirement,
    PortPolicy Policy) : IPort {
    public Type Type => typeof(TVal);
}
public static class Port {
    public static Port<TVal> Required<TVal>(string name, string code, string info, PortKind? kind = null, PortPolicy? policy = null) => Create<TVal>(name: name, code: code, info: info, kind: kind, access: Access.Item, requirement: Requirement.MustExist, policy: policy);
    public static Port<TVal> Optional<TVal>(string name, string code, string info, PortKind? kind = null, PortPolicy? policy = null) => Create<TVal>(name: name, code: code, info: info, kind: kind, access: Access.Item, requirement: Requirement.MayBeMissing, policy: policy);
    public static Port<TVal> List<TVal>(string name, string code, string info, Requirement requirement = Requirement.MustExist, PortKind? kind = null, PortPolicy? policy = null) => Create<TVal>(name: name, code: code, info: info, kind: kind, access: Access.Twig, requirement: requirement, policy: policy);
    public static Port<TVal> Tree<TVal>(string name, string code, string info, Requirement requirement = Requirement.MustExist, PortKind? kind = null, PortPolicy? policy = null) => Create<TVal>(name: name, code: code, info: info, kind: kind, access: Access.Tree, requirement: requirement, policy: policy);
    public static Port<int> Index(
        string name = "Index",
        string code = "I",
        string info = "Zero-based selector; clamped to [0, count-1].") => new(Name: name, Code: code, Info: info, Kind: PortKind.Index, Access: Access.Item, Requirement: Requirement.MayBeMissing, Policy: PortPolicy.Index());
    private static Port<TVal> Create<TVal>(string name, string code, string info, PortKind? kind, Access access, Requirement requirement, PortPolicy? policy) => new(Name: name, Code: code, Info: info, Kind: kind ?? PortKind.From(type: typeof(TVal)).IfNone(PortKind.Generic), Access: access, Requirement: requirement, Policy: policy ?? PortPolicy.Empty);
}
