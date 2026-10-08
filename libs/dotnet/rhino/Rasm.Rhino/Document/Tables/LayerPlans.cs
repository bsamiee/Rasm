using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;

namespace Rasm.Rhino.Document.Tables;

// --- [TYPES] ---------------------------------------------------------------------------
public enum LayerPrune { Keep, Delete, Purge }

// --- [MODELS] --------------------------------------------------------------------------
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
        from valid in Checked(plan).ToFin()
        let rows = Placed(live, valid, fresh)
        let stale = live.Order.Filter(id => !live.Nodes[id].IsReference && !rows.Exists(row => row.Id == id))
        from free in (Conflicts(rows), Kept(live, valid, stale)).Apply(static (_, _) => unit).As().ToFin()
        select Ops(live, valid, rows, stale);

    // --- [PLACEMENT]
    private sealed record Placement(LayerPath Path, Guid Id, Option<Guid> Parent, Option<LayerNode> Held, Option<Guid> Occupant, Seq<LayerEdit> Edits);

    private static Validation<Error, LayerPlan> Checked(LayerPlan plan) =>
        (Callbacks.Unique(plan.Layers, static layer => layer.Path, nameof(PlannedLayer.Path)),
         Callbacks.Unique(plan.Layers.Map(static layer => layer.Id).Somes(), static id => id, nameof(PlannedLayer.Id)),
         Missing.Unless(plan.Current.ForAll(current => Paths(plan).Exists(path => path == current)), nameof(LayerPlan.Current)).ToValidation())
        .Apply((_, _, _) => plan)
        .As();

    private static Seq<LayerPath> Paths(LayerPlan plan) =>
        plan.Layers.Bind(static layer => Lineage(layer.Path, static path => path.Parent)).Distinct();

    private static Seq<T> Lineage<T>(T node, Func<T, Option<T>> parent) =>
        parent(node).Map(above => Lineage(above, parent)).IfNone(Seq<T>()).Add(node);

    private static Seq<LayerPath> Preorder(Seq<LayerPath> paths, Option<LayerPath> parent) =>
        paths.Filter(path => path.Parent == parent).Bind(path => path.Cons(Preorder(paths, Some(path))));

    private static Seq<Placement> Placed(LayerTree live, LayerPlan plan, Func<Guid> fresh) {
        HashMap<(Option<Guid> Parent, LeafName Name), Guid> siblings = toHashMap(live.Order.Map(id => ((live.Parent(id), live.Nodes[id].Name), id)));
        HashMap<LayerPath, PlannedLayer> stated = toHashMap(plan.Layers.Map(static layer => (layer.Path, layer)));
        LanguageExt.HashSet<Guid> claimed = toHashSet(plan.Layers.Map(static layer => layer.Id).Somes());
        return Preorder(Paths(plan), Option<LayerPath>.None).Fold(
            (Ids: HashMap<LayerPath, Guid>(), Rows: Seq<Placement>()),
            (placed, path) => {
                Option<Guid> parent = path.Parent.Bind(placed.Ids.Find);
                Option<PlannedLayer> layer = stated.Find(path);
                Option<Guid> requested = layer.Bind(static row => row.Id);
                Option<Guid> occupant = siblings.Find((parent, path.Leaf));
                Option<LayerNode> held = (requested || occupant.Filter(id => !claimed.Contains(id))).Bind(live.Nodes.Find).Filter(static node => !node.IsReference);
                Guid id = held.Map(static node => node.Id).IfNone(() => requested.IfNone(fresh));
                return (placed.Ids.Add(path, id), placed.Rows.Add(new Placement(path, id, parent, held, occupant, layer.ToSeq().Bind(static row => row.Edits))));
            }).Rows;
    }

    private static Validation<Error, Unit> Conflicts(Seq<Placement> rows) =>
        rows.Traverse(static row => row.Occupant.Filter(occupant => occupant != row.Id).Match(
                Some: occupant => Validation.Fail<Error, Unit>(new LayerPathTaken(row.Path.Text, occupant)),
                None: static () => Validation.Success<Error, Unit>(unit)))
            .As()
            .Map(static _ => unit);

    private static Validation<Error, Unit> Kept(LayerTree live, LayerPlan plan, Seq<Guid> stale) =>
        plan.Current.IsNone && plan.Prune is not LayerPrune.Keep && stale.Exists(id => id == live.Current)
            ? Validation.Fail<Error, Unit>(new CurrentLayerPruned(live.Current))
            : Validation.Success<Error, Unit>(unit);

    // --- [CHANGES]
    private static Seq<LayerOp> Ops(LayerTree live, LayerPlan plan, Seq<Placement> rows, Seq<Guid> stale) {
        Option<Placement> named = plan.Current.Bind(path => rows.Find(row => row.Path == path));
        Seq<Guid> shown = Lineage(named.Map(static row => row.Id).IfNone(live.Current), id => rows.Find(row => row.Id == id).Match(Some: static row => row.Parent, None: () => live.Parent(id)));
        Seq<Placement> answered = rows.Map(row => shown.Exists(id => id == row.Id)
            ? row with { Edits = row.Edits.Filter(static edit => (edit.Restores & (RestoreLayerProperties.Visible | RestoreLayerProperties.Locked)) == RestoreLayerProperties.None) }
            : row);
        Seq<LayerOp> created = answered.Filter(static row => row.Held.IsNone)
            .Map<LayerOp>(static row => new LayerOp.Create(row.Path.Leaf, row.Parent.Map(Address), Some(row.Id), row.Edits));
        Seq<LayerOp> current = named.Filter(row => row.Id != live.Current).Map<LayerOp>(static row => new LayerOp.SetCurrent(Address(row.Id))).ToSeq();
        Seq<LayerOp> changed = answered.Choose(row => row.Held.Bind(node => Changed(live, row, node)));
        Seq<LayerOp> pruned = stale.Filter(id => !live.Parent(id).Exists(parent => stale.Exists(other => other == parent)))
            .Bind(id => plan.Prune switch {
                LayerPrune.Keep => Seq<LayerOp>(),
                LayerPrune.Delete => Seq<LayerOp>(new LayerOp.Delete(Address(id))),
                LayerPrune.Purge => Seq<LayerOp>(new LayerOp.Purge(Address(id))),
            });
        Seq<Guid> order = rows.Map(static row => row.Id) + (plan.Prune is LayerPrune.Keep ? stale : Seq<Guid>()) + live.Order.Filter(id => live.Nodes[id].IsReference);
        return created + current + changed + pruned + (plan.Ordered && order != live.Order ? Seq<LayerOp>(new LayerOp.Sort(order.Map(Address))) : Seq<LayerOp>());
    }

    private static Option<LayerOp> Changed(LayerTree live, Placement row, LayerNode node) {
        Seq<LayerEdit> pending = LayerProperties.Name.Set(row.Path.Leaf).Cons(row.Edits).Filter(edit => !edit.Holds(node)).Strict();
        return live.Parent(row.Id) != row.Parent
            ? Some<LayerOp>(new LayerOp.Reparent(Address(row.Id), row.Parent.Map(Address), pending))
            : pending.IsEmpty ? Option<LayerOp>.None : Some<LayerOp>(new LayerOp.Modify(Address(row.Id), pending));
    }

    private static LayerRef Address(Guid id) => new ComponentRef<Layer>.ById(id);
}
