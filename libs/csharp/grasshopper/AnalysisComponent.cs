using Analysis;
using Core.Domain;
using Core.Runtime;
using Grasshopper2.Components;
using Grasshopper2.Parameters;
using Grasshopper2.UI;
using GrasshopperIO;
using LanguageExt;
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
        _ = GeometryParameterKind
            .From(clrType: typeof(TInput))
            .Match(
                Some: (GeometryParameterKind kind) => kind.Build(
                    adder: inputs,
                    name: Input.Name,
                    code: Input.Code,
                    info: Input.Description,
                    access: Input.Access,
                    requirement: Input.Requirement),
                None: () => AddGenericInput(adder: inputs, spec: Input));
        _ = IndexInput.Match(
            Some: (IndexInputSpec spec) => AddIndexSlot(inputs: inputs, spec: spec),
            None: static () => Unit.Default);
    }
    private static Unit AddGenericInput(InputAdder adder, InputSpec spec) {
        _ = adder.AddGeneric(
            name: spec.Name,
            code: spec.Code,
            info: spec.Description,
            access: spec.Access,
            requirement: spec.Requirement);
        return Unit.Default;
    }
    private static Unit AddIndexSlot(InputAdder inputs, IndexInputSpec spec) {
        _ = inputs.AddIndex(
            name: spec.Name,
            code: spec.Code,
            info: spec.Description,
            access: Access.Item,
            requirement: spec.Requirement);
        return Unit.Default;
    }
    protected sealed override void AddOutputs(OutputAdder outputs) {
        ArgumentNullException.ThrowIfNull(argument: outputs);
        _ = Outputs
            .Map((IBridgeOutput<TInput> output) => GeometryParameterKind
                .From(clrType: output.ValueType)
                .Match(
                    Some: (GeometryParameterKind kind) => kind.Build(
                        adder: outputs,
                        name: output.Name,
                        code: output.Code,
                        info: output.Description,
                        access: Access.Twig),
                    None: () => AddGenericOutput(adder: outputs, output: output)))
            .Strict();
    }
    private static Unit AddGenericOutput(OutputAdder adder, IBridgeOutput<TInput> output) {
        _ = adder.AddGeneric(
            name: output.Name,
            code: output.Code,
            info: output.Description,
            access: Access.Twig);
        return Unit.Default;
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
