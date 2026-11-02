use core::{f64::consts::PI, iter};

use itertools::Itertools as _;
use kurbo::{Affine, Arc, BezPath, Line, ParamCurve as _, ParamCurveArclen as _, PathEl, PathSeg, Point, Rect, Shape as _, Vec2};
use linesweeper::{BinaryOp, FillRule};
use nonempty_collections::{NEVec, nev};
use rasm_linework::{Anchor, Baseline, Element, Path, Placement, Region, Width, regions};
use uom::{
    ConstZero as _,
    si::{
        angle::radian,
        f64::{Angle, Length},
        length::millimeter,
    },
};
use validated::Validated;

use crate::{
    AnnotationError, Graphics, Style,
    leaders::tipped,
    notation, stroked,
    text::{Label, Typesetter, labelled},
};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum Breakline {
    Zigzag,
    Wavy,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq)]
pub enum Mark {
    Cloud { boundary: Path, arc: Width },
    Break { line: Line, size: Width, form: Breakline = Breakline::Zigzag },
    Arrow { path: Path },
    Bar { at: Point, angle: Angle, divisions: NEVec<Length>, height: Width, linear: Option<notation::Linear> = None },
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub(crate) fn draw(typesetter: &mut Typesetter, style: &Style, placement: Placement, mark: &Mark) -> Result<Graphics, NEVec<AnnotationError>> {
    let tolerance = style.tolerance.into_inner();
    let unmasked = |elements: Vec<Element>| Graphics { elements, mask: Region::default() };
    let item = |index: usize| move |error: AnnotationError| AnnotationError::Item { index, errors: nev![error] };
    match mark {
        Mark::Cloud { boundary, arc } => {
            let chord = Some(arc.into_inner()).filter(|chord| *chord > 0.0).ok_or_else(|| nev![AnnotationError::Degenerate])?;
            let scalloped = |elements: &[PathEl]| -> Result<Vec<PathEl>, AnnotationError> {
                let ring = elements.iter().copied().chain((!matches!(elements.last(), Some(PathEl::ClosePath))).then_some(PathEl::ClosePath)).collect::<BezPath>();
                let measured = ring.segments().map(|segment| (segment, segment.arclen(tolerance))).collect::<Vec<_>>();
                let perimeter = measured.iter().map(|&(_, length)| length).sum::<f64>();
                let count = (perimeter / chord).round().max(2.0);
                let step = perimeter / count;
                let stations = NEVec::try_from_vec(
                    measured
                        .iter()
                        .scan(0.0, |reached: &mut f64, &(segment, length)| {
                            let from = *reached;
                            *reached += length;
                            Some((segment, from, *reached))
                        })
                        .flat_map(|(segment, from, to)| {
                            let last = (to / step).ceil().min(count);
                            iter::successors(Some((from / step).ceil()), |index| Some(index + 1.0))
                                .take_while(move |index| *index < last)
                                .map(move |index| segment.eval(segment.inv_arclen(index.mul_add(step, -from), tolerance)))
                        })
                        .collect::<Vec<_>>(),
                )
                .ok_or(AnnotationError::Degenerate)?;
                let sweep = PI.copysign(ring.area());
                Ok(iter::once(PathEl::MoveTo(*stations.first()))
                    .chain(stations.iter().copied().circular_tuple_windows::<(_, _)>().flat_map(|(start, end)| {
                        let center = start.midpoint(end);
                        Arc::new(center, Vec2::splat(start.distance(end) * 0.5), (start - center).atan2(), sweep, 0.0).append_iter(tolerance)
                    }))
                    .chain(iter::once(PathEl::ClosePath))
                    .collect())
            };
            (*placement * &**boundary)
                .subpaths()
                .enumerate()
                .map(|(index, elements)| Validated::from(scalloped(elements).map_err(item(index))))
                .collect::<Validated<Vec<_>, _>>()
                .ok()
                .and_then(|rings| stroked(rings.into_iter().flatten().collect(), &style.pen, None).map(|stroke| unmasked(vec![stroke])).map_err(|error| nev![error]))
        }
        Mark::Break { line, size, form } => {
            let [start, end] = [line.p0, line.p1].map(|point| *placement * point);
            let half = start.distance(end) * 0.5;
            let frame = (half > 0.0).then(|| Affine::rotate((end - start).atan2()).then_translate(start.midpoint(end).to_vec2())).ok_or_else(|| nev![AnnotationError::Degenerate])?;
            let size = size.into_inner();
            let course = match form {
                Breakline::Zigzag => {
                    let reach = (size * 0.5).min(half);
                    let [first, rest @ ..] = [(-half, 0.0), (-reach, 0.0), (-reach * 0.5, -reach), (reach * 0.5, reach), (reach, 0.0), (half, 0.0)].map(Point::from);
                    iter::once(PathEl::MoveTo(first)).chain(rest.map(PathEl::LineTo)).collect::<BezPath>()
                }
                Breakline::Wavy => {
                    let count = if size >= tolerance { (2.0 * half / size).round().max(1.0) } else { 1.0 };
                    let span = 2.0 * half / count;
                    iter::once(PathEl::MoveTo(Point::new(-half, 0.0)))
                        .chain((0_u32..).map(|index| (f64::from(index), if index.is_multiple_of(2) { -size } else { size })).take_while(|&(index, _)| index < count).map(|(index, crest)| {
                            let from = index.mul_add(span, -half);
                            PathEl::QuadTo(Point::new(0.5_f64.mul_add(span, from), crest), Point::new(from + span, 0.0))
                        }))
                        .collect::<BezPath>()
                }
            };
            stroked(frame * course, &style.pen, None).map(|stroke| unmasked(vec![stroke])).map_err(|error| nev![error])
        }
        Mark::Arrow { path } => {
            let paper = *placement * &**path;
            let heads = paper
                .reverse_subpaths()
                .subpaths()
                .enumerate()
                .map(|(index, elements)| Validated::from(tipped(style, &kurbo::segments(elements.iter().copied()).collect::<Vec<PathSeg>>()).map_err(item(index))))
                .collect::<Validated<Vec<_>, _>>();
            Validated::from(stroked(paper, &style.pen, None)).map2(heads, |stroke, terminators| unmasked(iter::once(stroke).chain(terminators).collect())).ok()
        }
        Mark::Bar { at, angle, divisions, height, linear } => {
            let linear = linear.as_ref().unwrap_or(&style.linear);
            let direction = (placement.with_translation(Vec2::ZERO) * Vec2::from_angle(angle.get::<radian>()).to_point()).to_vec2();
            let frame = Affine::rotate(direction.atan2()).then_translate((*placement * *at).to_vec2());
            let scale = direction.length();
            let [rise, gap] = [*height, style.gap].map(Width::into_inner);
            let lengths = divisions
                .iter()
                .enumerate()
                .map(|(index, division)| Validated::from((division.is_finite() && *division > Length::ZERO).then_some(*division).ok_or(AnnotationError::Degenerate).map_err(item(index))))
                .collect::<Validated<Vec<_>, _>>()
                .ok()?;
            let ticks = iter::once(Length::ZERO)
                .chain(lengths.iter().scan(Length::ZERO, |total, division| {
                    *total += *division;
                    Some(*total)
                }))
                .map(|length| (length, length.get::<millimeter>() * scale))
                .collect::<Vec<_>>();
            let total = ticks.last().map_or_default(|&(_, offset)| offset);
            let outline = BezPath::from_path_segments(
                iter::chain([Line::new((0.0, 0.0), (total, 0.0)), Line::new((0.0, -rise), (total, -rise))], ticks.iter().map(|&(_, offset)| Line::new((offset, 0.0), (offset, -rise - gap))))
                    .map(PathSeg::Line),
            );
            let solid = ticks.array_windows().step_by(2).flat_map(|&[(_, from), (_, to)]| Rect::new(from, -rise, to, 0.0).path_elements(tolerance)).collect::<BezPath>();
            let labels = ticks
                .iter()
                .map(|&(length, offset)| {
                    labelled(
                        typesetter,
                        style,
                        &Label { content: notation::linear(linear, length).into(), .. },
                        frame * Point::new(offset, gap.mul_add(-2.0, -rise)),
                        direction,
                        Anchor::Middle,
                        Baseline::Bottom,
                    )
                    .map_or_else(Validated::Fail, Validated::Good)
                })
                .collect::<Validated<Vec<_>, _>>();
            Validated::from(Region::new(&(frame * solid), FillRule::NonZero, style.tolerance).map_err(AnnotationError::from))
                .map3(Validated::from(stroked(frame * outline, &style.pen, None)), labels, |region, stroke, lettered| {
                    let (lettering, masks): (Vec<_>, Vec<_>) = lettered.into_iter().unzip();
                    (iter::chain([Element::Fill { region, color: style.pen.color.clone(), opacity: style.pen.opacity }, stroke], lettering.into_iter().flatten()).collect::<Vec<_>>(), masks)
                })
                .ok()
                .and_then(|(elements, masks)| {
                    regions::boolean(&Region::default(), &masks, BinaryOp::Union, style.tolerance).map(|mask| Graphics { elements, mask }).map_err(|error| nev![AnnotationError::from(error)])
                })
        }
    }
}
