#![feature(const_result_trait_fn, const_trait_impl, default_field_values)]

use core::{borrow::Borrow, f64::consts::FRAC_1_SQRT_2};
use std::sync::Arc;

use itertools::Itertools as _;
use kurbo::{Affine, BezPath, Circle, Line, PathEl, Point, Shape as _, Triangle, Vec2};
use linesweeper::{BinaryOp, FillRule};
use nonempty_collections::{NEVec, nev};
use parlance::FontFamily;
use rasm_linework::{Color, Element, FontSize, FontStyle, FontWeight, FontWidth, LineworkError, Opacity, Path, Pen, Placement, Region, Tolerance, Width, regions};
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

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Head {
    Arrow { angle: Angle, arrowhead: Arrowhead },
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
    pub weight: FontWeight = FontWeight::try_new(parlance::FontWeight::NORMAL.value()).ok().unwrap(),
    pub width: FontWidth = FontWidth::try_new(parlance::FontWidth::NORMAL.ratio()).ok().unwrap(),
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
    Table(tables::Table),
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
    #[error("family {0} holds no registered face")]
    Family(Arc<str>),
    #[error("font holds no readable face at index {0}")]
    Face(u32),
    #[error("glyph {glyph} draws no outline")]
    Outline { glyph: u32, source: ScaleError },
    #[error("geometry spans no length or direction")]
    Degenerate,
    #[error("symbol paint {0} lies outside the linework model")]
    Unsupported(symbols::Feature),
    #[error("symbol holds no text slot {0}")]
    Slot(Arc<str>),
    #[error("cell at row {row}, column {column} spans past the last column")]
    Cell { row: usize, column: usize },
    #[error("item {index}: {}", errors.iter().format("; "))]
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
    let mask = match drawn.as_slice() {
        [] => Region::default(),
        [first, rest @ ..] => regions::boolean(&first.mask, rest.iter().map(|graphics| &graphics.mask), BinaryOp::Union, style.tolerance).map_err(|error| nev![AnnotationError::from(error)])?,
    };
    Ok(Graphics { elements: drawn.into_iter().flat_map(|graphics| graphics.elements).collect(), mask })
}

pub(crate) fn terminated(style: &Style, tip: Point, toward: Vec2) -> Result<Element, AnnotationError> {
    let frame = (toward.is_finite() && toward != Vec2::ZERO).then(|| Affine::rotate(toward.atan2()).then_translate(tip.to_vec2())).ok_or(AnnotationError::Degenerate)?;
    let Terminator { size, head } = style.terminator;
    let length = size.into_inner();
    let tolerance = style.tolerance.into_inner();
    let filled = |shape: BezPath| Region::new(&(frame * shape), FillRule::NonZero, style.tolerance).map(|region| Element::Fill { region, color: style.pen.color.clone(), opacity: style.pen.opacity });
    let stroked = |shape: BezPath| Path::try_new(frame * shape).map(|path| Element::Stroke { path, pen: style.pen.clone(), linetype: None }).map_err(LineworkError::from);
    let disc = Circle::new(Point::ORIGIN, length * 0.5);
    match head {
        Head::Arrow { angle, arrowhead } => {
            let spread = length * (angle.get::<radian>() * 0.5).tan();
            let [left, right] = [Point::new(-length, spread), Point::new(-length, -spread)];
            match arrowhead {
                Arrowhead::Filled => filled(Triangle::new(Point::ORIGIN, left, right).to_path(tolerance)),
                Arrowhead::Closed => stroked(Triangle::new(Point::ORIGIN, left, right).to_path(tolerance)),
                Arrowhead::Open => stroked(BezPath::from_vec(vec![PathEl::MoveTo(left), PathEl::LineTo(Point::ORIGIN), PathEl::LineTo(right)])),
            }
        }
        Head::Oblique => {
            let reach = length * 0.5 * FRAC_1_SQRT_2;
            stroked(Line::new((-reach, reach), (reach, -reach)).to_path(tolerance))
        }
        Head::Dot => filled(disc.to_path(tolerance)),
        Head::Origin => stroked(disc.to_path(tolerance)),
    }
    .map_err(AnnotationError::from)
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
