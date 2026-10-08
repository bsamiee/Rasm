using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Shapes;
using Rasm.Rhino.Document.Tables;
using Rhino;
using Rhino.DocObjects;

namespace Rasm.Rhino.Blocks;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record PlannedDefinition(Option<string> Description, Option<Hyperlink> Link, Point3d BasePoint, Seq<GeometryPair> Members);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class DefinitionPlans {
    // --- [PROGRAM]
    public static IO<Seq<DefinitionOp>> Plan(RhinoDoc doc, TableSpec<PlannedDefinition> spec) =>
        from live in TableOps.Rows(doc.InstanceDefinitions)
        from plan in IO.lift(TableOps.Plan(live, spec).ToFin())
        from upserts in plan.Upserts.TraverseM(Upserted).As()
        from nesting in DefinitionGraph.Read(doc)
        select upserts.Flatten() + nesting.Removable(toHashSet(plan.Prunes.Map(static row => row.Id)))
            .Map<DefinitionOp>(static id => new DefinitionOp.Edit(new ComponentRef<InstanceDefinition>.ById(id), new DefinitionEdit.Delete(DeleteReferences: false)));

    public static IO<Committed<Seq<(DefinitionOp Op, int Index)>>> Reconcile(RhinoDoc doc, TableSpec<PlannedDefinition> spec, string name, RedrawPolicy redraw) =>
        from ops in Plan(doc, spec)
        from committed in ops.IsEmpty
            ? IO.pure(new Committed<Seq<(DefinitionOp Op, int Index)>>(Seq<(DefinitionOp Op, int Index)>(), None))
            : Commits.Commit(doc, name, redraw, ops.TraverseM(op => op.Apply(doc).Map(index => (Op: op, Index: index))).As())
        select committed;

    // --- [CHANGES]
    private static IO<Seq<DefinitionOp>> Upserted(TableUpsert<InstanceDefinition, PlannedDefinition> upsert) =>
        from compared in upsert.Existing.TraverseM(live => Stored(live, upsert.Spec)).As()
        let metadata = new DefinitionMetadata(upsert.Name, upsert.Spec.Description, upsert.Spec.Link)
        let stored = compared.Contains(true)
        select upsert.Existing.Filter(live => stored ? DefinitionMetadata.Of(live) != metadata : !string.Equals(live.Name, metadata.Name, StringComparison.Ordinal))
            .Map<DefinitionOp>(live => new DefinitionOp.Edit(new ComponentRef<InstanceDefinition>.ById(live.Id), new DefinitionEdit.Modify(metadata))).ToSeq()
            + (stored ? Seq<DefinitionOp>() : Seq<DefinitionOp>(new DefinitionOp.Add(metadata, upsert.Spec.BasePoint, upsert.Spec.Members, upsert.Existing.IsSome)));

    private static IO<bool> Stored(InstanceDefinition live, PlannedDefinition spec) =>
        from held in IO.lift(() => toSeq(live.GetObjects()).Map(Optional).Strict())
        from same in held.Count == spec.Members.Count && spec.Members.ForAll(static member => member.Attributes.IsNone)
            ? spec.Members.Zip(held)
                .TraverseM(pair => pair.Second.Match(Some: member => Same(pair.First.Geometry, spec.BasePoint, member.Geometry), None: static () => IO.pure(value: false)))
                .As().Map(static matches => matches.ForAll(static matched => matched))
            : IO.pure(value: false)
        select same;

    private static IO<bool> Same(GeometryBase planned, Point3d basePoint, GeometryBase stored) =>
        (from placed in use(Copies.DuplicateShallow(planned))
         from translated in IO.lift(Refused.Unless(placed.Translate(Point3d.Origin - basePoint), nameof(GeometryBase.Translate)))
         select GeometryBase.GeometryEquals(placed, stored)).Bracket();
}
