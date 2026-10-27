using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.FileIO;

namespace Rasm.Rhino.Document.Tables;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class LayerPrune {
    public static readonly LayerPrune Keep = new(static _ => None);
    public static readonly LayerPrune Delete = new(static target => Some<LayerOp>(new LayerOp.Delete(target)));
    public static readonly LayerPrune Purge = new(static target => Some<LayerOp>(new LayerOp.Purge(target)));

    [UseDelegateFromConstructor]
    public partial Option<LayerOp> Removal(LayerRef target);
}

public sealed record PlannedLayer(LayerPath Path, Option<Guid> Id, Seq<LayerEdit> Edits);

public sealed record LayerPlan(Seq<PlannedLayer> Layers, Option<LayerPath> Current, LayerPrune Prune, bool Ordered) {
    public Seq<Guid> Details => Layers.Bind(static layer => layer.Edits).Map(static edit => edit.Viewport).Somes().Distinct();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class LayerPlans {
    // --- [RECONCILE]
    public static IO<Committed<Seq<LayerOp>>> Reconcile(RhinoDoc doc, LayerPlan plan, string name, RedrawPolicy redraw) =>
        Commits.Commit(doc, name, redraw,
            from live in Layers.Read(doc, plan.Details)
            from ops in IO.lift(() => Plan(live, plan, Guid.NewGuid))
            from applied in ops.TraverseM(op => Layers.Apply(doc, op)).As()
            select ops);

    public static Fin<Seq<LayerOp>> Plan(LayerTree live, LayerPlan plan, Func<Guid> fresh) =>
        from valid in Checked(plan)
        let rows = Placed(live, valid, fresh)
        let stale = live.Order.Filter(id => !live.Nodes[id].IsReference && !rows.Exists(row => row.Id == id))
        from free in Conflicts(rows)
        select Ops(live, valid, rows, stale);

    // --- [PLACEMENT]
    private sealed record Placement(LayerPath Path, Guid Id, Option<Guid> Parent, Option<LayerNode> Held, Option<Guid> Occupant, Seq<LayerEdit> Edits);

    private static Fin<LayerPlan> Checked(LayerPlan plan) =>
        (Conversions.Unique(plan.Layers, static layer => layer.Path),
         Conversions.Unique(plan.Layers.Map(static layer => layer.Id).Somes(), static id => id),
         RhinoFault.Unless(plan.Current.ForAll(current => Paths(plan).Exists(path => path == current))))
        .Apply((_, _, _) => plan)
        .As();

    private static Seq<LayerPath> Paths(LayerPlan plan) =>
        plan.Layers.Bind(static layer => Lineage(layer.Path)).Distinct();

    private static Seq<LayerPath> Lineage(LayerPath path) =>
        path.Segments.Scan(Option<LayerPath>.None, static (parent, name) => Some(parent.Match(
            Some: above => above.Child(name),
            None: () => new LayerPath(Seq<string>(), name)))).Somes();

    private static Seq<LayerPath> Preorder(Seq<LayerPath> paths) {
        ILookup<Option<LayerPath>, LayerPath> children = paths.ToLookup(static path => path.Parent);
        Seq<LayerPath> Below(Option<LayerPath> parent) => toSeq(children[parent]).Bind(child => child.Cons(Below(Some(child))));
        return Below(None);
    }

    private static Seq<Placement> Placed(LayerTree live, LayerPlan plan, Func<Guid> fresh) {
        HashMap<NameHash, Guid> siblings = toHashMap(live.Order.Bind(id => live.Nodes[id].Name.Map(name => (new NameHash(name, Conversions.Unset(live.Parent(id)), ModelComponentType.Layer), id)).ToSeq()));
        HashMap<LayerPath, PlannedLayer> stated = toHashMap(plan.Layers.Map(static layer => (layer.Path, layer)));
        HashSet<Guid> claimed = toHashSet(plan.Layers.Map(static layer => layer.Id).Somes());
        return Preorder(Paths(plan)).Fold(
            (Ids: HashMap<LayerPath, Guid>(), Rows: Seq<Placement>()),
            (placed, path) => {
                Option<Guid> parent = path.Parent.Bind(placed.Ids.Find);
                Option<PlannedLayer> layer = stated.Find(path);
                Option<Guid> requested = layer.Bind(static row => row.Id);
                Option<Guid> occupant = siblings.Find(new NameHash(path.Leaf, Conversions.Unset(parent), ModelComponentType.Layer));
                Option<LayerNode> held = (requested || occupant.Filter(id => !claimed.Contains(id))).Bind(live.Nodes.Find).Filter(static node => !node.IsReference);
                Guid id = held.Map(static node => node.Id).IfNone(() => requested.IfNone(fresh));
                return (placed.Ids.Add(path, id), placed.Rows.Add(new Placement(path, id, parent, held, occupant, layer.ToSeq().Bind(static row => row.Edits))));
            }).Rows;
    }

    private static Fin<Unit> Conflicts(Seq<Placement> rows) =>
        rows.Traverse(static row => row.Occupant.Filter(occupant => occupant != row.Id).Match<Fin<Unit>>(
                Some: _ => new RhinoFault(row.Path.Text, RhinoReason.Taken),
                None: static () => unit))
            .As()
            .Map(static _ => unit);

    // --- [CHANGES]
    private static Seq<LayerOp> Ops(LayerTree live, LayerPlan plan, Seq<Placement> rows, Seq<Guid> stale) {
        Option<Placement> named = plan.Current.Bind(path => rows.Find(row => row.Path == path));
        HashMap<LayerPath, Guid> ids = toHashMap(rows.Map(static row => (row.Path, row.Id)));
        HashSet<Guid> shown = toHashSet(named.Match(Some: row => Lineage(row.Path).Map(path => ids[path]), None: () => live.Ancestors(live.Current)));
        Seq<Placement> answered = rows.Map(row => shown.Contains(row.Id)
            ? row with { Edits = row.Edits.Filter(static edit => (edit.Restores & (RestoreLayerProperties.Visible | RestoreLayerProperties.Locked)) is RestoreLayerProperties.None) }
            : row);
        Seq<LayerOp> created = answered.Filter(static row => row.Held.IsNone)
            .Map<LayerOp>(static row => new LayerOp.Create(row.Path.Leaf, row.Parent, Some(row.Id), row.Edits));
        Seq<LayerOp> current = named.Filter(row => row.Id != live.Current).Map<LayerOp>(static row => new LayerOp.SetCurrent(Address(row.Id))).ToSeq();
        Seq<LayerOp> changed = answered.Choose(row => row.Held.Bind(node => Changed(live, row, node)));
        HashSet<Guid> removed = toHashSet(stale);
        Seq<Guid> subtrees = stale.Filter(id => !live.Parent(id).Exists(removed.Contains));
        Seq<LayerOp> pruned = subtrees.Choose(id => plan.Prune.Removal(Address(id)));
        Seq<Guid> order = rows.Map(static row => row.Id) + (plan.Prune == LayerPrune.Keep ? stale : Seq<Guid>()) + live.Order.Filter(id => live.Nodes[id].IsReference);
        return created + current + changed + pruned + (plan.Ordered && order != live.Order ? Seq<LayerOp>(new LayerOp.Sort(order.Map(Address))) : Seq<LayerOp>());
    }

    private static Option<LayerOp> Changed(LayerTree live, Placement row, LayerNode node) {
        Seq<LayerEdit> pending = LayerProperties.Name.Set(row.Path.Leaf).Cons(row.Edits).Filter(edit => !edit.Holds(node)).Strict();
        return live.Parent(row.Id) != row.Parent
            ? Some<LayerOp>(new LayerOp.Reparent(Address(row.Id), row.Parent, pending))
            : pending.IsEmpty ? Option<LayerOp>.None : Some<LayerOp>(new LayerOp.Modify(Address(row.Id), pending));
    }

    private static LayerRef Address(Guid id) => new ComponentRef<Layer>.ById(id);
}
