using Rasm.Rhino.Document;
using Rhino;
using Rhino.DocObjects;
using Rhino.UI;

namespace Rasm.Rhino.Blocks;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class DefinitionPlans {
    public static IO<Seq<DefinitionOp>> Reconcile(RhinoDoc doc, TableSpec<DefinitionOp.Add> spec, RedrawPolicy redraw) =>
        from present in TableOps.Present(doc.InstanceDefinitions)
        from ops in IO.lift(() => Plan(present, spec))
        from applied in Definitions.Commit(doc, LOC.STR("Reconcile blocks"), redraw, ops)
        select ops;

    public static Fin<Seq<DefinitionOp>> Plan(Seq<InstanceDefinition> present, TableSpec<DefinitionOp.Add> spec) =>
        from plan in TableOps.Plan(present, spec, static row => Some(row.Metadata.Name)).ToFin()
        from ops in (
                plan.Upserts
                    .Traverse(static upsert => upsert.Present.Traverse(static row => ComponentRef.ByIndex.From(row.Index).Map(key => (Row: row, Key: key))).As().Map(held => (upsert.Spec, Held: held)))
                    .As(),
                plan.Prunes
                    .Traverse(static row => ComponentRef.ByIndex.From(row.Index).Map<DefinitionOp>(static key => new DefinitionOp.Edit(key, new DefinitionEdit.Delete(DeleteReferences: false))))
                    .As())
            .Apply(static (keyed, pruned) => keyed.Bind(static upsert => upsert.Held.Match(
                    Some: held =>
                        Answers.Found<DefinitionOp>(DefinitionMapper.MetadataOf(held.Row) != upsert.Spec.Metadata, new DefinitionOp.Edit(held.Key, new DefinitionEdit.Modify(upsert.Spec.Metadata))).ToSeq()
                        + Answers.Found<DefinitionOp>(
                            (upsert.Spec.Members.Count != held.Row.ObjectCount)
                            || upsert.Spec.Members.Zip(toSeq(held.Row.GetObjects())).Exists(pair =>
                                pair.First.Attributes.IsSome || pair.Second is null || !Stored(pair.First.Geometry, upsert.Spec.BasePoint, pair.Second.Geometry)),
                            upsert.Spec with { OverrideExisting = true }).ToSeq(),
                    None: () => Seq<DefinitionOp>(upsert.Spec)))
                    + pruned)
            .As()
        select ops;

    private static bool Stored(GeometryBase spec, Point3d basePoint, GeometryBase stored) {
        using GeometryBase placed = spec.Duplicate();
        return placed.Transform(Transform.Translation(Point3d.Origin - basePoint)) && GeometryBase.GeometryEquals(placed, stored);
    }
}
