using System.Reflection;
using Grasshopper2.Components;
using Grasshopper2.UI;
using GrasshopperIO;
using Rasm.Analysis;
namespace Rasm.Grasshopper;

// --- [SERVICES] ------------------------------------------------------------------------

/// <summary>
/// Polymorphic base for Rasm-family Grasshopper 2 components. Subclasses own typed input
/// acquisition, while this base owns port declaration, scope resolution, output execution,
/// and canvas fault routing.
/// </summary>
public abstract class Component<TIn> : Grasshopper2.Components.Component where TIn : notnull {
    protected static Nomen NomenOf<TSelf>() where TSelf : Component<TIn> =>
        typeof(TSelf).GetCustomAttribute<NomenAttribute>()?.Nomen
            ?? new Nomen(name: typeof(TSelf).Name, info: string.Empty);
    protected abstract Seq<IOutput<TIn>> Slots { get; }
    protected abstract Seq<IPort> Inputs { get; }

    protected Component(Nomen nomen) : base(nomen: nomen) { }
    protected Component(IReader reader) : base(reader: reader) { }

    protected sealed override void AddInputs(InputAdder inputs) {
        ArgumentNullException.ThrowIfNull(argument: inputs);
        _ = Inputs.Iter(port => port.Param.Bind(adder: inputs, name: port.Name, code: port.Code, info: port.Info, access: port.Access, requirement: port.Requirement));
    }
    protected sealed override void AddOutputs(OutputAdder outputs) {
        ArgumentNullException.ThrowIfNull(argument: outputs);
        _ = Slots.Map(slot => slot.Port.Param.Bind(adder: outputs, name: slot.Port.Name, code: slot.Port.Code, info: slot.Port.Info, access: slot.Port.Access)).Strict();
    }
    protected sealed override void Process(IDataAccess access) {
        ArgumentNullException.ThrowIfNull(argument: access);
        _ = access.Scope().Bind(scope => Read(access: access).Map(input => (Scope: scope, Hint: Hint(access: access), Input: input)))
            .Match(
                Succ: state => Slots.Iter((i, slot) => slot.Run(access: access, slot: i, scope: state.Scope, hint: state.Hint, input: state.Input)),
                Fail: error => (
                    access.MissingInput(error: error),
                    Slots.Iter((i, slot) => slot.Empty(access: access, slot: i))).Item2);
    }
    protected abstract Fin<TIn> Read(IDataAccess access);
    protected virtual Option<int> Hint(IDataAccess access) =>
        Option<int>.None;
}
