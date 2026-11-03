#![feature(cmp_minmax, const_result_trait_fn, const_trait_impl, default_field_values, float_conversions)]

use core::{
    borrow::Borrow,
    f64::consts::FRAC_1_SQRT_2,
    iter,
};
use std::sync::Arc;

use color::{AlphaColor, Srgb};
use kurbo::{Affine, BezPath, Circle, Line, ParamCurve as _, ParamCurveArclen as _, PathEl, PathSeg, Point, Rect, Shape, Size, Triangle, Vec2};
use linesweeper::{BinaryOp, FillRule};
use nonempty_collections::{IntoNonEmptyIterator as _, NEVec, NonEmptyIterator as _, nev};
use parlance::{FontFamily, FontFeature, ParseFontFamilyError};
use rasm_linework::{
    Anchor, Baseline, Color, Element, FontSize, FontStyle, FontWeight, FontWidth, Linetype, LineworkError, Opacity, OpacityError, Path, Pen, Placement, Region, Tolerance, Width, regions,
    transform,
};
use skrifa::ScaleError;
use uom::si::{
    angle::radian,
    f64::{Angle, Length},
    length::millimeter,
};
use validated::Validated;

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum Arrowhead {
    Filled,
    Closed,
    Open,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, derive_more::Display)]
#[display(rename_all = "lowercase")]
pub enum Lookup {
    Family,
    Slot,
    Element,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, derive_more::Display)]
pub enum Defect {
    #[display("was never registered")]
    Unregistered,
    #[display("declares no positive cap height")]
    CapHeight,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, derive_more::Display)]
#[display(rename_all = "lowercase")]
pub enum Feature {
    Gradient,
    Pattern,
    Mask,
    Filter,
    Blend,
    Opacity,
    Stroke,
    Font,
    #[display("text layout")]
    Layout,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, derive_more::Display)]
pub enum Geometry {
    #[display("points coincide or span no direction")]
    Coincident,
    #[display("lines are parallel")]
    Parallel,
    #[display("span is no longer than its gaps")]
    Gap,
    #[display("extent is not positive and finite")]
    Extent,
    #[display("domain spans no values")]
    Domain,
    #[display("station lies outside its line")]
    Station,
    #[display("spans columns the table lacks or another cell holds")]
    Overlap,
}

#[derive(Debug, Clone, PartialEq, Eq, Hash, derive_more::Display)]
pub enum Subject {
    #[display("item {_0}")]
    Item(usize),
    #[display("slot {_0}")]
    Slot(Arc<str>),
    #[display("cell at row {row}, column {column}")]
    Cell { row: usize, column: usize },
}

// --- [MODELS] --------------------------------------------------------------------------

#[nutype::nutype(validate(predicate = |angle| angle.is_normal() && angle.is_sign_positive() && *angle < Angle::HALF_TURN), derive(Debug, Clone, Copy, PartialEq))]
pub struct IncludedAngle(Angle);

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Head {
    Arrow { angle: IncludedAngle, arrowhead: Arrowhead },
    Oblique,
    Dot,
    Origin,
    Datum,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Terminator {
    pub size: Width,
    pub head: Head,
    pub pen: Pen,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Lettering {
    pub family: FontFamily<'static>,
    pub size: FontSize,
    pub weight: FontWeight = FontWeight::try_new(const { parlance::FontWeight::NORMAL.value() }).ok().unwrap(),
    pub width: FontWidth = FontWidth::try_new(const { parlance::FontWidth::NORMAL.ratio() }).ok().unwrap(),
    pub style: FontStyle = FontStyle::Normal,
    pub line_height: Option<Width> = None,
    pub features: Vec<FontFeature> = Vec::new(),
    pub color: Color,
    pub opacity: Opacity = Opacity::OPAQUE,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Style {
    pub pen: Pen,
    pub lettering: Lettering,
    pub gap: Width,
    pub linear: notation::Linear,
    pub angular: notation::Angular,
    pub dimension: dimensions::Style,
    pub leader: leaders::Style,
    pub arrow: Terminator,
    pub tolerance: Tolerance,
}

#[derive(Debug, Clone, PartialEq)]
pub enum Content {
    Label(text::Label),
    Symbol(symbols::Instance),
    Control(tolerances::FeatureControl),
}

#[derive(Debug, Clone)]
pub enum Annotation {
    Note(text::Note),
    Dimension(dimensions::Dimension),
    FeatureControl(tolerances::FeatureControl),
    Leader(leaders::Leader),
    Mark(Box<marks::Mark>),
    Symbol(symbols::Instance),
    Legend(legends::Legend),
    Table(Box<tables::Table>),
}

#[derive(Debug, Clone, Default, PartialEq)]
pub struct Graphics {
    pub elements: Vec<Element> = Vec::new(),
    pub masks: Vec<Region> = Vec::new(),
}

impl FromIterator<Self> for Graphics {
    fn from_iter<I>(iter: I) -> Self
    where I: IntoIterator<Item = Self> {
        iter.into_iter().fold(Self::default(), |Self { mut elements, mut masks }, part| {
            elements.extend(part.elements);
            masks.extend(part.masks);
            Self { elements, masks }
        })
    }
}

#[derive(Debug, Clone, PartialEq)]
pub struct Drawing {
    pub graphics: Vec<Graphics>,
    pub mask: Region,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum AnnotationError {
    #[error(transparent)]
    Linework(#[from] LineworkError),
    #[error(transparent)]
    Svg(#[from] usvg::Error),
    #[error("font {position} holds no readable face")]
    Font { position: usize },
    #[error("font family list {list} fails to parse")]
    Stack { list: Arc<str>, source: ParseFontFamilyError },
    #[error("no {lookup} named {name}")]
    Unknown { lookup: Lookup, name: Arc<str> },
    #[error("face {index} {defect}")]
    Face { index: u32, defect: Defect },
    #[error("glyph {glyph} draws no outline")]
    Outline { glyph: u32, source: ScaleError },
    #[error("{0}")]
    Geometry(Geometry),
    #[error("{0} lies outside the linework model")]
    Unsupported(Feature),
    #[error("{subject}")]
    Within { subject: Subject, source: Box<Self> },
}

impl From<AnnotationError> for NEVec<AnnotationError> {
    fn from(value: AnnotationError) -> Self { nev![value] }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn draw<I>(typesetter: &mut text::Typesetter, style: &Style, placement: Placement, annotations: I) -> Result<Drawing, NEVec<AnnotationError>>
where I: IntoIterator<Item: Borrow<Annotation>> {
    let graphics = indexed(annotations.into_iter().map(|annotation| match annotation.borrow() {
        Annotation::Note(note) => text::draw(typesetter, style, placement, note),
        Annotation::Dimension(dimension) => dimensions::draw(typesetter, style, placement, dimension),
        Annotation::FeatureControl(control) => tolerances::draw(typesetter, style, placement, control),
        Annotation::Leader(leader) => leaders::draw(typesetter, style, placement, leader),
        Annotation::Mark(mark) => marks::draw(typesetter, style, placement, mark),
        Annotation::Symbol(instance) => symbols::draw(typesetter, style, placement, instance),
        Annotation::Legend(legend) => legends::draw(typesetter, style, placement, legend),
        Annotation::Table(table) => tables::draw(typesetter, style, placement, table),
    }))?;
    let mask = regions::boolean(&Region::default(), graphics.iter().flat_map(|drawn| &drawn.masks), BinaryOp::Union, style.tolerance).map_err(AnnotationError::from)?;
    Ok(Drawing { graphics, mask })
}

pub(crate) fn drawn(typesetter: &mut text::Typesetter, style: &Style, placement: Placement, content: &Content, at: Point, anchor: Anchor, baseline: Baseline) -> Result<Graphics, NEVec<AnnotationError>> {
    let base = placement.inverse() * at;
    match content {
        Content::Label(label) => text::labelled(typesetter, style, label, at, Vec2::new(1.0, 0.0), anchor, baseline),
        Content::Symbol(instance) => symbols::draw(typesetter, style, placement, &symbols::Instance { at: base, ..instance.clone() }),
        Content::Control(control) => tolerances::draw(typesetter, style, placement, &tolerances::FeatureControl { at: base, ..control.clone() }).and_then(|frame| {
            let middle = bounds(&frame, Affine::IDENTITY).center().y;
            transformed(frame, Affine::translate((0.0, at.y - middle)))
        }),
    }
}

pub(crate) fn within<T>(subject: &Subject, result: Result<T, NEVec<AnnotationError>>) -> Validated<T, AnnotationError> {
    result.map_or_else(
        |errors| Validated::Fail(errors.into_nonempty_iter().map(|source| AnnotationError::Within { subject: subject.clone(), source: Box::new(source) }).collect()),
        Validated::Good,
    )
}

pub(crate) fn indexed<T, I>(results: I) -> Result<Vec<T>, NEVec<AnnotationError>>
where I: IntoIterator<Item = Result<T, NEVec<AnnotationError>>> {
    results.into_iter().enumerate().map(|(index, result)| within(&Subject::Item(index), result)).collect::<Validated<Vec<_>, _>>().ok()
}

pub(crate) const fn validated<T>(result: Result<T, NEVec<AnnotationError>>) -> Validated<T, AnnotationError> { result.map_or_else(Validated::Fail, Validated::Good) }

pub(crate) fn joined<E, G>(elements: E, graphics: G) -> Result<Graphics, NEVec<AnnotationError>>
where
    E: IntoIterator<Item = Result<Element, AnnotationError>>,
    G: IntoIterator<Item = Result<Graphics, NEVec<AnnotationError>>>,
{
    elements.into_iter().collect::<Validated<Vec<_>, _>>().map2(graphics.into_iter().map(validated).collect::<Validated<Vec<_>, _>>(), |elements, drawn| iter::once(Graphics { elements, .. }).chain(drawn).collect()).ok()
}

pub(crate) fn stroked(shape: BezPath, pen: &Pen, linetype: Option<Linetype>) -> Result<Element, AnnotationError> {
    Path::try_new(shape).map(|path| Element::Stroke { path, pen: pen.clone(), linetype }).map_err(|error| LineworkError::from(error).into())
}

pub(crate) fn filled(shape: &impl Shape, fill_rule: FillRule, color: Color, opacity: Opacity, tolerance: Tolerance) -> Result<Element, AnnotationError> {
    Region::new(shape, fill_rule, tolerance).map(|region| Element::Fill { region, color, opacity }).map_err(AnnotationError::from)
}

pub(crate) fn colored(color: AlphaColor<Srgb>) -> Result<(Color, Opacity), AnnotationError> {
    let (opaque, alpha) = color.split();
    Opacity::try_new(f64::from(alpha))
        .map(|opacity| (Color::Srgb(opaque), opacity))
        .map_err(|(OpacityError::FiniteViolated | OpacityError::GreaterOrEqualViolated | OpacityError::LessOrEqualViolated)| AnnotationError::Unsupported(Feature::Opacity))
}

pub(crate) fn terminated(terminator: &Terminator, head: Head, tip: Point, toward: Vec2, tolerance: Tolerance) -> Result<Element, AnnotationError> {
    (toward.is_finite() && toward != Vec2::ZERO).ok_or(AnnotationError::Geometry(Geometry::Coincident))?;
    let basis = oriented(tip, toward);
    let Terminator { size, head: _, pen } = terminator;
    let length = size.into_inner();
    let accuracy = tolerance.into_inner();
    let solid = |shape: BezPath| filled(&(basis * shape), FillRule::NonZero, pen.color.clone(), pen.opacity, tolerance);
    let outlined = |shape: BezPath| stroked(basis * shape, pen, None);
    let disc = || Circle::new(Point::ORIGIN, length * 0.5).to_path(accuracy);
    match head {
        Head::Arrow { angle, arrowhead } => {
            let spread = length * (angle.into_inner().get::<radian>() * 0.5).tan();
            let [left, right] = [Point::new(-length, spread), Point::new(-length, -spread)];
            match arrowhead {
                Arrowhead::Filled => solid(Triangle::new(Point::ORIGIN, left, right).to_path(accuracy)),
                Arrowhead::Closed => outlined(Triangle::new(Point::ORIGIN, left, right).to_path(accuracy)),
                Arrowhead::Open => outlined(polyline([left, Point::ORIGIN, right])),
            }
        }
        Head::Oblique => {
            let reach = length * 0.5 * FRAC_1_SQRT_2;
            outlined(Line::new((-reach, reach), (reach, -reach)).to_path(accuracy))
        }
        Head::Dot => solid(disc()),
        Head::Origin => outlined(disc()),
        Head::Datum => {
            let half = length / 3.0_f64.sqrt();
            solid(Triangle::new((0.0, half), (0.0, -half), (-length, 0.0)).to_path(accuracy))
        }
    }
}

pub(crate) fn measured(segments: impl IntoIterator<Item = PathSeg>, accuracy: f64) -> Vec<(PathSeg, f64, f64)> {
    segments
        .into_iter()
        .scan(0.0, |total: &mut f64, segment| {
            let from = *total;
            *total += segment.arclen(accuracy);
            Some((segment, from, *total))
        })
        .collect()
}

pub(crate) fn station(measured: &[(PathSeg, f64, f64)], at: f64, accuracy: f64) -> Option<(PathSeg, f64)> {
    measured.get(measured.partition_point(|&(_, _, to)| to < at)).filter(|_| at >= 0.0).map(|&(segment, from, to)| (segment, segment.inv_arclen((at - from).min(to - from), accuracy)))
}

pub(crate) fn tipped(terminator: &Terminator, head: Head, segments: impl IntoIterator<Item = PathSeg>, tolerance: Tolerance) -> Result<Element, AnnotationError> {
    let accuracy = tolerance.into_inner();
    let measured = measured(segments, accuracy);
    let length = measured.last().map_or_default(|&(_, _, to)| to);
    let [tip, base] = [0.0, terminator.size.into_inner().max(accuracy).min(length)].map(|at| station(&measured, at, accuracy).map(|(segment, parameter)| segment.eval(parameter)));
    let (tip, base) = tip.zip(base).ok_or(AnnotationError::Geometry(Geometry::Coincident))?;
    terminated(terminator, head, tip, tip - base, tolerance)
}

pub(crate) fn transformed(graphics: Graphics, affine: Affine) -> Result<Graphics, NEVec<AnnotationError>> {
    let Graphics { elements, masks } = graphics;
    let placement = Placement::try_new(affine).map_err(AnnotationError::from)?;
    validated(transform(elements, placement).map_err(|errors| errors.into_nonempty_iter().map(AnnotationError::from).collect()))
        .map2(masks.iter().map(|mask| mask.transform(placement).map_err(AnnotationError::from)).collect::<Validated<Vec<_>, _>>(), |elements, masks| Graphics { elements, masks })
        .ok()
}

#[must_use]
pub fn bounds(graphics: &Graphics, frame: Affine) -> Rect { graphics.masks.iter().map(|mask| (frame * mask.to_path(0.0)).bounding_box()).reduce(|left, right| left.union(right)).unwrap_or_default() }

pub(crate) fn graduated<I>(typesetter: &mut text::Typesetter, style: &Style, basis: Affine, size: Size, ticks: I, (direction, anchor, baseline): (Vec2, Anchor, Baseline)) -> Result<Graphics, NEVec<AnnotationError>>
where I: IntoIterator<Item = (f64, Option<String>)> {
    let accuracy = style.tolerance.into_inner();
    let gap = style.gap.into_inner();
    let Size { width: length, height: thickness } = size;
    let ticks = ticks.into_iter().collect::<Vec<_>>();
    let outline = iter::chain(
        Rect::new(0.0, 0.0, length, thickness).path_elements(accuracy),
        ticks.iter().flat_map(|&(position, _)| Line::new((position, thickness), (position, thickness + gap)).path_elements(accuracy)),
    )
    .collect::<BezPath>();
    let labels = ticks
        .into_iter()
        .filter_map(|(position, content)| {
            content.map(|content| text::labelled(typesetter, style, &text::Label { content: content.into(), .. }, basis * Point::new(position, gap.mul_add(2.0, thickness)), direction, anchor, baseline))
        })
        .map(validated)
        .collect::<Validated<Vec<_>, _>>();
    let bar = Region::new(&(basis * Rect::new(0.0, 0.0, length, thickness + gap).to_path(accuracy)), FillRule::NonZero, style.tolerance).map_err(AnnotationError::from);
    Validated::from(stroked(basis * outline, &style.pen, None))
        .map3(Validated::from(bar), labels, |stroke, bar, texts| iter::once(Graphics { elements: vec![stroke], masks: vec![bar] }).chain(texts).collect())
        .ok()
}

pub(crate) fn directed(affine: Affine, direction: Vec2) -> Vec2 { (affine.with_translation(Vec2::ZERO) * direction.to_point()).to_vec2() }

pub(crate) fn located(placement: Placement, at: Point, angle: Angle) -> (Point, Vec2) { (*placement * at, directed(*placement, Vec2::from_angle(angle.get::<radian>()))) }

pub(crate) fn oriented(origin: Point, direction: Vec2) -> Affine { Affine::rotate(direction.atan2()).then_translate(origin.to_vec2()) }

pub(crate) fn readable(direction: Vec2) -> Vec2 {
    let upright = if direction.x.abs() <= direction.y.abs() * (f64::EPSILON * 64.0) { direction.y <= 0.0 } else { direction.x > 0.0 };
    if upright { direction } else { -direction }
}

pub(crate) const fn fraction(anchor: Anchor) -> f64 {
    match anchor {
        Anchor::Start => 0.0,
        Anchor::Middle => 0.5,
        Anchor::End => 1.0,
    }
}

pub(crate) fn polyline(points: impl IntoIterator<Item = Point>) -> BezPath {
    let mut points = points.into_iter();
    iter::chain(points.next().map(PathEl::MoveTo), points.map(PathEl::LineTo)).collect()
}

pub(crate) fn edges(sizes: impl IntoIterator<Item = f64>) -> impl Iterator<Item = f64> {
    iter::once(0.0).chain(sizes.into_iter().scan(0.0, |total: &mut f64, size| {
        *total += size;
        Some(*total)
    }))
}

pub(crate) fn units(length: Length) -> f64 { length.get::<millimeter>() }

// --- [COMPOSITION] ---------------------------------------------------------------------

pub mod dimensions;
pub mod leaders;
pub mod legends;
pub mod marks;
pub mod notation;
pub mod symbols;
pub mod tables;
pub mod text;
pub mod tolerances;
