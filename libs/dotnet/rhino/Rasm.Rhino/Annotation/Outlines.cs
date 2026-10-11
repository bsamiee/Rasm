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
    public static IO<Seq<Seq<T>>> Create<T>(RhinoDoc doc, TextEntity text, OutlineOptions options,
        Func<TextEntity, DimensionStyle, bool, double, double, List<T[]>> create, string member, Action<Seq<Seq<T>>, IDisposable> retain) where T : GeometryBase =>
        DisposalOps.Acquire(output => options.Transform.IsSome
            ? DisposalOps.Within((Action<IDisposable> held) =>
                from copy in Copies.Duplicate(text, held)
                from outlines in Generate(doc, copy, options, create, member, output)
                select outlines, DisposalOps.Release)
            : Generate(doc, text, options, create, member, output), retain);

    public static IO<Seq<Seq<T>>> Create<T>(RhinoDoc doc, OutlineText text, OutlineOptions options,
        Func<TextEntity, DimensionStyle, bool, double, double, List<T[]>> create, string member, Action<Seq<Seq<T>>, IDisposable> retain) where T : GeometryBase =>
        from parent in text.Style.Match(
            Some: address => TableOps.Find(doc.DimStyles, address, includeDeleted: false),
            None: () => IO.lift(() => doc.DimStyles.Current))
        from face in text.Face.Resolve()
        from outlines in DisposalOps.Acquire(output => Typeset(doc, text, parent, face, options, create, member, output), retain)
        select outlines;

    private static IO<Seq<Seq<T>>> Typeset<T>(RhinoDoc doc, OutlineText text, DimensionStyle parent, Font face, OutlineOptions options,
        Func<TextEntity, DimensionStyle, bool, double, double, List<T[]>> create, string member, Action<IDisposable> output) where T : GeometryBase =>
        DisposalOps.Within((Action<IDisposable> held) =>
            from entity in Copies.Acquire(() => TextEntity.Create(text.Text, text.Plane, parent, wrapped: false, rectWidth: 0.0, rotationRadians: 0.0), nameof(TextEntity.Create), held)
            from assigned in IO.lift(() => Update((face, text.Height), entity))
            from generated in Generate(doc, entity, options, create, member, output)
            select generated, DisposalOps.Release);

    private static IO<Seq<Seq<T>>> Generate<T>(RhinoDoc doc, TextEntity text, OutlineOptions options,
        Func<TextEntity, DimensionStyle, bool, double, double, List<T[]>> create, string member, Action<IDisposable> retain) where T : GeometryBase =>
        from transformed in options.Transform.Traverse(xform => DisposalOps.Within((Action<IDisposable> held) =>
            from style in DimensionStyles.Effective(doc, text, None, held)
            from applied in IO.lift(() => Refused.Unless(text.Transform(xform, style), nameof(TextEntity.Transform)))
            select applied, DisposalOps.Release)).As()
        from outlines in DisposalOps.Within((Action<IDisposable> held) =>
            from style in DimensionStyles.Effective(doc, text, None, held)
            from curves in Copies.Acquire(() => {
                style.DrawForward = false;
                return create(text, style, options.SmallCapsScale.IsSome, options.SmallCapsScale.IfNone(1.0), options.Spacing);
            }, member, retain)
            select curves, DisposalOps.Release)
        select outlines;

    // --- [MAPPING]
    private static partial void Update((Font Font, double TextHeight) source, TextEntity target);
}
