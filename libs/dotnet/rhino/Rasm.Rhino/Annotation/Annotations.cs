using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Rasm.Rhino.Annotation.Styles;
using Rasm.Rhino.Document.Tables;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;

namespace Rasm.Rhino.Annotation;

// --- [TYPES] ---------------------------------------------------------------------------
public delegate IO<Seq<Seq<T>>> OutlineFactory<T>(TextEntity text, DimensionStyle style, bool makeSmallCaps, double smallCapsScale, double spacing)
    where T : GeometryBase;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record OutlineOptions(Option<double> SmallCapsScale, double Spacing, Option<Transform> Transform);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Annotations {
    // --- [CREATION]
    public static IO<T> Create<T>(
        RhinoDoc doc, Option<ComponentRef<DimensionStyle>> style, Seq<Step<DimensionStyle>> overrides, Func<DimensionStyle, T?> create, [CallerArgumentExpression(nameof(create))] string member = "")
        where T : AnnotationBase =>
        from parent in DimensionStyles.Resolve(doc, style)
        from annotation in Products.Of(() => create(parent), member)
        from overridden in unless(overrides.IsEmpty, DimensionStyles.Override(doc, annotation, overrides)).As()
        select annotation;

    public static IO<Guid> Add(RhinoDoc doc, AnnotationBase annotation, Option<Guid> page, Option<ObjectAttributes> attributes = default) =>
        (from held in use(() => attributes.Match(Some: static given => given.Duplicate(), None: doc.CreateDefaultAttributes))
         from placed in IO.lift(() => { (held.Space, held.ViewportId) = (page.IsSome ? ActiveSpace.PageSpace : ActiveSpace.ModelSpace, page.IfNone(Guid.Empty)); })
         from layered in IO.lift(() => DimensionLayers.Of(doc).Iter(index => held.LayerIndex = index))
         from id in IO.lift(() => Conversions.Required(Conversions.Present(doc.Objects.Add(annotation, held))))
         select id).Bracket();

    public static IO<Guid> Measured(
        RhinoDoc doc, DetailViewObject detail, Line span, Point3d through, Option<ComponentRef<DimensionStyle>> style, Seq<Step<DimensionStyle>> overrides) =>
        (from page in IO.lift(() => (Xform: detail.WorldToPageTransform, Scale: RhinoMath.UnitScale(doc.PageUnitSystem, doc.ModelUnitSystem) / detail.DetailGeometry.PageToModelRatio))
         let points = (Start: page.Xform * span.From, End: page.Xform * span.To, Line: page.Xform * through)
         from dimension in Create(doc, style, overrides, parent => LinearDimension.Create(
             AnnotationType.Rotated, parent, Plane.WorldXY, Vector3d.XAxis, points.Start, points.End, points.Line, Math.Atan2(points.End.Y - points.Start.Y, points.End.X - points.Start.X)))
         from measured in IO.lift(() => { (dimension.DetailMeasured, dimension.DistanceScale) = (detail.Id, page.Scale); })
         from id in Add(doc, dimension, Some(detail.Attributes.ViewportId))
         select id).Bracket();

    // --- [EDITS]
    public static IO<Unit> Edit<T>(RhinoDoc doc, Guid id, Seq<Step<T>> steps) where T : AnnotationBase =>
        (from resolved in ObjectTarget.Resolve<AnnotationObjectBase, T>(doc, id)
         from edited in Edits.Apply(Staged<T>(doc, id), resolved.Geometry, steps)
         select unit).Bracket();

    public static IO<Unit> SaveOverrides(RhinoDoc doc, Guid id) =>
        Edit(doc, id, [Rows.Write<AnnotationBase>(copy => DimensionStyles.Saved(doc, copy)).Write]);

    private static Stage<T> Staged<T>(RhinoDoc doc, Guid id) where T : AnnotationBase =>
        new(
            Copy: live =>
                from copy in Products.Of(() => live.Duplicate() as T)
                from parent in DimensionStyles.Parent(doc, live)
                from bound in IO.lift(() => { copy.ParentDimensionStyle = parent; })
                select copy,
            Unchanged: static (_, _) => false,
            Commit: (_, staged) =>
                from wrapped in IO.lift(() => { if (staged.TextIsWrapped) staged.WrapText(); })
                from replaced in Products.Accepted(() => doc.Objects.Replace(id, staged, ignoreModes: false))
                select staged);

    // --- [TEXT]
    public static IO<string> Text(RhinoDoc doc, DimensionObject owner) =>
        (from style in DimensionStyles.Effective(doc, owner.AnnotationGeometry, None)
         let units = owner.Attributes.Space is ActiveSpace.PageSpace ? doc.PageUnits : doc.ModelUnits
         from text in IO.lift(Fin<string> () => owner.AnnotationGeometry switch {
             LinearDimension linear => linear.GetDistanceDisplayText(units, style),
             AngularDimension angular => angular.GetAngleDisplayText(style),
             RadialDimension radial => radial.GetDistanceDisplayText(units, style),
             OrdinateDimension ordinate => ordinate.GetDistanceDisplayText(units, style),
             var other => new RhinoFault(other.AnnotationType.ToString(), RhinoReason.Unsupported),
         })
         select text).Bracket();

    public static IO<Transform> TextTransform(AnnotationBase annotation, DimensionStyle style, double scale, Option<RhinoViewport> viewport) =>
        (from info in viewport.Traverse(static view => use(() => new ViewportInfo(view))).As()
         from xform in IO.lift(Fin<Transform> () => (annotation, info.Case) switch {
             (Dimension dimension, ViewportInfo view) => dimension.GetTextTransform(view, style, scale, style.DrawForward),
             (TextEntity text, ViewportInfo view) => text.GetTextTransform(view, scale, style),
             (TextEntity text, _) => text.GetTextTransform(scale, style),
             var (other, _) => new RhinoFault(other.AnnotationType.ToString(), RhinoReason.Unsupported),
         })
         select xform).Bracket();

    public static IO<Seq<Seq<T>>> Outlines<T>(RhinoDoc doc, TextEntity text, OutlineOptions options, OutlineFactory<T> create) where T : GeometryBase =>
        from copy in Products.Of(() => text.Duplicate() as TextEntity)
        from transformed in options.Transform.Traverse(xform =>
            from placing in DimensionStyles.Effective(doc, copy, None)
            from moved in Products.Accepted(() => copy.Transform(xform, placing))
            from released in release(placing)
            select released).As()
        from style in DimensionStyles.Effective(doc, copy, None)
        from backward in IO.lift(() => { style.DrawForward = false; })
        from created in create(copy, style, options.SmallCapsScale.IsSome, options.SmallCapsScale.IfNone(1d), options.Spacing)
        from releasedStyle in release(style)
        from releasedCopy in release(copy)
        select created;

    extension(AnnotationBase annotation) {
        public Option<double> Wrap => Conversions.Present(annotation.TextIsWrapped, annotation.FormatWidth);

        public (string PlainText, Seq<(int Run, int Start, int Length)> Runs) Runs {
            get {
                int[] map = [];
                string plainText = annotation.GetPlainTextWithRunMap(ref map);
                return (plainText, toSeq(map.Chunk(size: 3)).Map(static run => (Run: run[0], Start: run[1], Length: run[2])).Strict());
            }
        }
    }

    extension(Leader leader) {
        public Option<double> Landing => Conversions.Present(leader.LeaderHasLanding, leader.LeaderLandingLength);
    }

    // --- [DISPLAY]
    public static IO<(Seq<Line> Lines, Seq<Arc> Arcs, Seq<Point3d> Text)> Display(Dimension dimension, DimensionStyle style, double scale) =>
        IO.lift(Fin<(Seq<Line> Lines, Seq<Arc> Arcs, Seq<Point3d> Text)> () => dimension switch {
            LinearDimension linear => Shown(linear.GetTextRectangle(out Point3d[] corners), corners, linear.GetDisplayLines(style, scale, out IEnumerable<Line> lines), lines, []),
            RadialDimension radial => Shown(radial.GetTextRectangle(out Point3d[] corners), corners, radial.GetDisplayLines(style, scale, out IEnumerable<Line> lines), lines, []),
            OrdinateDimension ordinate => Shown(ordinate.GetTextRectangle(out Point3d[] corners), corners, ordinate.GetDisplayLines(style, scale, out IEnumerable<Line> lines), lines, []),
            AngularDimension angular => Shown(angular.GetTextRectangle(out Point3d[] corners), corners, angular.GetDisplayLines(style, scale, out Line[] lines, out Arc[] arcs), lines, arcs),
            var other => new RhinoFault(other.AnnotationType.ToString(), RhinoReason.Unsupported),
        });

    private static Fin<(Seq<Line> Lines, Seq<Arc> Arcs, Seq<Point3d> Text)> Shown(bool texted, Point3d[] corners, bool lined, IEnumerable<Line> lines, IEnumerable<Arc> arcs) =>
        RhinoFault.Unless(texted && lined, RhinoReason.Declined, (texted, lined))
            .Map(_ => (toSeq(lines).Filter(static line => line.IsValid).Strict(), toSeq(arcs).Filter(static arc => arc.IsValid).Strict(), toSeq(corners)));

    public static IO<T> Display<T>(Hatch hatch, HatchPattern pattern, double scale, Func<(Seq<Curve> Bounds, Seq<Line> Lines, Option<Brep> Solid), IO<T>> body) =>
        (from display in IO.lift(() => {
            hatch.CreateDisplayGeometry(pattern, scale, out Curve[] bounds, out Line[] lines, out Brep solid);
            return (Bounds: bounds, Lines: lines, Solid: solid);
        })
         from bounds in Products.Many(() => display.Bounds)
         from solid in Products.Maybe(() => display.Solid)
         from value in body((bounds, toSeq(display.Lines), solid))
         select value).Bracket();
}
