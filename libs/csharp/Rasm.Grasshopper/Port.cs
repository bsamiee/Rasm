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
    public Type ValueType { get; }
}

// --- [MODELS] ---------------------------------------------------------------------------
public sealed record PortPolicy {
    private readonly Func<object, Unit> apply;
    private PortPolicy(Func<object, Unit> apply) => this.apply = apply;
    public static PortPolicy Empty { get; } = new(apply: static _ => Unit.Default);
    public static PortPolicy Vector(bool unitise = false, bool reverse = false) =>
        On<VectorParameter>(mutate: target => { target.UnitiseVectors = unitise; target.ReverseVectors = reverse; return Unit.Default; });
    public static PortPolicy Angle(int kind = 0, bool reduce = false) =>
        On<AngleParameter>(mutate: target => { target.EnforceKind = kind; target.ReduceAngles = reduce; return Unit.Default; });
    public static PortPolicy Category(string name) =>
        On<IParameter>(mutate: parameter => SetCustom(parameter: parameter, key: ModularComponent.__Category, set: (kv, k) => kv.Set(key: k, value: name)));
    public static PortPolicy Colour(Color color) =>
        On<IParameter>(mutate: parameter => SetCustom(parameter: parameter, key: ModularComponent.__Colour, set: (kv, k) => kv.Set(key: k, value: color)));
    public static PortPolicy Optional { get; } =
        On<IParameter>(mutate: parameter => SetCustom(parameter: parameter, key: ModularComponent.__Optional, set: (kv, k) => kv.Set(key: k, value: true)));
    public static PortPolicy Hidden { get; } = Compose(Optional,
        On<IParameter>(mutate: parameter => SetCustom(parameter: parameter, key: ModularComponent.__HideByDefault, set: (kv, k) => kv.Set(key: k, value: true))));
    public static PortPolicy Compose(params PortPolicy[] policies) {
        ArgumentNullException.ThrowIfNull(argument: policies);
        return new PortPolicy(apply: parameter => toSeq(policies).Iter(policy => policy.Apply(parameter: parameter)));
    }
    public Unit Apply(object parameter) {
        ArgumentNullException.ThrowIfNull(argument: parameter);
        return apply(arg: parameter);
    }
    public static PortPolicy On<TParam>(Func<TParam, Unit> mutate) where TParam : class =>
        new(apply: parameter => parameter switch {
            TParam target => mutate(arg: target),
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
    public Type ValueType => typeof(TVal);
}

// --- [CONSTANTS] ------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class PortKind {
    private const string Category = "";
    private delegate IParameter Input(ModularInputAdder adder, string name, string code, string info, Access access, Requirement requirement, bool hidden);
    private delegate IParameter Output(ModularOutputAdder adder, string name, string code, string info, Access access, bool hidden);
    private delegate IParameter RegularInput(InputAdder adder, string name, string code, string info, Access access, Requirement requirement);
    private delegate IParameter RegularOutput(OutputAdder adder, string name, string code, string info, Access access);
    public static readonly PortKind Point = Of<Point3d>(key: nameof(Point),
        regularInput: static (adder, name, code, info, access, requirement) => adder.AddPoint(name: name, code: code, info: info, access: access, requirement: requirement),
        modularInput: static (adder, name, code, info, access, requirement, hidden) => hidden ? adder.AddHiddenPoint(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access, requirement: requirement) : adder.AddPoint(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access, requirement: requirement),
        regularOutput: static (adder, name, code, info, access) => adder.AddPoint(name: name, code: code, info: info, access: access),
        modularOutput: static (adder, name, code, info, access, hidden) => hidden ? adder.AddHiddenPoint(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access) : adder.AddPoint(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access));
    public static readonly PortKind Vector = Of<Vector3d>(key: nameof(Vector),
        regularInput: static (adder, name, code, info, access, requirement) => adder.AddVector(name: name, code: code, info: info, access: access, requirement: requirement),
        modularInput: static (adder, name, code, info, access, requirement, hidden) => hidden ? adder.AddHiddenVector(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access, requirement: requirement) : adder.AddVector(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access, requirement: requirement),
        regularOutput: static (adder, name, code, info, access) => adder.AddVector(name: name, code: code, info: info, access: access),
        modularOutput: static (adder, name, code, info, access, hidden) => hidden ? adder.AddHiddenVector(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access) : adder.AddVector(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access));
    public static readonly PortKind Curve = Of<Curve>(key: nameof(Curve),
        regularInput: static (adder, name, code, info, access, requirement) => adder.AddCurve(name: name, code: code, info: info, access: access, requirement: requirement),
        modularInput: static (adder, name, code, info, access, requirement, hidden) => hidden ? adder.AddHiddenCurve(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access, requirement: requirement) : adder.AddCurve(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access, requirement: requirement),
        regularOutput: static (adder, name, code, info, access) => adder.AddCurve(name: name, code: code, info: info, access: access),
        modularOutput: static (adder, name, code, info, access, hidden) => hidden ? adder.AddHiddenCurve(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access) : adder.AddCurve(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access));
    public static readonly PortKind Brep = Of<Brep>(key: nameof(Brep),
        regularInput: static (adder, name, code, info, access, requirement) => adder.AddSurface(name: name, code: code, info: info, access: access, requirement: requirement),
        modularInput: static (adder, name, code, info, access, requirement, hidden) => hidden ? adder.AddHiddenSurface(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access, requirement: requirement) : adder.AddSurface(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access, requirement: requirement),
        regularOutput: static (adder, name, code, info, access) => adder.AddSurface(name: name, code: code, info: info, access: access),
        modularOutput: static (adder, name, code, info, access, hidden) => hidden ? adder.AddHiddenSurface(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access) : adder.AddSurface(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access));
    public static readonly PortKind Plane = Of<Plane>(key: nameof(Plane),
        regularInput: static (adder, name, code, info, access, requirement) => adder.AddPlane(name: name, code: code, info: info, access: access, requirement: requirement),
        modularInput: static (adder, name, code, info, access, requirement, hidden) => hidden ? adder.AddHiddenPlane(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access, requirement: requirement) : adder.AddPlane(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access, requirement: requirement),
        regularOutput: static (adder, name, code, info, access) => adder.AddPlane(name: name, code: code, info: info, access: access),
        modularOutput: static (adder, name, code, info, access, hidden) => hidden ? adder.AddHiddenPlane(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access) : adder.AddPlane(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access));
    public static readonly PortKind Integer = Of<int>(key: nameof(Integer),
        regularInput: static (adder, name, code, info, access, requirement) => adder.AddInteger(name: name, code: code, info: info, access: access, requirement: requirement),
        modularInput: static (adder, name, code, info, access, requirement, hidden) => hidden ? adder.AddHiddenInteger(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access, requirement: requirement) : adder.AddInteger(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access, requirement: requirement),
        regularOutput: static (adder, name, code, info, access) => adder.AddInteger(name: name, code: code, info: info, access: access),
        modularOutput: static (adder, name, code, info, access, hidden) => hidden ? adder.AddHiddenInteger(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access) : adder.AddInteger(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access));
    public static readonly PortKind Index = Of<int>(key: nameof(Index),
        regularInput: static (adder, name, code, info, access, requirement) => adder.AddIndex(name: name, code: code, info: info, access: access, requirement: requirement) switch {
            IntegerParameter parameter => ConfigureIndex(parameter: parameter),
        },
        modularInput: static (adder, name, code, info, access, requirement, hidden) => adder.RegularAdder.AddIndex(name: name, code: code, info: info, access: access, requirement: requirement) switch {
            IntegerParameter parameter => (hidden switch { true => PortPolicy.Hidden, false => PortPolicy.Optional }).Apply(parameter: ConfigureIndex(parameter: parameter)) switch { _ => parameter },
        },
        regularOutput: static (adder, name, code, info, access) => adder.AddInteger(name: name, code: code, info: info, access: access),
        modularOutput: static (adder, name, code, info, access, hidden) => hidden ? adder.AddHiddenInteger(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access) : adder.AddInteger(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access));
    public static readonly PortKind Interval = Of<Interval>(key: nameof(Interval),
        regularInput: static (adder, name, code, info, access, requirement) => adder.AddInterval(name: name, code: code, info: info, access: access, requirement: requirement),
        modularInput: static (adder, name, code, info, access, requirement, hidden) => hidden ? adder.AddHiddenInterval(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access, requirement: requirement) : adder.AddInterval(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access, requirement: requirement),
        regularOutput: static (adder, name, code, info, access) => adder.AddInterval(name: name, code: code, info: info, access: access),
        modularOutput: static (adder, name, code, info, access, hidden) => hidden ? adder.AddHiddenInterval(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access) : adder.AddInterval(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access));
    public static readonly PortKind Angle = Of<Angle>(key: nameof(Angle),
        regularInput: static (adder, name, code, info, access, requirement) => adder.AddAngle(name: name, code: code, info: info, access: access, requirement: requirement),
        modularInput: static (adder, name, code, info, access, requirement, hidden) => hidden ? adder.AddHiddenAngle(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access, requirement: requirement) : adder.AddAngle(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access, requirement: requirement),
        regularOutput: static (adder, name, code, info, access) => adder.AddAngle(name: name, code: code, info: info, access: access),
        modularOutput: static (adder, name, code, info, access, hidden) => hidden ? adder.AddHiddenAngle(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access) : adder.AddAngle(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access));
    public static readonly PortKind Number = Of<double>(key: nameof(Number),
        regularInput: static (adder, name, code, info, access, requirement) => adder.AddNumber(name: name, code: code, info: info, access: access, requirement: requirement),
        modularInput: static (adder, name, code, info, access, requirement, hidden) => hidden ? adder.AddHiddenNumber(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access, requirement: requirement) : adder.AddNumber(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access, requirement: requirement),
        regularOutput: static (adder, name, code, info, access) => adder.AddNumber(name: name, code: code, info: info, access: access),
        modularOutput: static (adder, name, code, info, access, hidden) => hidden ? adder.AddHiddenNumber(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access) : adder.AddNumber(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access));
    public static readonly PortKind Boolean = Of<bool>(key: nameof(Boolean),
        regularInput: static (adder, name, code, info, access, requirement) => adder.AddBoolean(name: name, code: code, info: info, access: access, requirement: requirement),
        modularInput: static (adder, name, code, info, access, requirement, hidden) => hidden ? adder.AddHiddenBoolean(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access, requirement: requirement) : adder.AddBoolean(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access, requirement: requirement),
        regularOutput: static (adder, name, code, info, access) => adder.AddBoolean(name: name, code: code, info: info, access: access),
        modularOutput: static (adder, name, code, info, access, hidden) => hidden ? adder.AddHiddenBoolean(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access) : adder.AddBoolean(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access));
    public static readonly PortKind Text = Of<string>(key: nameof(Text),
        regularInput: static (adder, name, code, info, access, requirement) => adder.AddText(name: name, code: code, info: info, access: access, requirement: requirement),
        modularInput: static (adder, name, code, info, access, requirement, hidden) => hidden ? adder.AddHiddenText(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access, requirement: requirement) : adder.AddText(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access, requirement: requirement),
        regularOutput: static (adder, name, code, info, access) => adder.AddText(name: name, code: code, info: info, access: access),
        modularOutput: static (adder, name, code, info, access, hidden) => hidden ? adder.AddHiddenText(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access) : adder.AddText(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access));
    public static readonly PortKind Mesh = Of<Mesh>(key: nameof(Mesh),
        regularInput: static (adder, name, code, info, access, requirement) => adder.AddMesh(name: name, code: code, info: info, access: access, requirement: requirement),
        modularInput: static (adder, name, code, info, access, requirement, hidden) => hidden ? adder.AddHiddenMesh(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access, requirement: requirement) : adder.AddMesh(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access, requirement: requirement),
        regularOutput: static (adder, name, code, info, access) => adder.AddMesh(name: name, code: code, info: info, access: access),
        modularOutput: static (adder, name, code, info, access, hidden) => hidden ? adder.AddHiddenMesh(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access) : adder.AddMesh(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access));
    public static readonly PortKind Polyline = Of<Polyline>(key: nameof(Polyline),
        regularInput: static (adder, name, code, info, access, requirement) => adder.AddPolyline(name: name, code: code, info: info, access: access, requirement: requirement),
        modularInput: static (adder, name, code, info, access, requirement, hidden) => hidden ? adder.AddHiddenPolyline(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access, requirement: requirement) : adder.AddPolyline(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access, requirement: requirement),
        regularOutput: static (adder, name, code, info, access) => adder.AddPolyline(name: name, code: code, info: info, access: access),
        modularOutput: static (adder, name, code, info, access, hidden) => hidden ? adder.AddHiddenPolyline(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access) : adder.AddPolyline(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access));
    public static readonly PortKind Generic = Of<object>(key: nameof(Generic),
        regularInput: static (adder, name, code, info, access, requirement) => adder.AddGeneric(name: name, code: code, info: info, access: access, requirement: requirement),
        modularInput: static (adder, name, code, info, access, requirement, hidden) => hidden ? adder.AddHiddenGeneric(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access, requirement: requirement) : adder.AddGeneric(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access, requirement: requirement),
        regularOutput: static (adder, name, code, info, access) => adder.AddGeneric(name: name, code: code, info: info, access: access),
        modularOutput: static (adder, name, code, info, access, hidden) => hidden ? adder.AddHiddenGeneric(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access) : adder.AddGeneric(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access));
    public Type Type { get; }
    public Type WireType { get; }
    [UseDelegateFromConstructor] private partial IParameter AddInput(ModularInputAdder adder, string name, string code, string info, Access access, Requirement requirement, bool hidden);
    [UseDelegateFromConstructor] private partial IParameter AddOutput(ModularOutputAdder adder, string name, string code, string info, Access access, bool hidden);
    public static PortKind Enum<T>(T initial) where T : struct, Enum =>
        Of<T>(
            key: typeof(T).Name,
            regularInput: (adder, name, code, info, access, requirement) => adder.AddEnum<T>(name: name, code: code, info: info, initial: initial, access: access, requirement: requirement),
            modularInput: (adder, name, code, info, access, requirement, hidden) => hidden ? adder.AddHiddenEnum(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, initial: initial, access: access, requirement: requirement) : adder.AddEnum(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, initial: initial, access: access, requirement: requirement),
            regularOutput: static (adder, name, code, info, access) => adder.AddEnum<T>(name: name, code: code, info: info, access: access),
            modularOutput: static (adder, name, code, info, access, hidden) => {
                IntegerParameter parameter = adder.RegularAdder.AddEnum<T>(name: name, code: code, info: info, access: access);
                _ = PortPolicy.Hidden.Apply(parameter: parameter);
                return parameter;
            },
            wireType: typeof(int));
    public static Option<PortKind> From(Type type) {
        ArgumentNullException.ThrowIfNull(argument: type);
        return type == typeof(Shape)
            ? Some(Generic)
            : toSeq(Items).Find(predicate: kind => kind.Type == type);
    }
    public IParameter Bind(ModularInputAdder adder, string name, string code, string info, Access access, Requirement requirement, PortPolicy policy, bool hidden) {
        ArgumentNullException.ThrowIfNull(argument: adder);
        ArgumentNullException.ThrowIfNull(argument: policy);
        IParameter parameter = AddInput(adder: adder, name: name, code: code, info: info, access: access, requirement: requirement, hidden: hidden);
        _ = policy.Apply(parameter: parameter);
        return parameter;
    }
    public IParameter Bind(ModularOutputAdder adder, string name, string code, string info, Access access, PortPolicy policy, bool hidden) {
        ArgumentNullException.ThrowIfNull(argument: adder);
        ArgumentNullException.ThrowIfNull(argument: policy);
        IParameter parameter = AddOutput(adder: adder, name: name, code: code, info: info, access: access, hidden: hidden);
        _ = policy.Apply(parameter: parameter);
        return parameter;
    }
    private static PortKind Of<T>(
        string key,
        RegularInput regularInput,
        Input modularInput,
        RegularOutput regularOutput,
        Output modularOutput,
        Type? wireType = null) =>
        new(
            key: key,
            type: typeof(T),
            wireType: wireType ?? typeof(T),
            addInput: (adder, name, code, info, access, requirement, hidden) => (hidden || requirement == Requirement.MayBeMissing) switch {
                true => modularInput(adder: adder, name: name, code: code, info: info, access: access, requirement: requirement, hidden: hidden),
                false => regularInput(adder: adder.RegularAdder, name: name, code: code, info: info, access: access, requirement: requirement),
            },
            addOutput: (adder, name, code, info, access, hidden) => hidden switch {
                true => modularOutput(adder: adder, name: name, code: code, info: info, access: access, hidden: true),
                false => regularOutput(adder: adder.RegularAdder, name: name, code: code, info: info, access: access),
            });
    private static IntegerParameter ConfigureIndex(IntegerParameter parameter) {
        parameter.Indexing = IndexModifier.Clip;
        return parameter;
    }
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
    public static Port<TVal> Tree<TVal>(string name, string code, string info, Requirement requirement = Requirement.MustExist, PortKind? kind = null, PortPolicy? policy = null) =>
        Of<TVal>(name: name, code: code, info: info, kind: kind, access: Access.Tree, requirement: requirement, policy: policy, fallback: Option<TVal>.None);
    public static Port<int> Index(
        string name = "Index",
        string code = "I",
        string info = "Zero-based selector; clamped to [0, count-1].") =>
        Optional<int>(name: name, code: code, info: info, kind: PortKind.Index);
    public static Port<Vector3d> Direction(
        string name = "Direction",
        string code = "D",
        string info = "Direction vector; missing Direction uses world Z.") =>
        Optional<Vector3d>(name: name, code: code, info: info, fallback: Some(Vector3d.ZAxis));
    public static Port<Angle> Angle(
        string name = "Angle",
        string code = "A",
        string info = "Angle in radians; missing Angle defaults to 0.") =>
        Optional<Angle>(name: name, code: code, info: info, fallback: Some(Grasshopper2.Types.Numeric.Angle.Zero));
    public static Port<Shape> Shape(
        string name = "Geometry",
        string code = "G",
        string info = "Geometry to analyse.",
        Access access = Access.Tree) =>
        Of<Shape>(name: name, code: code, info: info, kind: null, access: access, requirement: Requirement.MustExist, policy: null, fallback: Option<Shape>.None);
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
