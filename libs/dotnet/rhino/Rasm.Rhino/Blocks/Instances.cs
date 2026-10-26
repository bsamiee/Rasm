using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Modeling;
using Rasm.Rhino.Viewport;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;

namespace Rasm.Rhino.Blocks;

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
        from value in DisposalOps.Within(
            () => {
                resolved.Object.Explode(shown.SkipHidden, shown.Viewport, explodeNested, out RhinoObject?[] objects, out ObjectAttributes[] attributes, out Transform[] xforms);
                return (Objects: objects, Attributes: attributes, Xforms: xforms);
            },
            answer => IO.lift(() => Callbacks.Each(toSeq(answer.Objects.Zip(answer.Attributes, answer.Xforms)), static Fin<ExplodedPiece> (row, index) =>
                row.First is { } piece ? new ExplodedPiece(piece, row.Second, row.Third) : new Refused(nameof(InstanceObject.Explode), Refused.Declined, index))).Bind(body),
            static answer => DisposalOps.Release(toSeq(answer.Attributes)))
        select value;

    // --- [WRITES]
    public static IO<Seq<Guid>> Place(RhinoDoc doc, ComponentRef<InstanceDefinition> address, Seq<Transform> xforms, Option<ObjectAttributes> attributes, Option<HistoryRecord> history, bool reference) =>
        from definition in TableOps.Find(doc.InstanceDefinitions, address, includeDeleted: false)
        from ids in IO.lift(() => Callbacks.Each(xforms, (xform, index) =>
            Conversions.Present(doc.Objects.AddInstanceObject(definition.Index, xform, attributes.ValueUnsafe(), history.ValueUnsafe(), reference))
                .ToFin(new Refused(nameof(ObjectTable.AddInstanceObject), Refused.Declined, index))))
        select ids;

    public static IO<Seq<Guid>> Replace(RhinoDoc doc, ObjectTarget target, ComponentRef<InstanceDefinition> address) =>
        from definition in TableOps.Find(doc.InstanceDefinitions, address, includeDeleted: false)
        from objects in target.Objects(doc)
        from ids in IO.lift(() => Callbacks.Each(objects, (found, index) =>
            Refused.Unless(doc.Objects.ReplaceInstanceObject(found.Id, definition.Index), found.Id, nameof(ObjectTable.ReplaceInstanceObject), index)))
        select ids;

    public static IO<Seq<Guid>> AddExplodedPieces(RhinoDoc doc, Guid instance, bool explodeNested, PieceVisibility visibility, bool deleteInstance) =>
        from ids in Explode(doc, instance, explodeNested, visibility, pieces => pieces
            .Map(static (piece, index) => (Piece: piece, Index: index)).TraverseM(row => Added(doc, row.Piece, row.Index)).As())
        from deleted in when(deleteInstance, IO.lift(() => Refused.Unless(doc.Objects.Delete(instance, quiet: true), nameof(ObjectTable.Delete)))).As()
        select ids;

    private static IO<Guid> Added(RhinoDoc doc, ExplodedPiece piece, int index) =>
        (piece.Piece is InstanceObject nested
            ? from definition in IO.lift(() => Optional(nested.InstanceDefinition).ToFin(new Refused(nameof(InstanceObject.InstanceDefinition), Refused.Declined)))
              from id in IO.lift(() => Conversions.Required(doc.Objects.AddInstanceObject(definition.Index, piece.Xform * nested.InstanceXform, piece.Attributes), nameof(ObjectTable.AddInstanceObject)))
              select id
            : DisposalOps.Within<IDisposable, Guid>(retain =>
               from copy in Copies.Acquire(piece.Piece.DuplicateGeometry, nameof(RhinoObject.DuplicateGeometry), retain)
               from moved in Transformations.Transformed(copy, piece.Xform)
               from id in TableOps.Add(doc, new GeometryPair(moved, Some(piece.Attributes)), None, reference: false)
               select id, DisposalOps.Release))
        .MapFail(error => Error.Many(error.FoldM(cause => Seq(cause is Refused refused ? new Refused(refused.Member, refused.Reason, index) : cause)).As()));
}
