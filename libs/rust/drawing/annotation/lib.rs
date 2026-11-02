#![feature(const_result_trait_fn, const_trait_impl, default_field_values, error_reporter, float_conversions)]

use core::{borrow::Borrow, f64::consts::FRAC_1_SQRT_2, fmt};
use std::{error::Report, sync::Arc};

use kurbo::{Affine, BezPath, Circle, Line, PathEl, Point, Shape as _, Triangle, Vec2};
use linesweeper::{BinaryOp, FillRule};
use nonempty_collections::{NEVec, nev};
use parlance::{FontFamily, ParseFontFamilyError};
use rasm_linework::{Color, Element, FontSize, FontStyle, FontWeight, FontWidth, Linetype, LineworkError, Opacity, Path, Pen, Placement, Region, Tolerance, Width, regions};
use skrifa::ScaleError;
use uom::si::{angle::radian, f64::Angle};
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
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, derive_more::Display)]
pub enum Defect {
    #[display("holds no readable font")]
    Unreadable,
    #[display("was never registered")]
    Unregistered,
    #[display("declares no positive capital height")]
    Capless,
}

// --- [MODELS] --------------------------------------------------------------------------

#[nutype::nutype(validate(predicate = |angle| angle.is_normal() && angle.is_sign_positive() && *angle < Angle::HALF_TURN), derive(Debug, Clone, Copy, PartialEq))]
pub struct Aperture(Angle);

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Head {
    Arrow { angle: Aperture, arrowhead: Arrowhead },
    Oblique,
    Dot,
    Origin,
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Terminator {
    pub size: Width,
    pub head: Head,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Lettering {
    pub family: FontFamily<'static>,
    pub size: FontSize,
    pub weight: FontWeight = FontWeight::try_new(const { parlance::FontWeight::NORMAL.value() }).ok().unwrap(),
    pub width: FontWidth = FontWidth::try_new(const { parlance::FontWidth::NORMAL.ratio() }).ok().unwrap(),
    pub style: FontStyle = FontStyle::Normal,
    pub color: Color,
    pub opacity: Opacity = Opacity::OPAQUE,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Style {
    pub pen: Pen,
    pub lettering: Lettering,
    pub terminator: Terminator,
    pub gap: Width,
    pub overshoot: Width,
    pub landing: Width,
    pub orientation: dimensions::Orientation = dimensions::Orientation::Aligned,
    pub linear: notation::Linear,
    pub angular: notation::Angular,
    pub alternate: Option<notation::Linear> = None,
    pub tolerance: Tolerance,
}

#[derive(Debug, Clone)]
pub enum Annotation {
    Note(text::Note),
    Dimension(dimensions::Dimension),
    Leader(leaders::Leader),
    Mark(marks::Mark),
    Symbol(symbols::Instance),
    Legend(legends::Legend),
    Table(Box<tables::Table>),
}

#[derive(Debug, Clone, PartialEq)]
pub struct Graphics {
    pub elements: Vec<Element>,
    pub mask: Region,
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum AnnotationError {
    #[error(transparent)]
    Linework(#[from] LineworkError),
    #[error(transparent)]
    Svg(#[from] usvg::Error),
    #[error("font {position} holds no face")]
    Font { position: usize },
    #[error("font family list {list} fails to parse")]
    Stack { list: Arc<str>, source: ParseFontFamilyError },
    #[error("no {lookup} named {name}")]
    Unknown { lookup: Lookup, name: Arc<str> },
    #[error("face {index} {defect}")]
    Face { index: u32, defect: Defect },
    #[error("glyph {glyph} draws no outline")]
    Outline { glyph: u32, source: ScaleError },
    #[error("geometry spans no length or direction")]
    Degenerate,
    #[error("{0} lies outside the linework model")]
    Unsupported(symbols::Feature),
    #[error("cell at row {row}, column {column} spans columns the table lacks or another cell holds")]
    Cell { row: usize, column: usize },
    #[error("item {index} failed{}", fmt::from_fn(|formatter| .errors.iter().try_for_each(|error| write!(formatter, "\n{}", Report::new(error)))))]
    Item { index: usize, errors: NEVec<Self> },
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn draw<I>(typesetter: &mut text::Typesetter, style: &Style, placement: Placement, annotations: I) -> Result<Graphics, NEVec<AnnotationError>>
where I: IntoIterator<Item: Borrow<Annotation>> {
    let drawn = annotations
        .into_iter()
        .enumerate()
        .map(|(index, annotation)| {
            match annotation.borrow() {
                Annotation::Note(note) => text::draw(typesetter, style, placement, note),
                Annotation::Dimension(dimension) => dimensions::draw(typesetter, style, placement, dimension),
                Annotation::Leader(leader) => leaders::draw(typesetter, style, placement, leader),
                Annotation::Mark(mark) => marks::draw(typesetter, style, placement, mark),
                Annotation::Symbol(instance) => symbols::draw(typesetter, style, placement, instance),
                Annotation::Legend(legend) => legends::draw(typesetter, style, placement, legend),
                Annotation::Table(table) => tables::draw(typesetter, style, placement, table),
            }
            .map_err(|errors| AnnotationError::Item { index, errors })
        })
        .collect::<Validated<Vec<_>, _>>()
        .ok()?;
    let mask = regions::boolean(&Region::default(), drawn.iter().map(|graphics| &graphics.mask), BinaryOp::Union, style.tolerance).map_err(|error| nev![AnnotationError::from(error)])?;
    Ok(Graphics { elements: drawn.into_iter().flat_map(|graphics| graphics.elements).collect(), mask })
}

pub(crate) fn stroked(shape: BezPath, pen: &Pen, linetype: Option<Linetype>) -> Result<Element, AnnotationError> {
    Path::try_new(shape).map(|path| Element::Stroke { path, pen: pen.clone(), linetype }).map_err(|error| LineworkError::from(error).into())
}

fn terminated(style: &Style, tip: Point, toward: Vec2) -> Result<Element, AnnotationError> {
    (toward.is_finite() && toward != Vec2::ZERO).ok_or(AnnotationError::Degenerate)?;
    let frame = Affine::rotate(toward.atan2()).then_translate(tip.to_vec2());
    let Terminator { size, head } = style.terminator;
    let length = size.into_inner();
    let tolerance = style.tolerance.into_inner();
    let filled = |shape: BezPath| {
        Region::new(&(frame * shape), FillRule::NonZero, style.tolerance)
            .map(|region| Element::Fill { region, color: style.pen.color.clone(), opacity: style.pen.opacity })
            .map_err(AnnotationError::from)
    };
    let outlined = |shape: BezPath| stroked(frame * shape, &style.pen, None);
    let disc = || Circle::new(Point::ORIGIN, length * 0.5).to_path(tolerance);
    match head {
        Head::Arrow { angle, arrowhead } => {
            let spread = length * (angle.into_inner().get::<radian>() * 0.5).tan();
            let [left, right] = [Point::new(-length, spread), Point::new(-length, -spread)];
            match arrowhead {
                Arrowhead::Filled => filled(Triangle::new(Point::ORIGIN, left, right).to_path(tolerance)),
                Arrowhead::Closed => outlined(Triangle::new(Point::ORIGIN, left, right).to_path(tolerance)),
                Arrowhead::Open => outlined(BezPath::from_vec(vec![PathEl::MoveTo(left), PathEl::LineTo(Point::ORIGIN), PathEl::LineTo(right)])),
            }
        }
        Head::Oblique => {
            let reach = length * 0.5 * FRAC_1_SQRT_2;
            outlined(Line::new((-reach, reach), (reach, -reach)).to_path(tolerance))
        }
        Head::Dot => filled(disc()),
        Head::Origin => outlined(disc()),
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub mod dimensions;
pub mod leaders;
pub mod legends;
pub mod marks;
pub mod notation;
pub mod symbols;
pub mod tables;
pub mod text;
