using Rasm.Rhino.Document.Shapes;
using Rasm.Rhino.Document.Tables;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;

namespace Rasm.Rhino.Blocks;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record PieceVisibility {
    public sealed record Every() : PieceVisibility;

    public sealed record Shown() : PieceVisibility;

    public sealed record InViewport(RhinoViewport Viewport) : PieceVisibility;
}

public sealed record ExplodedPiece(RhinoObject Piece, ObjectAttributes Attributes, Transform Xform);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Instances {
    // --- [READS]
    public static IO<TValue> Explode<TValue>(RhinoDoc doc, Guid instance, bool explodeNested, PieceVisibility visibility, Func<Seq<ExplodedPiece>, IO<TValue>> body) =>
        (from resolved in ObjectTarget.Resolve<InstanceObject, InstanceReferenceGeometry>(doc, instance)
         from answer in use(() => {
             (bool skipHidden, Guid viewport) = visibility.Switch(
                 every: static _ => (false, Guid.Empty),
                 shown: static _ => (true, Guid.Empty),
                 inViewport: static shown => (true, shown.Viewport.Id));
             resolved.Object.Explode(skipHidden, viewport, explodeNested, out RhinoObject?[] objects, out ObjectAttributes[] attributes, out Transform[] xforms);
             return (Objects: objects, Attributes: attributes, Xforms: xforms);
         }, static answer => DisposalOps.Release(toSeq(answer.Attributes)))
         from pieces in IO.lift(Callbacks.Each(toSeq(answer.Objects.Zip(answer.Attributes, answer.Xforms)), static Fin<ExplodedPiece> (row, index) =>
             row.First is { } piece ? new ExplodedPiece(piece, row.Second, row.Third) : new RefusedElement(nameof(InstanceObject.Explode), index)))
         from result in body(pieces)
         select result).Bracket();

    // --- [WRITES]
    public static IO<Seq<Guid>> Place(RhinoDoc doc, ComponentRef<InstanceDefinition> address, Seq<Transform> xforms, Option<ObjectAttributes> attributes, Option<HistoryRecord> history, bool reference) =>
        from definition in TableOps.Find(doc.InstanceDefinitions, address, includeDeleted: false)
        from ids in IO.lift(() => Callbacks.Each(xforms, (xform, index) =>
            Conversions.Present(doc.Objects.AddInstanceObject(definition.Index, xform, attributes.ValueUnsafe(), history.ValueUnsafe(), reference))
                .ToFin(new RefusedElement(nameof(ObjectTable.AddInstanceObject), index))))
        select ids;

    public static IO<Seq<Guid>> Replace(RhinoDoc doc, ObjectTarget target, ComponentRef<InstanceDefinition> address) =>
        from definition in TableOps.Find(doc.InstanceDefinitions, address, includeDeleted: false)
        from objects in target.Objects(doc)
        from ids in IO.lift(() => Callbacks.Each(objects, (found, index) =>
            RefusedElement.Unless(doc.Objects.ReplaceInstanceObject(found.Id, definition.Index), found.Id, nameof(ObjectTable.ReplaceInstanceObject), index)))
        select ids;

    public static IO<Seq<Guid>> AddExplodedPieces(RhinoDoc doc, Guid instance, bool explodeNested, PieceVisibility visibility, bool deleteInstance) =>
        from ids in Explode(doc, instance, explodeNested, visibility, pieces => pieces
            .Map(static (piece, index) => (Piece: piece, Index: index)).TraverseM(row => Added(doc, row.Piece, row.Index)).As())
        from deleted in when(deleteInstance, IO.lift(() => Refused.Unless(doc.Objects.Delete(instance, quiet: true), nameof(ObjectTable.Delete)))).As()
        select ids;

    private static IO<Guid> Added(RhinoDoc doc, ExplodedPiece piece, int index) =>
        (piece.Piece is InstanceObject nested
            ? from definition in IO.lift(() => Optional(nested.InstanceDefinition).ToFin(new Refused(nameof(InstanceObject.InstanceDefinition))))
              from id in IO.lift(() => Conversions.Required(doc.Objects.AddInstanceObject(definition.Index, piece.Xform * nested.InstanceXform, piece.Attributes), nameof(ObjectTable.AddInstanceObject)))
              select id
            : (from copy in use(Copies.Acquire(piece.Piece.DuplicateGeometry, nameof(RhinoObject.DuplicateGeometry)))
               from moved in Transformations.Transformed(copy, piece.Xform)
               from id in TableOps.Add(doc, new GeometryPair(moved, Some(piece.Attributes)), None, reference: false)
               select id).Bracket())
        .MapFail(error => Indexed(error, index));

    private static Error Indexed(Error error, int index) =>
        Error.Many(error.FoldM(cause => Seq(cause is Refused refused ? new RefusedElement(refused.Member, index) : cause)).As());
}
