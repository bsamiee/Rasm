using Analysis;
using Core.Domain;
using Core.Runtime;
using Grasshopper2.Components;
using Grasshopper2.Parameters;
using Grasshopper2.Parameters.Standard;
using Grasshopper2.UI;
using GrasshopperIO;
using LanguageExt;
using Rhino.Geometry;
using static LanguageExt.Prelude;
namespace Grasshopper;

// --- [TYPES] -----------------------------------------------------------------------------------

public readonly record struct InputSpec(
    string Name,
    string Code,
    string Description,
    Access Access,
    Requirement Requirement) {
    public static InputSpec Generic =>
        new(Name: "Geometry", Code: "G", Description: "Geometry to analyse.", Access: Access.Item, Requirement: Requirement.MustExist);
}

public readonly record struct IndexInputSpec(
    string Name,
    string Code,
    string Description,
    int Default,
    Requirement Requirement) {
    public static IndexInputSpec Standard =>
        new(Name: "Index", Code: "I",
            Description: "Zero-based selector; clamped to the available range.",
            Default: 0, Requirement: Requirement.MayBeMissing);
}

public interface IBridgeOutput<TInput> where TInput : notnull {
    public string Name { get; }
    public string Code { get; }
    public string Description { get; }
    public Type ValueType { get; }
    public Unit Execute(IDataAccess access, int index, AnalysisRuntime scope, TInput geometry);
    public Unit WriteEmpty(IDataAccess access, int index);
}

// --- [CONSTANTS] -------------------------------------------------------------------------------

internal static class ParameterFactory {
    // Boundary-tier registry mapping Rhino.Geometry CLR types to canonical Grasshopper2.Parameters.Standard
    // implementations. Keys are CLR types; values are static factories returning IParameter via the
    // (name, code, info, access) ctor inherited from Grasshopper2.Parameters.Parameter. Lookup is O(1)
    // through immutable LanguageExt HashMap; absence falls through to GenericParameter (the universal
    // IParameter for object). This is the GH2-native polymorphic seam — OutputAdder.Add(IParameter)
    // and InputAdder.Add(IParameter, Requirement) accept any concrete implementation built here.
    internal static readonly HashMap<Type, Func<string, string, string, Access, IParameter>> Registry = HashMap(
        (typeof(Point3d), static (string n, string c, string i, Access a) => (IParameter)new Point3Parameter(n, c, i, a)),
        (typeof(Vector3d), static (string n, string c, string i, Access a) => (IParameter)new VectorParameter(n, c, i, a)),
        (typeof(Curve), static (string n, string c, string i, Access a) => (IParameter)new CurveParameter(n, c, i, a)),
        (typeof(Surface), static (string n, string c, string i, Access a) => (IParameter)new SurfaceParameter(n, c, i, a)),
        (typeof(Brep), static (string n, string c, string i, Access a) => (IParameter)new SurfaceParameter(n, c, i, a)),
        (typeof(Mesh), static (string n, string c, string i, Access a) => (IParameter)new MeshParameter(n, c, i, a)),
        (typeof(Box), static (string n, string c, string i, Access a) => (IParameter)new BoxParameter(n, c, i, a)),
        (typeof(Plane), static (string n, string c, string i, Access a) => (IParameter)new PlaneParameter(n, c, i, a)),
        (typeof(Line), static (string n, string c, string i, Access a) => (IParameter)new LineParameter(n, c, i, a)),
        (typeof(Circle), static (string n, string c, string i, Access a) => (IParameter)new CircleParameter(n, c, i, a)),
        (typeof(Arc), static (string n, string c, string i, Access a) => (IParameter)new ArcParameter(n, c, i, a)),
        (typeof(Sphere), static (string n, string c, string i, Access a) => (IParameter)new SphereParameter(n, c, i, a)));
    internal static readonly Func<string, string, string, Access, IParameter> Fallback =
        static (string n, string c, string i, Access a) => (IParameter)new GenericParameter(n, c, i, a);
    internal static IParameter Build(Type kind, string name, string code, string info, Access access) =>
        Registry
            .Find(key: kind)
            .Match(
                Some: (Func<string, string, string, Access, IParameter> factory) => factory(name, code, info, access),
                None: () => Fallback(name, code, info, access));
    internal static IParameter BuildIndex(string name, string code, string info, Access access) =>
        (IParameter)new IntegerParameter(name, code, info, access) { IsIndex = true };
}

// --- [SERVICES] --------------------------------------------------------------------------------

/// <summary>
/// Polymorphic base for analysis-style Grasshopper 2 components. Subclasses declare
/// <see cref="Outputs"/> as a heterogeneous sequence of <see cref="IBridgeOutput{TInput}"/> — each
/// slot carries its own value type — and a <see cref="Nomen"/>; the base owns input/output
/// declaration and the query execution loop. Both input and output types route through
/// <see cref="ParameterFactory"/> to the canonical Grasshopper2 <see cref="IParameter"/> for the
/// CLR type. Override <see cref="Input"/> to customise the geometry input slot, or
/// <see cref="IndexInput"/> to add an optional integer index input that is propagated to queries
/// via <see cref="AnalysisRuntime.Index"/>.
/// </summary>
public abstract class AnalysisComponent<TInput> : Component where TInput : notnull {
    protected abstract Seq<IBridgeOutput<TInput>> Outputs { get; }
    protected virtual InputSpec Input =>
        InputSpec.Generic;
    protected virtual Option<IndexInputSpec> IndexInput =>
        None;
    protected AnalysisComponent(Nomen nomen) : base(nomen: nomen) { }
    protected AnalysisComponent(IReader reader) : base(reader: reader) { }
    protected sealed override void AddInputs(InputAdder inputs) {
        ArgumentNullException.ThrowIfNull(argument: inputs);
        inputs.Add(
            parameter: ParameterFactory.Build(
                kind: typeof(TInput),
                name: Input.Name,
                code: Input.Code,
                info: Input.Description,
                access: Input.Access),
            requirement: Input.Requirement);
        _ = IndexInput.Match(
            Some: (IndexInputSpec spec) => AddIndexSlot(inputs: inputs, spec: spec),
            None: static () => Unit.Default);
    }
    private static Unit AddIndexSlot(InputAdder inputs, IndexInputSpec spec) {
        inputs.Add(
            parameter: ParameterFactory.BuildIndex(
                name: spec.Name,
                code: spec.Code,
                info: spec.Description,
                access: Access.Item),
            requirement: spec.Requirement);
        return Unit.Default;
    }
    protected sealed override void AddOutputs(OutputAdder outputs) {
        ArgumentNullException.ThrowIfNull(argument: outputs);
        _ = Outputs.Iter((IBridgeOutput<TInput> output) => outputs.Add(
            parameter: ParameterFactory.Build(
                kind: output.ValueType,
                name: output.Name,
                code: output.Code,
                info: output.Description,
                access: Access.Twig)));
    }
    protected sealed override void Process(IDataAccess access) {
        ArgumentNullException.ThrowIfNull(argument: access);
        AnalysisRuntime scope = access.ResolveScope();
        AnalysisRuntime scoped = IndexInput.Match(
            Some: (IndexInputSpec spec) => scope with {
                Index = IndexHint
                    .Create(value: (access.GetItem(index: 1, value: out int candidate), candidate) switch {
                        (true, int value) => value,
                        _ => spec.Default,
                    })
                    .Match(
                        Succ: static (IndexHint hint) => Some(hint),
                        Fail: static (Error _) => Option<IndexHint>.None),
            },
            None: () => scope);
        _ = (access.GetItem(index: 0, value: out object? item), item) switch {
            (true, TInput input) => Outputs.Iter((int slot, IBridgeOutput<TInput> output) =>
                output.Execute(access: access, index: slot, scope: scoped, geometry: input)),
            _ => HandleMissingInput(access: access),
        };
    }
    private Unit HandleMissingInput(IDataAccess access) {
        _ = access.AddMissingInputError(label: Input.Name);
        return Outputs.Iter((int slot, IBridgeOutput<TInput> output) =>
            output.WriteEmpty(access: access, index: slot));
    }
}
