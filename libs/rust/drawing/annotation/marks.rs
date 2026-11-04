use core::{f64::consts::PI, iter};

use itertools::{EitherOrBoth, Itertools as _};
use kurbo::{Affine, Arc, BezPath, Circle, Line, ParamCurve as _, ParamCurveArclen as _, ParamCurveDeriv as _, PathEl, PathSeg, Point, Rect, Shape as _, Size, Vec2};
use linesweeper::FillRule;
use nonempty_collections::NEVec;
use rasm_linework::{Anchor, Baseline, Linetype, Path, Pen, Placement, Width};
use uom::si::{
    f64::{Angle, Length},
    length::millimeter,
};

use crate::{
    AnnotationError, Geometry, Graphics, Head, bounds, directed, edges, filled, graduated, indexed, joined, located, measured,
    notation::{self, Sign},
    oriented, polyline, station, stroked, terminated,
    text::{Label, Typesetter, labelled},
    tipped, transformed, units,
};

// --- [MODELS] --------------------------------------------------------------------------

#[nutype::nutype(validate(finite, greater = 0.0), derive(Debug, Clone, Copy, PartialEq, PartialOrd))]
pub struct Chord(f64);

#[derive(Debug, Clone, PartialEq)]
pub enum Breakline {
    Zigzag { stations: NEVec<f64> },
    Wavy,
}

#[derive(Debug, Clone, PartialEq)]
pub enum Mark {
    Cloud { boundary: Path, arc: Chord },
    Break { line: Line, size: Width, form: Breakline },
    Arrow { path: Path, start: Option<Head> = None, label: Option<Label> = None },
    Bar { at: Point, angle: Angle, divisions: NEVec<Length>, subdivisions: Vec<Length> = Vec::new(), height: Width, linear: Option<notation::Linear> = None },
    Center { circle: Circle, size: Width, overshoot: Option<Width> = None, linetype: Option<Linetype> = None },
    Axis { line: Line, overshoot: Width, linetype: Option<Linetype> = None, bubbles: Option<EitherOrBoth<Label, Label>> = None },
    Cut { path: Path, pen: Pen, end: Width, linetype: Option<Linetype> = None, labels: Option<EitherOrBoth<Label, Label>> = None },
    Insulation { path: Path, thickness: Length },
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub(crate) fn draw(typesetter: &mut Typesetter, style: &crate::Style, placement: Placement, mark: &Mark) -> Result<Graphics, NEVec<AnnotationError>> {
    let accuracy = style.tolerance.into_inner();
    let mut beyond = |label: &Label, tip: Point, outward: Vec2| -> Result<Graphics, NEVec<AnnotationError>> {
        let set = labelled(typesetter, style, label, Point::ORIGIN, Vec2::new(1.0, 0.0), Anchor::Middle, Baseline::Middle)?;
        let middle = bounds(&set, Affine::IDENTITY).center();
        let reach = bounds(&set, oriented(middle, outward).inverse()).x1;
        transformed(set, Affine::translate(tip + outward * reach - middle))
    };
    match mark {
        Mark::Cloud { boundary, arc } => {
            let scalloped = |elements: &[PathEl]| -> Result<Vec<PathEl>, AnnotationError> {
                let ring = elements.iter().copied().chain((!matches!(elements.last(), Some(PathEl::ClosePath))).then_some(PathEl::ClosePath)).collect::<BezPath>();
                let measured = measured(ring.segments(), accuracy);
                let perimeter = measured.last().map_or_default(|&(_, _, to)| to);
                (perimeter > 0.0).ok_or(AnnotationError::Geometry(Geometry::Coincident))?;
                let count = (perimeter / arc.into_inner()).round().max(2.0).to_int_saturating::<u32>();
                let step = perimeter / f64::from(count);
                let stations = (0..count)
                    .map(|index| station(&measured, f64::from(index) * step, accuracy).map(|(segment, parameter)| segment.eval(parameter)))
                    .collect::<Option<Vec<_>>>()
                    .and_then(NEVec::try_from_vec)
                    .ok_or(AnnotationError::Geometry(Geometry::Coincident))?;
                let sweep = PI.copysign(ring.area());
                Ok(iter::once(PathEl::MoveTo(*stations.first()))
                    .chain(stations.iter().copied().circular_tuple_windows::<(_, _)>().flat_map(|(start, end)| {
                        let middle = start.midpoint(end);
                        Arc::new(middle, Vec2::splat(start.distance(end) * 0.5), (start - middle).atan2(), sweep, 0.0).append_iter(accuracy)
                    }))
                    .chain(iter::once(PathEl::ClosePath))
                    .collect())
            };
            indexed::<_, Vec<_>, _>((*placement * &**boundary).subpaths().map(|elements| scalloped(elements).map_err(NEVec::from)))
                .and_then(|rings| stroked(rings.into_iter().flatten().collect(), &style.pen, None).map(|stroke| Graphics { elements: vec![stroke], .. }).map_err(NEVec::from))
        }
        Mark::Break { line, size, form } => breakline(style, placement, *line, *size, form),
        Mark::Arrow { path, start, label } => {
            let path = *placement * &**path;
            let tip = |head: Head, outline: &BezPath| {
                indexed(outline.subpaths().map(|elements| tipped(&style.arrow, head, kurbo::segments(elements.iter().copied()), style.tolerance).map_err(NEVec::from)))
                    .map(|elements| Graphics { elements, .. })
            };
            let text = label.as_ref().map(|label| {
                let segment = path.segments().next().ok_or(AnnotationError::Geometry(Geometry::Coincident))?;
                let direction = (segment.end() - segment.start()).normalize();
                labelled(typesetter, style, label, segment.eval(0.5) - direction.turn_90() * style.gap.into_inner(), direction, Anchor::Middle, Baseline::Bottom)
            });
            joined([stroked(path.clone(), &style.pen, None)], iter::once(tip(style.arrow.head, &path.reverse_subpaths())).chain(start.map(|head| tip(head, &path))).chain(text))
        }
        Mark::Bar { at, angle, divisions, subdivisions, height, linear } => {
            let (origin, direction) = located(placement, *at, *angle);
            let linear = linear.as_ref().unwrap_or(&style.linear);
            let scale = direction.length();
            let thickness = height.into_inner();
            let lengths = indexed::<_, Vec<_>, _>(
                iter::chain(subdivisions, divisions.iter())
                    .map(|division| (division.is_finite() && division.value > 0.0).then_some(units(*division)).ok_or_else(|| AnnotationError::Geometry(Geometry::Extent).into())),
            )?;
            let left = subdivisions.iter().map(|division| units(*division)).sum::<f64>();
            let positions = edges(lengths).collect::<Vec<_>>();
            let basis = oriented(origin, direction) * Affine::FLIP_Y * Affine::translate((-left * scale, 0.0));
            let solid = positions.array_windows().step_by(2).flat_map(|&[from, to]| Rect::new(from * scale, 0.0, to * scale, thickness).path_elements(accuracy)).collect::<BezPath>();
            let ticks = positions.iter().enumerate().map(|(index, &position)| {
                (position * scale, (index == 0 || index >= subdivisions.len()).then(|| notation::linear(linear, Length::new::<millimeter>((position - left).abs()), Sign::Negative)))
            });
            joined(
                [filled(basis * solid, FillRule::NonZero, style.pen.color.clone(), style.pen.opacity, style.tolerance)],
                [graduated(
                    typesetter,
                    style,
                    basis,
                    Size::new(positions.last().map_or_default(|&position| position * scale), thickness),
                    ticks,
                    (direction.normalize(), Anchor::Middle, Baseline::Bottom),
                )],
            )
        }
        Mark::Center { circle, size, overshoot, linetype } => {
            let center = *placement * circle.center;
            let axes = [Vec2::new(1.0, 0.0), Vec2::new(0.0, 1.0)].map(|axis| directed(*placement, axis).normalize());
            let crossed = |reach: f64| axes.iter().flat_map(|&axis| Line::new(center - axis * reach, center + axis * reach).path_elements(accuracy)).collect::<BezPath>();
            let radius = circle.radius * placement.determinant().abs().sqrt();
            joined(iter::once(stroked(crossed(size.into_inner()), &style.pen, None)).chain(overshoot.map(|run| stroked(crossed(radius + run.into_inner()), &style.pen, linetype.clone()))), [])
        }
        Mark::Axis { line, overshoot, linetype, bubbles } => {
            let [start, end] = [line.p0, line.p1].map(|point| *placement * point);
            let direction = (end - start).normalize();
            direction.is_finite().ok_or(AnnotationError::Geometry(Geometry::Coincident))?;
            let run = direction * overshoot.into_inner();
            let [first, last] = [start - run, end + run];
            let (opening, closing) = bubbles.as_ref().map_or((None, None), |ends| ends.as_ref().left_and_right());
            joined(
                [stroked(Line::new(first, last).to_path(accuracy), &style.pen, linetype.clone())],
                [(opening, first, -direction), (closing, last, direction)].into_iter().filter_map(|(label, tip, outward)| label.map(|label| beyond(label, tip, outward))),
            )
        }
        Mark::Cut { path, pen, end, linetype, labels } => {
            let path = *placement * &**path;
            let reach = end.into_inner();
            let measured = measured(path.segments(), accuracy);
            let (Some(&(first, _, _)), Some(&(last, _, length))) = (measured.first(), measured.last()) else { return Err(AnnotationError::Geometry(Geometry::Coincident).into()) };
            (length > reach * 2.0).ok_or(AnnotationError::Geometry(Geometry::Gap))?;
            let corners = measured
                .array_windows()
                .filter(|&&[(before, _, _), (after, _, _)]| (tangent(before, 1.0) - tangent(after, 0.0)).length() * reach > accuracy)
                .map(|&[(_, _, joint), _]| (joint - reach, joint + reach));
            let thick = BezPath::from_path_segments(iter::once((0.0, reach)).chain(corners).chain(iter::once((length - reach, length))).flat_map(|(from, to)| {
                measured.iter().filter(move |&&(_, start, stop)| start < to && stop > from).map(move |&(segment, start, stop)| {
                    let [low, high] = [from, to].map(|at| segment.inv_arclen((at - start).clamp(0.0, stop - start), accuracy));
                    segment.subsegment(low..high)
                })
            }));
            let (opening, closing) = labels.as_ref().map_or((None, None), |ends| ends.as_ref().left_and_right());
            let views = [(first.start(), tangent(first, 0.0).turn_90(), opening), (last.end(), tangent(last, 1.0).turn_90(), closing)];
            let arrows = views.iter().flat_map(|&(base, view, _)| {
                [stroked(Line::new(base, base + view * reach).to_path(accuracy), &style.pen, None), terminated(&style.arrow, style.arrow.head, base + view * reach, view, style.tolerance)]
            });
            joined(
                iter::chain([stroked(path, &style.pen, linetype.clone()), stroked(thick, pen, None)], arrows),
                views.into_iter().filter_map(|(base, view, label)| label.map(|label| beyond(label, base + view * reach, view))),
            )
        }
        Mark::Insulation { path, thickness } => insulation(style, &(*placement * &**path), units(*thickness) * placement.determinant().abs().sqrt() * 0.5),
    }
}

fn breakline(style: &crate::Style, placement: Placement, line: Line, size: Width, form: &Breakline) -> Result<Graphics, NEVec<AnnotationError>> {
    let accuracy = style.tolerance.into_inner();
    let [start, end] = [line.p0, line.p1].map(|point| *placement * point);
    let length = start.distance(end);
    (length > 0.0).ok_or(AnnotationError::Geometry(Geometry::Coincident))?;
    let size = size.into_inner();
    let reach = size * 0.5;
    let broken = match form {
        Breakline::Zigzag { stations } => polyline(
            iter::once(Point::ORIGIN)
                .chain(
                    indexed::<_, Vec<_>, _>(
                        stations.iter().map(|&station| (0.0..=1.0).contains(&station).then_some(station * length).ok_or_else(|| AnnotationError::Geometry(Geometry::Station).into())),
                    )?
                    .into_iter()
                    .sorted_by(f64::total_cmp)
                    .flat_map(|center| [(center - reach, 0.0), (reach.mul_add(-0.5, center), -reach), (reach.mul_add(0.5, center), reach), (center + reach, 0.0)].map(Point::from)),
                )
                .chain(iter::once(Point::new(length, 0.0))),
        ),
        Breakline::Wavy => {
            let count = if size >= accuracy { (length / size).round().max(1.0).to_int_saturating::<u32>() } else { 1 };
            let span = length / f64::from(count);
            iter::once(PathEl::MoveTo(Point::ORIGIN))
                .chain((0..count).map(|index| {
                    let from = f64::from(index) * span;
                    PathEl::QuadTo(Point::new(0.5_f64.mul_add(span, from), if index.is_multiple_of(2) { -size } else { size }), Point::new(from + span, 0.0))
                }))
                .collect::<BezPath>()
        }
    };
    stroked(oriented(start, end - start) * broken, &style.pen, None).map(|stroke| Graphics { elements: vec![stroke], .. }).map_err(NEVec::from)
}

fn insulation(style: &crate::Style, path: &BezPath, half: f64) -> Result<Graphics, NEVec<AnnotationError>> {
    let accuracy = style.tolerance.into_inner();
    (half.is_finite() && half >= accuracy).ok_or(AnnotationError::Geometry(Geometry::Extent))?;
    let measured = measured(path.segments(), accuracy);
    let length = measured.last().map_or_default(|&(_, _, to)| to);
    let count = (length / half).floor().to_int_saturating::<u32>();
    (count > 0).ok_or(AnnotationError::Geometry(Geometry::Extent))?;
    let loops = (0..=count)
        .map(|index| {
            station(&measured, (f64::from(index) * half).min(length), accuracy).map(|(segment, parameter)| {
                let along = tangent(segment, parameter);
                (segment.eval(parameter) + along.turn_90() * if index.is_multiple_of(2) { -half } else { half }, along)
            })
        })
        .collect::<Option<Vec<_>>>()
        .ok_or(AnnotationError::Geometry(Geometry::Coincident))?;
    let reach = half * 0.75;
    let batt = iter::chain(
        loops.first().map(|&(point, _)| PathEl::MoveTo(point)),
        loops.array_windows().map(|&[(from, leaving), (to, arriving)]| PathEl::CurveTo(from - leaving * reach, to + arriving * reach, to)),
    )
    .collect::<BezPath>();
    stroked(batt, &style.pen, None).map(|stroke| Graphics { elements: vec![stroke], .. }).map_err(NEVec::from)
}

fn tangent(segment: PathSeg, parameter: f64) -> Vec2 { segment.to_cubic().deriv().eval(parameter).to_vec2().normalize() }
