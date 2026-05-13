using Eto.Drawing;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.Components;
using Grasshopper2.Doc;
using Grasshopper2.Parameters;
using Requirement = Grasshopper2.Parameters.Requirement;

namespace Rasm.Grasshopper;

// --- [TYPES] ----------------------------------------------------------------------------
public interface IPort {
    public string Name { get; }
    public string Code { get; }
    public string Info { get; }
    public PortKind Kind { get; }
    public Access Access { get; }
    public Requirement Requirement { get; }
    public PortPolicy Policy { get; }
    public Option<object> FallbackValue { get; }
}

// --- [MODELS] ---------------------------------------------------------------------------
public sealed record PortPolicy {
    private readonly Func<object, Unit> apply;
    private PortPolicy(Func<object, Unit> apply) => this.apply = apply;
    public static PortPolicy Empty { get; } = new(apply: static _ => Unit.Default);
    public static PortPolicy Vector(bool unitise = false, bool reverse = false) =>
        On<VectorParameter>(mutate: target => { target.UnitiseVectors = unitise; target.ReverseVectors = reverse; });
    public static PortPolicy Angle(int kind = 0, bool reduce = false) =>
        On<AngleParameter>(mutate: target => { target.EnforceKind = kind; target.ReduceAngles = reduce; });
    public static PortPolicy Index(IndexModifier indexing = IndexModifier.Clip) =>
        On<IntegerParameter>(mutate: target => { target.IsIndex = true; target.Indexing = indexing; });
    public static PortPolicy Category(string name) =>
        On<IParameter>(mutate: parameter => SetCustom(parameter: parameter, key: ModularComponent.__Category, set: (kv, k) => kv.Set(key: k, value: name)));
    public static PortPolicy Colour(Color color) =>
        On<IParameter>(mutate: parameter => SetCustom(parameter: parameter, key: ModularComponent.__Colour, set: (kv, k) => kv.Set(key: k, value: color)));
    public static PortPolicy Hidden { get; } = On<IParameter>(mutate: parameter => Seq(ModularComponent.__Optional, ModularComponent.__HideByDefault)
        .Iter(key => SetCustom(parameter: parameter, key: key, set: (kv, k) => kv.Set(key: k, value: true))));
    public static PortPolicy Compose(params PortPolicy[] policies) {
        ArgumentNullException.ThrowIfNull(argument: policies);
        return new PortPolicy(apply: parameter => toSeq(policies).Iter(policy => policy.Apply(parameter: parameter)));
    }
    public Unit Apply(object parameter) {
        ArgumentNullException.ThrowIfNull(argument: parameter);
        return apply(arg: parameter);
    }
    public static PortPolicy On<TParam>(Action<TParam> mutate) where TParam : class =>
        new(apply: parameter => parameter switch {
            TParam target => fun((TParam t) => { mutate(obj: t); return Unit.Default; })(target),
            _ => Unit.Default,
        });
    private static Unit SetCustom(IParameter parameter, string key, Action<KeyedValues, string> set) {
        set(arg1: parameter.CustomValues, arg2: key);
        return Unit.Default;
    }
}
public readonly record struct Port<TVal>(
    string Name,
    string Code,
    string Info,
    PortKind Kind,
    Access Access,
    Requirement Requirement,
    PortPolicy Policy,
    Option<TVal> Fallback) : IPort {
    public Option<object> FallbackValue => Fallback.Map(static value => (object)value!);
}

// --- [CONSTANTS] ------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class PortKind {
    private delegate IParameter Input(InputAdder adder, string name, string code, string info, Access access, Requirement requirement);
    private delegate IParameter Output(OutputAdder adder, string name, string code, string info, Access access);
    public static readonly PortKind Point = Of<Point3d>(key: nameof(Point), input: static (adder, name, code, info, access, requirement) => adder.AddPoint(name: name, code: code, info: info, access: access, requirement: requirement), output: static (adder, name, code, info, access) => adder.AddPoint(name: name, code: code, info: info, access: access));
    public static readonly PortKind Vector = Of<Vector3d>(key: nameof(Vector), input: static (adder, name, code, info, access, requirement) => adder.AddVector(name: name, code: code, info: info, access: access, requirement: requirement), output: static (adder, name, code, info, access) => adder.AddVector(name: name, code: code, info: info, access: access));
    public static readonly PortKind Curve = Of<Curve>(key: nameof(Curve), input: static (adder, name, code, info, access, requirement) => adder.AddCurve(name: name, code: code, info: info, access: access, requirement: requirement), output: static (adder, name, code, info, access) => adder.AddCurve(name: name, code: code, info: info, access: access));
    public static readonly PortKind Brep = Of<Brep>(key: nameof(Brep), input: static (adder, name, code, info, access, requirement) => adder.AddSurface(name: name, code: code, info: info, access: access, requirement: requirement), output: static (adder, name, code, info, access) => adder.AddSurface(name: name, code: code, info: info, access: access));
    public static readonly PortKind Plane = Of<Plane>(key: nameof(Plane), input: static (adder, name, code, info, access, requirement) => adder.AddPlane(name: name, code: code, info: info, access: access, requirement: requirement), output: static (adder, name, code, info, access) => adder.AddPlane(name: name, code: code, info: info, access: access));
    public static readonly PortKind Index = Of<int>(key: nameof(Index), input: static (adder, name, code, info, access, requirement) => adder.AddIndex(name: name, code: code, info: info, access: access, requirement: requirement), output: static (adder, name, code, info, access) => { IntegerParameter parameter = adder.AddInteger(name: name, code: code, info: info, access: access); parameter.IsIndex = true; return parameter; });
    public static readonly PortKind Integer = Of<int>(key: nameof(Integer), input: static (adder, name, code, info, access, requirement) => adder.AddInteger(name: name, code: code, info: info, access: access, requirement: requirement), output: static (adder, name, code, info, access) => adder.AddInteger(name: name, code: code, info: info, access: access));
    public static readonly PortKind Interval = Of<Interval>(key: nameof(Interval), input: static (adder, name, code, info, access, requirement) => adder.AddInterval(name: name, code: code, info: info, access: access, requirement: requirement), output: static (adder, name, code, info, access) => adder.AddInterval(name: name, code: code, info: info, access: access));
    public static readonly PortKind Angle = Of<Angle>(key: nameof(Angle), input: static (adder, name, code, info, access, requirement) => adder.AddAngle(name: name, code: code, info: info, access: access, requirement: requirement), output: static (adder, name, code, info, access) => adder.AddAngle(name: name, code: code, info: info, access: access));
    public static readonly PortKind Number = Of<double>(key: nameof(Number), input: static (adder, name, code, info, access, requirement) => adder.AddNumber(name: name, code: code, info: info, access: access, requirement: requirement), output: static (adder, name, code, info, access) => adder.AddNumber(name: name, code: code, info: info, access: access));
    public static readonly PortKind Boolean = Of<bool>(key: nameof(Boolean), input: static (adder, name, code, info, access, requirement) => adder.AddBoolean(name: name, code: code, info: info, access: access, requirement: requirement), output: static (adder, name, code, info, access) => adder.AddBoolean(name: name, code: code, info: info, access: access));
    public static readonly PortKind Text = Of<string>(key: nameof(Text), input: static (adder, name, code, info, access, requirement) => adder.AddText(name: name, code: code, info: info, access: access, requirement: requirement), output: static (adder, name, code, info, access) => adder.AddText(name: name, code: code, info: info, access: access));
    public static readonly PortKind Mesh = Of<Mesh>(key: nameof(Mesh), input: static (adder, name, code, info, access, requirement) => adder.AddMesh(name: name, code: code, info: info, access: access, requirement: requirement), output: static (adder, name, code, info, access) => adder.AddMesh(name: name, code: code, info: info, access: access));
    public static readonly PortKind Generic = Of<object>(key: nameof(Generic), input: static (adder, name, code, info, access, requirement) => adder.AddGeneric(name: name, code: code, info: info, access: access, requirement: requirement), output: static (adder, name, code, info, access) => adder.AddGeneric(name: name, code: code, info: info, access: access));
    public Type Type { get; }
    [UseDelegateFromConstructor] private partial IParameter AddInput(InputAdder adder, string name, string code, string info, Access access, Requirement requirement);
    [UseDelegateFromConstructor] private partial IParameter AddOutput(OutputAdder adder, string name, string code, string info, Access access);
    public static PortKind Enum<T>(T initial) where T : struct, Enum =>
        Of<T>(
            key: typeof(T).Name,
            input: (adder, name, code, info, access, requirement) => adder.AddEnum(name: name, code: code, info: info, initial: initial, access: access, requirement: requirement),
            output: static (adder, name, code, info, access) => adder.AddEnum<T>(name: name, code: code, info: info, access: access));
    public static Option<PortKind> From(Type type) {
        ArgumentNullException.ThrowIfNull(argument: type);
        return type == typeof(Shape)
            ? Some(Generic)
            : toSeq(Items).Find(predicate: kind => kind.Type == type);
    }
    public Unit Bind(InputAdder adder, string name, string code, string info, Access access, Requirement requirement, PortPolicy policy, bool hidden) {
        ArgumentNullException.ThrowIfNull(argument: adder);
        ArgumentNullException.ThrowIfNull(argument: policy);
        IParameter parameter = AddInput(adder: adder, name: name, code: code, info: info, access: access, requirement: requirement);
        _ = policy.Apply(parameter: parameter);
        return hidden ? PortPolicy.Hidden.Apply(parameter: parameter) : Unit.Default;
    }
    public Unit Bind(OutputAdder adder, string name, string code, string info, Access access, PortPolicy policy, bool hidden) {
        ArgumentNullException.ThrowIfNull(argument: adder);
        ArgumentNullException.ThrowIfNull(argument: policy);
        IParameter parameter = AddOutput(adder: adder, name: name, code: code, info: info, access: access);
        _ = policy.Apply(parameter: parameter);
        return hidden ? PortPolicy.Hidden.Apply(parameter: parameter) : Unit.Default;
    }
    private static PortKind Of<T>(
        string key,
        Func<InputAdder, string, string, string, Access, Requirement, IParameter> input,
        Func<OutputAdder, string, string, string, Access, IParameter> output) =>
        new(key: key, type: typeof(T), addInput: input, addOutput: output);
}

// --- [SERVICES] -------------------------------------------------------------------------
public static class Port {
    public static Port<TVal> Required<TVal>(string name, string code, string info, PortKind? kind = null, PortPolicy? policy = null) =>
        Of<TVal>(name: name, code: code, info: info, kind: kind, access: Access.Item, requirement: Requirement.MustExist, policy: policy, fallback: Option<TVal>.None);
    public static Port<TVal> Optional<TVal>(string name, string code, string info, PortKind? kind = null, PortPolicy? policy = null, string category = "Optional", Option<TVal> fallback = default) =>
        Of<TVal>(
            name: name, code: code, info: info, kind: kind,
            access: Access.Item, requirement: Requirement.MayBeMissing,
            policy: PortPolicy.Compose(policy ?? DefaultPolicy(type: typeof(TVal)), PortPolicy.Category(name: category)),
            fallback: fallback);
    public static Port<TVal> List<TVal>(string name, string code, string info, Requirement requirement = Requirement.MustExist, PortKind? kind = null, PortPolicy? policy = null) =>
        Of<TVal>(name: name, code: code, info: info, kind: kind, access: Access.Twig, requirement: requirement, policy: policy, fallback: Option<TVal>.None);
    public static Port<int> Index(
        string name = "Index",
        string code = "I",
        string info = "Zero-based selector; clamped to [0, count-1].") =>
        Optional<int>(name: name, code: code, info: info, kind: PortKind.Index, policy: PortPolicy.Index());
    public static Port<Vector3d> Direction(
        string name = "Direction",
        string code = "D",
        string info = "Direction vector; missing Direction uses world Z.") =>
        Optional<Vector3d>(name: name, code: code, info: info, fallback: Some(Vector3d.ZAxis));
    public static Port<Shape> Shape(
        string name = "Geometry",
        string code = "G",
        string info = "Geometry to analyse.") =>
        Of<Shape>(name: name, code: code, info: info, kind: null, access: Access.Item, requirement: Requirement.MustExist, policy: null, fallback: Option<Shape>.None);
    private static Port<TVal> Of<TVal>(string name, string code, string info, PortKind? kind, Access access, Requirement requirement, PortPolicy? policy, Option<TVal> fallback) =>
        new(Name: name, Code: code, Info: info,
            Kind: kind ?? PortKind.From(type: typeof(TVal)).IfNone(PortKind.Generic),
            Access: access, Requirement: requirement,
            Policy: policy ?? DefaultPolicy(type: typeof(TVal)),
            Fallback: fallback);
    private static PortPolicy DefaultPolicy(Type type) => type switch {
        _ when type == typeof(Vector3d) => PortPolicy.Vector(unitise: true),
        _ when type == typeof(Angle) => PortPolicy.Angle(reduce: true),
        _ => PortPolicy.Empty,
    };
}
