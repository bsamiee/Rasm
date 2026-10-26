using Eto.Drawing;
using Grasshopper2.Doc;
using Grasshopper2.Parameters;
using Grasshopper2.Parameters.Special;
using Grasshopper2.Undo;
using Rasm.Rhino;

namespace Rasm.Grasshopper.Doc;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Wiring {
    // --- [RULES]
    private static bool Connected(IParameter source, IParameter target) =>
        target.Inputs.Forwards.Contains(source.InstanceId) && source.Outputs.Forwards.Contains(target.InstanceId);

    private static Fin<Unit> Valid(IParameter source, IParameter target) =>
        source != target
        && source is { Kind: not Kind.Input, Document: { } document, Attributes: not IParameterAttributes { HasOutlet: false } }
        && target is { Kind: not Kind.Output, Attributes: not IParameterAttributes { HasInlet: false } }
        && target.Document == document
            ? unit
            : Error.New($"parameter {source.InstanceId} cannot wire to {target.InstanceId}");

    private static Fin<Unit> Created(IParameter source, IParameter target) =>
        Valid(source, target).Bind(_ => Connected(source, target) ? Error.New($"wire from {source.InstanceId} to {target.InstanceId} exists") : Fin.Succ(unit));

    // --- [CONNECTIONS]
    public static IO<Unit> Connect(IParameter source, IParameter target, Option<(int AtSource, int AtTarget)> order, ActionList undo) =>
        IO.lift(Fin<Unit> () => order.IsNone && Connected(source, target)
            ? unit
            : Valid(source, target).Map(_ => ignore(order.Match(
                Some: at => Connections.Connect(source, target, at.AtSource, at.AtTarget, undo),
                None: () => Connections.Connect(source, target, undo)))));

    public static IO<Unit> ReplaceSource(IParameter oldSource, IParameter newSource, IParameter target, ActionList undo) =>
        Rewired(Seq((newSource, target)), () => Connections.ReplaceSource(oldSource, newSource, target, undo), nameof(Connections.ReplaceSource));

    public static IO<Unit> ReplaceTarget(IParameter source, IParameter oldTarget, IParameter newTarget, ActionList undo) =>
        Rewired(Seq((source, newTarget)), () => Connections.ReplaceTarget(source, oldTarget, newTarget, undo), nameof(Connections.ReplaceTarget));

    public static IO<Unit> SwapSources(IParameter sourceA, IParameter sourceB, IParameter targetA, IParameter targetB, ActionList undo) =>
        Rewired(Seq((sourceA, targetB), (sourceB, targetA)), () => Connections.SwapSources(sourceA, sourceB, targetA, targetB, undo), nameof(Connections.SwapSources));

    public static IO<Unit> CutOutMiddleMan(IParameter left, IParameter middle, IParameter right, ActionList undo) =>
        Rewired(Seq((left, right)), () => Connections.CutOutMiddleMan(left, middle, right, undo), nameof(Connections.CutOutMiddleMan));

    private static IO<Unit> Rewired(Seq<(IParameter Source, IParameter Target)> created, Func<bool> rewire, string member) =>
        IO.lift(() => created.Traverse(static wire => Created(wire.Source, wire.Target)).As().Bind(_ => Refused.Unless(rewire(), member)));

    // --- [OBJECTS]
    public static IO<(Shout Shout, Listen Listen)> SplitWire(IParameter source, IParameter target, Option<string> name, PointF location, ActionList actions) =>
        from checkedWire in IO.lift(Fin<Unit> () => Connected(source, target) ? unit : Error.New($"no wire from {source.InstanceId} to {target.InstanceId}"))
        from split in IO.lift(() => Refused.Unless(
            source.Document.Methods.SplitWire(source, target, Conversions.Unset(name), location, out Shout shout, out Listen listen, actions), (Shout: shout, Listen: listen), nameof(DocumentMethods.SplitWire)))
        select split;

    public static IO<HashMap<Guid, Guid>> Absorb(ObjectList objects, ObjectList other, bool reinstateInputs, bool reinstateOutputs, ActionList actions) =>
        objects == other
            ? IO.pure(HashMap<Guid, Guid>())
            : from ids in IO.lift(other.ChangeAllIds)
              from absorbed in IO.lift(() => objects.Absorb(other, reinstateInputs, reinstateOutputs, actions))
              select ids.ToHashMap();
}
