use core::{f64::consts::PI, iter};

use itertools::Itertools as _;
use kurbo::{Affine, Arc, BezPath, Circle, Line, ParamCurve as _, ParamCurveArclen as _, ParamCurveDeriv as _, PathEl, PathSeg, Point, Rect, Shape as _, Size, Vec2};
use linesweeper::FillRule;
use nonempty_collections::NEVec;
use rasm_linework::{Anchor, Baseline, Element, Linetype, Path, Placement, Width};
use uom::si::f64::{Angle, Length};
use validated::Validated;

use crate::{
    AnnotationError, Geometry, Graphics, Head, Terminator, directed, edges, filled, graduated, indexed, located, model,
    notation::{self, Sign},
    oriented, polyline, stroked,
    text::{Label, Typesetter, labelled},
    tipped, units, validated,
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
    Axis { line: Line, overshoot: Width, linetype: Option<Linetype> = None },
    Insulation { path: Path, thickness: Length },
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub(crate) fn draw(typesetter: &mut Typesetter, style: &crate::Style, placement: Placement, mark: &Mark) -> Result<Graphics, NEVec<AnnotationError>> {
    let accuracy = style.tolerance.into_inner();
    let lines = |elements: Vec<Element>| Graphics { elements, masks: Vec::new() };
    let coincident = || AnnotationError::Geometry(Geometry::Coincident);
    match mark {
        Mark::Cloud { boundary, arc } => indexed((*placement * &**boundary).subpaths().map(|elements| scalloped(elements, arc.into_inner(), accuracy).map_err(NEVec::from)))
            .and_then(|rings| stroked(rings.into_iter().flatten().collect(), &style.pen, None).map(|stroke| lines(vec![stroke])).map_err(NEVec::from)),
        Mark::Break { line, size, form } => {
            let [start, end] = [line.p0, line.p1].map(|point| *placement * point);
            let length = start.distance(end);
            (length > 0.0).ok_or_else(coincident)?;
            let size = size.into_inner();
            let course = match form {
                Breakline::Zigzag { stations } => {
                    let reach = size * 0.5;
                    let centers = indexed(stations.iter().map(|&station| (0.0..=1.0).contains(&station).then_some(station * length).ok_or_else(|| AnnotationError::Geometry(Geometry::Station).into())))?;
                    polyline(
                        iter::once(Point::ORIGIN)
                            .chain(centers.into_iter().sorted_by(f64::total_cmp).flat_map(|center| [(center - reach, 0.0), (center - reach * 0.5, -reach), (center + reach * 0.5, reach), (center + reach, 0.0)].map(Point::from)))
                            .chain(iter::once(Point::new(length, 0.0))),
                    )
                }
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
            stroked(oriented(start, end - start) * course, &style.pen, None).map(|stroke| lines(vec![stroke])).map_err(NEVec::from)
        }
        Mark::Arrow { path, start, label } => {
            let paper = *placement * &**path;
            let tip = |terminator: &Terminator, outline: &BezPath| indexed(outline.subpaths().map(|elements| tipped(terminator, &kurbo::segments(elements.iter().copied()).collect::<Vec<PathSeg>>(), style.tolerance).map_err(NEVec::from)));
            let starts = start.map(|head| tip(&Terminator { head, ..style.arrow.clone() }, &paper)).transpose().map(Option::unwrap_or_default);
            let text = label
                .as_ref()
                .map(|label| {
                    let segment = paper.segments().next().ok_or_else(coincident)?;
                    let direction = (segment.end() - segment.start()).normalize();
                    labelled(typesetter, style, label, segment.eval(0.5) - direction.turn_90() * style.gap.into_inner(), direction, Anchor::Middle, Baseline::Bottom)
                })
                .transpose();
            Validated::from(stroked(paper.clone(), &style.pen, None))
                .map4(validated(tip(&style.arrow, &paper.reverse_subpaths())), validated(starts), validated(text), |stroke, ends, beginnings, lettered| {
                    iter::once(lines(iter::once(stroke).chain(ends).chain(beginnings).collect())).chain(lettered).collect()
                })
                .ok()
        }
        Mark::Bar { at, angle, divisions, subdivisions, height, linear } => {
            let linear = linear.as_ref().unwrap_or(&style.linear);
            let (origin, direction) = located(placement, *at, *angle);
            let scale = direction.length();
            let thickness = height.into_inner();
            let lengths = indexed(iter::chain(subdivisions, divisions.iter()).map(|division| (division.is_finite() && division.value > 0.0).then_some(units(*division)).ok_or_else(|| AnnotationError::Geometry(Geometry::Division).into())))?;
            let left = subdivisions.iter().map(|division| units(*division)).sum::<f64>();
            let positions = edges(lengths).collect::<Vec<_>>();
            let count = subdivisions.len();
            let basis = oriented(origin, direction) * Affine::FLIP_Y * Affine::translate((-left * scale, 0.0));
            let solid = positions.array_windows().step_by(2).flat_map(|&[from, to]| Rect::new(from * scale, 0.0, to * scale, thickness).path_elements(accuracy)).collect::<BezPath>();
            let ticks = positions.iter().enumerate().map(|(index, &position)| (position * scale, (index == 0 || index >= count).then(|| notation::linear(linear, model((position - left).abs()), Sign::Negative)))).collect::<Vec<_>>();
            Validated::from(filled(&(basis * solid), FillRule::NonZero, style.pen.color.clone(), style.pen.opacity, style.tolerance))
                .map2(
                    validated(graduated(typesetter, style, basis, Size::new(positions.last().map_or_default(|&position| position * scale), thickness), ticks, (direction.normalize(), Anchor::Middle, Baseline::Bottom))),
                    |fill, scale_bar| [lines(vec![fill]), scale_bar].into_iter().collect(),
                )
                .ok()
        }
        Mark::Center { circle, size, overshoot, linetype } => {
            let center = *placement * circle.center;
            let axes = [Vec2::new(1.0, 0.0), Vec2::new(0.0, 1.0)].map(|axis| directed(*placement, axis).normalize());
            let crossed = |reach: f64| axes.iter().flat_map(|&axis| Line::new(center - axis * reach, center + axis * reach).path_elements(accuracy)).collect::<BezPath>();
            let radius = circle.radius * placement.determinant().abs().sqrt();
            Validated::from(stroked(crossed(size.into_inner()), &style.pen, None))
                .map2(Validated::from(overshoot.map(|run| stroked(crossed(radius + run.into_inner()), &style.pen, linetype.clone())).transpose()), |mark, centerlines| lines(iter::once(mark).chain(centerlines).collect()))
                .ok()
        }
        Mark::Axis { line, overshoot, linetype } => {
            let [start, end] = [line.p0, line.p1].map(|point| *placement * point);
            let direction = (end - start).normalize();
            direction.is_finite().ok_or_else(coincident)?;
            let run = direction * overshoot.into_inner();
            stroked(Line::new(start - run, end + run).to_path(accuracy), &style.pen, linetype.clone()).map(|stroke| lines(vec![stroke])).map_err(NEVec::from)
        }
        Mark::Insulation { path, thickness } => {
            let paper = *placement * &**path;
            let half = units(*thickness) * placement.determinant().abs().sqrt() * 0.5;
            (half.is_finite() && half >= accuracy).ok_or(AnnotationError::Geometry(Geometry::Extent))?;
            let measured = paper.segments().map(|segment| (segment, segment.arclen(accuracy))).collect::<Vec<_>>();
            let offsets = edges(measured.iter().map(|&(_, length)| length)).collect::<Vec<_>>();
            let count = (offsets.last().copied().unwrap_or_default() / half).floor().to_int_saturating::<u32>();
            (count > 0).ok_or(AnnotationError::Geometry(Geometry::Extent))?;
            let station = |at: f64| {
                iter::zip(&measured, offsets.array_windows()).find(|&(_, &[from, to])| at >= from && at <= to).map(|(&(segment, length), &[from, _])| {
                    let parameter = segment.inv_arclen((at - from).min(length), accuracy);
                    (segment.eval(parameter), segment.to_cubic().deriv().eval(parameter).to_vec2().normalize())
                })
            };
            let loops = (0..=count)
                .map(|index| station(f64::from(index) * half).map(|(point, tangent)| (point + tangent.turn_90() * if index.is_multiple_of(2) { -half } else { half }, tangent)))
                .collect::<Option<Vec<_>>>()
                .ok_or_else(coincident)?;
            let reach = half * 0.75;
            let batt = loops
                .first()
                .map(|&(point, _)| PathEl::MoveTo(point))
                .into_iter()
                .chain(loops.array_windows().map(|&[(from, leaving), (to, arriving)]| PathEl::CurveTo(from - leaving * reach, to + arriving * reach, to)))
                .collect::<BezPath>();
            stroked(batt, &style.pen, None).map(|stroke| lines(vec![stroke])).map_err(NEVec::from)
        }
    }
}

fn scalloped(elements: &[PathEl], chord: f64, accuracy: f64) -> Result<Vec<PathEl>, AnnotationError> {
    let ring = elements.iter().copied().chain((!matches!(elements.last(), Some(PathEl::ClosePath))).then_some(PathEl::ClosePath)).collect::<BezPath>();
    let measured = ring.segments().map(|segment| (segment, segment.arclen(accuracy))).collect::<Vec<_>>();
    let offsets = edges(measured.iter().map(|&(_, length)| length)).collect::<Vec<_>>();
    let perimeter = offsets.last().copied().unwrap_or_default();
    let count = (perimeter / chord).round().max(2.0);
    let step = perimeter / count;
    let stations = NEVec::try_from_vec(
        iter::zip(&measured, offsets.array_windows())
            .flat_map(|(&(segment, _), &[from, to])| {
                let last = (to / step).ceil().min(count);
                iter::successors(Some((from / step).ceil()), |index| Some(index + 1.0))
                    .take_while(move |index| *index < last)
                    .map(move |index| segment.eval(segment.inv_arclen(index.mul_add(step, -from), accuracy)))
            })
            .collect::<Vec<_>>(),
    )
    .ok_or(AnnotationError::Geometry(Geometry::Coincident))?;
    let sweep = PI.copysign(ring.area());
    Ok(iter::once(PathEl::MoveTo(*stations.first()))
        .chain(stations.iter().copied().circular_tuple_windows::<(_, _)>().flat_map(|(start, end)| {
            let center = start.midpoint(end);
            Arc::new(center, Vec2::splat(start.distance(end) * 0.5), (start - center).atan2(), sweep, 0.0).append_iter(accuracy)
        }))
        .chain(iter::once(PathEl::ClosePath))
        .collect())
}
