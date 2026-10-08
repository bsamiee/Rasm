using Rasm.Rhino.Annotation.Styles;
using Rasm.Rhino.Document.Shapes;
using Rasm.Rhino.Document.Tables;
using Rhino;
using Rhino.DocObjects;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Annotation;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record OutlineText(string Text, Plane Plane, Option<ComponentRef<DimensionStyle>> Style, FontQuery Face, double Height);

public sealed record OutlineOptions(Option<double> SmallCapsScale, double Spacing, Option<Transform> Transform);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
public static partial class Outlines {
    // --- [OUTLINES]
    public static IO<Seq<Seq<T>>> Create<T>(RhinoDoc doc, TextEntity text, OutlineOptions options, Func<TextEntity, DimensionStyle, bool, double, double, List<T[]>> create, string member) where T : GeometryBase =>
        options.Transform.IsSome
            ? (from copy in use(Copies.Duplicate(text))
               from outlines in Generate(doc, copy, options, create, member)
               select outlines).Bracket()
            : Generate(doc, text, options, create, member);

    public static IO<Seq<Seq<T>>> Create<T>(RhinoDoc doc, OutlineText text, OutlineOptions options, Func<TextEntity, DimensionStyle, bool, double, double, List<T[]>> create, string member) where T : GeometryBase =>
        from parent in text.Style.Match(
            Some: address => TableOps.Find(doc.DimStyles, address, includeDeleted: false),
            None: () => IO.lift(() => doc.DimStyles.Current))
        from face in text.Face.Resolve()
        from outlines in (
            from entity in use(Copies.Acquire(() => TextEntity.Create(text.Text, text.Plane, parent, wrapped: false, rectWidth: 0.0, rotationRadians: 0.0), nameof(TextEntity.Create)))
            from assigned in IO.lift(() => Update((face, text.Height), entity))
            from generated in Generate(doc, entity, options, create, member)
            select generated).Bracket()
        select outlines;

    private static IO<Seq<Seq<T>>> Generate<T>(RhinoDoc doc, TextEntity text, OutlineOptions options, Func<TextEntity, DimensionStyle, bool, double, double, List<T[]>> create, string member) where T : GeometryBase =>
        from transformed in options.Transform.Traverse(xform => (
            from style in use(DimensionStyles.Effective(doc, text, None))
            from applied in IO.lift(() => Refused.Unless(text.Transform(xform, style), nameof(TextEntity.Transform)))
            select applied).Bracket()).As()
        from outlines in (
            from style in use(DimensionStyles.Effective(doc, text, None))
            from curves in Copies.Acquire(() => {
                Update(source: false, style);
                return create(text, style, options.SmallCapsScale.IsSome, options.SmallCapsScale.IfNone(1.0), options.Spacing);
            }, member)
            select curves).Bracket()
        select outlines;

    // --- [MAPPING]
    private static partial void Update((Font Font, double TextHeight) source, TextEntity target);

    [MapPropertyFromSource(nameof(DimensionStyle.DrawForward))]
    private static partial void Update(bool source, DimensionStyle target);
}
