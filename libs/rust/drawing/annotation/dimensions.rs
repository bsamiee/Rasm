use core::{
    cmp,
    f64::consts::{FRAC_PI_2, TAU},
    iter,
};
use std::sync::Arc;

use enum_map::EnumMap;
use itertools::{EitherOrBoth, Itertools as _};
use kurbo::{Affine, Axis, BezPath, Circle, Line, ParamCurve, PathEl, Point, Rect, Shape, Vec2};
use linesweeper::BinaryOp;
use nonempty_collections::{NEVec, nev};
use rasm_linework::{Anchor, Baseline, Element, FontSize, Placement, Region, Side, Width, regions};
use uom::{
    marker,
    si::{
        self, Quantity, SI,
        angle::radian,
        f64::{Angle, Length},
    },
};
use validated::Validated;

use crate::{
    AnnotationError, Content, Geometry, Graphics, Head, Lettering, Terminator, directed, fraction, indexed, model,
    notation::{self, Sign},
    oriented, polyline, readable, stroked, symbols, tables, terminated,
    text::{Frame, Label, Typesetter, labelled},
    transformed, validated,
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
    #[display("⌀")]
    Diameter,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum Sweep {
    Interior,
    Reflex,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, derive_more::Display)]
pub enum Characteristic {
    #[display("⏤")]
    Straightness,
    #[display("⏥")]
    Flatness,
    #[display("○")]
    Circularity,
    #[display("⌭")]
    Cylindricity,
    #[display("⌒")]
    LineProfile,
    #[display("⌓")]
    SurfaceProfile,
    #[display("∠")]
    Angularity,
    #[display("⟂")]
    Perpendicularity,
    #[display("∥")]
    Parallelism,
    #[display("⌖")]
    Position,
    #[display("◎")]
    Concentricity,
    #[display("⌯")]
    Symmetry,
    #[display("↗")]
    CircularRunout,
    #[display("⌰")]
    TotalRunout,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, derive_more::Display)]
pub enum Modifier {
    #[display("Ⓜ")]
    MaximumMaterial,
    #[display("Ⓛ")]
    LeastMaterial,
    #[display("Ⓟ")]
    ProjectedZone,
    #[display("Ⓕ")]
    FreeState,
    #[display("Ⓣ")]
    TangentPlane,
    #[display("Ⓤ")]
    UnequalProfile,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, derive_more::Display)]
pub enum Zone {
    #[display("")]
    Width,
    #[display("⌀")]
    Diameter,
    #[display("S⌀")]
    Spherical,
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
    Parallel { spacing: Width },
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

#[derive(Debug, Clone, PartialEq, Eq, Hash)]
pub struct Datum {
    pub letter: Arc<str>,
    pub modifiers: Vec<Modifier> = Vec::new(),
}

#[derive(Debug, Clone, PartialEq)]
pub struct FeatureControl {
    pub at: Point,
    pub characteristic: Characteristic,
    pub zone: Zone = Zone::Width,
    pub tolerance: Length,
    pub modifiers: Vec<Modifier> = Vec::new(),
    pub datums: Vec<Datum> = Vec::new(),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub(crate) fn draw(typesetter: &mut Typesetter, style: &crate::Style, placement: Placement, dimension: &Dimension) -> Result<Graphics, NEVec<AnnotationError>> {
    match &dimension.measure {
        Measure::Linear(measure) => linear(typesetter, style, placement, dimension, measure),
        Measure::Angular(measure) => angular(typesetter, style, placement, dimension, measure),
        Measure::Radial(measure) => radial(typesetter, style, placement, dimension, measure),
        Measure::ArcLength(measure) => arc_length(typesetter, style, placement, dimension, measure),
        Measure::Ordinate(measure) => ordinate(typesetter, style, placement, dimension, measure),
        Measure::Level(measure) => level(typesetter, style, placement, dimension, measure),
    }
}

pub(crate) fn controlled(typesetter: &mut Typesetter, style: &crate::Style, placement: Placement, control: &FeatureControl) -> Result<Graphics, NEVec<AnnotationError>> {
    let FeatureControl { at, characteristic, zone, tolerance, modifiers, datums } = control;
    let cell = |content: String| tables::Cell { content: Content::Label(Label { content: content.into(), .. }), .. };
    let row = NEVec::from((
        cell(characteristic.to_string()),
        iter::once(format!("{zone}{}{}", notation::linear(&style.linear, *tolerance, Sign::Negative), modifiers.iter().join("")))
            .chain(datums.iter().map(|Datum { letter, modifiers: marks }| format!("{letter}{}", marks.iter().join(""))))
            .map(cell)
            .collect::<Vec<_>>(),
    ));
    let ruled = |rule: tables::Rule| match rule {
        tables::Rule::Border | tables::Rule::Columns => Some(style.pen.clone()),
        tables::Rule::Header | tables::Rule::Rows => None,
    };
    tables::draw(
        typesetter,
        style,
        placement,
        &tables::Table {
            at: *at,
            columns: NEVec::from_elem(tables::Column { anchor: Anchor::Middle, vertical: Anchor::Middle, .. }, row.len()),
            rows: nev![row.into()],
            rules: EnumMap::from_fn(ruled),
            ..
        },
    )
}

fn linear(typesetter: &mut Typesetter, style: &crate::Style, placement: Placement, dimension: &Dimension, measure: &Linear) -> Result<Graphics, NEVec<AnnotationError>> {
    let Linear { origin, ends, direction, oblique, offset, arrangement, tolerancing } = measure;
    let accuracy = style.tolerance.into_inner();
    let overshoot = style.dimension.overshoot.into_inner();
    let measuring = direction.map_or_else(|| (*ends.last() - *origin).normalize(), |angle| Vec2::from_angle(angle.get::<radian>()));
    let paper = directed(*placement, measuring).normalize();
    paper.is_finite().ok_or(AnnotationError::Geometry(Geometry::Coincident))?;
    let start = *placement * *origin;
    let basis = oriented(start, paper);
    let normal = paper.turn_90();
    let leaning = oblique.map_or(normal, |angle| directed(*placement, Vec2::from_angle(angle.get::<radian>())).normalize());
    let lean = normal.dot(leaning);
    (lean.is_finite() && lean != 0.0).ok_or(AnnotationError::Geometry(Geometry::Parallel))?;
    let foot = |point: Point, level: f64| {
        let at = *placement * point;
        at + leaning * ((level - normal.dot(at - start)) / lean)
    };
    let position = dimension.position.map(|at| *placement * at);
    let labels = |first: Point, second: Point| written(style, dimension, "", tolerancing.as_ref(), model(measuring.dot(second - first).abs()), |length| nominal(style, length), |length, sign| notation::linear(&style.linear, length, sign));
    let span = |typesetter: &mut Typesetter, first: Point, second: Point, level: f64| spanned(typesetter, style, &labels(first, second), &Line::new(foot(first, level), foot(second, level)), EitherOrBoth::Both((), ()), position);
    let points = iter::once(*origin).chain(ends.iter().copied()).collect::<Vec<_>>();
    let flush = || points.iter().map(|&point| (point, *offset)).collect::<Vec<_>>();
    let (reaches, spans) = match arrangement {
        Arrangement::Chain => (flush(), indexed(points.array_windows().map(|&[first, second]| span(typesetter, first, second, *offset))).map(Graphics::from_iter)),
        Arrangement::Parallel { spacing } => {
            let levels = iter::successors(Some(*offset), |level| Some(spacing.into_inner().copysign(*offset) + level)).take(ends.len().get()).collect::<Vec<_>>();
            (
                iter::chain(levels.last().map(|&outermost| (*origin, outermost)), ends.iter().copied().zip(levels.iter().copied())).collect(),
                indexed(iter::zip(ends.iter(), &levels).map(|(&end, &level)| span(typesetter, *origin, end, level))).map(Graphics::from_iter),
            )
        }
        Arrangement::Running => {
            let base = foot(*origin, *offset);
            let outward = if offset.is_sign_negative() { -normal } else { normal };
            let local = basis.inverse();
            let marked = |typesetter: &mut Typesetter, end: Point| {
                let tip = foot(end, *offset);
                Validated::from(terminated(&style.dimension.terminator, tip, (tip - base).normalize(), style.tolerance))
                    .map2(validated(beside(typesetter, style, &labels(*origin, end), tip + outward * overshoot, outward)), |arrow, text| [Graphics { elements: vec![arrow], masks: Vec::new() }, text].into_iter().collect())
                    .ok()
            };
            let spans = points.iter().map(|&point| (local * foot(point, *offset)).x).minmax_by(f64::total_cmp).into_option().ok_or_else(|| AnnotationError::Geometry(Geometry::Coincident).into()).and_then(|(low, high)| {
                Validated::from(stroked(Line::new(basis * Point::new(low, *offset), basis * Point::new(high, *offset)).to_path(accuracy), &style.pen, None))
                    .map4(
                        Validated::from(terminated(&Terminator { head: Head::Origin, ..style.dimension.terminator.clone() }, base, paper, style.tolerance)),
                        validated(beside(typesetter, style, &[Label { content: nominal(style, model(0.0)).into(), .. }], base + outward * overshoot, outward)),
                        validated(indexed(ends.iter().map(|&end| marked(typesetter, end)))),
                        |course, indicator, zero, marks| iter::once(Graphics { elements: vec![course, indicator], masks: Vec::new() }).chain(iter::once(zero)).chain(marks).collect(),
                    )
                    .ok()
            });
            (flush(), spans)
        }
    };
    let [first, rest] = dimension.extensions;
    reaches
        .into_iter()
        .enumerate()
        .filter_map(|(index, (point, level))| extended(style, if index == 0 { first } else { rest }, *placement * point, foot(point, level)))
        .collect::<Validated<Vec<_>, _>>()
        .map2(validated(spans), |lines, drawn| [Graphics { elements: lines, masks: Vec::new() }, drawn].into_iter().collect())
        .ok()
}

fn angular(typesetter: &mut Typesetter, style: &crate::Style, placement: Placement, dimension: &Dimension, measure: &Angular) -> Result<Graphics, NEVec<AnnotationError>> {
    let Angular { first, second, radius, sweep, tolerancing } = measure;
    let vertex = first.crossing_point(*second).filter(|vertex| vertex.is_finite()).ok_or(AnnotationError::Geometry(Geometry::Parallel))?;
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
    (paper_first.is_finite() && paper_second.is_finite()).ok_or(AnnotationError::Geometry(Geometry::Coincident))?;
    let center = *placement * vertex;
    let reach = radius.into_inner();
    let course = kurbo::Arc::new(center, Vec2::splat(reach), paper_first.atan2(), swept(paper_first, paper_second), 0.0);
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
    [(first, paper_first, first_extension), (second, paper_second, rest_extension)]
        .into_iter()
        .filter_map(|(line, direction, extension)| {
            let (low, high) = [line.p0, line.p1].into_iter().map(|point| direction.dot(*placement * point - center)).minmax_by(f64::total_cmp).into_option()?;
            extended(style, extension, center + direction * reach.clamp(low, high), center + direction * reach)
        })
        .collect::<Validated<Vec<_>, _>>()
        .map2(validated(spanned(typesetter, style, &labels, &course, EitherOrBoth::Both((), ()), dimension.position.map(|at| *placement * at))), |lines, drawn| {
            [Graphics { elements: lines, masks: Vec::new() }, drawn].into_iter().collect()
        })
        .ok()
}

fn radial(typesetter: &mut Typesetter, style: &crate::Style, placement: Placement, dimension: &Dimension, measure: &Radial) -> Result<Graphics, NEVec<AnnotationError>> {
    let Radial { circle, at, size, jog, tolerancing } = measure;
    let spoke = Vec2::from_angle(at.get::<radian>());
    let center = *placement * circle.center;
    let [rim, opposite] = [circle.radius, -circle.radius].map(|distance| *placement * (circle.center + spoke * distance));
    let measured = match size {
        Size::Radius => circle.radius,
        Size::Diameter => circle.radius * 2.0,
    };
    let labels = written(style, dimension, &size.to_string(), tolerancing.as_ref(), model(measured.abs()), |length| nominal(style, length), |length, sign| notation::linear(&style.linear, length, sign));
    let position = dimension.position.map(|point| *placement * point);
    match (size, jog) {
        (Size::Radius, None) => spanned(typesetter, style, &labels, &Line::new(center, rim), EitherOrBoth::Right(()), position),
        (Size::Radius, Some(relocated)) => {
            let stand_in = *placement * *relocated;
            let direction = (rim - center).normalize();
            let shift = direction * ((rim - stand_in).dot(direction) * 0.4);
            let knee = rim - shift;
            Validated::from(stroked(polyline([stand_in, stand_in + shift, knee]), &style.pen, None))
                .map2(validated(spanned(typesetter, style, &labels, &Line::new(knee, rim), EitherOrBoth::Right(()), position)), |leg, drawn| {
                    [Graphics { elements: vec![leg], masks: Vec::new() }, drawn].into_iter().collect()
                })
                .ok()
        }
        (Size::Diameter, None | Some(_)) => spanned(typesetter, style, &labels, &Line::new(opposite, rim), EitherOrBoth::Both((), ()), position),
    }
}

fn arc_length(typesetter: &mut Typesetter, style: &crate::Style, placement: Placement, dimension: &Dimension, measure: &ArcLength) -> Result<Graphics, NEVec<AnnotationError>> {
    let ArcLength { arc, offset, tolerancing } = measure;
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
    let course = kurbo::Arc::new(center, Vec2::splat(radius), opening.atan2(), if parallel { opening.cross(closing).atan2(opening.dot(closing)) } else { sweep }, 0.0);
    let model_accuracy = accuracy / placement.determinant().abs().sqrt();
    let labels = written(
        style,
        dimension,
        "⌒",
        tolerancing.as_ref(),
        model(arc.path_segments(model_accuracy).perimeter(model_accuracy)),
        |length| nominal(style, length),
        |length, sign| notation::linear(&style.linear, length, sign),
    );
    let [first, rest] = dimension.extensions;
    [extended(style, first, start, near), extended(style, rest, end, far)]
        .into_iter()
        .flatten()
        .collect::<Validated<Vec<_>, _>>()
        .map2(validated(spanned(typesetter, style, &labels, &course, EitherOrBoth::Both((), ()), dimension.position.map(|at| *placement * at))), |lines, drawn| {
            [Graphics { elements: lines, masks: Vec::new() }, drawn].into_iter().collect()
        })
        .ok()
}

fn ordinate(typesetter: &mut Typesetter, style: &crate::Style, placement: Placement, dimension: &Dimension, measure: &Ordinate) -> Result<Graphics, NEVec<AnnotationError>> {
    let Ordinate { origin, feature, axis, end, tolerancing } = measure;
    let gap = style.gap.into_inner();
    let [base, tip] = [feature, end].map(|point| *placement * *point);
    let [measuring, leading] = [axis.pack_vec2(1.0, 0.0), axis.pack_vec2(0.0, 1.0)].map(|direction| directed(*placement, direction).normalize());
    let basis = Affine::new([measuring.x, measuring.y, leading.x, leading.y, base.x, base.y]);
    let offset = basis.inverse() * tip;
    (offset.y.abs() > gap).ok_or(AnnotationError::Geometry(Geometry::Gap))?;
    let jog = (offset.x.abs() * 0.5).min(offset.y.abs().mul_add(0.5, -gap).max(0.0)).copysign(offset.y);
    let leader = polyline([Point::new(0.0, gap.copysign(offset.y)), Point::new(0.0, offset.y.mul_add(0.5, -jog)), Point::new(offset.x, offset.y.mul_add(0.5, jog)), offset].into_iter().dedup().map(|point| basis * point));
    let outward = if offset.y.is_sign_negative() { -leading } else { leading };
    let labels = written(
        style,
        dimension,
        "",
        tolerancing.as_ref(),
        model(feature.get_coord(*axis) - origin.get_coord(*axis)),
        |length| nominal(style, length),
        |length, sign| notation::linear(&style.linear, length, sign),
    );
    Validated::from(stroked(leader, &style.pen, None))
        .map2(validated(beside(typesetter, style, &labels, tip, outward)), |line, text| [Graphics { elements: vec![line], masks: Vec::new() }, text].into_iter().collect())
        .ok()
}

fn level(typesetter: &mut Typesetter, style: &crate::Style, placement: Placement, dimension: &Dimension, measure: &Level) -> Result<Graphics, NEVec<AnnotationError>> {
    let Level { datum, instance, slot } = measure;
    let value = dimension.stated.as_deref().map_or_else(|| notation::linear(&style.linear, model(instance.at.y - datum), Sign::Explicit), str::to_owned);
    let content = format!("{}{value}{}", dimension.prefix.as_deref().unwrap_or_default(), dimension.suffix.as_deref().unwrap_or_default());
    symbols::draw(typesetter, style, placement, &symbols::Instance { slots: iter::chain(instance.slots.clone(), [(Arc::clone(slot), Arc::from(content))]).collect(), ..instance.clone() })
}

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

fn nominal(style: &crate::Style, length: Length) -> String {
    match &style.dimension.alternate {
        Some(alternate) => format!("{} [{}]", notation::linear(&style.linear, length, Sign::Negative), notation::linear(alternate, length, Sign::Negative)),
        None => notation::linear(&style.linear, length, Sign::Negative),
    }
}

fn extended(style: &crate::Style, extension: Extension, feature: Point, foot: Point) -> Option<Result<Element, AnnotationError>> {
    let [gap, overshoot] = [style.gap, style.dimension.overshoot].map(Width::into_inner);
    let reach = feature.distance(foot);
    let start = match extension {
        Extension::Suppressed => None,
        Extension::Drawn => (reach > gap).then_some(gap / reach),
        Extension::Fixed(length) => (reach > 0.0).then_some(1.0 - (length.into_inner() - overshoot) / reach),
    }?;
    Some(stroked(Line::new(feature, foot).subsegment(start..overshoot / reach + 1.0).to_path(style.tolerance.into_inner()), &style.pen, None))
}

fn reading(orientation: Orientation, line: Vec2) -> Vec2 {
    match orientation {
        Orientation::Aligned => readable(line),
        Orientation::Unidirectional => Vec2::new(1.0, 0.0),
    }
}

fn extent(graphics: &Graphics, from: Point, along: Vec2) -> Rect {
    let local = oriented(from, along).inverse();
    graphics.masks.iter().map(|mask| (local * mask.path_elements(0.0).collect::<BezPath>()).bounding_box()).reduce(|left, right| left.union(right)).unwrap_or_default()
}

fn lettered(typesetter: &mut Typesetter, style: &crate::Style, labels: &[Label], line: Vec2) -> Result<Graphics, NEVec<AnnotationError>> {
    let direction = reading(style.dimension.orientation, line);
    let gap = style.gap.into_inner();
    labels
        .iter()
        .try_fold((Point::ORIGIN, Graphics::default()), |(cursor, drawn), label| {
            let set = labelled(typesetter, style, label, cursor, direction, Anchor::Start, Baseline::Middle)?;
            let next = cursor + direction * (extent(&set, cursor, direction).x1 + gap);
            Ok::<_, NEVec<AnnotationError>>((next, [drawn, set].into_iter().collect()))
        })
        .map(|(_, drawn)| drawn)
}

fn placed(style: &crate::Style, graphics: Graphics, at: Point, line: Vec2, anchor: Anchor) -> Result<Graphics, NEVec<AnnotationError>> {
    let spanning = extent(&graphics, Point::ORIGIN, line);
    let along = -(spanning.width().mul_add(fraction(anchor), spanning.x0));
    let (across, rise) = match style.dimension.orientation {
        Orientation::Aligned => {
            let direction = readable(line);
            (direction.turn_90(), -extent(&graphics, Point::ORIGIN, direction).y1)
        }
        Orientation::Unidirectional => (line.turn_90(), -spanning.center().y),
    };
    transformed(graphics, Affine::translate((at + line * along + across * rise).to_vec2()))
}

fn beside(typesetter: &mut Typesetter, style: &crate::Style, labels: &[Label], at: Point, direction: Vec2) -> Result<Graphics, NEVec<AnnotationError>> {
    lettered(typesetter, style, labels, direction).and_then(|lettering| placed(style, lettering, at, direction, Anchor::Start))
}

fn spanned<C>(typesetter: &mut Typesetter, style: &crate::Style, labels: &[Label], course: &C, heads: EitherOrBoth<(), ()>, position: Option<Point>) -> Result<Graphics, NEVec<AnnotationError>>
where C: ParamCurve + Shape {
    let accuracy = style.tolerance.into_inner();
    let settings = &style.dimension;
    let [size, overshoot] = [settings.terminator.size, settings.overshoot].map(Width::into_inner);
    let tangent = |at: f64| {
        course
            .subsegment(at..at + 1.0)
            .path_segments(accuracy)
            .next()
            .map(|segment| {
                let cubic = segment.to_cubic();
                (cubic.p1 - cubic.p0).normalize()
            })
            .filter(|direction| direction.is_finite())
    };
    let [Some(opening), Some(along), Some(closing)] = [0.0, 0.5, 1.0].map(tangent) else { return Err(AnnotationError::Geometry(Geometry::Coincident).into()) };
    let length = course.path_segments(accuracy).perimeter(accuracy);
    let (start, end) = heads.left_and_right();
    let ends = f64::from(u8::from(start.is_some())) + f64::from(u8::from(end.is_some()));
    let lettering = lettered(typesetter, style, labels, along)?;
    let width = extent(&lettering, Point::ORIGIN, along).width();
    let text_inside = width <= length;
    let (flipped, head) = match (ends.mul_add(size, width) <= length, settings.substitute) {
        (true, None | Some(_)) => (false, settings.terminator.head),
        (false, Some(substitute)) => (false, substitute),
        (false, None) => (true, settings.terminator.head),
    };
    let tail = (size + overshoot) / length;
    let reach = |held: Option<()>| held.map_or_default(|()| if flipped { tail } else { settings.overrun.map_or_default(|overrun| overrun.into_inner() / length) });
    let outer = course.subsegment(0.0..end.map_or_default(|()| tail) + 1.0).end();
    let text = match (position, text_inside) {
        (Some(at), true | false) => placed(style, lettering, at, along, Anchor::Middle)?,
        (None, true) => placed(style, lettering, course.eval(0.5), along, Anchor::Middle)?,
        (None, false) => {
            let turn = reading(settings.orientation, closing).atan2() - reading(settings.orientation, along).atan2();
            placed(style, transformed(lettering, Affine::rotate(turn))?, outer, closing, Anchor::Start)?
        }
    };
    let (underline, crossing) = match (position, text_inside, settings.orientation) {
        (None, false, Orientation::Aligned) => (Some(PathEl::LineTo(outer + closing * extent(&text, outer, closing).x1)), false),
        (None, true, Orientation::Unidirectional) => (None, true),
        (Some(_), true | false, Orientation::Aligned | Orientation::Unidirectional) | (None, true, Orientation::Aligned) | (None, false, Orientation::Unidirectional) => (None, false),
    };
    let leader = position.map(|at| Line::new(course.eval(0.5), at));
    let region = (crossing || leader.is_some()).then(|| regions::boolean(&Region::default(), &text.masks, BinaryOp::Union, style.tolerance)).transpose().map_err(AnnotationError::from)?;
    let outside = |element: Element| match &region {
        Some(kept) => Element::Clip { region: kept.clone(), keep: Side::Outside, elements: vec![element] },
        None => element,
    };
    let line = stroked(iter::chain(course.subsegment(-reach(start)..reach(end) + 1.0).to_path(accuracy), underline).collect(), &style.pen, None).map(|stroke| if crossing { outside(stroke) } else { stroke });
    let joined = leader.map(|segment| stroked(segment.to_path(accuracy), &style.pen, None).map(outside)).transpose();
    let terminator = Terminator { head, ..settings.terminator.clone() };
    let arrows = [start.map(|()| (course.start(), -opening)), end.map(|()| (course.end(), closing))]
        .into_iter()
        .flatten()
        .map(|(tip, outward)| terminated(&terminator, tip, if flipped { -outward } else { outward }, style.tolerance))
        .collect::<Validated<Vec<_>, _>>();
    Validated::from(line)
        .map3(Validated::from(joined), arrows, |dimension_line, leg, terminators| Graphics {
            elements: iter::once(dimension_line).chain(leg).chain(terminators).chain(text.elements).collect(),
            masks: text.masks,
        })
        .ok()
}
