using System.Reflection;
using Eto.Drawing;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.Components;
using Grasshopper2.Doc;
using Grasshopper2.Parameters;
using Requirement = Grasshopper2.Parameters.Requirement;

namespace Rasm.Grasshopper;

// --- [TYPES] ----------------------------------------------------------------------------
public abstract class Port {
    private protected Port(string name, string code, string info, PortKind kind, Access access, Requirement requirement, PortPolicy policy) {
        Name = name;
        Code = code;
        Info = info;
        Kind = kind;
        Access = access;
        Requirement = requirement;
        Policy = policy;
    }
    public string Name { get; }
    public string Code { get; }
    public string Info { get; }
    public PortKind Kind { get; }
    public Access Access { get; }
    public Requirement Requirement { get; }
    public PortPolicy Policy { get; }
    public static Port<TVal> Of<TVal>(
        string name,
        string code,
        string info,
        Access access = Access.Item,
        Requirement requirement = Requirement.MustExist,
        PortKind? kind = null,
        PortPolicy? policy = null) {
        ArgumentNullException.ThrowIfNull(argument: name);
        ArgumentNullException.ThrowIfNull(argument: code);
        ArgumentNullException.ThrowIfNull(argument: info);
        PortPolicy resolved = policy ?? DefaultPolicy(type: typeof(TVal));
        return new(
            name: name,
            code: code,
            info: info,
            kind: (LanguageExt.Prelude.Optional(kind).Filter(candidate => candidate.Accepts(type: typeof(TVal))) | PortKind.From(type: typeof(TVal))).IfNone(PortKind.Generic),
            access: access,
            requirement: requirement,
            policy: requirement switch {
                Requirement.MayBeMissing => resolved + PortPolicy.Optional + PortPolicy.Category(name: "Optional"),
                _ => resolved,
            });
    }
    public static Port<Shape> Shape(
        string name = "Geometry",
        string code = "G",
        string info = "Geometry to analyse.",
        Access access = Access.Tree) =>
        Of<Shape>(name: name, code: code, info: info, access: access);
    private static PortPolicy DefaultPolicy(Type type) => type switch {
        _ when type == typeof(Vector3d) => PortPolicy.Vector(unitise: true),
        _ when type == typeof(Angle) => PortPolicy.Angle(reduce: true),
        _ => PortPolicy.Empty,
    };
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
    public static PortPolicy Curve(CurveParameter.NormalisationMethod normaliseDomains = CurveParameter.NormalisationMethod.None, bool flip = false) =>
        On<CurveParameter>(mutate: target => { target.NormaliseDomains = normaliseDomains; target.FlipCurves = flip; return Unit.Default; });
    public static PortPolicy Surface(bool acceptMeshes = false, CurveParameter.NormalisationMethod normaliseDomains = CurveParameter.NormalisationMethod.None, bool flip = false) =>
        On<SurfaceParameter>(mutate: target => { target.AcceptMeshes = acceptMeshes; target.NormaliseDomains = normaliseDomains; target.FlipSurfaces = flip; return Unit.Default; });
    public static PortPolicy Index(IndexModifier indexing = IndexModifier.Clip) =>
        On<IntegerParameter>(mutate: target => { target.IsIndex = true; target.Indexing = indexing; return Unit.Default; });
    public static PortPolicy Category(string name, Color? colour = null) =>
        Compose(
            On<IParameter>(mutate: parameter => SetCustom(parameter: parameter, key: ModularComponent.__Category, set: (kv, k) => kv.Set(key: k, value: name))),
            colour switch {
                Color value => On<IParameter>(mutate: parameter => SetCustom(parameter: parameter, key: ModularComponent.__Colour, set: (kv, k) => kv.Set(key: k, value: value))),
                _ => Empty,
            });
    public static PortPolicy Optional { get; } =
        On<IParameter>(mutate: parameter => SetCustom(parameter: parameter, key: ModularComponent.__Optional, set: (kv, k) => kv.Set(key: k, value: true)));
    public static PortPolicy Hidden { get; } = Compose(Optional,
        On<IParameter>(mutate: parameter => SetCustom(parameter: parameter, key: ModularComponent.__HideByDefault, set: (kv, k) => kv.Set(key: k, value: true))));
    public static PortPolicy Compose(params PortPolicy[] policies) {
        ArgumentNullException.ThrowIfNull(argument: policies);
        return new PortPolicy(apply: parameter => toSeq(policies).Iter(policy => policy.Apply(parameter: parameter)));
    }
    public static PortPolicy Add(PortPolicy left, PortPolicy right) => Compose(left, right);
    public static PortPolicy operator +(PortPolicy left, PortPolicy right) => Add(left: left, right: right);
    public Unit Apply(object parameter) {
        ArgumentNullException.ThrowIfNull(argument: parameter);
        return apply(arg: parameter);
    }
    private static PortPolicy On<TParam>(Func<TParam, Unit> mutate) where TParam : class =>
        new(apply: parameter => parameter switch {
            TParam target => mutate(arg: target),
            _ => Unit.Default,
        });
    private static Unit SetCustom(IParameter parameter, string key, Action<KeyedValues, string> set) {
        set(arg1: parameter.CustomValues, arg2: key);
        return Unit.Default;
    }
}
public sealed class Port<TVal> : Port {
    internal Port(
        string name,
        string code,
        string info,
        PortKind kind,
        Access access,
        Requirement requirement,
        PortPolicy policy) : base(name: name, code: code, info: info, kind: kind, access: access, requirement: requirement, policy: policy) { }
}

// --- [CONSTANTS] ------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class PortKind {
    private const string Category = "";
    private delegate IParameter Input(ModularInputAdder adder, string name, string code, string info, Access access, Requirement requirement, bool hidden);
    private delegate IParameter Output(ModularOutputAdder adder, string name, string code, string info, Access access, bool hidden);
    private delegate IParameter RegularInput(InputAdder adder, string name, string code, string info, Access access, Requirement requirement);
    private delegate IParameter RegularOutput(OutputAdder adder, string name, string code, string info, Access access);
    public static readonly PortKind Path = Standard<Grasshopper2.Data.Path>(key: nameof(Path), regularInput: static (a, n, c, i, x, r) => a.AddPath(name: n, code: c, info: i, access: x, requirement: r), modularInput: static (a, n, c, i, x, r, _) => a.AddPath(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x, requirement: r), hiddenInput: static (a, n, c, i, x, r, _) => a.AddHiddenPath(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x, requirement: r), regularOutput: static (a, n, c, i, x) => a.AddPath(name: n, code: c, info: i, access: x), modularOutput: static (a, n, c, i, x, _) => a.AddPath(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x), hiddenOutput: static (a, n, c, i, x, _) => a.AddHiddenPath(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x));
    public static readonly PortKind Site = Standard<Grasshopper2.Data.Site>(key: nameof(Site), regularInput: static (a, n, c, i, x, r) => a.AddSite(name: n, code: c, info: i, access: x, requirement: r), modularInput: static (a, n, c, i, x, r, _) => a.AddSite(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x, requirement: r), hiddenInput: static (a, n, c, i, x, r, _) => a.AddHiddenSite(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x, requirement: r), regularOutput: static (a, n, c, i, x) => a.AddSite(name: n, code: c, info: i, access: x), modularOutput: static (a, n, c, i, x, _) => a.AddSite(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x), hiddenOutput: static (a, n, c, i, x, _) => a.AddHiddenSite(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x));
    public static readonly PortKind Colour = Standard<Grasshopper2.Types.Colour.Colour>(key: nameof(Colour), regularInput: static (a, n, c, i, x, r) => a.AddColour(name: n, code: c, info: i, access: x, requirement: r), modularInput: static (a, n, c, i, x, r, _) => a.AddColour(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x, requirement: r), hiddenInput: static (a, n, c, i, x, r, _) => a.AddHiddenColour(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x, requirement: r), regularOutput: static (a, n, c, i, x) => a.AddColour(name: n, code: c, info: i, access: x), modularOutput: static (a, n, c, i, x, _) => a.AddColour(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x), hiddenOutput: static (a, n, c, i, x, _) => a.AddHiddenColour(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x));
    public static readonly PortKind Point = Standard<Point3d>(key: nameof(Point), regularInput: static (a, n, c, i, x, r) => a.AddPoint(name: n, code: c, info: i, access: x, requirement: r), modularInput: static (a, n, c, i, x, r, _) => a.AddPoint(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x, requirement: r), hiddenInput: static (a, n, c, i, x, r, _) => a.AddHiddenPoint(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x, requirement: r), regularOutput: static (a, n, c, i, x) => a.AddPoint(name: n, code: c, info: i, access: x), modularOutput: static (a, n, c, i, x, _) => a.AddPoint(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x), hiddenOutput: static (a, n, c, i, x, _) => a.AddHiddenPoint(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x));
    public static readonly PortKind Vector = Standard<Vector3d>(key: nameof(Vector), regularInput: static (a, n, c, i, x, r) => a.AddVector(name: n, code: c, info: i, access: x, requirement: r), modularInput: static (a, n, c, i, x, r, _) => a.AddVector(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x, requirement: r), hiddenInput: static (a, n, c, i, x, r, _) => a.AddHiddenVector(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x, requirement: r), regularOutput: static (a, n, c, i, x) => a.AddVector(name: n, code: c, info: i, access: x), modularOutput: static (a, n, c, i, x, _) => a.AddVector(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x), hiddenOutput: static (a, n, c, i, x, _) => a.AddHiddenVector(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x));
    public static readonly PortKind Line = Standard<Line>(key: nameof(Line), regularInput: static (a, n, c, i, x, r) => a.AddLine(name: n, code: c, info: i, access: x, requirement: r), modularInput: static (a, n, c, i, x, r, _) => a.AddLine(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x, requirement: r), hiddenInput: static (a, n, c, i, x, r, _) => a.AddHiddenLine(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x, requirement: r), regularOutput: static (a, n, c, i, x) => a.AddLine(name: n, code: c, info: i, access: x), modularOutput: static (a, n, c, i, x, _) => a.AddLine(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x), hiddenOutput: static (a, n, c, i, x, _) => a.AddHiddenLine(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x));
    public static readonly PortKind Arc = Standard<Arc>(key: nameof(Arc), regularInput: static (a, n, c, i, x, r) => a.AddArc(name: n, code: c, info: i, access: x, requirement: r), modularInput: static (a, n, c, i, x, r, _) => a.AddArc(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x, requirement: r), hiddenInput: static (a, n, c, i, x, r, _) => a.AddHiddenArc(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x, requirement: r), regularOutput: static (a, n, c, i, x) => a.AddArc(name: n, code: c, info: i, access: x), modularOutput: static (a, n, c, i, x, _) => a.AddArc(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x), hiddenOutput: static (a, n, c, i, x, _) => a.AddHiddenArc(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x));
    public static readonly PortKind Circle = Standard<Circle>(key: nameof(Circle), regularInput: static (a, n, c, i, x, r) => a.AddCircle(name: n, code: c, info: i, access: x, requirement: r), modularInput: static (a, n, c, i, x, r, _) => a.AddCircle(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x, requirement: r), hiddenInput: static (a, n, c, i, x, r, _) => a.AddHiddenCircle(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x, requirement: r), regularOutput: static (a, n, c, i, x) => a.AddCircle(name: n, code: c, info: i, access: x), modularOutput: static (a, n, c, i, x, _) => a.AddCircle(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x), hiddenOutput: static (a, n, c, i, x, _) => a.AddHiddenCircle(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x));
    public static readonly PortKind Rectangle = Standard<Rectangle3d>(key: nameof(Rectangle), regularInput: static (a, n, c, i, x, r) => a.AddRectangle(name: n, code: c, info: i, access: x, requirement: r), modularInput: static (a, n, c, i, x, r, _) => a.AddRectangle(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x, requirement: r), hiddenInput: static (a, n, c, i, x, r, _) => a.AddHiddenRectangle(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x, requirement: r), regularOutput: static (a, n, c, i, x) => a.AddRectangle(name: n, code: c, info: i, access: x), modularOutput: static (a, n, c, i, x, _) => a.AddRectangle(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x), hiddenOutput: static (a, n, c, i, x, _) => a.AddHiddenRectangle(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x));
    public static readonly PortKind Curve = Standard<Curve>(key: nameof(Curve), regularInput: static (a, n, c, i, x, r) => a.AddCurve(name: n, code: c, info: i, access: x, requirement: r), modularInput: static (a, n, c, i, x, r, _) => a.AddCurve(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x, requirement: r), hiddenInput: static (a, n, c, i, x, r, _) => a.AddHiddenCurve(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x, requirement: r), regularOutput: static (a, n, c, i, x) => a.AddCurve(name: n, code: c, info: i, access: x), modularOutput: static (a, n, c, i, x, _) => a.AddCurve(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x), hiddenOutput: static (a, n, c, i, x, _) => a.AddHiddenCurve(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x));
    public static readonly PortKind Brep = Standard<Brep>(key: nameof(Brep), regularInput: static (a, n, c, i, x, r) => a.AddSurface(name: n, code: c, info: i, access: x, requirement: r), modularInput: static (a, n, c, i, x, r, _) => a.AddSurface(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x, requirement: r), hiddenInput: static (a, n, c, i, x, r, _) => a.AddHiddenSurface(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x, requirement: r), regularOutput: static (a, n, c, i, x) => a.AddSurface(name: n, code: c, info: i, access: x), modularOutput: static (a, n, c, i, x, _) => a.AddSurface(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x), hiddenOutput: static (a, n, c, i, x, _) => a.AddHiddenSurface(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x));
    public static readonly PortKind Surface = Standard<Surface>(key: nameof(Surface), regularInput: static (a, n, c, i, x, r) => a.AddSurface(name: n, code: c, info: i, access: x, requirement: r), modularInput: static (a, n, c, i, x, r, _) => a.AddSurface(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x, requirement: r), hiddenInput: static (a, n, c, i, x, r, _) => a.AddHiddenSurface(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x, requirement: r), regularOutput: static (a, n, c, i, x) => a.AddSurface(name: n, code: c, info: i, access: x), modularOutput: static (a, n, c, i, x, _) => a.AddSurface(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x), hiddenOutput: static (a, n, c, i, x, _) => a.AddHiddenSurface(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x));
    public static readonly PortKind Box = Standard<Box>(key: nameof(Box), regularInput: static (a, n, c, i, x, r) => a.AddBox(name: n, code: c, info: i, access: x, requirement: r), modularInput: static (a, n, c, i, x, r, _) => a.AddBox(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x, requirement: r), hiddenInput: static (a, n, c, i, x, r, _) => a.AddHiddenBox(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x, requirement: r), regularOutput: static (a, n, c, i, x) => a.AddBox(name: n, code: c, info: i, access: x), modularOutput: static (a, n, c, i, x, _) => a.AddBox(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x), hiddenOutput: static (a, n, c, i, x, _) => a.AddHiddenBox(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x));
    public static readonly PortKind Sphere = Standard<Sphere>(key: nameof(Sphere), regularInput: static (a, n, c, i, x, r) => a.AddSphere(name: n, code: c, info: i, access: x, requirement: r), modularInput: static (a, n, c, i, x, r, _) => a.AddSphere(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x, requirement: r), hiddenInput: static (a, n, c, i, x, r, _) => a.AddHiddenSphere(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x, requirement: r), regularOutput: static (a, n, c, i, x) => a.AddSphere(name: n, code: c, info: i, access: x), modularOutput: static (a, n, c, i, x, _) => a.AddSphere(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x), hiddenOutput: static (a, n, c, i, x, _) => a.AddHiddenSphere(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x));
    public static readonly PortKind Plane = Standard<Plane>(key: nameof(Plane), regularInput: static (a, n, c, i, x, r) => a.AddPlane(name: n, code: c, info: i, access: x, requirement: r), modularInput: static (a, n, c, i, x, r, _) => a.AddPlane(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x, requirement: r), hiddenInput: static (a, n, c, i, x, r, _) => a.AddHiddenPlane(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x, requirement: r), regularOutput: static (a, n, c, i, x) => a.AddPlane(name: n, code: c, info: i, access: x), modularOutput: static (a, n, c, i, x, _) => a.AddPlane(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x), hiddenOutput: static (a, n, c, i, x, _) => a.AddHiddenPlane(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x));
    public static readonly PortKind Transform = Standard<Transform>(key: nameof(Transform), regularInput: static (a, n, c, i, x, r) => a.AddTransform(name: n, code: c, info: i, access: x, requirement: r), modularInput: static (a, n, c, i, x, r, _) => a.AddTransform(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x, requirement: r), hiddenInput: static (a, n, c, i, x, r, _) => a.AddHiddenTransform(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x, requirement: r), regularOutput: static (a, n, c, i, x) => a.AddTransform(name: n, code: c, info: i, access: x), modularOutput: static (a, n, c, i, x, _) => a.AddTransform(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x), hiddenOutput: static (a, n, c, i, x, _) => a.AddHiddenTransform(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x));
    public static readonly PortKind Integer = Standard<int>(key: nameof(Integer), regularInput: static (a, n, c, i, x, r) => a.AddInteger(name: n, code: c, info: i, access: x, requirement: r), modularInput: static (a, n, c, i, x, r, _) => a.AddInteger(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x, requirement: r), hiddenInput: static (a, n, c, i, x, r, _) => a.AddHiddenInteger(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x, requirement: r), regularOutput: static (a, n, c, i, x) => a.AddInteger(name: n, code: c, info: i, access: x), modularOutput: static (a, n, c, i, x, _) => a.AddInteger(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x), hiddenOutput: static (a, n, c, i, x, _) => a.AddHiddenInteger(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x));
    public static readonly PortKind Index = Of<int>(key: nameof(Index),
        regularInput: static (adder, name, code, info, access, requirement) => adder.AddIndex(name: name, code: code, info: info, access: access, requirement: requirement) switch {
            IntegerParameter parameter => ConfigureIndex(parameter: parameter),
        },
        modularInput: static (adder, name, code, info, access, requirement, hidden) => adder.RegularAdder.AddIndex(name: name, code: code, info: info, access: access, requirement: requirement) switch {
            IntegerParameter parameter => (hidden switch { true => PortPolicy.Hidden, false => PortPolicy.Optional }).Apply(parameter: ConfigureIndex(parameter: parameter)) switch { _ => parameter },
        },
        regularOutput: static (adder, name, code, info, access) => adder.AddInteger(name: name, code: code, info: info, access: access),
        modularOutput: static (adder, name, code, info, access, hidden) => hidden ? adder.AddHiddenInteger(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access) : adder.AddInteger(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access));
    public static readonly PortKind Interval = Standard<Interval>(key: nameof(Interval), regularInput: static (a, n, c, i, x, r) => a.AddInterval(name: n, code: c, info: i, access: x, requirement: r), modularInput: static (a, n, c, i, x, r, _) => a.AddInterval(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x, requirement: r), hiddenInput: static (a, n, c, i, x, r, _) => a.AddHiddenInterval(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x, requirement: r), regularOutput: static (a, n, c, i, x) => a.AddInterval(name: n, code: c, info: i, access: x), modularOutput: static (a, n, c, i, x, _) => a.AddInterval(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x), hiddenOutput: static (a, n, c, i, x, _) => a.AddHiddenInterval(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x));
    public static readonly PortKind Angle = Standard<Angle>(key: nameof(Angle), regularInput: static (a, n, c, i, x, r) => a.AddAngle(name: n, code: c, info: i, access: x, requirement: r), modularInput: static (a, n, c, i, x, r, _) => a.AddAngle(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x, requirement: r), hiddenInput: static (a, n, c, i, x, r, _) => a.AddHiddenAngle(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x, requirement: r), regularOutput: static (a, n, c, i, x) => a.AddAngle(name: n, code: c, info: i, access: x), modularOutput: static (a, n, c, i, x, _) => a.AddAngle(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x), hiddenOutput: static (a, n, c, i, x, _) => a.AddHiddenAngle(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x));
    public static readonly PortKind Number = Standard<double>(key: nameof(Number), regularInput: static (a, n, c, i, x, r) => a.AddNumber(name: n, code: c, info: i, access: x, requirement: r), modularInput: static (a, n, c, i, x, r, _) => a.AddNumber(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x, requirement: r), hiddenInput: static (a, n, c, i, x, r, _) => a.AddHiddenNumber(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x, requirement: r), regularOutput: static (a, n, c, i, x) => a.AddNumber(name: n, code: c, info: i, access: x), modularOutput: static (a, n, c, i, x, _) => a.AddNumber(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x), hiddenOutput: static (a, n, c, i, x, _) => a.AddHiddenNumber(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x));
    public static readonly PortKind Boolean = Standard<bool>(key: nameof(Boolean), regularInput: static (a, n, c, i, x, r) => a.AddBoolean(name: n, code: c, info: i, access: x, requirement: r), modularInput: static (a, n, c, i, x, r, _) => a.AddBoolean(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x, requirement: r), hiddenInput: static (a, n, c, i, x, r, _) => a.AddHiddenBoolean(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x, requirement: r), regularOutput: static (a, n, c, i, x) => a.AddBoolean(name: n, code: c, info: i, access: x), modularOutput: static (a, n, c, i, x, _) => a.AddBoolean(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x), hiddenOutput: static (a, n, c, i, x, _) => a.AddHiddenBoolean(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x));
    public static readonly PortKind Text = Standard<string>(key: nameof(Text), regularInput: static (a, n, c, i, x, r) => a.AddText(name: n, code: c, info: i, access: x, requirement: r), modularInput: static (a, n, c, i, x, r, _) => a.AddText(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x, requirement: r), hiddenInput: static (a, n, c, i, x, r, _) => a.AddHiddenText(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x, requirement: r), regularOutput: static (a, n, c, i, x) => a.AddText(name: n, code: c, info: i, access: x), modularOutput: static (a, n, c, i, x, _) => a.AddText(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x), hiddenOutput: static (a, n, c, i, x, _) => a.AddHiddenText(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x));
    public static readonly PortKind Mesh = Standard<Mesh>(key: nameof(Mesh), regularInput: static (a, n, c, i, x, r) => a.AddMesh(name: n, code: c, info: i, access: x, requirement: r), modularInput: static (a, n, c, i, x, r, _) => a.AddMesh(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x, requirement: r), hiddenInput: static (a, n, c, i, x, r, _) => a.AddHiddenMesh(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x, requirement: r), regularOutput: static (a, n, c, i, x) => a.AddMesh(name: n, code: c, info: i, access: x), modularOutput: static (a, n, c, i, x, _) => a.AddMesh(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x), hiddenOutput: static (a, n, c, i, x, _) => a.AddHiddenMesh(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x));
    public static readonly PortKind Polyline = Standard<Polyline>(key: nameof(Polyline), regularInput: static (a, n, c, i, x, r) => a.AddPolyline(name: n, code: c, info: i, access: x, requirement: r), modularInput: static (a, n, c, i, x, r, _) => a.AddPolyline(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x, requirement: r), hiddenInput: static (a, n, c, i, x, r, _) => a.AddHiddenPolyline(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x, requirement: r), regularOutput: static (a, n, c, i, x) => a.AddPolyline(name: n, code: c, info: i, access: x), modularOutput: static (a, n, c, i, x, _) => a.AddPolyline(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x), hiddenOutput: static (a, n, c, i, x, _) => a.AddHiddenPolyline(name: n, code: c, info: i, category: Category, colour: Colors.Transparent, access: x));
    public static readonly PortKind Generic = Of<object>(key: nameof(Generic),
        regularInput: static (adder, name, code, info, access, requirement) => adder.AddGeneric(name: name, code: code, info: info, access: access, requirement: requirement),
        modularInput: static (adder, name, code, info, access, requirement, hidden) => hidden ? adder.AddHiddenGeneric(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access, requirement: requirement) : adder.AddGeneric(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access, requirement: requirement),
        regularOutput: static (adder, name, code, info, access) => adder.AddGeneric(name: name, code: code, info: info, access: access),
        modularOutput: static (adder, name, code, info, access, hidden) => hidden ? adder.AddHiddenGeneric(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access) : adder.AddGeneric(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, access: access));
    public Type Type { get; }
    public Type WireType { get; }
    internal bool Accepts(Type type) => Type == type || WireType == type || (type.IsEnum && Type == type && WireType == typeof(int));
    [UseDelegateFromConstructor] private partial IParameter AddInput(ModularInputAdder adder, string name, string code, string info, Access access, Requirement requirement, bool hidden);
    [UseDelegateFromConstructor] private partial IParameter AddOutput(ModularOutputAdder adder, string name, string code, string info, Access access, bool hidden);
    public static PortKind Enum<T>(T initial) where T : struct, Enum =>
        Of<T>(
            key: typeof(T).FullName ?? typeof(T).Name,
            regularInput: (adder, name, code, info, access, requirement) => adder.AddEnum<T>(name: name, code: code, info: info, initial: initial, access: access, requirement: requirement),
            modularInput: (adder, name, code, info, access, requirement, hidden) => hidden ? adder.AddHiddenEnum(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, initial: initial, access: access, requirement: requirement) : adder.AddEnum(name: name, code: code, info: info, category: Category, colour: Colors.Transparent, initial: initial, access: access, requirement: requirement),
            regularOutput: static (adder, name, code, info, access) => adder.AddEnum<T>(name: name, code: code, info: info, access: access),
            modularOutput: static (adder, name, code, info, access, hidden) => {
                IntegerParameter parameter = new(nomen: new Nomen(name: name, info: info), access: access) { UserName = code };
                _ = parameter.Presets.AddEnum<T>();
                Action<IParameter, string, Color> add = hidden switch {
                    true => adder.AddHidden,
                    false => adder.Add,
                };
                add(arg1: parameter, arg2: Category, arg3: Colors.Transparent);
                return parameter;
            },
            wireType: typeof(int));
    public static Option<PortKind> From(Type type) {
        ArgumentNullException.ThrowIfNull(argument: type);
        return type == typeof(Shape)
            ? Some(Generic)
            : type.IsEnum && System.Enum.GetUnderlyingType(enumType: type) == typeof(int)
                ? EnumKind(type: type)
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
    private static PortKind Standard<T>(
        string key,
        RegularInput regularInput,
        Input modularInput,
        Input hiddenInput,
        RegularOutput regularOutput,
        Output modularOutput,
        Output hiddenOutput,
        Type? wireType = null) =>
        Of<T>(
            key: key,
            regularInput: regularInput,
            modularInput: (adder, name, code, info, access, requirement, hidden) => (hidden ? hiddenInput : modularInput)(adder, name, code, info, access, requirement, hidden),
            regularOutput: regularOutput,
            modularOutput: (adder, name, code, info, access, hidden) => (hidden ? hiddenOutput : modularOutput)(adder, name, code, info, access, hidden),
            wireType: wireType);
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
    private static IntegerParameter ConfigureIndex(IntegerParameter parameter) =>
        PortPolicy.Index().Apply(parameter: parameter) switch { _ => parameter };
    private static PortKind EnumDefault<T>() where T : struct, Enum {
        T[] values = System.Enum.GetValues<T>();
        return Enum(initial: values.Length > 0 ? values[0] : default);
    }
    private static Option<PortKind> EnumKind(Type type) =>
        Optional(typeof(PortKind).GetMethod(name: nameof(EnumDefault), bindingAttr: BindingFlags.NonPublic | BindingFlags.Static))
            .Bind(method => Optional(method.MakeGenericMethod(typeArguments: [type]).Invoke(obj: null, parameters: null) as PortKind));
}
