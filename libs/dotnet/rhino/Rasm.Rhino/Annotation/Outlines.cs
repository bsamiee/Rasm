using Rasm.Rhino.Annotation.Styles;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Modeling;
using Rhino;
using Rhino.DocObjects;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Annotation;

// --- [TYPES] ---------------------------------------------------------------------------
public delegate IO<Seq<Seq<T>>> OutlineFactory<T>(TextEntity text, DimensionStyle style, bool makeSmallCaps, double smallCapsScale, double spacing, Action<IDisposable> retain)
    where T : GeometryBase;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record OutlineText(string Text, Plane Plane, Option<ComponentRef<DimensionStyle>> Style, FontQuery Face, double Height);

public sealed record OutlineOptions(Option<double> SmallCapsScale, double Spacing, Option<Transform> Transform);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
public static partial class Outlines {
    // --- [OUTLINES]
    public static IO<Seq<Seq<T>>> Create<T>(RhinoDoc doc, TextEntity text, OutlineOptions options,
        OutlineFactory<T> create, Action<Seq<Seq<T>>, IDisposable> retain) where T : GeometryBase =>
        DisposalOps.Acquire(output => options.Transform.IsSome
            ? DisposalOps.Within((Action<IDisposable> held) =>
                from copy in Copies.Duplicate(text, held)
                from outlines in Generate(doc, copy, options, create, output)
                select outlines, DisposalOps.Release)
            : Generate(doc, text, options, create, output), retain);

    public static IO<Seq<Seq<T>>> Create<T>(RhinoDoc doc, OutlineText text, OutlineOptions options,
        OutlineFactory<T> create, Action<Seq<Seq<T>>, IDisposable> retain) where T : GeometryBase =>
        from parent in DimensionStyles.Resolve(doc, text.Style)
        from face in text.Face.Resolve()
        from outlines in DisposalOps.Acquire(output => Typeset(doc, text, parent, face, options, create, output), retain)
        select outlines;

    private static IO<Seq<Seq<T>>> Typeset<T>(RhinoDoc doc, OutlineText text, DimensionStyle parent, Font face, OutlineOptions options,
        OutlineFactory<T> create, Action<IDisposable> output) where T : GeometryBase =>
        DisposalOps.Within((Action<IDisposable> held) =>
            from entity in Copies.Acquire(nameof(TextEntity.Create), _ => Answer.One(TextEntity.Create(text.Text, text.Plane, parent, wrapped: false, rectWidth: 0.0, rotationRadians: 0.0)), held)
            from assigned in IO.lift(() => Update((face, text.Height), entity))
            from generated in Generate(doc, entity, options, create, output)
            select generated, DisposalOps.Release);

    private static IO<Seq<Seq<T>>> Generate<T>(RhinoDoc doc, TextEntity text, OutlineOptions options,
        OutlineFactory<T> create, Action<IDisposable> retain) where T : GeometryBase =>
        from transformed in options.Transform.Traverse(xform => DisposalOps.Within((Action<IDisposable> held) =>
            from style in DimensionStyles.Effective(doc, text, None, held)
            from applied in IO.lift(() => Refused.Unless(text.Transform(xform, style), nameof(TextEntity.Transform)))
            select applied, DisposalOps.Release)).As()
        from outlines in DisposalOps.Within((Action<IDisposable> held) =>
            from style in DimensionStyles.Effective(doc, text, None, held)
            from backward in IO.lift(() => { style.DrawForward = false; })
            from created in create(text, style, options.SmallCapsScale.IsSome, options.SmallCapsScale.IfNone(1.0), options.Spacing, retain)
            select created, DisposalOps.Release)
        select outlines;

    // --- [MAPPING]
    private static partial void Update((Font Font, double TextHeight) source, TextEntity target);
}
