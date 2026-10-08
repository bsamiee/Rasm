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
        ObjectTarget.Resolve<InstanceObject, InstanceReferenceGeometry>(doc, instance).Bind(resolved =>
            IO.lift(() => Exploded(resolved.Object, explodeNested, visibility)).Bracket(
                Use: rows => IO.lift(Callbacks.Each(rows, static Fin<ExplodedPiece> (row, index) =>
                        row.Piece is { } piece ? new ExplodedPiece(piece, row.Attributes, row.Xform) : new RefusedElement(nameof(InstanceObject.Explode), index)))
                    .Bind(body),
                Fin: static rows => DisposalOps.Release(rows.Map(static row => row.Attributes))));

    private static Seq<(RhinoObject? Piece, ObjectAttributes Attributes, Transform Xform)> Exploded(InstanceObject instance, bool explodeNested, PieceVisibility visibility) {
        (bool skipHidden, Guid viewport) = visibility.Switch(
            every: static _ => (false, Guid.Empty),
            shown: static _ => (true, Guid.Empty),
            inViewport: static shown => (true, shown.Viewport.Id));
        instance.Explode(skipHidden, viewport, explodeNested, out RhinoObject?[] pieces, out ObjectAttributes[] attributes, out Transform[] xforms);
        return toSeq(pieces.Zip(attributes, xforms)).Strict();
    }

    // --- [WRITES]
    public static IO<Seq<Guid>> Place(RhinoDoc doc, ComponentRef<InstanceDefinition> address, Seq<Transform> xforms, Option<ObjectAttributes> attributes, Option<HistoryRecord> history, bool reference) =>
        TableOps.Find(doc.InstanceDefinitions, address, includeDeleted: false).Bind(definition => IO.lift(() =>
            Callbacks.Each(xforms, (xform, index) =>
                Conversions.Present(doc.Objects.AddInstanceObject(definition.Index, xform, attributes.ValueUnsafe(), history.ValueUnsafe(), reference))
                    .ToFin(new RefusedElement(nameof(ObjectTable.AddInstanceObject), index)))));

    public static IO<Seq<Guid>> Replace(RhinoDoc doc, ObjectTarget target, ComponentRef<InstanceDefinition> address) =>
        from definition in TableOps.Find(doc.InstanceDefinitions, address, includeDeleted: false)
        from objects in target.Objects(doc)
        from ids in IO.lift(() => Callbacks.Each(objects, (found, index) =>
            RefusedElement.Unless(doc.Objects.ReplaceInstanceObject(found.Id, definition.Index), found.Id, nameof(ObjectTable.ReplaceInstanceObject), index)))
        select ids;

    public static IO<Seq<Guid>> AddExplodedPieces(RhinoDoc doc, Guid instance, bool explodeNested, PieceVisibility visibility, bool deleteInstance) =>
        from ids in Explode(doc, instance, explodeNested, visibility, pieces => pieces
            .Map(static (piece, index) => (Piece: piece, Index: index))
            .TraverseM(row => Added(doc, row.Piece).MapFail(error => error.Filter<Refused>().Head is Refused refused ? new RefusedElement(refused.Member, row.Index) : error))
            .As())
        from deleted in when(deleteInstance, IO.lift(() => Refused.Unless(doc.Objects.Delete(instance, quiet: true), nameof(ObjectTable.Delete)))).As()
        select ids;

    private static IO<Guid> Added(RhinoDoc doc, ExplodedPiece piece) =>
        piece.Piece is InstanceObject nested
            ? IO.lift(() =>
                from definition in Optional(nested.InstanceDefinition).ToFin(new Refused(nameof(InstanceObject.InstanceDefinition)))
                from id in Conversions.Required(doc.Objects.AddInstanceObject(definition.Index, piece.Xform * nested.InstanceXform, piece.Attributes), nameof(ObjectTable.AddInstanceObject))
                select id)
            : (from copy in use(Copies.Acquire(piece.Piece.DuplicateGeometry, nameof(RhinoObject.DuplicateGeometry)))
               from moved in Transformations.Transformed(copy, piece.Xform)
               from id in TableOps.Add(doc, new GeometryPair(moved, Some(piece.Attributes)), None, reference: false)
               select id).Bracket();
}
