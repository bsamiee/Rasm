using Rasm.Rhino.Viewport;
using Rhino;
using Rhino.DocObjects;
using Rhino.Runtime;

namespace Rasm.Rhino.Blocks;

// --- [MODELS] --------------------------------------------------------------------------
[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record PieceVisibility {
    public sealed record Every : PieceVisibility;

    public sealed record Shown : PieceVisibility;

    public sealed record InViewport(ViewportTarget Viewport) : PieceVisibility;

    internal IO<(bool SkipHidden, Guid Viewport)> Resolved(RhinoDoc doc) =>
        Switch(
            doc,
            every: static (_, _) => IO.pure((SkipHidden: false, Viewport: Guid.Empty)),
            shown: static (_, _) => IO.pure((SkipHidden: true, Viewport: Guid.Empty)),
            inViewport: static (document, shown) => Viewports.With(document, shown.Viewport, static row => IO.pure((SkipHidden: true, Viewport: row.Viewport.Id))));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Instances {
    // --- [FACTS]
    extension(InstanceObject instance) {
        public Option<int> Depth(InstanceDefinition definition) =>
            Conversions.Present(instance.UsesDefinition(definition.Index, out int nestingLevel), nestingLevel);

        public IO<Seq<(TextFields.InstanceAttributeField Field, string Value)>> AttributeFields() =>
            from definition in IO.lift(() => Conversions.Required(instance.InstanceDefinition))
            from fields in IO.lift(() => Conversions.Rows(TextFields.GetInstanceAttributeFields(definition)))
            select fields.Map(field => (Field: field, Value: Conversions.Present(instance.Attributes.GetUserString(field.Key)).IfNone(field.DefaultValue))).Strict();

        // --- [EXPLODE]
        public IO<TValue> Explode<TValue>(RhinoDoc doc, bool explodeNested, PieceVisibility visibility, Func<Seq<(RhinoObject Piece, ObjectAttributes Attributes, Transform Xform)>, IO<TValue>> body) =>
            from shown in visibility.Resolved(doc)
            from value in (from answer in IO.lift(() => {
                instance.Explode(shown.SkipHidden, shown.Viewport, explodeNested, out RhinoObject?[] objects, out ObjectAttributes[] attributes, out Transform[] xforms);
                return (Objects: objects, Attributes: attributes, Xforms: xforms);
            })
                           from owned in Products.Many(() => answer.Attributes, required: false)
                           from pieces in IO.lift(() => Products.Each(toSeq(answer.Objects.Zip(answer.Attributes, answer.Xforms)), static (row, index) =>
                               Conversions.Required(row.First, index).Map(held => (Piece: held, Attributes: row.Second, Xform: row.Third))))
                           from result in body(pieces)
                           select result).Bracket()
            select value;

        public IO<Seq<Guid>> AddExplodedPieces(RhinoDoc doc, bool explodeNested, PieceVisibility visibility, bool deleteInstance) =>
            from ids in instance.Explode(doc, explodeNested, visibility, pieces => pieces.Map((piece, index) => Added(doc, piece, index)).TraverseM(static added => added).As())
            from deleted in when(deleteInstance, Products.Accepted(() => doc.Objects.Delete(instance, quiet: true))).As()
            select ids;
    }

    // --- [ATTRIBUTES]
    public static Fin<Seq<Step<ObjectAttributes>>> Filled(InstanceDefinition definition, HashMap<string, string> values) =>
        toHashSet(Conversions.Rows(TextFields.GetInstanceAttributeFields(definition)).Map(static field => field.Key)) switch {
            var keys => Products.Each(toSeq<(string Key, string Value)>(values), (entry, index) => RhinoFault.Unless(keys.Contains(entry.Key), RhinoReason.Missing, entry.Key, index)
                .Map(_ => Steps.Of<ObjectAttributes>(staged => staged.SetUserString(entry.Key, entry.Value)))),
        };

    // --- [WRITES]
    public static IO<Seq<Guid>> Place(RhinoDoc doc, InstanceDefinition definition, Seq<Transform> xforms, Option<ObjectAttributes> attributes, Option<HistoryRecord> history, bool reference) =>
        IO.lift(() => Products.Each(xforms, (xform, index) =>
            Conversions.Required(Conversions.Present(doc.Objects.AddInstanceObject(definition.Index, xform, attributes.ValueUnsafe(), history.ValueUnsafe(), reference)), index)));

    private static IO<Guid> Added(RhinoDoc doc, (RhinoObject Piece, ObjectAttributes Attributes, Transform Xform) piece, int index) =>
        piece.Piece is InstanceObject nested
            ? IO.lift(() =>
                from definition in Conversions.Required(nested.InstanceDefinition, index)
                from id in Conversions.Required(Conversions.Present(doc.Objects.AddInstanceObject(definition.Index, piece.Xform * nested.InstanceXform, piece.Attributes)), index)
                select id)
            : (from copy in use(() => piece.Piece.DuplicateGeometry())
               from moved in IO.lift(() => RhinoFault.Unless(copy.Transform(piece.Xform), RhinoReason.Declined, piece.Xform, index))
               from id in IO.lift(() => Conversions.Required(Conversions.Present(doc.Objects.Add(copy, piece.Attributes)), index))
               select id).Bracket();
}
