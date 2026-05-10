using System.Reflection;
using Analysis;
using Core.Domain;
using Grasshopper2.Components;
using Grasshopper2.Parameters;
using Grasshopper2.UI;
using GrasshopperIO;
namespace Grasshopper;

// --- [SERVICES] --------------------------------------------------------------------------------

/// <summary>
/// Polymorphic base for Rasm-family Grasshopper 2 components. Subclasses declare a Nomen, the
/// heterogeneous output catalog (<see cref="Slots"/>) of <see cref="IOutput{TIn}"/> entries — each
/// slot carries its own value type — and (optionally) the primary input shape and any auxiliary
/// scalar/index inputs. The base owns input/output declaration via <see cref="Param.Resolve"/> and
/// the <see cref="Process"/> lifecycle: resolve scope, resolve primary input, iterate slots,
/// route faults to the canvas. The <typeparamref name="TIn"/> constraint pins the boundary to the
/// closed <see cref="RhinoGeometry"/> Union; non-Union inputs surface an explicit canvas error.
/// </summary>
public abstract class Component<TIn> : Grasshopper2.Components.Component where TIn : RhinoGeometry {
    protected static Nomen NomenOf<TSelf>() where TSelf : Component<TIn> =>
        typeof(TSelf).GetCustomAttribute<NomenAttribute>()?.Nomen
            ?? new Nomen(name: typeof(TSelf).Name, info: string.Empty);
    protected abstract Seq<IOutput<TIn>> Slots { get; }
    protected virtual Port<TIn> Primary =>
        Port.Required<TIn>(name: "Geometry", code: "G", info: "Geometry to process.");
    protected virtual Seq<IPort> Auxiliaries => [];

    protected Component(Nomen nomen) : base(nomen: nomen) { }
    protected Component(IReader reader) : base(reader: reader) { }

    protected sealed override void AddInputs(InputAdder inputs) {
        ArgumentNullException.ThrowIfNull(argument: inputs);
        _ = Param.Resolve(type: Primary.Type).Bind(adder: inputs, name: Primary.Name, code: Primary.Code, info: Primary.Info, access: Primary.Access, requirement: Primary.Requirement);
        _ = Auxiliaries.Iter((IPort port) => Add(adder: inputs, port: port));
    }
    protected sealed override void AddOutputs(OutputAdder outputs) {
        ArgumentNullException.ThrowIfNull(argument: outputs);
        _ = Slots.Map((IOutput<TIn> slot) => Param.Resolve(type: slot.Type).Bind(adder: outputs, name: slot.Name, code: slot.Code, info: slot.Info, access: Access.Twig)).Strict();
    }
    protected sealed override void Process(IDataAccess access) {
        ArgumentNullException.ThrowIfNull(argument: access);
        Option<int> hint = access.Index(ports: Auxiliaries);
        _ = access.Scope().Bind((Analyze.Scope scope) => access.Item<TIn>(slot: 0, port: Primary).Map((TIn input) => (Scope: scope, Input: input)))
            .Match(
                Succ: (state) => Slots.Iter((int i, IOutput<TIn> slot) => slot.Run(access: access, slot: i, scope: state.Scope, hint: hint, input: state.Input)),
                Fail: (Error error) => (
                    access.MissingInput(port: Primary, error: error),
                    Slots.Iter((int i, IOutput<TIn> slot) => slot.Empty(access: access, slot: i))).Item2);
    }
    private static Unit Add(InputAdder adder, IPort port) =>
        port.IsIndex switch {
            true => Indexed(adder: adder, port: port),
            false => Param.Resolve(type: port.Type).Bind(adder: adder, name: port.Name, code: port.Code, info: port.Info, access: port.Access, requirement: port.Requirement),
        };
    private static Unit Indexed(InputAdder adder, IPort port) {
        _ = adder.AddIndex(name: port.Name, code: port.Code, info: port.Info, access: port.Access, requirement: port.Requirement);
        return Unit.Default;
    }
}
