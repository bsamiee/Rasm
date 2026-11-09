
use core::{
    cmp,
    f64::consts::{FRAC_PI_2, TAU},
    iter,
};
use std::sync::Arc;

use itertools::{EitherOrBoth, Itertools as _};
use kurbo::{Affine, Axis, Circle, Line, ParamCurve, PathEl, Point, Shape, Vec2};
use linesweeper::BinaryOp;
use nonempty_collections::NEVec;
use rasm_drawing::{Anchor, Baseline, Element, FontSize, Placement, Region, Side, Width, regions};
use uom::{
    marker,
    si::{
        self, Quantity, SI,
        angle::radian,
        f64::{Angle, Length},
    },
};

use crate::{
    DraftingError, Geometry, Graphics, Head, Lettering, Terminator, bounds, directed, fraction, indexed, joined,
    notation::{self, Sign},
    oriented, polyline, readable, stroked, symbols, terminated,
    text::{Frame, Label, Typesetter, labelled},
    transformed,
};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum Orientation {
    Aligned,
    Unidirectional,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, derive_more::Display)]
pub enum Size {
    #[display("R")]
    Radius,
    #[display("CR")]
    ControlledRadius,
    #[display("SR")]
    SphericalRadius,
    #[display("⌀")]
    Diameter,
    #[display("S⌀")]
    SphericalDiameter,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum Sweep {
    Interior,
    Reflex,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq)]
pub struct Style {
    pub terminator: Terminator,
    pub orientation: Orientation = Orientation::Aligned,
    pub overshoot: Width,
    pub overrun: Option<Width> = None,
    pub substitute: Option<Head> = None,
    pub stack_size: Option<FontSize> = None,
    pub alternate: Option<notation::Linear> = None,
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Arrangement {
    Chain,
    Parallel {
        spacing: Width,
    },
    Running,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Tolerancing<Q> {
    Deviations([Q; 2]),
    Limits([Q; 2]),
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Extension {
    Drawn,
    Fixed(Width),
    Suppressed,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Linear {
    pub origin: Point,
    pub ends: NEVec<Point>,
    pub direction: Option<Angle> = None,
    pub oblique: Option<Angle> = None,
    pub offset: f64,
    pub arrangement: Arrangement = Arrangement::Chain,
    pub tolerancing: Option<Tolerancing<Length>> = None,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Angular {
    pub first: Line,
    pub second: Line,
    pub radius: Width,
    pub sweep: Sweep = Sweep::Interior,
    pub tolerancing: Option<Tolerancing<Angle>> = None,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Radial {
    pub circle: Circle,
    pub at: Angle,
    pub size: Size,
    pub jog: Option<Point> = None,
    pub tolerancing: Option<Tolerancing<Length>> = None,
}

#[derive(Debug, Clone, PartialEq)]
pub struct ArcLength {
    pub arc: kurbo::Arc,
    pub offset: f64,
    pub tolerancing: Option<Tolerancing<Length>> = None,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Ordinate {
    pub origin: Point,
    pub feature: Point,
    pub axis: Axis,
    pub end: Point,
    pub tolerancing: Option<Tolerancing<Length>> = None,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Level {
    pub datum: f64,
    pub instance: symbols::Instance,
    pub slot: Arc<str>,
}

#[derive(Debug, Clone, PartialEq)]
pub enum Measure {
    Linear(Linear),
    Angular(Angular),
    Radial(Radial),
    ArcLength(ArcLength),
    Ordinate(Ordinate),
    Level(Level),
}

#[derive(Debug, Clone, PartialEq)]
pub struct Dimension {
    pub measure: Measure,
    pub stated: Option<Arc<str>> = None,
    pub prefix: Option<Arc<str>> = None,
    pub suffix: Option<Arc<str>> = None,
    pub frame: Option<Frame> = None,
    pub position: Option<Point> = None,
    pub extensions: [Extension; 2] = [Extension::Drawn; 2],
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub(crate) fn draw(typesetter: &mut Typesetter, style: &crate::Style, placement: Placement, dimension: &Dimension) -> Result<Graphics, NEVec<DraftingError>> {
    fn written<D>(
        style: &crate::Style,
        dimension: &Dimension,
        symbol: &str,
        tolerancing: Option<&Tolerancing<Quantity<D, SI<f64>, f64>>>,
        measured: Quantity<D, SI<f64>, f64>,
        nominal: impl Fn(Quantity<D, SI<f64>, f64>) -> String,
        deviation: impl Fn(Quantity<D, SI<f64>, f64>, Sign) -> String,
    ) -> Vec<Label>
    where
        D: si::Dimension + ?Sized,
        D::Kind: marker::Add + marker::Neg,
    {
        let ordered = |[first, second]: [Quantity<D, SI<f64>, f64>; 2]| cmp::minmax_by(first, second, |left, right| left.value.total_cmp(&right.value));
        let framed = |content: String| Label { content: content.into(), frame: dimension.frame, .. };
        let plain = |content: &str| Label { content: content.into(), .. };
        let base = || format!("{symbol}{}", dimension.stated.as_deref().map_or_else(|| nominal(measured), str::to_owned));
        let (head, stack) = match tolerancing {
            None => (Some(framed(base())), None),
            Some(Tolerancing::Deviations(deviations)) => match ordered(*deviations) {
                [lower, upper] if upper == -lower && upper.value > 0.0 => (Some(framed(format!("{} ±{}", base(), deviation(upper, Sign::Negative)))), None),
                [lower, upper] => (
                    Some(framed(base())),
                    Some(Label {
                        content: format!("{}\n{}", deviation(upper, Sign::Explicit), deviation(lower, Sign::Explicit)).into(),
                        lettering: style.dimension.stack_size.map(|size| Lettering { size, ..style.lettering.clone() }),
                        ..
                    }),
                ),
            },
            Some(Tolerancing::Limits(deviations)) => {
                let [lower, upper] = ordered(*deviations);
                ((!symbol.is_empty()).then(|| plain(symbol)), Some(framed(format!("{}\n{}", nominal(measured + upper), nominal(measured + lower)))))
            }
        };
        [dimension.prefix.as_deref().map(plain), head, stack, dimension.suffix.as_deref().map(plain)].into_iter().flatten().collect()
    }
    fn spanned<C>(typesetter: &mut Typesetter, style: &crate::Style, labels: &[Label], curve: &C, heads: EitherOrBoth<(), ()>, position: Option<Point>) -> Result<Graphics, NEVec<DraftingError>>
    where C: ParamCurve + Shape {
        let accuracy = style.tolerance.into_inner();
        let settings = &style.dimension;
        let [size, overshoot] = [settings.terminator.size, settings.overshoot].map(Width::into_inner);
        let tangent = |at: f64| {
            curve
                .subsegment(at..at + 1.0)
                .path_segments(accuracy)
                .next()
                .map(|segment| {
                    let cubic = segment.to_cubic();
                    (cubic.p1 - cubic.p0).normalize()
                })
                .filter(|direction| direction.is_finite())
        };
        let [Some(opening), Some(along), Some(closing)] = [0.0, 0.5, 1.0].map(tangent) else { return Err(DraftingError::Geometry(Geometry::Coincident).into()) };
        let length = curve.to_path(accuracy).perimeter(accuracy);
        let (start, end) = heads.left_and_right();
        let ends = f64::from(u8::from(start.is_some())) + f64::from(u8::from(end.is_some()));
        let strip = row(typesetter, style, labels, along)?;
        let width = bounds(&strip, oriented(Point::ORIGIN, along).inverse()).width();
        let text_inside = width <= length;
        let (flipped, head) = match (ends.mul_add(size, width) <= length, settings.substitute) {
            (true, None | Some(_)) => (false, settings.terminator.head),
            (false, Some(substitute)) => (false, substitute),
            (false, None) => (true, settings.terminator.head),
        };
        let tail = (size + overshoot) / length;
        let reach = |held: Option<()>| held.map_or_default(|()| if flipped { tail } else { settings.overrun.map_or_default(|overrun| overrun.into_inner() / length) });
        let outer = curve.subsegment(0.0..end.map_or_default(|()| tail) + 1.0).end();
        let text = match (position, text_inside) {
            (Some(at), true | false) => placed(style, strip, at, along, Anchor::Middle)?,
            (None, true) => placed(style, strip, curve.eval(0.5), along, Anchor::Middle)?,
            (None, false) => {
                let turn = reading(settings.orientation, closing).atan2() - reading(settings.orientation, along).atan2();
                placed(style, transformed(strip, Affine::rotate(turn))?, outer, closing, Anchor::Start)?
            }
        };
        let (underline, crossing) = match (position, text_inside, settings.orientation) {
            (None, false, Orientation::Aligned) => (Some(PathEl::LineTo(outer + closing * bounds(&text, oriented(outer, closing).inverse()).x1)), false),
            (None, true, Orientation::Unidirectional) => (None, true),
            (Some(_), true | false, Orientation::Aligned | Orientation::Unidirectional) | (None, true, Orientation::Aligned) | (None, false, Orientation::Unidirectional) => (None, false),
        };
        let leader = position.map(|at| Line::new(curve.eval(0.5), at));
        let region = (crossing || leader.is_some()).then(|| regions::boolean(&Region::default(), &text.masks, BinaryOp::Union, style.tolerance)).transpose().map_err(DraftingError::from)?;
        let outside = |element: Element| match &region {
            Some(kept) => Element::Clip { region: kept.clone(), keep: Side::Outside, elements: vec![element].into() },
            None => element,
        };
        let line = stroked(iter::chain(curve.subsegment(-reach(start)..reach(end) + 1.0).to_path(accuracy), underline).collect(), &style.pen, None)
            .map(|stroke| if crossing { outside(stroke) } else { stroke });
        let leg = leader.map(|segment| stroked(segment.to_path(accuracy), &style.pen, None).map(outside));
        let arrows = [start.map(|()| (curve.start(), -opening)), end.map(|()| (curve.end(), closing))]
            .into_iter()
            .flatten()
            .map(|(tip, outward)| terminated(&settings.terminator, head, tip, if flipped { -outward } else { outward }, style.tolerance));
        joined(iter::once(line).chain(leg).chain(arrows), [Ok(text)])
    }
    let nominal = |length: Length| match &style.dimension.alternate {
        Some(alternate) => format!("{} [{}]", notation::linear(&style.linear, length, Sign::Negative), notation::linear(alternate, length, Sign::Negative)),
        None => notation::linear(&style.linear, length, Sign::Negative),
    };
    let lengths = |symbol: &str, tolerancing: Option<&Tolerancing<Length>>, measured: Length| {
        written(style, dimension, symbol, tolerancing, measured, nominal, |length, sign| notation::linear(&style.linear, length, sign))
    };
    let extended = |extension: Extension, feature: Point, foot: Point| {
        let [gap, overshoot] = [style.gap, style.dimension.overshoot].map(Width::into_inner);
        let reach = feature.distance(foot);
        let start = match extension {
            Extension::Suppressed => None,
            Extension::Drawn => (reach > gap).then_some(gap / reach),
            Extension::Fixed(length) => (reach > 0.0).then_some(1.0 - (length.into_inner() - overshoot) / reach),
        }?;
        Some(stroked(Line::new(feature, foot).subsegment(start..overshoot / reach + 1.0).to_path(style.tolerance.into_inner()), &style.pen, None))
    };
    let beside =
        |typesetter: &mut Typesetter, labels: &[Label], at: Point, direction: Vec2| row(typesetter, style, labels, direction).and_then(|strip| placed(style, strip, at, direction, Anchor::Start));
    let linear = |typesetter: &mut Typesetter, Linear { origin, ends, direction, oblique, offset, arrangement, tolerancing }: &Linear| {
        let accuracy = style.tolerance.into_inner();
        let overshoot = style.dimension.overshoot.into_inner();
        let measuring = direction.map_or_else(|| (*ends.last() - *origin).normalize(), |angle| Vec2::from_angle(angle.get::<radian>()));
        let paper = directed(*placement, measuring).normalize();
        paper.is_finite().ok_or(DraftingError::Geometry(Geometry::Coincident))?;
        let start = *placement * *origin;
        let basis = oriented(start, paper);
        let normal = paper.turn_90();
        let leaning = oblique.map_or(normal, |angle| directed(*placement, Vec2::from_angle(angle.get::<radian>())).normalize());
        let lean = normal.dot(leaning);
        (lean.is_finite() && lean != 0.0).ok_or(DraftingError::Geometry(Geometry::Parallel))?;
        let foot = |point: Point, level: f64| {
            let at = *placement * point;
            at + leaning * ((level - normal.dot(at - start)) / lean)
        };
        let position = dimension.position.map(|at| *placement * at);
        let labels = |first: Point, second: Point| lengths("", tolerancing.as_ref(), *style.unit * measuring.dot(second - first).abs());
        let mut span =
            |first: Point, second: Point, level: f64| spanned(typesetter, style, &labels(first, second), &Line::new(foot(first, level), foot(second, level)), EitherOrBoth::Both((), ()), position);
        let points = iter::once(*origin).chain(ends.iter().copied());
        let flush = || points.clone().map(|point| (point, *offset)).collect::<Vec<_>>();
        let (reaches, spans) = match arrangement {
            Arrangement::Chain => (flush(), indexed(points.clone().tuple_windows().map(|(first, second)| span(first, second, *offset)))),
            Arrangement::Parallel { spacing } => {
                let levels = iter::successors(Some(*offset), |level| Some(spacing.into_inner().copysign(*offset) + level)).take(ends.len().get()).collect::<Vec<_>>();
                (
                    iter::chain(levels.last().map(|&outermost| (*origin, outermost)), ends.iter().copied().zip(levels.iter().copied())).collect(),
                    indexed(iter::zip(ends.iter(), &levels).map(|(&end, &level)| span(*origin, end, level))),
                )
            }
            Arrangement::Running => {
                let base = foot(*origin, *offset);
                let outward = if offset.is_sign_negative() { -normal } else { normal };
                let local = basis.inverse();
                let terminator = &style.dimension.terminator;
                let zero = beside(typesetter, &[Label { content: nominal(*style.unit * 0.0).into(), .. }], base + outward * overshoot, outward);
                let mut marked = |end: Point| {
                    let tip = foot(end, *offset);
                    joined([terminated(terminator, terminator.head, tip, (tip - base).normalize(), style.tolerance)], [beside(typesetter, &labels(*origin, end), tip + outward * overshoot, outward)])
                };
                let first = (local * base).x;
                let [low, high] =
                    ends.iter().map(|&point| (local * foot(point, *offset)).x).fold([first; 2], |[low, high], next| [cmp::min_by(low, next, f64::total_cmp), cmp::max_by(high, next, f64::total_cmp)]);
                let spans = joined(
                    [
                        stroked(Line::new(basis * Point::new(low, *offset), basis * Point::new(high, *offset)).to_path(accuracy), &style.pen, None),
                        terminated(terminator, Head::Origin, base, paper, style.tolerance),
                    ],
                    [zero, indexed(ends.iter().map(|&end| marked(end)))],
                );
                (flush(), spans)
            }
        };
        let [first, rest] = dimension.extensions;
        joined(reaches.into_iter().enumerate().filter_map(|(index, (point, level))| extended(if index == 0 { first } else { rest }, *placement * point, foot(point, level))), [spans])
    };
    let angular = |typesetter: &mut Typesetter, Angular { first, second, radius, sweep, tolerancing }: &Angular| {
        let vertex = first.crossing_point(*second).filter(|vertex| vertex.is_finite()).ok_or(DraftingError::Geometry(Geometry::Parallel))?;
        let ray = |line: &Line| if line.p0.distance(vertex) < line.p1.distance(vertex) { line.p1 - vertex } else { line.p0 - vertex };
        let swept = |from: Vec2, to: Vec2| {
            let interior = from.cross(to).atan2(from.dot(to));
            match sweep {
                Sweep::Interior => interior,
                Sweep::Reflex => interior - TAU.copysign(interior),
            }
        };
        let [model_first, model_second] = [first, second].map(ray);
        let [paper_first, paper_second] = [model_first, model_second].map(|direction| directed(*placement, direction).normalize());
        (paper_first.is_finite() && paper_second.is_finite()).ok_or(DraftingError::Geometry(Geometry::Coincident))?;
        let center = *placement * vertex;
        let reach = radius.into_inner();
        let curve = kurbo::Arc::new(center, Vec2::splat(reach), paper_first.atan2(), swept(paper_first, paper_second), 0.0);
        let labels = written(
            style,
            dimension,
            "",
            tolerancing.as_ref(),
            Angle::new::<radian>(swept(model_first, model_second).abs()),
            |angle| notation::angular(&style.angular, angle, Sign::Negative),
            |angle, sign| notation::angular(&style.angular, angle, sign),
        );
        let [first_extension, rest_extension] = dimension.extensions;
        joined(
            [(first, paper_first, first_extension), (second, paper_second, rest_extension)].into_iter().filter_map(|(line, direction, extension)| {
                let [low, high] = cmp::minmax_by(direction.dot(*placement * line.p0 - center), direction.dot(*placement * line.p1 - center), f64::total_cmp);
                extended(extension, center + direction * reach.clamp(low, high), center + direction * reach)
            }),
            [spanned(typesetter, style, &labels, &curve, EitherOrBoth::Both((), ()), dimension.position.map(|at| *placement * at))],
        )
    };
    let radial = |typesetter: &mut Typesetter, Radial { circle, at, size, jog, tolerancing }: &Radial| {
        let spoke = Vec2::from_angle(at.get::<radian>());
        let center = *placement * circle.center;
        let [rim, opposite] = [circle.radius, -circle.radius].map(|distance| *placement * (circle.center + spoke * distance));
        let measured = match size {
            Size::Radius | Size::ControlledRadius | Size::SphericalRadius => circle.radius,
            Size::Diameter | Size::SphericalDiameter => circle.radius * 2.0,
        };
        let labels = lengths(&size.to_string(), tolerancing.as_ref(), *style.unit * measured.abs());
        let position = dimension.position.map(|point| *placement * point);
        match (size, jog) {
            (Size::Radius | Size::ControlledRadius | Size::SphericalRadius, None) => spanned(typesetter, style, &labels, &Line::new(center, rim), EitherOrBoth::Right(()), position),
            (Size::Radius | Size::ControlledRadius | Size::SphericalRadius, Some(relocated)) => {
                let stand_in = *placement * *relocated;
                let direction = (rim - center).normalize();
                let shift = direction * ((rim - stand_in).dot(direction) * 0.4);
                let knee = rim - shift;
                joined([stroked(polyline([stand_in, stand_in + shift, knee]), &style.pen, None)], [spanned(typesetter, style, &labels, &Line::new(knee, rim), EitherOrBoth::Right(()), position)])
            }
            (Size::Diameter | Size::SphericalDiameter, None | Some(_)) => spanned(typesetter, style, &labels, &Line::new(opposite, rim), EitherOrBoth::Both((), ()), position),
        }
    };
    let arc_length = |typesetter: &mut Typesetter, ArcLength { arc, offset, tolerancing }: &ArcLength| {
        let accuracy = style.tolerance.into_inner();
        let center = *placement * arc.center;
        let [start, middle, end] = [arc.start(), arc.eval(0.5), arc.end()].map(|point| *placement * point);
        let radius = center.distance(start) + offset;
        let sweep = arc.sweep_angle * placement.determinant().signum();
        let bisector = (middle - center).normalize();
        let parallel = sweep.abs() <= FRAC_PI_2;
        let foot = |point: Point| {
            let relative = point - center;
            let projected = relative.dot(bisector);
            if parallel { point + bisector * (projected.mul_add(projected, radius.mul_add(radius, -relative.hypot2())).sqrt() - projected) } else { center + relative.normalize() * radius }
        };
        let [near, far] = [start, end].map(foot);
        let [opening, closing] = [near, far].map(|point| point - center);
        let curve = kurbo::Arc::new(center, Vec2::splat(radius), opening.atan2(), if parallel { opening.cross(closing).atan2(opening.dot(closing)) } else { sweep }, 0.0);
        let model_accuracy = accuracy / placement.determinant().abs().sqrt();
        let labels = lengths("⌒", tolerancing.as_ref(), *style.unit * arc.to_path(model_accuracy).perimeter(model_accuracy));
        let [first, rest] = dimension.extensions;
        joined(
            [extended(first, start, near), extended(rest, end, far)].into_iter().flatten(),
            [spanned(typesetter, style, &labels, &curve, EitherOrBoth::Both((), ()), dimension.position.map(|at| *placement * at))],
        )
    };
    let ordinate = |typesetter: &mut Typesetter, Ordinate { origin, feature, axis, end, tolerancing }: &Ordinate| {
        let gap = style.gap.into_inner();
        let [base, tip] = [feature, end].map(|point| *placement * *point);
        let [measuring, leading] = [axis.pack_vec2(1.0, 0.0), axis.pack_vec2(0.0, 1.0)].map(|direction| directed(*placement, direction).normalize());
        let basis = Affine::new([measuring.x, measuring.y, leading.x, leading.y, base.x, base.y]);
        let offset = basis.inverse() * tip;
        (offset.y.abs() > gap).ok_or(DraftingError::Geometry(Geometry::Gap))?;
        let jog = (offset.x.abs() * 0.5).min(offset.y.abs().mul_add(0.5, -gap).max(0.0)).copysign(offset.y);
        let leader = polyline(
            [Point::new(0.0, gap.copysign(offset.y)), Point::new(0.0, offset.y.mul_add(0.5, -jog)), Point::new(offset.x, offset.y.mul_add(0.5, jog)), offset]
                .into_iter()
                .dedup()
                .map(|point| basis * point),
        );
        let outward = if offset.y.is_sign_negative() { -leading } else { leading };
        let labels = lengths("", tolerancing.as_ref(), *style.unit * (feature.get_coord(*axis) - origin.get_coord(*axis)));
        joined([stroked(leader, &style.pen, None)], [beside(typesetter, &labels, tip, outward)])
    };
    let level = |typesetter: &mut Typesetter, Level { datum, instance, slot }: &Level| {
        let value = dimension.stated.as_deref().map_or_else(|| notation::linear(&style.linear, *style.unit * (instance.at.y - datum), Sign::Explicit), str::to_owned);
        let content = format!("{}{value}{}", dimension.prefix.as_deref().unwrap_or_default(), dimension.suffix.as_deref().unwrap_or_default());
        symbols::draw(typesetter, style, placement, &symbols::Instance { slots: iter::chain(instance.slots.clone(), [(Arc::clone(slot), Arc::from(content))]).collect(), ..instance.clone() })
    };
    match &dimension.measure {
        Measure::Linear(measure) => linear(typesetter, measure),
        Measure::Angular(measure) => angular(typesetter, measure),
        Measure::Radial(measure) => radial(typesetter, measure),
        Measure::ArcLength(measure) => arc_length(typesetter, measure),
        Measure::Ordinate(measure) => ordinate(typesetter, measure),
        Measure::Level(measure) => level(typesetter, measure),
    }
}

fn reading(orientation: Orientation, line: Vec2) -> Vec2 {
    match orientation {
        Orientation::Aligned => readable(line),
        Orientation::Unidirectional => Vec2::new(1.0, 0.0),
    }
}

fn row(typesetter: &mut Typesetter, style: &crate::Style, labels: &[Label], line: Vec2) -> Result<Graphics, NEVec<DraftingError>> {
    let direction = reading(style.dimension.orientation, line);
    let gap = style.gap.into_inner();
    labels
        .iter()
        .try_fold((Point::ORIGIN, Graphics::default()), |(cursor, drawn), label| {
            let set = labelled(typesetter, style, label, cursor, direction, Anchor::Start, Baseline::Middle)?;
            let next = cursor + direction * (bounds(&set, oriented(cursor, direction).inverse()).x1 + gap);
            Ok::<_, NEVec<DraftingError>>((next, [drawn, set].into_iter().collect()))
        })
        .map(|(_, drawn)| drawn)
}

fn placed(style: &crate::Style, graphics: Graphics, at: Point, line: Vec2, anchor: Anchor) -> Result<Graphics, NEVec<DraftingError>> {
    let spanning = bounds(&graphics, oriented(Point::ORIGIN, line).inverse());
    let along = -(spanning.width().mul_add(fraction(anchor), spanning.x0));
    let (across, rise) = match style.dimension.orientation {
        Orientation::Aligned => {
            let direction = readable(line);
            (direction.turn_90(), -bounds(&graphics, oriented(Point::ORIGIN, direction).inverse()).y1)
        }
        Orientation::Unidirectional => (line.turn_90(), -spanning.center().y),
    };
    transformed(graphics, Affine::translate((at + line * along + across * rise).to_vec2()))
}
