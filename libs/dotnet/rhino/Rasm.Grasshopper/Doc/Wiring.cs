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

    private static Fin<Unit> Existing(IParameter source, IParameter target) =>
        WireAbsent.Unless(Connected(source, target), new WireEnds(source.InstanceId, target.InstanceId));

    private static Fin<Unit> Created(IParameter source, IParameter target) =>
        source != target
        && source is { Kind: not Kind.Input, Document: { } document, Attributes: not IParameterAttributes { HasOutlet: false } }
        && target is { Kind: not Kind.Output, Attributes: not IParameterAttributes { HasInlet: false } }
        && target.Document == document
            ? WirePresent.Unless(!Connected(source, target), new WireEnds(source.InstanceId, target.InstanceId))
            : new InvalidWire(new WireEnds(source.InstanceId, target.InstanceId));

    // --- [CONNECTIONS]
    public static IO<Unit> Connect(IParameter source, IParameter target, Option<(int AtSource, int AtTarget)> order, ActionList undo) =>
        IO.lift(Fin<Unit> () => order.IsNone && Connected(source, target)
            ? unit
            : Created(source, target).Map(_ => order.Match(
                Some: at => Connections.Connect(source, target, at.AtSource, at.AtTarget, undo),
                None: () => Connections.Connect(source, target, undo))).Map(static _ => unit));

    public static IO<Unit> ReplaceSource(IParameter oldSource, IParameter newSource, IParameter target, ActionList undo) =>
        IO.lift(() => (Existing(oldSource, target).ToValidation(), Created(newSource, target).ToValidation())
            .Apply((_, _) => Connections.ReplaceSource(oldSource, newSource, target, undo))
            .As()
            .ToFin())
        .Map(static _ => unit);

    public static IO<Unit> ReplaceTarget(IParameter source, IParameter oldTarget, IParameter newTarget, ActionList undo) =>
        IO.lift(() => (Existing(source, oldTarget).ToValidation(), Created(source, newTarget).ToValidation())
            .Apply((_, _) => Connections.ReplaceTarget(source, oldTarget, newTarget, undo))
            .As()
            .ToFin())
        .Map(static _ => unit);

    public static IO<Unit> SwapSources(IParameter sourceA, IParameter sourceB, IParameter targetA, IParameter targetB, ActionList undo) =>
        IO.lift(() => (Existing(sourceA, targetA).ToValidation(), Existing(sourceB, targetB).ToValidation(), Created(sourceA, targetB).ToValidation(), Created(sourceB, targetA).ToValidation())
            .Apply((_, _, _, _) => Connections.SwapSources(sourceA, sourceB, targetA, targetB, undo))
            .As()
            .ToFin())
        .Map(static _ => unit);

    public static IO<Unit> CutOutMiddleMan(IParameter left, IParameter middle, IParameter right, ActionList undo) =>
        IO.lift(() => (Existing(left, middle).ToValidation(), Existing(middle, right).ToValidation(), Created(left, right).ToValidation())
            .Apply((_, _, _) => Connections.CutOutMiddleMan(left, middle, right, undo))
            .As()
            .ToFin())
        .Map(static _ => unit);

    // --- [OBJECTS]
    public static IO<(Shout Shout, Listen Listen)> SplitWire(IParameter source, IParameter target, Option<string> name, PointF location, ActionList actions) =>
        from checkedWire in IO.lift(() => Existing(source, target))
        from split in IO.lift(() => {
            _ = source.Document.Methods.SplitWire(source, target, Conversions.Unset(name), location, out Shout shout, out Listen listen, actions);
            return (Shout: shout, Listen: listen);
        })
        select split;

    public static IO<HashMap<Guid, Guid>> Absorb(ObjectList objects, ObjectList other, bool reinstateInputs, bool reinstateOutputs, ActionList actions) =>
        objects == other
            ? IO.lift(() => objects.Absorb(other, reinstateInputs, reinstateOutputs, actions)).Map(static _ => HashMap<Guid, Guid>())
            : from ids in IO.lift(other.ChangeAllIds)
              from _ in IO.lift(() => objects.Absorb(other, reinstateInputs, reinstateOutputs, actions))
              select toHashMap(ids.Select(static pair => (pair.Key, pair.Value)));
}
