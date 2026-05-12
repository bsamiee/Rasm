using System.Collections.Frozen;
using Eto.Drawing;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.Components;
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
        On<IParameter>(mutate: parameter => CustomKey.Category.Set(parameter: parameter, value: name));
    public static PortPolicy Colour(Color color) =>
        On<IParameter>(mutate: parameter => CustomKey.Colour.Set(parameter: parameter, value: color));
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
}

// --- [CONSTANTS] ------------------------------------------------------------------------
// Typed wrap over GH2 ModularComponent custom-value magic strings. The whole canonical surface
// (Optional / HideByDefault / Category / Colour) routes through this single SmartEnum so future
// GH2 internal renames break compilation here at a single point rather than scattered
// `parameter.CustomValues.Set("__category", ...)` literals across plugin authors.
[SmartEnum<string>]
internal sealed partial class CustomKey {
    public static readonly CustomKey Optional = new(key: ModularComponent.__Optional);
    public static readonly CustomKey HideByDefault = new(key: ModularComponent.__HideByDefault);
    public static readonly CustomKey Category = new(key: ModularComponent.__Category);
    public static readonly CustomKey Colour = new(key: ModularComponent.__Colour);
    internal Unit Set(IParameter parameter, bool value) {
        ArgumentNullException.ThrowIfNull(argument: parameter);
        parameter.CustomValues.Set(key: Key, value: value);
        return Unit.Default;
    }
    internal Unit Set(IParameter parameter, string value) {
        ArgumentNullException.ThrowIfNull(argument: parameter);
        parameter.CustomValues.Set(key: Key, value: value);
        return Unit.Default;
    }
    internal Unit Set(IParameter parameter, Color value) {
        ArgumentNullException.ThrowIfNull(argument: parameter);
        parameter.CustomValues.Set(key: Key, value: value);
        return Unit.Default;
    }
}

// --- [SERVICES] -------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class PortKind {
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
        return (type.Equals(o: typeof(int)), type.Equals(o: typeof(Shape))) switch {
            (true, _) => Some(Integer),
            (_, true) => Some(Generic),
            _ => Optional(Lookup.GetValueOrDefault(type)),
        };
    }
    // FrozenDictionary on `Type` — three structural reasons:
    // (1) `Type` keys lack a usable LanguageExt trait: the v5 reflection-based Ord/Hashable
    //     resolvers walk every loaded assembly via `Module.GetDefinedTypes()` and one of the Rhino
    //     assemblies throws during enumeration, poisoning Map<Type,_> and HashMap<Type,_>.
    // (2) Referencing the items by name avoids the SmartEnum `Items` collection, whose `Lazy`
    //     backing field lives in the auto-generated partial; cross-partial field-initialiser
    //     ordering is implementation-defined, so `_lookups` may still be null at this point.
    // (3) FrozenDictionary is the .NET 9 optimal read-only hash dictionary — eager construction at
    //     cctor, allocation-free lookups, no Lazy indirection, no per-call overhead.
    // The dictionary-initialiser indexer form tolerates the typeof(int) collision between Index and
    // Integer (Integer wins by declaration order, matching the typeof(int) special case in `From`).
    private static readonly FrozenDictionary<Type, PortKind> Lookup = BuildLookup();
    [BoundaryAdapter]
    private static FrozenDictionary<Type, PortKind> BuildLookup() =>
        new Dictionary<Type, PortKind> {
            [Point.Type] = Point,
            [Vector.Type] = Vector,
            [Curve.Type] = Curve,
            [Brep.Type] = Brep,
            [Plane.Type] = Plane,
            [Index.Type] = Index,
            [Integer.Type] = Integer,
            [Interval.Type] = Interval,
            [Angle.Type] = Angle,
            [Number.Type] = Number,
            [Boolean.Type] = Boolean,
            [Text.Type] = Text,
            [Mesh.Type] = Mesh,
            [Generic.Type] = Generic,
        }.ToFrozenDictionary();
    public Unit Bind(InputAdder adder, string name, string code, string info, Access access, Requirement requirement, PortPolicy policy, bool hidden) {
        ArgumentNullException.ThrowIfNull(argument: adder);
        ArgumentNullException.ThrowIfNull(argument: policy);
        IParameter parameter = AddInput(adder: adder, name: name, code: code, info: info, access: access, requirement: requirement);
        _ = policy.Apply(parameter: parameter);
        return ApplyHidden(parameter: parameter, hidden: hidden);
    }
    public Unit Bind(OutputAdder adder, string name, string code, string info, Access access, PortPolicy policy, bool hidden) {
        ArgumentNullException.ThrowIfNull(argument: adder);
        ArgumentNullException.ThrowIfNull(argument: policy);
        IParameter parameter = AddOutput(adder: adder, name: name, code: code, info: info, access: access);
        _ = policy.Apply(parameter: parameter);
        return ApplyHidden(parameter: parameter, hidden: hidden);
    }
    private static Unit ApplyHidden(IParameter parameter, bool hidden) =>
        hidden switch {
            true => Seq(CustomKey.Optional, CustomKey.HideByDefault).Iter(key => key.Set(parameter: parameter, value: true)),
            false => Unit.Default,
        };
    private static PortKind Of<T>(
        string key,
        Func<InputAdder, string, string, string, Access, Requirement, IParameter> input,
        Func<OutputAdder, string, string, string, Access, IParameter> output) =>
        new(key: key, type: typeof(T), addInput: input, addOutput: output);
}

public readonly record struct Port<TVal>(
    string Name,
    string Code,
    string Info,
    PortKind Kind,
    Access Access,
    Requirement Requirement,
    PortPolicy Policy) : IPort;

public static class Port {
    public static Port<TVal> Required<TVal>(string name, string code, string info, PortKind? kind = null, PortPolicy? policy = null) =>
        Create<TVal>(name: name, code: code, info: info, kind: kind, access: Access.Item, requirement: Requirement.MustExist, policy: policy);
    public static Port<TVal> Optional<TVal>(string name, string code, string info, PortKind? kind = null, PortPolicy? policy = null, string category = "Optional") =>
        Create<TVal>(
            name: name, code: code, info: info, kind: kind,
            access: Access.Item, requirement: Requirement.MayBeMissing,
            policy: PortPolicy.Compose(policy ?? DefaultPolicy(type: typeof(TVal)), PortPolicy.Category(name: category)));
    public static Port<TVal> List<TVal>(string name, string code, string info, Requirement requirement = Requirement.MustExist, PortKind? kind = null, PortPolicy? policy = null) =>
        Create<TVal>(name: name, code: code, info: info, kind: kind, access: Access.Twig, requirement: requirement, policy: policy);
    public static Port<int> Index(
        string name = "Index",
        string code = "I",
        string info = "Zero-based selector; clamped to [0, count-1].") =>
        new(Name: name, Code: code, Info: info, Kind: PortKind.Index, Access: Access.Item, Requirement: Requirement.MayBeMissing, Policy: PortPolicy.Index());
    public static Port<Shape> Shape(
        string name = "Geometry",
        string code = "G",
        string info = "Geometry to analyse.") =>
        Create<Shape>(name: name, code: code, info: info, kind: null, access: Access.Item, requirement: Requirement.MustExist, policy: null);
    private static Port<TVal> Create<TVal>(string name, string code, string info, PortKind? kind, Access access, Requirement requirement, PortPolicy? policy) =>
        new(Name: name, Code: code, Info: info,
            Kind: kind ?? PortKind.From(type: typeof(TVal)).IfNone(PortKind.Generic),
            Access: access, Requirement: requirement,
            Policy: policy ?? DefaultPolicy(type: typeof(TVal)));
    private static PortPolicy DefaultPolicy(Type type) => PolicyDefaults.Find(type).IfNone(PortPolicy.Empty);
    private static readonly Map<Type, PortPolicy> PolicyDefaults =
        Map<Type, PortPolicy>()
            .Add(typeof(Vector3d), PortPolicy.Vector(unitise: true))
            .Add(typeof(Angle), PortPolicy.Angle(reduce: true));
}
