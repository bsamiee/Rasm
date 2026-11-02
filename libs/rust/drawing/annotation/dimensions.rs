use core::{f64::consts::FRAC_PI_2, iter};
use std::sync::Arc;

use itertools::{EitherOrBoth, Itertools as _};
use kurbo::{Affine, Axis, BezPath, Circle, Line, ParamCurve, PathEl, PathSeg, Point, Rect, Shape, Vec2};
use linesweeper::BinaryOp;
use nonempty_collections::{IntoNonEmptyIterator as _, NEVec, NonEmptyIterator as _, nev};
use rasm_linework::{Anchor, Baseline, Element, Placement, Region, Side, Width, regions, transform};
use uom::{
    marker,
    si::{
        self, Quantity, SI,
        angle::radian,
        f64::{Angle, Length},
        length::millimeter,
    },
};
use validated::Validated;

use crate::{
    AnnotationError, Graphics, Head, Style, Terminator, notation, stroked, terminated,
    text::{Frame, Label, Typesetter, labelled},
};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum Orientation {
    Aligned,
    Unidirectional,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, derive_more::Display)]
pub enum Span {
    #[display("R")]
    Radius,
    #[display("⌀")]
    Diameter,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Direction {
    Aligned,
    Fixed(Angle),
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Arrangement {
    Chain,
    Parallel { spacing: Width },
    Running,
}

#[derive(Debug, Clone, PartialEq, Eq)]
pub enum Value<Q> {
    Stated(Arc<str>),
    Deviations { stated: Option<Arc<str>>, deviations: [Q; 2] },
    Limits([Q; 2]),
}

#[derive(Debug, Clone, PartialEq)]
pub enum Measure {
    Linear { origin: Point, ends: NEVec<Point>, direction: Direction, offset: f64, arrangement: Arrangement = Arrangement::Chain, value: Option<Value<Length>> = None },
    Angular { first: Line, second: Line, radius: Width, value: Option<Value<Angle>> = None },
    Radial { circle: Circle, at: Angle, span: Span, value: Option<Value<Length>> = None },
    ArcLength { arc: kurbo::Arc, offset: f64, value: Option<Value<Length>> = None },
    Ordinate { origin: Point, feature: Point, axis: Axis, end: Point, value: Option<Value<Length>> = None },
}

#[derive(Debug, Clone, PartialEq)]
pub struct Dimension {
    pub measure: Measure,
    pub prefix: Option<Arc<str>> = None,
    pub suffix: Option<Arc<str>> = None,
    pub frame: Option<Frame> = None,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

fn extent(mask: &Region, from: Point, along: Vec2) -> Rect { (Affine::translate(-from.to_vec2()).then_rotate(-along.atan2()) * mask.path_elements(0.0).collect::<BezPath>()).bounding_box() }

fn reading(orientation: Orientation, line: Vec2) -> Vec2 {
    match orientation {
        Orientation::Aligned if (-FRAC_PI_2..FRAC_PI_2).contains(&line.atan2()) => line,
        Orientation::Aligned => -line,
        Orientation::Unidirectional => Axis::Horizontal.pack_vec2(1.0, 0.0),
    }
}

fn lettered(typesetter: &mut Typesetter, style: &Style, labels: &[Label], line: Vec2) -> Result<(Vec<Element>, Region), NEVec<AnnotationError>> {
    let direction = reading(style.orientation, line);
    let gap = style.gap.into_inner();
    let (_, elements, masks) = labels.iter().try_fold((Point::ORIGIN, Vec::new(), Vec::new()), |(cursor, mut elements, mut masks), label| {
        let (drawn, mask) = labelled(typesetter, style, label, cursor, direction, Anchor::Start, Baseline::Middle)?;
        let next = cursor + direction * (extent(&mask, cursor, direction).x1 + gap);
        elements.extend(drawn);
        masks.push(mask);
        Ok::<_, NEVec<AnnotationError>>((next, elements, masks))
    })?;
    regions::boolean(&Region::default(), &masks, BinaryOp::Union, style.tolerance).map(|mask| (elements, mask)).map_err(|error| nev![AnnotationError::from(error)])
}

fn placed(style: &Style, (elements, mask): (Vec<Element>, Region), at: Point, line: Vec2, anchor: Anchor) -> Result<(Vec<Element>, Region), NEVec<AnnotationError>> {
    let spanning = extent(&mask, Point::ORIGIN, line);
    let along = match anchor {
        Anchor::Start => -spanning.x0,
        Anchor::Middle => -spanning.center().x,
        Anchor::End => -spanning.x1,
    };
    let direction = reading(style.orientation, line);
    let (across, rise) = match style.orientation {
        Orientation::Aligned => (direction.turn_90(), -extent(&mask, Point::ORIGIN, direction).y1),
        Orientation::Unidirectional => (line.turn_90(), -spanning.center().y),
    };
    let shift = Placement::try_new(Affine::translate((at + line * along + across * rise).to_vec2())).map_err(|error| nev![AnnotationError::from(error)])?;
    Validated::from(mask.transform(shift).map_err(AnnotationError::from))
        .map2(transform(elements, shift).map_or_else(|errors| Validated::Fail(errors.into_nonempty_iter().map(AnnotationError::from).collect()), Validated::Good), |region, moved| (moved, region))
        .ok()
}

pub(crate) fn draw(typesetter: &mut Typesetter, style: &Style, placement: Placement, dimension: &Dimension) -> Result<Graphics, NEVec<AnnotationError>> {
    fn written<D>(
        dimension: &Dimension,
        symbol: &str,
        value: Option<&Value<Quantity<D, SI<f64>, f64>>>,
        measured: Quantity<D, SI<f64>, f64>,
        nominal: impl Fn(Quantity<D, SI<f64>, f64>) -> String,
        deviation: impl Fn(Quantity<D, SI<f64>, f64>) -> String,
    ) -> Vec<Label>
    where
        D: si::Dimension + ?Sized,
        D::Kind: marker::Add,
    {
        let ordered = |[first, second]: [Quantity<D, SI<f64>, f64>; 2]| if second < first { [second, first] } else { [first, second] };
        let signed = |quantity: Quantity<D, SI<f64>, f64>| {
            let text = deviation(quantity);
            if text.starts_with('-') || !text.contains(|digit: char| digit.is_ascii_digit() && digit != '0') { text } else { format!("+{text}") }
        };
        let framed = |content: String| Label { content: content.into(), frame: dimension.frame, .. };
        let plain = |content: &str| Label { content: content.into(), .. };
        let (head, stack) = match value {
            None => (Some(framed(format!("{symbol}{}", nominal(measured)))), None),
            Some(Value::Stated(text)) => (Some(framed(format!("{symbol}{text}"))), None),
            Some(Value::Deviations { stated, deviations }) => {
                let base = format!("{symbol}{}", stated.as_deref().map_or_else(|| nominal(measured), str::to_owned));
                let [lower, upper] = ordered(*deviations).map(signed);
                match upper.strip_prefix('+').filter(|magnitude| lower.strip_prefix('-') == Some(magnitude)) {
                    Some(magnitude) => (Some(framed(format!("{base} ±{magnitude}"))), None),
                    None => (Some(framed(base)), Some(plain(&format!("{upper}\n{lower}")))),
                }
            }
            Some(Value::Limits(deviations)) => {
                let [lower, upper] = ordered(*deviations);
                ((!symbol.is_empty()).then(|| plain(symbol)), Some(framed(format!("{}\n{}", nominal(measured + upper), nominal(measured + lower)))))
            }
        };
        [dimension.prefix.as_deref().map(plain), head, stack, dimension.suffix.as_deref().map(plain)].into_iter().flatten().collect()
    }
    fn spanned<C>(typesetter: &mut Typesetter, style: &Style, labels: &[Label], course: &C, heads: EitherOrBoth<(), ()>) -> Result<(Vec<Element>, Region), NEVec<AnnotationError>>
    where C: ParamCurve + Shape {
        let tolerance = style.tolerance.into_inner();
        let [size, overshoot] = [style.terminator.size, style.overshoot].map(Width::into_inner);
        let tangent = |at: f64| {
            course
                .subsegment(at..at + 1.0)
                .path_segments(tolerance)
                .next()
                .map(|segment| {
                    let cubic = segment.to_cubic();
                    (cubic.p1 - cubic.p0).normalize()
                })
                .filter(|direction| direction.is_finite())
        };
        let [Some(opening), Some(along), Some(closing)] = [0.0, 0.5, 1.0].map(tangent) else { return Err(nev![AnnotationError::Degenerate]) };
        let length = course.perimeter(tolerance);
        let (start, end) = heads.left_and_right();
        let tail = |held: Option<()>| held.map_or_default(|()| (size + overshoot) / length);
        let tips = [start.map(|()| (course.start(), -opening)), end.map(|()| (course.end(), closing))];
        let inner = lettered(typesetter, style, labels, along)?;
        let width = extent(&inner.1, Point::ORIGIN, along).width();
        let arrows_inside = tips.iter().flatten().map(|_| size).sum::<f64>() + width <= length;
        let text_inside = width <= length;
        let outer = course.subsegment(0.0..tail(end) + 1.0).end();
        let (glyphs, mask) = if text_inside {
            placed(style, inner, course.eval(0.5), along, Anchor::Middle)
        } else {
            lettered(typesetter, style, labels, closing).and_then(|beyond| placed(style, beyond, outer, closing, Anchor::Start))
        }?;
        let underline = match (text_inside, style.orientation) {
            (false, Orientation::Aligned) => Some(PathEl::LineTo(outer + closing * extent(&mask, outer, closing).x1)),
            (true, Orientation::Aligned | Orientation::Unidirectional) | (false, Orientation::Unidirectional) => None,
        };
        let course_path = if arrows_inside { course.to_path(tolerance) } else { course.subsegment(-tail(start)..tail(end) + 1.0).to_path(tolerance) };
        let line = stroked(iter::chain(course_path, underline).collect(), &style.pen, None).map(|stroke| match (text_inside, style.orientation) {
            (true, Orientation::Unidirectional) => Element::Clip { region: mask.clone(), keep: Side::Outside, elements: vec![stroke] },
            (true, Orientation::Aligned) | (false, Orientation::Aligned | Orientation::Unidirectional) => stroke,
        });
        let arrows = tips.into_iter().flatten().map(|(tip, outward)| terminated(style, tip, if arrows_inside { outward } else { -outward })).collect::<Validated<Vec<_>, _>>();
        Validated::from(line).map2(arrows, |dimension_line, terminators| (iter::once(dimension_line).chain(terminators).chain(glyphs).collect(), mask)).ok()
    }
    let tolerance = style.tolerance.into_inner();
    let [gap, overshoot] = [style.gap, style.overshoot].map(Width::into_inner);
    let mapped = |point: Point| *placement * point;
    let toward = |direction: Vec2| (placement.with_translation(Vec2::ZERO) * direction.to_point()).to_vec2().normalize();
    let degenerate = || nev![AnnotationError::Degenerate];
    let extension = |feature: Point, foot: Point| {
        let reach = feature.distance(foot);
        (reach > gap).then(|| stroked(Line::new(feature, foot).subsegment(gap / reach..overshoot / reach + 1.0).to_path(tolerance), &style.pen, None))
    };
    let nominal = |measured: Length| match &style.alternate {
        Some(alternate) => format!("{} [{}]", notation::linear(&style.linear, measured), notation::linear(alternate, measured)),
        None => notation::linear(&style.linear, measured),
    };
    let deviation = |measured: Length| notation::linear(&style.linear, measured);
    let angular = |measured: Angle| notation::angular(&style.angular, measured);
    let (extensions, spans) = match &dimension.measure {
        Measure::Linear { origin, ends, direction, offset, arrangement, value } => {
            let model = match direction {
                Direction::Aligned => (*ends.last() - *origin).normalize(),
                Direction::Fixed(angle) => Vec2::from_angle(angle.get::<radian>()),
            };
            let paper = toward(model);
            let frame = paper.is_finite().then(|| Affine::rotate(paper.atan2()).then_translate(mapped(*origin).to_vec2())).ok_or_else(degenerate)?;
            let local = frame.inverse();
            let foot = |point: Point, level: f64| frame * Point::new((local * mapped(point)).x, level);
            let labels = |first: Point, second: Point| written(dimension, "", value.as_ref(), Length::new::<millimeter>(model.dot(second - first).abs()), nominal, deviation);
            let points = iter::once(*origin).chain(ends.iter().copied()).collect::<Vec<_>>();
            let reaches = match arrangement {
                Arrangement::Chain | Arrangement::Running => points.iter().map(|&point| (point, *offset)).collect::<Vec<_>>(),
                Arrangement::Parallel { spacing } => {
                    let levels = iter::successors(Some(*offset), |level| Some(spacing.into_inner().copysign(*offset) + level)).take(ends.len().get()).collect::<Vec<_>>();
                    iter::chain(levels.last().map(|&outermost| (*origin, outermost)), ends.iter().copied().zip(levels)).collect()
                }
            };
            let spans = match arrangement {
                Arrangement::Chain => points
                    .array_windows()
                    .map(|&[first, second]| spanned(typesetter, style, &labels(first, second), &Line::new(foot(first, *offset), foot(second, *offset)), EitherOrBoth::Both((), ())))
                    .collect::<Vec<_>>(),
                Arrangement::Parallel { spacing: _ } => reaches
                    .iter()
                    .skip(1)
                    .map(|&(end, level)| spanned(typesetter, style, &labels(*origin, end), &Line::new(foot(*origin, level), foot(end, level)), EitherOrBoth::Both((), ())))
                    .collect(),
                Arrangement::Running => {
                    let base = foot(*origin, *offset);
                    let outward = if offset.is_sign_negative() { -paper.turn_90() } else { paper.turn_90() };
                    let (low, high) = points.iter().map(|&point| (local * mapped(point)).x).minmax_by(f64::total_cmp).into_option().ok_or_else(degenerate)?;
                    let origin_style = Style { terminator: Terminator { head: Head::Origin, ..style.terminator }, ..style.clone() };
                    let course = Validated::from(stroked(Line::new(frame * Point::new(low, *offset), frame * Point::new(high, *offset)).to_path(tolerance), &style.pen, None))
                        .map2(Validated::from(terminated(&origin_style, base, paper)), |line, indicator| (vec![line, indicator], Region::default()))
                        .ok();
                    let zero = lettered(typesetter, style, &[Label { content: nominal(Length::new::<millimeter>(0.0)).into(), .. }], outward)
                        .and_then(|lettering| placed(style, lettering, base + outward * overshoot, outward, Anchor::Start));
                    iter::chain(
                        [course, zero],
                        ends.iter().map(|&end| {
                            let tip = foot(end, *offset);
                            Validated::from(terminated(style, tip, (tip - base).normalize()))
                                .map2(
                                    lettered(typesetter, style, &labels(*origin, end), outward)
                                        .and_then(|lettering| placed(style, lettering, tip + outward * overshoot, outward, Anchor::Start))
                                        .map_or_else(Validated::Fail, Validated::Good),
                                    |arrow, (glyphs, mask)| (iter::once(arrow).chain(glyphs).collect(), mask),
                                )
                                .ok()
                        }),
                    )
                    .collect()
                }
            };
            (reaches.into_iter().map(|(point, level)| extension(mapped(point), foot(point, level))).collect::<Vec<_>>(), spans)
        }
        Measure::Angular { first, second, radius, value } => {
            let vertex = first.crossing_point(*second).filter(|vertex| vertex.is_finite()).ok_or_else(degenerate)?;
            let [model_first, model_second] = [first, second].map(|line| line.p1 - line.p0);
            let center = mapped(vertex);
            let reach = radius.into_inner();
            let [ray_first, ray_second] = [model_first, model_second].map(toward);
            let course = kurbo::Arc::new(center, Vec2::splat(reach), ray_first.atan2(), ray_first.cross(ray_second).atan2(ray_first.dot(ray_second)), 0.0);
            let measured = Angle::new::<radian>(model_first.cross(model_second).atan2(model_first.dot(model_second)).abs());
            (
                [(first, ray_first), (second, ray_second)]
                    .into_iter()
                    .map(|(line, ray)| {
                        let (low, high) = [line.p0, line.p1].into_iter().map(|point| ray.dot(mapped(point) - center)).minmax_by(f64::total_cmp).into_option()?;
                        extension(center + ray * reach.clamp(low, high), center + ray * reach)
                    })
                    .collect::<Vec<_>>(),
                vec![spanned(typesetter, style, &written(dimension, "", value.as_ref(), measured, angular, angular), &course, EitherOrBoth::Both((), ()))],
            )
        }
        Measure::Radial { circle, at, span, value } => {
            let spoke = Vec2::from_angle(at.get::<radian>());
            let [rim, opposite] = [circle.radius, -circle.radius].map(|distance| mapped(circle.center + spoke * distance));
            let (course, heads, measured) = match span {
                Span::Radius => (Line::new(mapped(circle.center), rim), EitherOrBoth::Right(()), circle.radius),
                Span::Diameter => (Line::new(opposite, rim), EitherOrBoth::Both((), ()), circle.radius * 2.0),
            };
            let labels = written(dimension, &span.to_string(), value.as_ref(), Length::new::<millimeter>(measured.abs()), nominal, deviation);
            (Vec::new(), vec![spanned(typesetter, style, &labels, &course, heads)])
        }
        Measure::ArcLength { arc, offset, value } => {
            let center = mapped(arc.center);
            let [start, middle, end] = [arc.start(), arc.eval(0.5), arc.end()].map(mapped);
            let level = center.distance(start) + offset;
            let sweep = arc.sweep_angle * placement.determinant().signum();
            let bisector = (middle - center).normalize();
            let parallel = sweep.abs() <= FRAC_PI_2;
            let foot = |point: Point| {
                let relative = point - center;
                let projected = relative.dot(bisector);
                if parallel { point + bisector * (projected.mul_add(projected, level.mul_add(level, -relative.hypot2())).sqrt() - projected) } else { center + relative.normalize() * level }
            };
            let [near, far] = [start, end].map(foot);
            let [opening, closing] = [near, far].map(|point| point - center);
            let course = kurbo::Arc::new(center, Vec2::splat(level), opening.atan2(), if parallel { opening.cross(closing).atan2(opening.dot(closing)) } else { sweep }, 0.0);
            let measured = Length::new::<millimeter>(arc.perimeter(tolerance / placement.determinant().abs().sqrt()));
            (
                vec![extension(start, near), extension(end, far)],
                vec![spanned(typesetter, style, &written(dimension, "⌒", value.as_ref(), measured, nominal, deviation), &course, EitherOrBoth::Both((), ()))],
            )
        }
        Measure::Ordinate { origin, feature, axis, end, value } => {
            let [base, tip] = [feature, end].map(|point| mapped(*point));
            let [measuring, leading] = [axis.pack_vec2(1.0, 0.0), axis.pack_vec2(0.0, 1.0)].map(toward);
            let frame = Affine::new([measuring.x, measuring.y, leading.x, leading.y, base.x, base.y]);
            let offset = frame.inverse() * tip;
            (offset.y.abs() > gap).ok_or(degenerate())?;
            let jog = (offset.x.abs() * 0.5).min(offset.y.abs().mul_add(0.5, -gap).max(0.0)).copysign(offset.y);
            let leader = [Point::new(0.0, gap.copysign(offset.y)), Point::new(0.0, offset.y.mul_add(0.5, -jog)), Point::new(offset.x, offset.y.mul_add(0.5, jog)), offset]
                .into_iter()
                .dedup()
                .map(|point| frame * point)
                .tuple_windows()
                .map(|(from, to)| PathSeg::Line(Line::new(from, to)));
            let outward = if offset.y.is_sign_negative() { -leading } else { leading };
            let labels = written(dimension, "", value.as_ref(), Length::new::<millimeter>(feature.get_coord(*axis) - origin.get_coord(*axis)), nominal, deviation);
            (
                vec![Some(stroked(BezPath::from_path_segments(leader), &style.pen, None))],
                vec![lettered(typesetter, style, &labels, outward).and_then(|lettering| placed(style, lettering, tip, outward, Anchor::Start))],
            )
        }
    };
    let (lines, (texts, masks)) = extensions
        .into_iter()
        .flatten()
        .collect::<Validated<Vec<_>, _>>()
        .map2(spans.into_iter().enumerate().map(|(index, span)| span.map_err(|errors| AnnotationError::Item { index, errors })).collect::<Validated<Vec<_>, _>>(), |strokes, drawn| {
            (strokes, drawn.into_iter().unzip::<_, _, Vec<_>, Vec<_>>())
        })
        .ok()?;
    regions::boolean(&Region::default(), &masks, BinaryOp::Union, style.tolerance)
        .map(|mask| Graphics { elements: iter::chain(lines, texts.into_iter().flatten()).collect(), mask })
        .map_err(|error| nev![AnnotationError::from(error)])
}
