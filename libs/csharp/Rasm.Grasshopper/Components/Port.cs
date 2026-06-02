using System.Collections.Frozen;
using System.Globalization;
using System.Reflection;
using Requirement = Grasshopper2.Parameters.Requirement;

namespace Rasm.Grasshopper.Components;

// --- [TYPES] ------------------------------------------------------------------------------
public enum AngleEnforcement { None = 0, Degrees = 1, Radians = 2, Turns = 3 }

[Union]
public abstract partial record Capability {
    public sealed record Vector(bool Unitise = false, bool Reverse = false) : Capability;
    public sealed record Angle(AngleEnforcement Kind = AngleEnforcement.None, bool Reduce = false) : Capability;
    public sealed record Index(IndexModifier Indexing = IndexModifier.Clip) : Capability;
    public sealed record Curve(CurveParameter.NormalisationMethod Normalise = CurveParameter.NormalisationMethod.None, bool Flip = false) : Capability;
    public sealed record Surface(bool AcceptMeshes = false, CurveParameter.NormalisationMethod Normalise = CurveParameter.NormalisationMethod.None, bool Flip = false) : Capability;
    public sealed record Elective : Capability;
    public sealed record Hidden : Capability;
    public sealed record Category(string Name, Option<Color> Colour = default) : Capability;
    public sealed record Preset(Seq<(int Value, string Label)> Choices) : Capability;
    public sealed record Validate(Func<object, bool> Predicate, string Message) : Capability;
    public sealed record Seed(object Value) : Capability;
    public sealed record Many(Seq<Capability> Items) : Capability;

    public static Capability Default(object value) => new Seed(Value: value);
    public static Capability Empty => new Many(Items: Seq<Capability>());
    public static Capability operator +(Capability left, Capability right) => new Many(Items: Seq(left, right));

    internal Fin<IParameter> Apply(IParameter parameter) => Projection.ApplyTo(parameter);
    internal Seq<(Func<object, bool> Predicate, string Message)> Validators => Projection.Validators;
    internal bool CompatibleWith(PortKind kind) => Projection.CompatibleWith(kind);
    private Rule Projection => Switch(
        vector: v => Rule.Mutating<VectorParameter>(
            compatibleWith: static kind => kind == PortKind.Vector,
            mutate: target => { target.UnitiseVectors = v.Unitise; target.ReverseVectors = v.Reverse; return unit; }),
        angle: a => Rule.Mutating<AngleParameter>(
            compatibleWith: static kind => kind == PortKind.Angle,
            mutate: target => { target.EnforceKind = (int)a.Kind; target.ReduceAngles = a.Reduce; return unit; }),
        index: i => Rule.Mutating<IntegerParameter>(
            compatibleWith: static kind => kind == PortKind.Index || kind == PortKind.Integer,
            mutate: target => { target.IsIndex = true; target.Indexing = i.Indexing; return unit; }),
        curve: c => Rule.Mutating<CurveParameter>(
            compatibleWith: static kind => kind == PortKind.Curve,
            mutate: target => { target.NormaliseDomains = c.Normalise; target.FlipCurves = c.Flip; return unit; }),
        surface: s => Rule.Mutating<SurfaceParameter>(
            compatibleWith: static kind => kind == PortKind.Surface || kind == PortKind.Brep,
            mutate: target => { target.AcceptMeshes = s.AcceptMeshes; target.NormaliseDomains = s.Normalise; target.FlipSurfaces = s.Flip; return unit; }),
        elective: _ => Rule.Universal(applyTo: static parameter => OnCustom(parameter: parameter, write: values => values.Set(key: ModularComponent.__Optional, value: true))),
        hidden: _ => Rule.Universal(applyTo: static parameter => OnCustom(parameter: parameter, write: values => values.Set(key: ModularComponent.__HideByDefault, value: true))
            .Bind(p => OnCustom(parameter: p, write: values => values.Set(key: ModularComponent.__Optional, value: true)))),
        category: c => Rule.Universal(applyTo: parameter => OnCustom(parameter: parameter, write: values => values.Set(key: ModularComponent.__Category, value: c.Name))
            .Bind(p => c.Colour switch {
                { IsSome: true, Case: Color colour } => OnCustom(parameter: p, write: values => values.Set(key: ModularComponent.__Colour, value: colour)),
                _ => Fin.Succ(p),
            })),
        preset: pr => Rule.Mutating<IntegerParameter>(
            compatibleWith: static kind => kind == PortKind.Integer || kind == PortKind.Index,
            mutate: target => pr.Choices.Iter(choice => target.Presets.Add(name: choice.Label, info: string.Empty, value: choice.Value))),
        validate: v => Rule.Universal(validators: Seq((v.Predicate, v.Message))),
        // Seed data belongs in PersistentDataWeak; GH2 exposes no ModularComponent default marker.
        seed: d => Rule.Universal(applyTo: parameter => { parameter.PersistentDataWeak = Garden.TreeFromList(new object[] { d.Value }); return Fin.Succ(parameter); }),
        many: m => m.Items.Fold(Rule.Universal(), static (rules, capability) => rules + capability.Projection));

    private static Fin<IParameter> On<TParam>(IParameter parameter, Func<TParam, Unit> mutate) where TParam : class =>
        parameter switch {
            // BOUNDARY ADAPTER -- native GH2 parameter mutation; capability is rejected when the kind cannot host it.
            TParam target => mutate(arg: target) switch { _ => Fin.Succ(parameter) },
            _ => Fin.Fail<IParameter>(Op.Of(name: nameof(Capability)).InvalidInput()),
        };
    private static Fin<IParameter> OnCustom(IParameter parameter, Action<Grasshopper2.Doc.KeyedValues> write) =>
        On<IParameter>(parameter: parameter, mutate: target => { write(target.CustomValues); return unit; });

    private readonly record struct Rule(
        Func<IParameter, Fin<IParameter>> ApplyTo,
        Seq<(Func<object, bool> Predicate, string Message)> Validators,
        Func<PortKind, bool> CompatibleWith) {
        internal static Rule Universal(
            Func<IParameter, Fin<IParameter>>? applyTo = null,
            Seq<(Func<object, bool> Predicate, string Message)> validators = default) =>
            new(
                ApplyTo: applyTo ?? (static (IParameter parameter) => Fin.Succ(parameter)),
                Validators: validators,
                CompatibleWith: static _ => true);
        internal static Rule Mutating<TParam>(Func<PortKind, bool> compatibleWith, Func<TParam, Unit> mutate) where TParam : class =>
            new(
                ApplyTo: parameter => On(parameter: parameter, mutate: mutate),
                Validators: Seq<(Func<object, bool>, string)>(),
                CompatibleWith: compatibleWith);
        public static Rule operator +(Rule left, Rule right) =>
            new(
                ApplyTo: parameter => left.ApplyTo(parameter).Bind(p => right.ApplyTo(p)),
                Validators: left.Validators.Concat(right.Validators),
                CompatibleWith: kind => left.CompatibleWith(kind) && right.CompatibleWith(kind));
    }
}

// --- [MODELS] -----------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class PortKind {
    private delegate IParameter AddIn(InputAdder adder, string name, string code, string info, Access access, Requirement requirement);
    private delegate IParameter AddOut(OutputAdder adder, string name, string code, string info, Access access);
    public static readonly PortKind Path = Of<Grasshopper2.Data.Path>(key: nameof(Path), input: static (a, n, c, i, x, r) => a.AddPath(name: n, code: c, info: i, access: x, requirement: r), output: static (a, n, c, i, x) => a.AddPath(name: n, code: c, info: i, access: x));
    public static readonly PortKind Site = Of<Site>(key: nameof(Site), input: static (a, n, c, i, x, r) => a.AddSite(name: n, code: c, info: i, access: x, requirement: r), output: static (a, n, c, i, x) => a.AddSite(name: n, code: c, info: i, access: x));
    public static readonly PortKind Topological = Of<Guid>(key: nameof(Topological), input: static (a, n, c, i, x, r) => a.AddTopological(name: n, code: c, info: i));
    public static readonly PortKind Colour = Of<Grasshopper2.Types.Colour.Colour>(key: nameof(Colour), input: static (a, n, c, i, x, r) => a.AddColour(name: n, code: c, info: i, access: x, requirement: r), output: static (a, n, c, i, x) => a.AddColour(name: n, code: c, info: i, access: x));
    public static readonly PortKind Point = Of<Point3d>(key: nameof(Point), input: static (a, n, c, i, x, r) => a.AddPoint(name: n, code: c, info: i, access: x, requirement: r), output: static (a, n, c, i, x) => a.AddPoint(name: n, code: c, info: i, access: x));
    public static readonly PortKind Vector = Of<Vector3d>(key: nameof(Vector), input: static (a, n, c, i, x, r) => a.AddVector(name: n, code: c, info: i, access: x, requirement: r), output: static (a, n, c, i, x) => a.AddVector(name: n, code: c, info: i, access: x), policy: new Capability.Vector(Unitise: true));
    public static readonly PortKind Line = Of<Line>(key: nameof(Line), input: static (a, n, c, i, x, r) => a.AddLine(name: n, code: c, info: i, access: x, requirement: r), output: static (a, n, c, i, x) => a.AddLine(name: n, code: c, info: i, access: x));
    public static readonly PortKind Arc = Of<Arc>(key: nameof(Arc), input: static (a, n, c, i, x, r) => a.AddArc(name: n, code: c, info: i, access: x, requirement: r), output: static (a, n, c, i, x) => a.AddArc(name: n, code: c, info: i, access: x));
    public static readonly PortKind Circle = Of<Circle>(key: nameof(Circle), input: static (a, n, c, i, x, r) => a.AddCircle(name: n, code: c, info: i, access: x, requirement: r), output: static (a, n, c, i, x) => a.AddCircle(name: n, code: c, info: i, access: x));
    public static readonly PortKind Rectangle = Of<Rectangle3d>(key: nameof(Rectangle), input: static (a, n, c, i, x, r) => a.AddRectangle(name: n, code: c, info: i, access: x, requirement: r), output: static (a, n, c, i, x) => a.AddRectangle(name: n, code: c, info: i, access: x));
    public static readonly PortKind Curve = Of<Curve>(key: nameof(Curve), input: static (a, n, c, i, x, r) => a.AddCurve(name: n, code: c, info: i, access: x, requirement: r), output: static (a, n, c, i, x) => a.AddCurve(name: n, code: c, info: i, access: x));
    public static readonly PortKind Brep = Of<Brep>(key: nameof(Brep), input: static (a, n, c, i, x, r) => a.AddSurface(name: n, code: c, info: i, access: x, requirement: r), output: static (a, n, c, i, x) => a.AddSurface(name: n, code: c, info: i, access: x));
    public static readonly PortKind Surface = Of<Surface>(key: nameof(Surface), input: static (a, n, c, i, x, r) => a.AddSurface(name: n, code: c, info: i, access: x, requirement: r), output: static (a, n, c, i, x) => a.AddSurface(name: n, code: c, info: i, access: x));
    public static readonly PortKind Box = Of<Box>(key: nameof(Box), input: static (a, n, c, i, x, r) => a.AddBox(name: n, code: c, info: i, access: x, requirement: r), output: static (a, n, c, i, x) => a.AddBox(name: n, code: c, info: i, access: x));
    public static readonly PortKind Cage = Of<Cage>(key: nameof(Cage), input: static (a, n, c, i, x, r) => a.AddCage(name: n, code: c, info: i, access: x, requirement: r), output: static (a, n, c, i, x) => a.AddCage(name: n, code: c, info: i, access: x));
    public static readonly PortKind Sphere = Of<Sphere>(key: nameof(Sphere), input: static (a, n, c, i, x, r) => a.AddSphere(name: n, code: c, info: i, access: x, requirement: r), output: static (a, n, c, i, x) => a.AddSphere(name: n, code: c, info: i, access: x));
    public static readonly PortKind Plane = Of<Plane>(key: nameof(Plane), input: static (a, n, c, i, x, r) => a.AddPlane(name: n, code: c, info: i, access: x, requirement: r), output: static (a, n, c, i, x) => a.AddPlane(name: n, code: c, info: i, access: x));
    public static readonly PortKind Dot = Of<TextDot>(key: nameof(Dot), output: static (a, n, c, i, x) => a.AddDot(name: n, code: c, info: i, access: x));
    public static readonly PortKind Transform = Of<Transform>(key: nameof(Transform), input: static (a, n, c, i, x, r) => a.AddTransform(name: n, code: c, info: i, access: x, requirement: r), output: static (a, n, c, i, x) => a.AddTransform(name: n, code: c, info: i, access: x));
    public static readonly PortKind View = Of<Rhino.DocObjects.ViewportInfo>(key: nameof(View), input: static (a, n, c, i, x, r) => a.AddView(name: n, code: c, info: i, access: x, requirement: r), output: static (a, n, c, i, x) => a.AddView(name: n, code: c, info: i, access: x));
    public static readonly PortKind Graph = Of<Grasshopper2.Types.Graphs.Graph>(key: nameof(Graph), input: static (a, n, c, i, x, r) => a.AddGraph(name: n, code: c, info: i, access: x, requirement: r), output: static (a, n, c, i, x) => a.AddGraph(name: n, code: c, info: i, access: x));
    public static readonly PortKind Field = Of<Grasshopper2.Types.Fields.Field>(key: nameof(Field), input: static (a, n, c, i, x, r) => a.AddField(name: n, code: c, info: i, access: x, requirement: r), output: static (a, n, c, i, x) => a.AddField(name: n, code: c, info: i, access: x));
    public static readonly PortKind Function = Of<Grasshopper2.Types.Functions.Function>(key: nameof(Function), input: static (a, n, c, i, x, r) => a.AddFunction(name: n, code: c, info: i, access: x, requirement: r), output: static (a, n, c, i, x) => a.AddFunction(name: n, code: c, info: i, access: x));
    public static readonly PortKind Tuple = Of<Grasshopper2.Types.NTuple>(key: nameof(Tuple), input: static (a, n, c, i, x, r) => a.AddTuple(name: n, code: c, info: i, access: x, requirement: r), output: static (a, n, c, i, x) => a.AddTuple(name: n, code: c, info: i, access: x));
    public static readonly PortKind Integer = Of<int>(key: nameof(Integer), input: static (a, n, c, i, x, r) => a.AddInteger(name: n, code: c, info: i, access: x, requirement: r), output: static (a, n, c, i, x) => a.AddInteger(name: n, code: c, info: i, access: x));
    public static readonly PortKind Index = Of<int>(key: nameof(Index), input: static (a, n, c, i, x, r) => a.AddIndex(name: n, code: c, info: i, access: x, requirement: r), output: static (a, n, c, i, x) => a.AddInteger(name: n, code: c, info: i, access: x), policy: new Capability.Index());
    public static readonly PortKind Interval = Of<Interval>(key: nameof(Interval), input: static (a, n, c, i, x, r) => a.AddInterval(name: n, code: c, info: i, access: x, requirement: r), output: static (a, n, c, i, x) => a.AddInterval(name: n, code: c, info: i, access: x));
    public static readonly PortKind Angle = Of<Angle>(key: nameof(Angle), input: static (a, n, c, i, x, r) => a.AddAngle(name: n, code: c, info: i, access: x, requirement: r), output: static (a, n, c, i, x) => a.AddAngle(name: n, code: c, info: i, access: x), policy: new Capability.Angle(Reduce: true));
    public static readonly PortKind Number = Of<double>(key: nameof(Number), input: static (a, n, c, i, x, r) => a.AddNumber(name: n, code: c, info: i, access: x, requirement: r), output: static (a, n, c, i, x) => a.AddNumber(name: n, code: c, info: i, access: x));
    public static readonly PortKind Complex = Of<System.Numerics.Complex>(key: nameof(Complex), input: static (a, n, c, i, x, r) => a.AddComplex(name: n, code: c, info: i, access: x, requirement: r), output: static (a, n, c, i, x) => a.AddComplex(name: n, code: c, info: i, access: x));
    public static readonly PortKind Numeric = Of<object>(key: nameof(Numeric), input: static (a, n, c, i, x, r) => a.AddNumeric(name: n, code: c, info: i, access: x, requirement: r), output: static (a, n, c, i, x) => a.AddNumeric(name: n, code: c, info: i, access: x));
    public static readonly PortKind Guid = Of<Guid>(key: nameof(Guid), output: static (a, n, c, i, x) => a.AddGuid(name: n, code: c, info: i, access: x));
    public static readonly PortKind Random = Of<Grasshopper2.Types.Random.RandomEngine>(key: nameof(Random), input: static (a, n, c, i, x, r) => a.AddRandom(name: n, code: c, info: i, access: x, requirement: r), output: static (a, n, c, i, x) => a.AddRandom(name: n, code: c, info: i, access: x));
    public static readonly PortKind ContinuousDistribution = Of<Grasshopper2.Types.Random.ContinuousDistribution>(key: nameof(ContinuousDistribution), input: static (a, n, c, i, x, r) => a.AddContinuous(name: n, code: c, info: i, access: x, requirement: r), output: static (a, n, c, i, x) => a.AddContinuous(name: n, code: c, info: i, access: x));
    public static readonly PortKind DiscreteDistribution = Of<Grasshopper2.Types.Random.DiscreteDistribution>(key: nameof(DiscreteDistribution), input: static (a, n, c, i, x, r) => a.AddDiscrete(name: n, code: c, info: i, access: x, requirement: r), output: static (a, n, c, i, x) => a.AddDiscrete(name: n, code: c, info: i, access: x));
    public static readonly PortKind Boolean = Of<bool>(key: nameof(Boolean), input: static (a, n, c, i, x, r) => a.AddBoolean(name: n, code: c, info: i, access: x, requirement: r), output: static (a, n, c, i, x) => a.AddBoolean(name: n, code: c, info: i, access: x));
    public static readonly PortKind Text = Of<string>(key: nameof(Text), input: static (a, n, c, i, x, r) => a.AddText(name: n, code: c, info: i, access: x, requirement: r), output: static (a, n, c, i, x) => a.AddText(name: n, code: c, info: i, access: x));
    public static readonly PortKind TextPattern = Of<string>(key: nameof(TextPattern), input: static (a, n, c, i, x, r) => a.AddTextPattern(name: n, code: c, info: i, access: x, requirement: r));
    public static readonly PortKind Gradient = Of<Grasshopper2.Types.Colour.Gradient>(key: nameof(Gradient), input: static (a, n, c, i, x, r) => a.AddGradient(name: n, code: c, info: i, access: x, requirement: r), output: static (a, n, c, i, x) => a.AddGradient(name: n, code: c, info: i, access: x));
    public static readonly PortKind DateTime = Of<DateTime>(key: nameof(DateTime), input: static (a, n, c, i, x, r) => a.AddDateTime(name: n, code: c, info: i, access: x, requirement: r), output: static (a, n, c, i, x) => a.AddDateTime(name: n, code: c, info: i, access: x));
    public static readonly PortKind TimeSpan = Of<TimeSpan>(key: nameof(TimeSpan), input: static (a, n, c, i, x, r) => a.AddTimeSpan(name: n, code: c, info: i, access: x, requirement: r), output: static (a, n, c, i, x) => a.AddTimeSpan(name: n, code: c, info: i, access: x));
#pragma warning disable CS0618
    public static readonly PortKind Language = Of<Grasshopper2.Types.Linguistic.Language>(key: nameof(Language), input: static (a, n, c, i, x, r) => a.AddLanguage(name: n, code: c, info: i, access: x, requirement: r), output: static (a, n, c, i, x) => a.AddLanguage(name: n, code: c, info: i, access: x));
#pragma warning restore CS0618
    public static readonly PortKind MetaName = Of<MetaName>(key: nameof(MetaName), input: static (a, n, c, i, x, r) => a.AddMetaName(name: n, code: c, info: i, access: x, requirement: r), output: static (a, n, c, i, x) => a.AddMetaName(name: n, code: c, info: i, access: x));
    public static readonly PortKind MetaData = Of<MetaData>(key: nameof(MetaData), input: static (a, n, c, i, x, r) => a.AddMetaData(name: n, code: c, info: i, access: x, requirement: r), output: static (a, n, c, i, x) => a.AddMetaData(name: n, code: c, info: i, access: x));
    public static readonly PortKind Mesh = Of<Mesh>(key: nameof(Mesh), input: static (a, n, c, i, x, r) => a.AddMesh(name: n, code: c, info: i, access: x, requirement: r), output: static (a, n, c, i, x) => a.AddMesh(name: n, code: c, info: i, access: x));
    public static readonly PortKind Polyline = Of<Polyline>(key: nameof(Polyline), input: static (a, n, c, i, x, r) => a.AddPolyline(name: n, code: c, info: i, access: x, requirement: r), output: static (a, n, c, i, x) => a.AddPolyline(name: n, code: c, info: i, access: x));
    public static readonly PortKind Generic = Of<object>(key: nameof(Generic), input: static (a, n, c, i, x, r) => a.AddGeneric(name: n, code: c, info: i, access: x, requirement: r), output: static (a, n, c, i, x) => a.AddGeneric(name: n, code: c, info: i, access: x));
    public static readonly PortKind Triangle = Of<Triangle>(key: nameof(Triangle), input: static (a, n, c, i, x, r) => a.AddTriangle(name: n, code: c, info: i, access: x, requirement: r), output: static (a, n, c, i, x) => a.AddTriangle(name: n, code: c, info: i, access: x));
    public static readonly PortKind Tube = Of<Tube>(key: nameof(Tube), input: static (a, n, c, i, x, r) => a.AddTube(name: n, code: c, info: i, access: x, requirement: r), output: static (a, n, c, i, x) => a.AddTube(name: n, code: c, info: i, access: x));
    public static readonly PortKind Region = Of<Grasshopper2.Types.Shapes.Region>(key: nameof(Region), input: static (a, n, c, i, x, r) => a.AddRegion(name: n, code: c, info: i, access: x, requirement: r), output: static (a, n, c, i, x) => a.AddRegion(name: n, code: c, info: i, access: x));
    public static readonly PortKind CurveLocus = Of<CurveLocus>(key: nameof(CurveLocus), input: static (a, n, c, i, x, r) => a.AddCurveLocus(name: n, code: c, info: i, access: x, requirement: r), output: static (a, n, c, i, x) => a.AddCurveLocus(name: n, code: c, info: i, access: x));
    public static readonly PortKind SurfaceLocus = Of<SurfaceLocus>(key: nameof(SurfaceLocus), input: static (a, n, c, i, x, r) => a.AddSurfaceLocus(name: n, code: c, info: i, access: x, requirement: r), output: static (a, n, c, i, x) => a.AddSurfaceLocus(name: n, code: c, info: i, access: x));
    public static readonly PortKind MeshFacet = Of<MeshFace>(key: nameof(MeshFacet), input: static (a, n, c, i, x, r) => a.AddMeshFacet(name: n, code: c, info: i, access: x, requirement: r), output: static (a, n, c, i, x) => a.AddMeshFacet(name: n, code: c, info: i, access: x));
    public static readonly PortKind NPoint = Of<Grasshopper2.Types.Coordinates.NPoint>(key: nameof(NPoint), input: static (a, n, c, i, x, r) => a.AddNPoint(name: n, code: c, info: i, access: x, requirement: r), output: static (a, n, c, i, x) => a.AddNPoint(name: n, code: c, info: i, access: x));
    public static readonly PortKind UvPoint = Of<Point2d>(key: nameof(UvPoint), input: static (a, n, c, i, x, r) => a.AddUvPoint(name: n, code: c, info: i, access: x, requirement: r), output: static (a, n, c, i, x) => a.AddUvPoint(name: n, code: c, info: i, access: x));
    public static readonly PortKind Deform = Of<Deform>(key: nameof(Deform), input: static (a, n, c, i, x, r) => a.AddDeform(name: n, code: c, info: i, access: x, requirement: r), output: static (a, n, c, i, x) => a.AddDeform(name: n, code: c, info: i, access: x));
    public Type Type { get; }
    public Type WireType { get; }
    internal bool SupportsInput { get; }
    internal bool SupportsOutput { get; }
    internal Capability DefaultPolicy { get; }
    [UseDelegateFromConstructor] private partial IParameter AddInput(InputAdder adder, string name, string code, string info, Access access, Requirement requirement);
    [UseDelegateFromConstructor] private partial IParameter AddOutput(OutputAdder adder, string name, string code, string info, Access access);
    internal static Seq<(Type Type, Seq<PortKind> Kinds)> TypeCollisions =>
        TypeSideGroups.Choose(static group => TypeDefault(type: group.Type, side: group.Side, kinds: group.Kinds).IsSome
            ? Option<(Type Type, Seq<PortKind> Kinds)>.None
            : Some((group.Type, group.Kinds)));
    public static Option<PortKind> From(Type type) => From(type: type, side: Side.Input);
    internal static Option<PortKind> From(Type type, Side side) {
        ArgumentNullException.ThrowIfNull(argument: type);
        return type == typeof(Shape)
            ? Some(Generic)
            : type.IsEnum && System.Enum.GetUnderlyingType(enumType: type) == typeof(int) && !type.IsDefined(attributeType: typeof(FlagsAttribute), inherit: false)
                ? EnumKind(type: type)
            : TypeDefaults.Value.TryGetValue(key: (type, side), value: out PortKind? kind) ? Some(kind) : Option<PortKind>.None;
    }
    public static PortKind Register<T>(
        string key,
        Func<InputAdder, string, string, string, Access, Requirement, IParameter>? input = null,
        Func<OutputAdder, string, string, string, Access, IParameter>? output = null,
        Type? wireType = null,
        Capability? policy = null) =>
        Of<T>(
            key: key,
            input: input is null ? null : (adder, name, code, info, access, requirement) => input(adder, name, code, info, access, requirement),
            output: output is null ? null : (adder, name, code, info, access) => output(adder, name, code, info, access),
            wireType: wireType,
            policy: policy);
    public static PortKind Enum<T>(T initial) where T : struct, Enum =>
        (System.Enum.GetUnderlyingType(enumType: typeof(T)), typeof(T).IsDefined(attributeType: typeof(FlagsAttribute), inherit: false)) switch {
            (Type backing, _) when backing != typeof(int) => throw new ArgumentException(message: "Grasshopper enum ports require System.Int32-backed enums.", paramName: nameof(initial)),
            (_, true) => throw new ArgumentException(message: "Grasshopper enum ports do not support flags enums.", paramName: nameof(initial)),
            _ => Of<T>(
                key: typeof(T).FullName ?? typeof(T).Name,
                input: (a, n, c, i, x, r) => a.AddEnum(name: n, code: c, info: i, initial: initial, access: x, requirement: r),
                output: static (a, n, c, i, x) => a.AddEnum<T>(name: n, code: c, info: i, access: x),
                wireType: typeof(int)),
        };
    internal bool Accepts(Type type, Side side) => (this == Generic || Type == type) && Supports(side: side);
    internal bool Supports(Side side) => side switch {
        Side.Input => SupportsInput,
        Side.Output => SupportsOutput,
        _ => false,
    };
    internal bool RequiresWire<T>() => Type == typeof(T) && WireType != typeof(T);
    internal Fin<Flow<T>> Decode<T>(Flow<int> value) =>
        guard(System.Enum.IsDefined(enumType: Type, value: value.Item), Op.Of(name: Type.Name).InvalidInput()).ToFin()
            .Map(_ => value.Project(item: (T)System.Enum.ToObject(enumType: Type, value: value.Item)));
    internal static Flow<int> Encode<T>(Flow<T> value) =>
        value.Project(item: Convert.ToInt32(value: value.Item, provider: CultureInfo.InvariantCulture));
    internal IParameter Bind(ModularInputAdder adder, Port port, bool hidden) {
        ArgumentNullException.ThrowIfNull(argument: adder);
        ArgumentNullException.ThrowIfNull(argument: port);
        return Apply(parameter: AddInput(adder: adder.RegularAdder, name: port.Name, code: port.Code, info: port.Info, access: port.Access, requirement: port.Requirement), policy: hidden ? port.Policy + new Capability.Hidden() : port.Policy);
    }
    internal IParameter Bind(ModularOutputAdder adder, Port port, bool hidden) {
        ArgumentNullException.ThrowIfNull(argument: adder);
        ArgumentNullException.ThrowIfNull(argument: port);
        return Apply(parameter: AddOutput(adder: adder.RegularAdder, name: port.Name, code: port.Code, info: port.Info, access: port.Access), policy: hidden ? port.Policy + new Capability.Hidden() : port.Policy);
    }
    // BOUNDARY ADAPTER -- component specs are developer contracts; incompatible policies must not degrade native registration silently.
    private static IParameter Apply(IParameter parameter, Capability policy) =>
        policy.Apply(parameter: parameter).IfFail(error => throw new InvalidOperationException(message: error.Message));
    private static PortKind Of<T>(string key, AddIn? input = null, AddOut? output = null, Type? wireType = null, Capability? policy = null) =>
        new(
            key: key,
            type: typeof(T),
            wireType: wireType ?? typeof(T),
            supportsInput: input is not null,
            supportsOutput: output is not null,
            defaultPolicy: policy ?? Capability.Empty,
            addInput: (adder, name, code, info, access, requirement) => (input ?? Unsupported)(adder: adder, name: name, code: code, info: info, access: access, requirement: requirement),
            addOutput: (adder, name, code, info, access) => (output ?? Unsupported)(adder: adder, name: name, code: code, info: info, access: access));
    // BOUNDARY ADAPTER -- registration-time fallback for a side the kind cannot host; validation rejects such specs before this fires.
    private static IParameter Unsupported(InputAdder adder, string name, string code, string info, Access access, Requirement requirement) =>
        throw new NotSupportedException(message: $"Port kind does not support input registration: {name}.");
    private static IParameter Unsupported(OutputAdder adder, string name, string code, string info, Access access) =>
        throw new NotSupportedException(message: $"Port kind does not support output registration: {name}.");
    private static Seq<(Type Type, Seq<PortKind> Kinds)> TypeGroups =>
        toSeq(Items.GroupBy(keySelector: static kind => kind.Type).Select(static group => (Type: group.Key, Kinds: toSeq(group))));
    private static Seq<(Type Type, Side Side, Seq<PortKind> Kinds)> TypeSideGroups =>
        toSeq(TypeGroups.Bind(static group => Seq(Side.Input, Side.Output)
            .Map(side => (group.Type, Side: side, Kinds: group.Kinds.Filter(kind => kind.Supports(side: side))))
            .Filter(static group => !group.Kinds.IsEmpty)));
    private static Option<PortKind> TypeDefault(Type type, Side side, Seq<PortKind> kinds) =>
        ExplicitTypeDefault(type: type, side: side).Match(
            Some: kind => kinds.Exists(candidate => candidate == kind) ? Some(kind) : Option<PortKind>.None,
            None: () => kinds.Count == 1 ? kinds.Head : Option<PortKind>.None);
    private static readonly FrozenDictionary<(Type Type, Side Side), PortKind> ExplicitDefaults =
        new Dictionary<(Type, Side), PortKind> {
            [(typeof(int), Side.Input)] = Integer, [(typeof(int), Side.Output)] = Integer,
            [(typeof(string), Side.Input)] = Text, [(typeof(string), Side.Output)] = Text,
            [(typeof(object), Side.Input)] = Generic, [(typeof(object), Side.Output)] = Generic,
            [(typeof(Guid), Side.Input)] = Topological, [(typeof(Guid), Side.Output)] = Guid,
        }.ToFrozenDictionary();
    private static Option<PortKind> ExplicitTypeDefault(Type type, Side side) =>
        ExplicitDefaults.TryGetValue(key: (type, side), value: out PortKind? kind) ? Some(kind) : Option<PortKind>.None;
    private static readonly Lazy<FrozenDictionary<(Type Type, Side Side), PortKind>> TypeDefaults =
        new(static () => TypeSideGroups
            .Choose(static group => TypeDefault(type: group.Type, side: group.Side, kinds: group.Kinds).Map(kind => (group.Type, group.Side, Kind: kind)))
            .ToFrozenDictionary(keySelector: static item => (item.Type, item.Side), elementSelector: static item => item.Kind));
    private static PortKind EnumDefault<T>() where T : struct, Enum {
        T[] values = System.Enum.GetValues<T>();
        return Enum(initial: values.Length > 0 ? values[0] : default);
    }
    private static readonly MethodInfo EnumDefaultMethod =
        typeof(PortKind).GetMethod(name: nameof(EnumDefault), bindingAttr: BindingFlags.NonPublic | BindingFlags.Static)!;
    private static Option<PortKind> EnumKind(Type type) =>
        Optional(EnumDefaultMethod.MakeGenericMethod(typeArguments: [type]).Invoke(obj: null, parameters: null) as PortKind);
}

public abstract class Port {
    private protected Port(string name, string code, string info, PortKind kind, Access access, Requirement requirement, Capability policy) {
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
    public Capability Policy { get; }
    public static Port<TVal> Of<TVal>(
        string name,
        string code,
        string info,
        Access access = Access.Item,
        Requirement requirement = Requirement.MustExist,
        PortKind? kind = null,
        Capability? policy = null) =>
        Of<TVal>(name: name, code: code, info: info, access: access, requirement: requirement, kind: kind, policy: policy, side: Side.Input);
    internal static Port<TVal> Of<TVal>(
        string name,
        string code,
        string info,
        Access access,
        Requirement requirement,
        PortKind? kind,
        Capability? policy,
        Side side = Side.Input) {
        ArgumentNullException.ThrowIfNull(argument: name);
        ArgumentNullException.ThrowIfNull(argument: code);
        ArgumentNullException.ThrowIfNull(argument: info);
        Type type = typeof(TVal);
        PortKind resolvedKind = Optional(kind)
            .Map(candidate => candidate.Accepts(type: type, side: side) switch {
                true => candidate,
                false => throw new ArgumentException(message: $"Port kind '{candidate}' cannot register {type.Name} on {side}.", paramName: nameof(kind)),
            })
            .IfNone(() => PortKind.From(type: type, side: side).IfNone(PortKind.Generic));
        Access resolvedAccess = resolvedKind == PortKind.Topological ? Access.Tree : access;
        Requirement resolvedRequirement = resolvedKind == PortKind.Topological ? Requirement.MayBeMissing : requirement;
        Capability resolved = policy ?? resolvedKind.DefaultPolicy;
        return new(
            name: name,
            code: code,
            info: info,
            kind: resolvedKind,
            access: resolvedAccess,
            requirement: resolvedRequirement,
            policy: resolvedRequirement switch {
                Requirement.MayBeMissing => resolved + new Capability.Elective() + new Capability.Category(Name: "Optional"),
                _ => resolved,
            });
    }
    public static Port<Shape> Shape(
        string name = "Geometry",
        string code = "G",
        string info = "Geometry to analyse.",
        Access access = Access.Tree) =>
        Of<Shape>(name: name, code: code, info: info, access: access);
    public static Port<TVal> Enum<TVal>(
        string name,
        string code,
        string info,
        TVal initial,
        Access access = Access.Item) where TVal : struct, Enum =>
        Of<TVal>(name: name, code: code, info: info, access: access, kind: PortKind.Enum(initial: initial), policy: Capability.Default(value: initial));
}
public sealed class Port<TVal> : Port {
    internal Port(
        string name,
        string code,
        string info,
        PortKind kind,
        Access access,
        Requirement requirement,
        Capability policy) : base(name: name, code: code, info: info, kind: kind, access: access, requirement: requirement, policy: policy) { }
}
