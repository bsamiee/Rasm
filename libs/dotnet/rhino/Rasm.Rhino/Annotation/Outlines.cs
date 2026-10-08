using Rasm.Rhino.Annotation.Styles;
using Rasm.Rhino.Document.Shapes;
using Rasm.Rhino.Document.Tables;
using Rhino;
using Rhino.DocObjects;

namespace Rasm.Rhino.Annotation;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record OutlineText {
    public sealed record Entity(TextEntity Text) : OutlineText;

    public sealed record Written(string Text, Plane Plane, Option<ComponentRef<DimensionStyle>> Style, FontQuery Face, double Height) : OutlineText;
}

public sealed record OutlineOptions(Option<double> SmallCapsScale, double Spacing, Option<Transform> Transform);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Outlines {
    // --- [OUTLINES]
    public static IO<Seq<Seq<Curve>>> Curves(RhinoDoc doc, OutlineText text, bool allowOpen, OutlineOptions options) =>
        Outlined(doc, text, options, (entity, style, makeSmallCaps, smallCapsScale, spacing) => entity.CreateCurvesGrouped(style, allowOpen, makeSmallCaps, smallCapsScale, spacing), nameof(TextEntity.CreateCurvesGrouped));

    public static IO<Seq<Seq<Brep>>> Surfaces(RhinoDoc doc, OutlineText text, OutlineOptions options) =>
        Outlined(doc, text, options, static (entity, style, makeSmallCaps, smallCapsScale, spacing) => entity.CreateSurfacesGrouped(style, makeSmallCaps, smallCapsScale, spacing), nameof(TextEntity.CreateSurfacesGrouped));

    public static IO<Seq<Seq<Brep>>> PolySurfaces(RhinoDoc doc, OutlineText text, double height, OutlineOptions options) =>
        Outlined(doc, text, options, (entity, style, makeSmallCaps, smallCapsScale, spacing) => entity.CreatePolysurfacesGrouped(style, makeSmallCaps, smallCapsScale, height, spacing), nameof(TextEntity.CreatePolysurfacesGrouped));

    public static IO<Seq<Seq<Extrusion>>> Extrusions(RhinoDoc doc, OutlineText text, double height, OutlineOptions options) =>
        Outlined(doc, text, options, (entity, style, makeSmallCaps, smallCapsScale, spacing) => entity.CreateExtrusionsGrouped(style, makeSmallCaps, smallCapsScale, height, spacing), nameof(TextEntity.CreateExtrusionsGrouped));

    private static IO<Seq<Seq<T>>> Outlined<T>(RhinoDoc doc, OutlineText text, OutlineOptions options, Func<TextEntity, DimensionStyle, bool, double, double, List<T[]>> create, string member) where T : GeometryBase =>
        Sourced(doc, text, entity => Framed(doc, entity, options.Transform, (framed, style) =>
            Copies.Acquire<T>(() => create(framed, style, options.SmallCapsScale.IsSome, options.SmallCapsScale.IfNone(1.0), options.Spacing), member)));

    // --- [SOURCES]
    private static IO<A> Sourced<A>(RhinoDoc doc, OutlineText text, Func<TextEntity, IO<A>> body) =>
        text.Switch(
            (Doc: doc, Body: body),
            entity: static (state, entity) => state.Body(entity.Text),
            written: static (state, written) =>
                from parent in written.Style.Match(
                    Some: address => TableOps.Find(state.Doc.DimStyles, address, includeDeleted: false),
                    None: () => IO.lift(() => state.Doc.DimStyles.Current))
                from face in written.Face.Resolve()
                from outlined in (
                    from created in use(Copies.Acquire(() => TextEntity.Create(written.Text, written.Plane, parent, wrapped: false, rectWidth: 0.0, rotationRadians: 0.0), nameof(TextEntity.Create)))
                    from overridden in IO.lift(() => {
                        created.Font = face;
                        created.TextHeight = written.Height;
                    })
                    from value in state.Body(created)
                    select value).Bracket()
                select outlined);

    // --- [FRAMES]
    private static IO<A> Framed<A>(RhinoDoc doc, TextEntity text, Option<Transform> transform, Func<TextEntity, DimensionStyle, IO<A>> body) =>
        transform.Match(
            Some: xform => (
                from copy in use(Copies.Duplicate(text))
                from transformed in DimensionStyles.Effective(doc, copy, None, style => IO.lift(() => Refused.Unless(copy.Transform(xform, style), nameof(TextEntity.Transform))))
                from value in Styled(doc, copy, body)
                select value).Bracket(),
            None: () => Styled(doc, text, body));

    private static IO<A> Styled<A>(RhinoDoc doc, TextEntity text, Func<TextEntity, DimensionStyle, IO<A>> body) =>
        DimensionStyles.Effective(doc, text, None, style => IO.lift(() => { style.DrawForward = false; }).Bind(_ => body(text, style)));
}
