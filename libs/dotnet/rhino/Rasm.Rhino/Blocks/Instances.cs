using Rasm.RhinoCommon.Document.Tables;
using Rasm.RhinoCommon.Modeling;
using Rasm.RhinoCommon.Viewport;
using Rhino;
using Rhino.DocObjects;

namespace Rasm.RhinoCommon.Blocks;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record PieceVisibility {
    public sealed record Every() : PieceVisibility;

    public sealed record Shown() : PieceVisibility;

    public sealed record InViewport(ViewportTarget Viewport) : PieceVisibility;
}

public sealed record ExplodedPiece(RhinoObject Piece, ObjectAttributes Attributes, Transform Xform);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Instances {
    // --- [READS]
    public static IO<TValue> Explode<TValue>(RhinoDoc doc, Guid instance, bool explodeNested, PieceVisibility visibility, Func<Seq<ExplodedPiece>, IO<TValue>> body) =>
        from resolved in ObjectTarget.Resolve<InstanceObject, InstanceReferenceGeometry>(doc, instance)
        from shown in visibility.Switch(
            doc,
            every: static (_, _) => IO.pure((SkipHidden: false, Viewport: Guid.Empty)),
            shown: static (_, _) => IO.pure((SkipHidden: true, Viewport: Guid.Empty)),
            inViewport: static (document, shown) => Viewports.With(document, shown.Viewport, static row => IO.pure((SkipHidden: true, Viewport: row.Viewport.Id))))
        from value in (from answer in IO.lift(() => {
            resolved.Object.Explode(shown.SkipHidden, shown.Viewport, explodeNested, out RhinoObject?[] objects, out ObjectAttributes[] attributes, out Transform[] xforms);
            return (Objects: objects, Attributes: attributes, Xforms: xforms);
        })
                       from owned in Products.Made(answer.Attributes)
                       from pieces in IO.lift(() => Callbacks.Each(toSeq(answer.Objects.Zip(answer.Attributes, answer.Xforms)), static (row, index) =>
                           row switch {
                               var (piece, attributes, xform) => Conversions.Found(piece, index).Map(held => new ExplodedPiece(held, attributes, xform)),
                           }))
                       from result in body(pieces)
                       select result).Bracket()
        select value;

    // --- [WRITES]
    public static IO<Seq<Guid>> Place(RhinoDoc doc, ComponentRef<InstanceDefinition> address, Seq<Transform> xforms, Option<ObjectAttributes> attributes, Option<HistoryRecord> history, bool reference) =>
        from definition in TableOps.Find(doc.InstanceDefinitions, address, includeDeleted: false)
        from ids in IO.lift(() => Callbacks.Each(xforms, (xform, index) =>
            Conversions.Found(Conversions.Present(doc.Objects.AddInstanceObject(definition.Index, xform, attributes.ValueUnsafe(), history.ValueUnsafe(), reference)), index)))
        select ids;

    public static IO<Seq<Guid>> Replace(RhinoDoc doc, ObjectTarget target, ComponentRef<InstanceDefinition> address) =>
        from definition in TableOps.Find(doc.InstanceDefinitions, address, includeDeleted: false)
        from objects in target.Objects(doc)
        from ids in IO.lift(() => Callbacks.Each(objects, (found, index) =>
            RhinoFault.Unless(doc.Objects.ReplaceInstanceObject(found.Id, definition.Index), index).Map(_ => found.Id)))
        select ids;

    public static IO<Seq<Guid>> AddExplodedPieces(RhinoDoc doc, Guid instance, bool explodeNested, PieceVisibility visibility, bool deleteInstance) =>
        from ids in Explode(doc, instance, explodeNested, visibility, pieces => pieces
            .Map(static (piece, index) => (Piece: piece, Index: index)).TraverseM(row => Added(doc, row.Piece, row.Index)).As())
        from deleted in when(deleteInstance, IO.lift(() => RhinoFault.Unless(doc.Objects.Delete(instance, quiet: true)))).As()
        select ids;

    private static IO<Guid> Added(RhinoDoc doc, ExplodedPiece piece, int index) =>
        (piece.Piece is InstanceObject nested
            ? from definition in IO.lift(() => Conversions.Found(nested.InstanceDefinition))
              from id in IO.lift(() => Conversions.Found(Conversions.Present(doc.Objects.AddInstanceObject(definition.Index, piece.Xform * nested.InstanceXform, piece.Attributes))))
              select id
            : (from copy in Products.Made(piece.Piece.DuplicateGeometry())
               from moved in Transformations.Transformed(copy, piece.Xform)
               from id in TableOps.Add(doc, new GeometryPair(moved, Some(piece.Attributes)), None, reference: false)
               select id).Bracket())
        .MapFail(error => Error.Many(error.FoldM(cause => Seq(cause is RhinoFault fault ? new RhinoFault(fault.Subject, fault.Reason, index, fault.Inner) : cause)).As()));
}
