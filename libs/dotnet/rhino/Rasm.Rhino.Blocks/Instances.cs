using LanguageExt.UnsafeValueAccess;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.Runtime;
using Rhino.UI;

namespace Rasm.Rhino.Blocks;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record InstancePlacement(Transform Xform, Option<ObjectAttributes> Attributes, Option<HistoryRecord> History, bool Reference);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FieldSource {
    public sealed record Literal(string Text) : FieldSource;

    public sealed record FromObject(Guid TextObjectId) : FieldSource;

    public sealed record FromDefinition(ComponentRef Key) : FieldSource;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PieceVisibility {
    public sealed record Every() : PieceVisibility;

    public sealed record Shown() : PieceVisibility;

    public sealed record InViewport(Guid ViewportId) : PieceVisibility;
}

public sealed record ExplodedPiece(GeometryPair Piece, Transform Xform);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Instances {
    // --- [WRITES]
    public static IO<Seq<Guid>> Place(RhinoDoc doc, ComponentRef key, Seq<InstancePlacement> placements, RedrawPolicy redraw) =>
        Commits.Commit(doc, LOC.STR("Place block"), redraw,
            from definition in Definitions.Require(doc, key, includeDeleted: false)
            from ids in placements
                .Map(static (placement, index) => (Placement: placement, Index: index))
                .TraverseM(row =>
                    from valid in when(!row.Placement.Xform.IsValid, IO.fail<Unit>(new InvalidElement(nameof(Transform.IsValid), row.Index)))
                    from id in TableOps.WithAttributes(doc, row.Placement.Attributes, attributes => IO.lift(() => Answers.NonEmpty(
                        doc.Objects.AddInstanceObject(definition.Index, row.Placement.Xform, attributes, row.Placement.History.ValueUnsafe(), row.Placement.Reference),
                        nameof(ObjectTable.AddInstanceObject))))
                    select id)
                .As()
            select ids);

    public static IO<Seq<Guid>> AddExplodedPieces(RhinoDoc doc, Guid instance, bool explodeNested, bool deleteInstance, RedrawPolicy redraw) =>
        Commits.Commit(doc, LOC.STR("Add exploded block pieces"), redraw,
            from reference in Instance(doc, instance)
            from exploded in IO.lift(() => {
                reference.Explode(explodeNested, out RhinoObject[] pieces, out ObjectAttributes[] attributes, out Transform[] _);
                return (Pieces: toSeq(pieces), Attributes: toSeq(attributes));
            })
            from released in Disposal.Release(exploded.Pieces.Map<IDisposable>(static piece => piece) + exploded.Attributes.Map<IDisposable>(static attributes => attributes))
            from ids in IO.lift(() => Optional(doc.Objects.AddExplodedInstancePieces(reference, explodeNested, deleteInstance)).ToSeq().Bind(static pieces => toSeq(pieces)).Strict())
            from complete in IO.lift(() => CountMismatch.Unless(exploded.Pieces.Count, ids.Count, nameof(ObjectTable.AddExplodedInstancePieces)))
            select ids);

    // --- [READS]
    public static IO<TValue> Explode<TValue>(RhinoDoc doc, Guid instance, bool explodeNested, PieceVisibility visibility, Func<Seq<ExplodedPiece>, IO<TValue>> body) =>
        from reference in Instance(doc, instance)
        let hidden = visibility.Switch(
            every: static _ => (SkipHidden: false, Viewport: Guid.Empty),
            shown: static _ => (SkipHidden: true, Viewport: Guid.Empty),
            inViewport: static viewport => (SkipHidden: true, Viewport: viewport.ViewportId))
        from exploded in IO.lift(() => {
            reference.Explode(hidden.SkipHidden, hidden.Viewport, explodeNested, out RhinoObject[] pieces, out ObjectAttributes[] attributes, out Transform[] xforms);
            return toSeq(pieces).Map((piece, index) => (Piece: piece, Attributes: attributes[index], Xform: xforms[index])).Strict();
        })
        from value in Disposal.Using(
            IO.pure(exploded.Map<IDisposable>(static row => row.Piece) + exploded.Map<IDisposable>(static row => row.Attributes)),
            _ => Disposal.Using(
                Disposal.AcquireAll(exploded.Map(static row => DuplicateMode.Duplicate.Acquire(row.Piece.Geometry))),
                handles => body(exploded.Zip(handles, static (row, handle) => new ExplodedPiece(new GeometryPair(handle.Value, Some(row.Attributes)), row.Xform)))))
        select value;

    public static IO<Seq<TextFields.InstanceAttributeField>> Fields(RhinoDoc doc, FieldSource source) =>
        source.Switch(
            doc,
            literal: static (_, literal) => IO.lift(() => toSeq(TextFields.GetInstanceAttributeFields(literal.Text))),
            fromObject: static (document, fromObject) =>
                from text in Queries.Resolve<TextObject, TextEntity>(document, fromObject.TextObjectId)
                from fields in IO.lift(() => toSeq(TextFields.GetInstanceAttributeFields(text.Object)))
                select fields,
            fromDefinition: static (document, fromDefinition) =>
                from definition in Definitions.Require(document, fromDefinition.Key, includeDeleted: false)
                from fields in IO.lift(() => toSeq(TextFields.GetInstanceAttributeFields(definition)))
                select fields);

    private static IO<InstanceObject> Instance(RhinoDoc doc, Guid id) =>
        Queries.Resolve<InstanceObject, InstanceReferenceGeometry>(doc, id).Map(static resolved => resolved.Object);
}
