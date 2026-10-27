using Rasm.Rhino.Annotation.Styles;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Modeling;
using Rhino;
using Rhino.DocObjects;

namespace Rasm.Rhino.Annotation;

// --- [TYPES] ---------------------------------------------------------------------------
public delegate IO<Seq<Seq<T>>> OutlineFactory<T>(TextEntity text, DimensionStyle style, bool makeSmallCaps, double smallCapsScale, double spacing)
    where T : GeometryBase;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record OutlineText(string Text, Plane Plane, Option<ComponentRef<DimensionStyle>> Style, FontQuery Face, double Height);

public sealed record OutlineOptions(Option<double> SmallCapsScale, double Spacing, Option<Transform> Transform);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Outlines {
    // --- [OUTLINES]
    public static IO<Seq<Seq<T>>> Create<T>(RhinoDoc doc, TextEntity text, OutlineOptions options, OutlineFactory<T> create) where T : GeometryBase =>
        options.Transform.IsSome
            ? from copy in Copies.Duplicate(text)
              from outlines in Generate(doc, copy, options, create)
              from released in release(copy)
              select outlines
            : Generate(doc, text, options, create);

    public static IO<Seq<Seq<T>>> Create<T>(RhinoDoc doc, OutlineText text, OutlineOptions options, OutlineFactory<T> create) where T : GeometryBase =>
        from parent in DimensionStyles.Resolve(doc, text.Style)
        from face in text.Face.Resolve()
        from entity in Products.Made(TextEntity.Create(text.Text, text.Plane, parent, wrapped: false, rectWidth: 0.0, rotationRadians: 0.0))
        from assigned in IO.lift(() => { (entity.Font, entity.TextHeight) = (face, text.Height); })
        from outlines in Generate(doc, entity, options, create)
        from released in release(entity)
        select outlines;

    private static IO<Seq<Seq<T>>> Generate<T>(RhinoDoc doc, TextEntity text, OutlineOptions options, OutlineFactory<T> create) where T : GeometryBase =>
        from transformed in options.Transform.Traverse(xform =>
            (from style in DimensionStyles.Effective(doc, text, None)
             from applied in IO.lift(() => RhinoFault.Unless(text.Transform(xform, style)))
             select applied).Bracket()).As()
        from style in DimensionStyles.Effective(doc, text, None)
        from backward in IO.lift(() => { style.DrawForward = false; })
        from created in create(text, style, options.SmallCapsScale.IsSome, options.SmallCapsScale.IfNone(1.0), options.Spacing)
        from released in release(style)
        select created;
}
