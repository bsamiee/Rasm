using System.Reflection;
using Grasshopper2.Components;
using Grasshopper2.UI;
using GrasshopperIO;
using Rasm.Analysis;
using Rasm.Domain;
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
        Seq<IPort> inputs = Inputs;
        Hints hints = Hints.Capture(inputs: inputs);
        _ = access.Scope().Bind(scope => Read(access: access).Map(input => (Scope: scope, Hints: hints, Input: input)))
            .Match(
                Succ: state => Slots.Iter((i, slot) => slot.Run(access: access, slot: i, scope: state.Scope, hints: state.Hints, input: state.Input)),
                Fail: error => (
                    access.MissingInput(error: error),
                    Slots.Iter((i, slot) => slot.Empty(access: access, slot: i))).Item2);
    }
    protected abstract Fin<TIn> Read(IDataAccess access);
}

/// <summary>
/// Shared base for geometry-query components that operate on the canonical Shape input.
/// </summary>
public abstract class ShapeComponent : Component<Shape> {
    protected static readonly Port<Shape> Geometry = Port.Required<Shape>(
        param: Param.Generic,
        name: "Geometry",
        code: "G",
        info: "Geometry to analyse.");
    protected virtual Seq<IPort> Controls =>
        Seq<IPort>();
    protected sealed override Seq<IPort> Inputs =>
        toSeq(Seq<IPort>(Geometry).Concat(second: Controls));

    protected ShapeComponent(Nomen nomen) : base(nomen: nomen) { }
    protected ShapeComponent(IReader reader) : base(reader: reader) { }

    protected static Output<Shape, object, TOut> ShapeOutput<TOut>(Port<TOut> port, Query<object, TOut> query) =>
        Output.Of<Shape, object, TOut>(port: port, select: static shape => shape.Inner, query: query);
    protected static Output<Shape, object, TOut> IndexedShapeOutput<TOut>(Port<TOut> port, Port<int> index, Func<int?, Query<object, TOut>> build) =>
        Output.Indexed<Shape, object, TOut>(port: port, index: index, select: static shape => shape.Inner, build: build);
    protected static Output<Shape, object, TOut> ControlledShapeOutput<TControl, TOut>(Port<TOut> port, Port<TControl> control, Func<Option<TControl>, Query<object, TOut>> build) =>
        Output.Controlled<Shape, object, TControl, TOut>(port: port, control: control, select: static shape => shape.Inner, build: build);
    protected static Output<Shape, object, TOut> ControlledShapeOutput<TControlA, TControlB, TOut>(Port<TOut> port, Port<TControlA> controlA, Port<TControlB> controlB, Func<Option<TControlA>, Option<TControlB>, Query<object, TOut>> build) =>
        Output.Controlled<Shape, object, TControlA, TControlB, TOut>(port: port, controlA: controlA, controlB: controlB, select: static shape => shape.Inner, build: build);
    protected sealed override Fin<Shape> Read(IDataAccess access) =>
        access.ReadShape(slot: 0, port: Geometry);
}
