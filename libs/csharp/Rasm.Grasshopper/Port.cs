using Grasshopper2.Components;
using Grasshopper2.Data;
using Grasshopper2.Data.Meta;
using Grasshopper2.Parameters;
using Grasshopper2.Parameters.Standard;
using Grasshopper2.UI;
namespace Rasm.Grasshopper;

// --- [TYPES] ---------------------------------------------------------------------------

public interface IPort {
    public string Name { get; }
    public string Code { get; }
    public string Info { get; }
    public Type Type { get; }
    public Param Param { get; }
    public Access Access { get; }
    public Requirement Requirement { get; }
    public PortPolicy Policy { get; }
}

// --- [MODELS] --------------------------------------------------------------------------

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
    public Option<TVal> Value =>
        Values.Find(static value => !value.IsNull).Map(static value => value.Value);
    public Seq<TVal> NonNullValues =>
        Values.Filter(static value => !value.IsNull).Map(static value => value.Value);
}

public readonly record struct Port<TVal>(
    string Name,
    string Code,
    string Info,
    Param Param,
    Access Access,
    Requirement Requirement,
    PortPolicy Policy) : IPort {
    public Type Type => typeof(TVal);
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class Port {
    public static Port<TVal> Required<TVal>(string name, string code, string info, Param? param = null, PortPolicy? policy = null) =>
        new(Name: name, Code: code, Info: info, Param: param ?? Param.From(type: typeof(TVal)).IfNone(Param.Generic), Access: Access.Item, Requirement: Requirement.MustExist, Policy: policy ?? PortPolicy.Empty);
    public static Port<TVal> Optional<TVal>(string name, string code, string info, Param? param = null, PortPolicy? policy = null) =>
        new(Name: name, Code: code, Info: info, Param: param ?? Param.From(type: typeof(TVal)).IfNone(Param.Generic), Access: Access.Item, Requirement: Requirement.MayBeMissing, Policy: policy ?? PortPolicy.Empty);
    public static Port<TVal> List<TVal>(string name, string code, string info, Requirement requirement = Requirement.MustExist, Param? param = null, PortPolicy? policy = null) =>
        new(Name: name, Code: code, Info: info, Param: param ?? Param.From(type: typeof(TVal)).IfNone(Param.Generic), Access: Access.Twig, Requirement: requirement, Policy: policy ?? PortPolicy.Empty);
    public static Port<TVal> Tree<TVal>(string name, string code, string info, Requirement requirement = Requirement.MustExist, Param? param = null, PortPolicy? policy = null) =>
        new(Name: name, Code: code, Info: info, Param: param ?? Param.From(type: typeof(TVal)).IfNone(Param.Generic), Access: Access.Tree, Requirement: requirement, Policy: policy ?? PortPolicy.Empty);
    public static Port<int> Index(
        string name = "Index",
        string code = "I",
        string info = "Zero-based selector; clamped to [0, count-1].") =>
        new(Name: name, Code: code, Info: info, Param: Param.Index, Access: Access.Item, Requirement: Requirement.MayBeMissing, Policy: PortPolicy.Index());
}
