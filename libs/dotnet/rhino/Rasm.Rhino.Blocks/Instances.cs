using Rasm.Rhino.Document;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.UI;

namespace Rasm.Rhino.Blocks;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record InstancePlacement(Transform Xform, Option<ObjectAttributes> Attributes, Option<HistoryRecord> History, bool Reference);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PieceVisibility {
    public sealed record Every() : PieceVisibility;

    public sealed record Shown() : PieceVisibility;

    public sealed record InViewport(Guid ViewportId) : PieceVisibility;
}

public sealed record ExplodedPiece(RhinoObject Piece, ObjectAttributes Attributes, Transform Xform) : IDisposable {
    public void Dispose() {
        Piece.Dispose();
        Attributes.Dispose();
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Instances {
    // --- [READS]
    public static IO<TValue> Explode<TValue>(RhinoDoc doc, Guid instance, bool explodeNested, PieceVisibility visibility, Func<Seq<ExplodedPiece>, IO<TValue>> body) =>
        from reference in Queries.Resolve<InstanceObject, InstanceReferenceGeometry>(doc, instance)
        let host = visibility.Switch(
            every: static _ => (SkipHidden: false, Viewport: Guid.Empty),
            shown: static _ => (SkipHidden: true, Viewport: Guid.Empty),
            inViewport: static viewport => (SkipHidden: true, Viewport: viewport.ViewportId))
        from value in DisposalOps.Using(IO.lift(() => Exploded(reference.Object, host.SkipHidden, host.Viewport, explodeNested)), body)
        select value;

    private static Seq<ExplodedPiece> Exploded(InstanceObject reference, bool skipHidden, Guid viewport, bool explodeNested) {
        reference.Explode(skipHidden, viewport, explodeNested, out RhinoObject[] pieces, out ObjectAttributes[] attributes, out Transform[] xforms);
        return toSeq(pieces).Zip(toSeq(attributes)).Zip(toSeq(xforms), static (row, xform) => new ExplodedPiece(row.First, row.Second, xform)).Strict();
    }

    // --- [WRITES]
    public static IO<Seq<Guid>> Place(RhinoDoc doc, ComponentRef key, Seq<InstancePlacement> placements, RedrawPolicy redraw) =>
        Commits.Commit(doc, LOC.STR("Insert block"), redraw,
            from definition in TableOps.Find(doc.InstanceDefinitions, key, includeDeleted: false)
            from ids in placements
                .Map((placement, index) => TableOps.WithAttributes(doc, placement.Attributes, attributes => IO.lift(() =>
                    Answers.Present(doc.Objects.AddInstanceObject(definition.Index, placement.Xform, attributes, placement.History.ValueUnsafe(), placement.Reference))
                        .ToFin(new RefusedElement(nameof(ObjectTable.AddInstanceObject), index)))))
                .TraverseM(identity)
                .As()
            select ids);

    public static IO<Seq<Guid>> Replace(RhinoDoc doc, ObjectTarget target, ComponentRef key, RedrawPolicy redraw) =>
        Commits.Commit(doc, LOC.STR("Replace block"), redraw,
            from definition in TableOps.Find(doc.InstanceDefinitions, key, includeDeleted: false)
            from objects in Queries.Evaluate(doc, target)
            from ids in objects
                .Map((found, index) => IO.lift(() =>
                    RefusedElement.Unless(doc.Objects.ReplaceInstanceObject(found.Id, definition.Index), found.Id, nameof(ObjectTable.ReplaceInstanceObject), index)))
                .TraverseM(identity)
                .As()
            select ids);

    public static IO<Seq<Guid>> AddExplodedPieces(RhinoDoc doc, Guid instance, bool explodeNested, PieceVisibility visibility, bool deleteInstance, RedrawPolicy redraw) =>
        Commits.Commit(doc, LOC.STR("Explode block"), redraw,
            from ids in Explode(doc, instance, explodeNested, visibility, pieces => pieces
                .Map((piece, index) => piece.Piece switch {
                    InstanceObject nested => IO.lift(() => Answers.Present(doc.Objects.AddInstanceObject(nested.InstanceDefinition.Index, piece.Xform * nested.InstanceXform, piece.Attributes))
                        .ToFin(new RefusedElement(nameof(ObjectTable.AddInstanceObject), index))),
                    RhinoObject member => DisposalOps.Using(
                        IO.lift(() => Missing.Unless(member.DuplicateGeometry(), nameof(RhinoObject.DuplicateGeometry))),
                        copy => IO.lift(() =>
                            from moved in RefusedElement.Unless(copy.Transform(piece.Xform), nameof(GeometryBase.Transform), index)
                            from id in Answers.Present(doc.Objects.Add(copy, piece.Attributes)).ToFin(new RefusedElement(nameof(ObjectTable.Add), index))
                            select id)),
                })
                .TraverseM(identity)
                .As())
            from deleted in when(deleteInstance, IO.lift(() => Refused.Unless(doc.Objects.Delete(instance, quiet: true), nameof(ObjectTable.Delete)))).As()
            select ids);
}
