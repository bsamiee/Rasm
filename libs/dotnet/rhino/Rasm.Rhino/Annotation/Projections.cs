using Rasm.Drafting;
using Rasm.Rhino.Annotation.Styles;
using Rasm.Rhino.Document.Tables;
using Rhino;
using Rhino.DocObjects;

namespace Rasm.Rhino.Annotation;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Projections {
    // --- [DRAWING]
    public static IO<Seq<Guid>> Draw(RhinoDoc doc, HiddenLineDrawing drawing, LineGroup lines, Transform placement, ObjectAttributes attributes, Seq<ClippingPlaneObject> clippers) =>
        from styles in toSeq(LineRole.Items).TraverseM(role => Styled(doc, lines, role).Map(style => (Role: role, Style: style))).As().Map(static held => toHashMap(held))
        let tolerance = attributes.Space is ActiveSpace.PageSpace ? doc.PageAbsoluteTolerance : doc.ModelAbsoluteTolerance
        from segments in IO.lift(() => toSeq(drawing.Segments).Strict())
        from strokes in segments.Map(static segment => Role(segment).Map(role => (Segment: segment, Role: role))).Somes().TraverseM(stroke =>
            (from curve in Placed(stroke.Segment.CurveGeometry, placement)
             from id in Added(doc, curve, attributes, styles.Find(stroke.Role))
             select id).Bracket()).As()
        from hatches in toSeq(segments.Map(static segment => segment switch {
                    {
                        ParentCurve: { SilhouetteType: SilhouetteType.SectionCut, SourceObject.Tag: RhinoObject source } parent,
                        SegmentVisibility: HiddenLineDrawingSegment.Visibility.Visible or HiddenLineDrawingSegment.Visibility.Duplicate,
                    } => Some((Source: source, Plane: parent.ClippingPlaneIndex, Curve: segment.CurveGeometry)),
            _ => Option<(RhinoObject Source, int Plane, Curve Curve)>.None,
        }).Somes().GroupBy(static cut => (cut.Source, cut.Plane)))
            .TraverseM(cut => Sectioned(doc, cut.Key.Source, clippers.At(cut.Key.Plane), toSeq(cut).Map(static held => held.Curve), placement, attributes, tolerance)).As()
        select strokes + hatches.Flatten();

    private static Option<LineRole> Role(HiddenLineDrawingSegment segment) =>
        (segment.SegmentVisibility, segment.ParentCurve?.SilhouetteType) switch {
            (HiddenLineDrawingSegment.Visibility.Visible, SilhouetteType.SectionCut) => LineRole.Cut,
            (HiddenLineDrawingSegment.Visibility.Visible, SilhouetteType.Tangent or SilhouetteType.TangentProjects or SilhouetteType.NonSilhouetteTangent) => LineRole.Tangent,
            (HiddenLineDrawingSegment.Visibility.Visible, _) => LineRole.Outline,
            (HiddenLineDrawingSegment.Visibility.Hidden, _) => LineRole.Hidden,
            _ => None,
        };

    private static IO<(int Linetype, double Width)> Styled(RhinoDoc doc, LineGroup lines, LineRole role) =>
        lines.Width(role.Weight) switch {
            var width => (role.Type == LineType.Continuous
                    ? IO.pure(-1)
                    : Rows.Find(doc.Linetypes, new ComponentRef<Linetype>.ByName(Linetypes.Name(role.Type, width))).Map(static row => row.Index))
                .Map(index => (Linetype: index, Width: width.Width.Millimeters.ToDouble())),
        };

    private static IO<Curve> Placed(Curve source, Transform placement) =>
        from copy in Products.Of(source.DuplicateCurve)
        from moved in Products.Accepted(() => copy.Transform(placement))
        select copy;

    private static IO<Guid> Added(RhinoDoc doc, GeometryBase geometry, ObjectAttributes attributes, Option<(int Linetype, double Width)> style) =>
        (from held in use(attributes.Duplicate)
         from styled in IO.lift(() => style.Iter(stroke =>
             (held.LinetypeSource, held.LinetypeIndex, held.PlotWeightSource, held.PlotWeight) =
                 (ObjectLinetypeSource.LinetypeFromObject, stroke.Linetype, ObjectPlotWeightSource.PlotWeightFromObject, stroke.Width)))
         from id in IO.lift(() => Conversions.Required(Conversions.Present(doc.Objects.Add(geometry, held))))
         select id).Bracket();

    // --- [SECTIONS]
    private static IO<Seq<Guid>> Sectioned(
        RhinoDoc doc, RhinoObject source, Option<ClippingPlaneObject> clipper, Seq<Curve> cuts, Transform placement, ObjectAttributes attributes, double tolerance) =>
        (from style in clipper.Traverse(held => Products.Of(() => source.Attributes.ComputedSectionStyle(doc, held.Attributes, computeColors: true, Guid.Empty))).As()
         let fill = from held in style
                    from index in Conversions.Present(held.HatchIndex)
                    select (Index: index, Rotation: held.HatchRotationRadians, held.HatchScale)
         from added in fill.Traverse(hatching =>
             from placed in cuts.TraverseM(cut => Placed(cut, placement)).As()
             from loops in Products.Many(() => Curve.JoinCurves(placed, tolerance))
             from hatches in Products.Many(() => Hatch.Create(loops.Filter(static loop => loop.IsClosed), hatching.Index, hatching.Rotation, hatching.HatchScale, tolerance), required: false)
             from ids in hatches.TraverseM(hatch => Added(doc, hatch, attributes, None)).As()
             select ids).As()
         select added.ToSeq().Flatten()).Bracket();
}
