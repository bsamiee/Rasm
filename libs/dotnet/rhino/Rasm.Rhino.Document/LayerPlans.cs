using Rhino;
using Rhino.UI;

namespace Rasm.Rhino.Document;

// --- [TYPES] ---------------------------------------------------------------------------
public enum LayerPrune { Keep = 0, Delete = 1, Purge = 2 }

// --- [MODELS] --------------------------------------------------------------------------
public sealed record LayerSpec {
    private LayerSpec(LayerPath path, Option<Guid> id, Seq<LayerEdit> edits) => (Path, Id, Edits) = (path, id, edits);

    public LayerPath Path { get; }

    public Option<Guid> Id { get; }

    public Seq<LayerEdit> Edits { get; }

    public static Fin<LayerSpec> From(LayerPath path, Option<Guid> id, Seq<LayerEdit> edits) =>
        id.Traverse(static value => Answers.Present(value).ToFin(new Invalid(nameof(Id)))).As().Map(valid => new LayerSpec(path, valid, edits));
}

public sealed record LayerTableSpec(Seq<LayerSpec> Layers, Option<LayerPath> Current, LayerPrune Prune, bool Ordered) {
    public Seq<Guid> Viewports => Layers.Bind(static layer => layer.Edits).Map(static edit => edit.Viewport).Somes().Distinct();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class LayerPlans {
    // --- [RECONCILE]
    public static IO<Seq<LayerOp>> Reconcile(RhinoDoc doc, LayerTableSpec spec, RedrawPolicy redraw) =>
        from live in Layers.ReadLayers(doc, spec.Viewports)
        from ops in IO.lift(() => Plan(live, spec, Guid.NewGuid))
        from applied in Commits.Commit(doc, LOC.STR("Reconcile layers"), redraw, ops.TraverseM(op => Layers.ApplyLayerOp(doc, op)).As())
        select ops;

    public static Fin<Seq<LayerOp>> Plan(LayerTree live, LayerTableSpec spec, Func<Guid> fresh) =>
        from located in (Checked(spec), Located(live.Roots, Option<LayerPath>.None)).Apply(static (_, rows) => rows).As().ToFin()
        let own = located.Filter(static row => !row.Node.IsReference)
        let byPath = toHashMap(own.Map(static row => (row.Path, row)))
        let byId = toHashMap(own.Map(static row => (row.Node.Id, row)))
        let claimed = toHashSet(spec.Layers.Map(static layer => layer.Id).Somes().Filter(byId.ContainsKey))
        from desired in Preorder(Paths(spec), Option<LayerPath>.None)
            .Traverse(path => Matched(path, spec.Layers.Find(layer => layer.Path == path), claimed, byPath, byId, fresh))
            .As()
            .ToFin()
        let stale = own.Filter(row => !desired.Exists(wanted => wanted.Id == row.Node.Id))
        from current in Current(live.Current, spec, desired, stale)
        select Ops(live, spec, desired, stale, located.Filter(static row => row.Node.IsReference), current);

    // --- [MATCHING]
    private sealed record LiveRow(LayerPath Path, LayerNode Node, LayerRef Ref);

    private sealed record Desired(LayerPath Path, Guid Id, LayerRef Ref, Option<LayerNode> Live, Seq<LayerEdit> Edits);

    private static Validation<Error, Unit> Checked(LayerTableSpec spec) =>
        (Answers.Unique(spec.Layers.Map(static layer => layer.Path), nameof(LayerSpec.Path)),
         Answers.Unique(spec.Layers.Map(static layer => layer.Id).Somes(), nameof(LayerSpec.Id)),
         Missing.Unless(spec.Current.ForAll(current => Paths(spec).Exists(path => path == current)), nameof(LayerTableSpec.Current)).ToValidation())
        .Apply(static (_, _, _) => unit)
        .As();

    private static Validation<Error, Seq<LiveRow>> Located(Seq<LayerNode> nodes, Option<LayerPath> parent) =>
        nodes.Traverse(node =>
                from name in Answers.Validated<LayerName, string>(node.Name).MapFail(_ => new InvalidElement(nameof(LayerName), node.Index)).ToValidation()
                let path = new LayerPath(parent, name)
                from below in Located(node.Children, Some(path))
                select new LiveRow(path, node, new LayerRef.Row(new ComponentRef.ById(node.Id))).Cons(below))
            .As()
            .Map(static rows => rows.Flatten());

    private static Seq<LayerPath> Paths(LayerTableSpec spec) =>
        spec.Layers.Bind(static layer => layer.Path.Ancestors.Add(layer.Path)).Distinct();

    private static Seq<LayerPath> Preorder(Seq<LayerPath> paths, Option<LayerPath> parent) =>
        paths.Filter(path => path.Parent == parent).Bind(path => path.Cons(Preorder(paths, Some(path))));

    private static Validation<Error, Desired> Matched(LayerPath path, Option<LayerSpec> layer, LanguageExt.HashSet<Guid> claimed, HashMap<LayerPath, LiveRow> byPath, HashMap<Guid, LiveRow> byId, Func<Guid> fresh) {
        Option<Guid> requested = layer.Bind(static spec => spec.Id);
        Option<LiveRow> owned = requested.Bind(byId.Find);
        Option<LiveRow> occupant = byPath.Find(path);
        Option<LiveRow> matched = owned || occupant;
        Guid id = matched.Map(static row => row.Node.Id).IfNone(() => requested.IfNone(fresh));
        Option<LayerNode> live = matched.Map(static row => row.Node);
        Seq<LayerEdit> edits = layer.ToSeq().Bind(static spec => spec.Edits);
        return occupant
            .Filter(other => owned.Match(Some: row => row.Node.Id != other.Node.Id, None: () => requested.IsSome || claimed.Contains(other.Node.Id)))
            .Match(
                Some: other => Validation.Fail<Error, Desired>(new LayerPathTaken(path.FullPath, other.Node.Id)),
                None: () => ComponentRef.ById.From(id).ToValidation().Map(key => new Desired(path, id, new LayerRef.Row(key), live, edits)));
    }

    // --- [CHANGES]
    private static Fin<Seq<LayerOp>> Current(Guid current, LayerTableSpec spec, Seq<Desired> desired, Seq<LiveRow> stale) =>
        spec.Current.Match<Fin<Seq<LayerOp>>>(
            Some: path => desired.Find(row => row.Path == path).Filter(row => row.Id != current).Map<LayerOp>(static row => new LayerOp.SetCurrent(row.Ref, Quiet: true)).ToSeq(),
            None: () => spec.Prune != LayerPrune.Keep && stale.Exists(row => row.Node.Id == current) ? new CurrentLayerPruned(current) : Seq<LayerOp>());

    private static Seq<LayerOp> Ops(LayerTree live, LayerTableSpec spec, Seq<Desired> desired, Seq<LiveRow> stale, Seq<LiveRow> references, Seq<LayerOp> current) {
        HashMap<LayerPath, Desired> byPath = toHashMap(desired.Map(static row => (row.Path, row)));
        Seq<(Guid Id, LayerRef Ref)> order =
            desired.Map(static row => (row.Id, row.Ref))
            + (spec.Prune == LayerPrune.Keep ? stale : Seq<LiveRow>()).Map(static row => (row.Node.Id, row.Ref))
            + references.Map(static row => (row.Node.Id, row.Ref));
        return desired.Filter(static row => row.Live.IsNone).Map<LayerOp>(row => new LayerOp.Create(row.Path.Leaf, Parent(byPath, row).Map(static above => above.Ref), Some(row.Id), row.Edits))
            + current
            + Moved(byPath, desired)
            + Pruned(spec.Prune, stale)
            + Modified(desired)
            + (spec.Ordered && !order.Map(static row => row.Id).Equals(live.Nodes.Map(static node => node.Id)) ? Seq<LayerOp>(new LayerOp.Sort(order.Map(static row => row.Ref))) : Seq<LayerOp>());
    }

    private static Option<Desired> Parent(HashMap<LayerPath, Desired> byPath, Desired row) =>
        row.Path.Parent.Bind(byPath.Find);

    private static Seq<LayerOp> Moved(HashMap<LayerPath, Desired> byPath, Seq<Desired> desired) =>
        from row in desired
        from node in row.Live.ToSeq()
        let parent = Parent(byPath, row)
        where node.ParentLayerId != parent.Map(static above => above.Id)
        select (LayerOp)new LayerOp.Reparent(row.Ref, parent.Map(static above => above.Ref));

    private static Seq<LayerOp> Modified(Seq<Desired> desired) =>
        from row in desired
        from node in row.Live.ToSeq()
        let pending = LayerProperties.Name.Set(row.Path.Leaf).Cons(row.Edits).Filter(edit => !edit.Holds(node)).Strict()
        where !pending.IsEmpty
        select (LayerOp)new LayerOp.Modify(row.Ref, pending);

    private static Seq<LayerOp> Pruned(LayerPrune prune, Seq<LiveRow> stale) =>
        stale
            .Filter(row => !row.Node.ParentLayerId.Exists(parent => stale.Exists(other => other.Node.Id == parent)))
            .Bind(row => prune switch {
                LayerPrune.Keep => Seq<LayerOp>(),
                LayerPrune.Delete => Seq<LayerOp>(new LayerOp.Delete(row.Ref, Quiet: true)),
                LayerPrune.Purge => Seq<LayerOp>(new LayerOp.Purge(row.Ref, Quiet: true)),
            });
}
