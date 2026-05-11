using System.Drawing;
using Grasshopper2.Parameters;
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
public sealed record PortPolicy {
    private readonly Func<object, Unit> apply;
    private PortPolicy(Func<object, Unit> apply) => this.apply = apply;
    public static PortPolicy Empty { get; } = new(apply: static _ => Unit.Default);
    public static PortPolicy Vector(bool unitise = false, bool reverse = false) => new(apply: parameter => parameter switch {
        VectorParameter vector => fun((VectorParameter target) => { target.UnitiseVectors = unitise; target.ReverseVectors = reverse; return Unit.Default; })(vector),
        _ => Unit.Default,
    });
    public static PortPolicy Angle(int kind = 0, bool reduce = false) => new(apply: parameter => parameter switch {
        AngleParameter angle => fun((AngleParameter target) => { target.EnforceKind = kind; target.ReduceAngles = reduce; return Unit.Default; })(angle),
        _ => Unit.Default,
    });
    public static PortPolicy Curve(CurveParameter.NormalisationMethod domains = CurveParameter.NormalisationMethod.None, bool flip = false) => new(apply: parameter => parameter switch {
        CurveParameter curve => fun((CurveParameter target) => { target.NormaliseDomains = domains; target.FlipCurves = flip; return Unit.Default; })(curve),
        _ => Unit.Default,
    });
    public static PortPolicy Surface(bool acceptMeshes = false, CurveParameter.NormalisationMethod domains = CurveParameter.NormalisationMethod.None, bool flip = false) => new(apply: parameter => parameter switch {
        SurfaceParameter surface => fun((SurfaceParameter target) => { target.AcceptMeshes = acceptMeshes; target.NormaliseDomains = domains; target.FlipSurfaces = flip; return Unit.Default; })(surface),
        _ => Unit.Default,
    });
    public static PortPolicy Index(IndexModifier indexing = IndexModifier.Clip) => new(apply: parameter => parameter switch {
        IntegerParameter integer => fun((IntegerParameter target) => { target.IsIndex = true; target.Indexing = indexing; return Unit.Default; })(integer),
        _ => Unit.Default,
    });
    public Unit Apply(object parameter) {
        ArgumentNullException.ThrowIfNull(argument: parameter);
        return apply(arg: parameter);
    }
}
[SmartEnum<string>]
public sealed partial class PortKind {
    private delegate IParameter Input(InputAdder adder, string name, string code, string info, Access access, Requirement requirement);
    private delegate IParameter Output(OutputAdder adder, string name, string code, string info, Access access);
    public static readonly PortKind Point = Of<Point3d>(key: nameof(Point), input: static (adder, name, code, info, access, requirement) => adder.AddPoint(name: name, code: code, info: info, access: access, requirement: requirement), output: static (adder, name, code, info, access) => adder.AddPoint(name: name, code: code, info: info, access: access));
    public static readonly PortKind Vector = Of<Vector3d>(key: nameof(Vector), input: static (adder, name, code, info, access, requirement) => adder.AddVector(name: name, code: code, info: info, access: access, requirement: requirement), output: static (adder, name, code, info, access) => adder.AddVector(name: name, code: code, info: info, access: access));
    public static readonly PortKind Curve = Of<Curve>(key: nameof(Curve), input: static (adder, name, code, info, access, requirement) => adder.AddCurve(name: name, code: code, info: info, access: access, requirement: requirement), output: static (adder, name, code, info, access) => adder.AddCurve(name: name, code: code, info: info, access: access));
    public static readonly PortKind CurveLocus = Of<CurveLocus>(key: nameof(CurveLocus), input: static (adder, name, code, info, access, requirement) => adder.AddCurveLocus(name: name, code: code, info: info, access: access, requirement: requirement), output: static (adder, name, code, info, access) => adder.AddCurveLocus(name: name, code: code, info: info, access: access));
    public static readonly PortKind Brep = Of<Brep>(key: nameof(Brep), input: static (adder, name, code, info, access, requirement) => adder.AddSurface(name: name, code: code, info: info, access: access, requirement: requirement), output: static (adder, name, code, info, access) => adder.AddSurface(name: name, code: code, info: info, access: access));
    public static readonly PortKind SurfaceLocus = Of<SurfaceLocus>(key: nameof(SurfaceLocus), input: static (adder, name, code, info, access, requirement) => adder.AddSurfaceLocus(name: name, code: code, info: info, access: access, requirement: requirement), output: static (adder, name, code, info, access) => adder.AddSurfaceLocus(name: name, code: code, info: info, access: access));
    public static readonly PortKind Plane = Of<Plane>(key: nameof(Plane), input: static (adder, name, code, info, access, requirement) => adder.AddPlane(name: name, code: code, info: info, access: access, requirement: requirement), output: static (adder, name, code, info, access) => adder.AddPlane(name: name, code: code, info: info, access: access));
    public static readonly PortKind Index = Of<int>(key: nameof(Index), input: static (adder, name, code, info, access, requirement) => adder.AddIndex(name: name, code: code, info: info, access: access, requirement: requirement), output: static (adder, name, code, info, access) => { IntegerParameter parameter = adder.AddInteger(name: name, code: code, info: info, access: access); parameter.IsIndex = true; return parameter; });
    public static readonly PortKind Integer = Of<int>(key: nameof(Integer), input: static (adder, name, code, info, access, requirement) => adder.AddInteger(name: name, code: code, info: info, access: access, requirement: requirement), output: static (adder, name, code, info, access) => adder.AddInteger(name: name, code: code, info: info, access: access));
    public static readonly PortKind Number = Of<double>(key: nameof(Number), input: static (adder, name, code, info, access, requirement) => adder.AddNumber(name: name, code: code, info: info, access: access, requirement: requirement), output: static (adder, name, code, info, access) => adder.AddNumber(name: name, code: code, info: info, access: access));
    public static readonly PortKind Text = Of<string>(key: nameof(Text), input: static (adder, name, code, info, access, requirement) => adder.AddText(name: name, code: code, info: info, access: access, requirement: requirement), output: static (adder, name, code, info, access) => adder.AddText(name: name, code: code, info: info, access: access));
    public static readonly PortKind Boolean = Of<bool>(key: nameof(Boolean), input: static (adder, name, code, info, access, requirement) => adder.AddBoolean(name: name, code: code, info: info, access: access, requirement: requirement), output: static (adder, name, code, info, access) => adder.AddBoolean(name: name, code: code, info: info, access: access));
    public static readonly PortKind Colour = Of<Color>(key: nameof(Colour), input: static (adder, name, code, info, access, requirement) => adder.AddColour(name: name, code: code, info: info, access: access, requirement: requirement), output: static (adder, name, code, info, access) => adder.AddColour(name: name, code: code, info: info, access: access));
    public static readonly PortKind Interval = Of<Interval>(key: nameof(Interval), input: static (adder, name, code, info, access, requirement) => adder.AddInterval(name: name, code: code, info: info, access: access, requirement: requirement), output: static (adder, name, code, info, access) => adder.AddInterval(name: name, code: code, info: info, access: access));
    public static readonly PortKind Angle = Of<Angle>(key: nameof(Angle), input: static (adder, name, code, info, access, requirement) => adder.AddAngle(name: name, code: code, info: info, access: access, requirement: requirement), output: static (adder, name, code, info, access) => adder.AddAngle(name: name, code: code, info: info, access: access));
    public static readonly PortKind Transform = Of<Transform>(key: nameof(Transform), input: static (adder, name, code, info, access, requirement) => adder.AddTransform(name: name, code: code, info: info, access: access, requirement: requirement), output: static (adder, name, code, info, access) => adder.AddTransform(name: name, code: code, info: info, access: access));
    public static readonly PortKind Generic = Of<object>(key: nameof(Generic), input: static (adder, name, code, info, access, requirement) => adder.AddGeneric(name: name, code: code, info: info, access: access, requirement: requirement), output: static (adder, name, code, info, access) => adder.AddGeneric(name: name, code: code, info: info, access: access));
    public Type Type { get; }
    private Input AddInput { get; }
    private Output AddOutput { get; }
    public static PortKind Enum<T>(T initial) where T : struct, Enum =>
        Of<T>(
            key: typeof(T).Name,
            input: (adder, name, code, info, access, requirement) => adder.AddEnum(name: name, code: code, info: info, initial: initial, access: access, requirement: requirement),
            output: static (adder, name, code, info, access) => adder.AddEnum<T>(name: name, code: code, info: info, access: access));
    public static Option<PortKind> From(Type type) {
        ArgumentNullException.ThrowIfNull(argument: type);
        return type.Equals(o: typeof(int)) ? Some(Integer) : toSeq(Items).Find(kind => kind.Type.Equals(o: type));
    }
    public Unit Bind(InputAdder adder, string name, string code, string info, Access access, Requirement requirement, PortPolicy policy) {
        ArgumentNullException.ThrowIfNull(argument: adder);
        ArgumentNullException.ThrowIfNull(argument: policy);
        IParameter parameter = AddInput(adder: adder, name: name, code: code, info: info, access: access, requirement: requirement);
        return policy.Apply(parameter: parameter);
    }
    public Unit Bind(OutputAdder adder, string name, string code, string info, Access access, PortPolicy policy) {
        ArgumentNullException.ThrowIfNull(argument: adder);
        ArgumentNullException.ThrowIfNull(argument: policy);
        IParameter parameter = AddOutput(adder: adder, name: name, code: code, info: info, access: access);
        return policy.Apply(parameter: parameter);
    }
    private static PortKind Of<T>(string key, Input input, Output output) => new(key: key, type: typeof(T), addInput: input, addOutput: output);
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
