#![feature(default_field_values)]

use std::{iter, ops::Range, sync::Arc};

use color::{OpaqueColor, Srgb};
use geo_types::{Coord, LineString};
use kurbo::{Affine, BezPath, Cap, Join, PathEl, Point, Rect, Shape, Vec2, flatten};
use linesweeper::{
    Error, FillRule,
    topology::{Contour, Topology, WindingNumber},
};
use nonempty_collections::NEVec;

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Alignment {
    Start(f64),
    Center,
    Corners(f64),
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum Side {
    Inside,
    Outside,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum Target {
    Model,
    Drafting,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, PartialOrd, Ord)]
pub enum EdgeKind {
    Crease,
    Silhouette,
    Boundary,
    Cut,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, PartialOrd, Ord)]
pub enum Visibility {
    Visible,
    Hidden,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, PartialOrd, Ord)]
pub enum Anchor {
    Start,
    Middle,
    End,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, PartialOrd, Ord)]
pub enum Baseline {
    Top,
    Middle,
    Alphabetic,
    Bottom,
}

// --- [MODELS] --------------------------------------------------------------------------

#[nutype::nutype(validate(finite, greater = 0.0), derive(Debug, Clone, Copy, PartialEq, PartialOrd))]
pub struct Tolerance(f64);

#[nutype::nutype(validate(finite, greater_or_equal = 0.0), derive(Debug, Clone, Copy, PartialEq, PartialOrd))]
pub struct Width(f64);

#[nutype::nutype(validate(finite, greater = 0.0), derive(Debug, Clone, Copy, PartialEq, PartialOrd))]
pub struct FontSize(f64);

#[nutype::nutype(
    validate(predicate = |pattern: &NEVec<f64>| {
        let period = pattern.iter().sum::<f64>();
        pattern.iter().all(|dash| *dash >= 0.0_f64) && period.is_finite() && period > 0.0_f64
    }),
    derive(Debug, Clone, PartialEq, Deref)
)]
pub struct Dashes(NEVec<f64>);

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub struct Cmyk {
    pub cyan: u8,
    pub magenta: u8,
    pub yellow: u8,
    pub black: u8,
}

#[derive(Debug, Clone, PartialEq)]
pub enum Color {
    Srgb(OpaqueColor<Srgb>),
    Cmyk(Cmyk),
    Spot { name: Arc<str>, tint: u8, alternate: Cmyk },
}

#[derive(Debug, Clone, PartialEq)]
pub struct Pen {
    pub width: Width,
    pub color: Color,
    pub cap: Cap = Cap::Round,
    pub join: Join = Join::Round,
    pub miter_limit: f64 = 4.0,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Linetype {
    pub dashes: Dashes,
    pub alignment: Alignment,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Family {
    pub angle: f64,
    pub origin: Point,
    pub delta: Vec2,
    pub dashes: Option<Dashes>,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Pattern {
    pub families: NEVec<Family>,
    pub target: Target,
}

#[derive(Debug, Clone, Default, PartialEq, derive_more::Deref)]
#[deref(forward)]
pub struct Region(Vec<Contour>);

impl Region {
    pub fn new(shape: &impl Shape, fill_rule: FillRule, tolerance: Tolerance) -> Result<Self, LineworkError> {
        let closed = shape
            .path_elements(tolerance.into_inner())
            .collect::<BezPath>()
            .subpaths()
            .flat_map(|elements| iter::chain(elements.iter().copied(), (!matches!(elements.last(), Some(PathEl::ClosePath))).then_some(PathEl::ClosePath)))
            .collect::<BezPath>();
        swept::<i32>(vec![(&closed, ())], tolerance, |&winding| fill_rule.is_inside(winding))
    }

    #[must_use]
    pub fn transform(self, affine: Affine) -> Self {
        match affine.determinant() {
            0.0_f64 => Self::default(),
            determinant => Self(
                self.0
                    .into_iter()
                    .map(|Contour { path, parent, outer }| {
                        let moved = affine * path;
                        Contour { path: if determinant.is_sign_negative() { moved.reverse_subpaths() } else { moved }, parent, outer }
                    })
                    .collect(),
            ),
        }
    }
}

#[derive(Debug, Clone, PartialEq)]
pub struct Edge {
    pub path: BezPath,
    pub kind: EdgeKind,
    pub visibility: Visibility,
    pub depth: f64,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Text {
    pub content: Arc<str>,
    pub family: Arc<str>,
    pub size: FontSize,
    pub placement: Affine,
    pub anchor: Anchor = Anchor::Start,
    pub baseline: Baseline = Baseline::Alphabetic,
    pub width: Option<f64> = None,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Glyph {
    pub id: u32,
    pub cluster: Range<usize>,
    pub advance: Vec2,
    pub offset: Vec2,
}

#[derive(Debug, Clone, PartialEq)]
pub struct GlyphRun {
    pub font: Arc<[u8]>,
    pub index: u32,
    pub family: Arc<str>,
    pub size: FontSize,
    pub variations: Arc<[([u8; 4], f32)]>,
    pub glyphs: Vec<Glyph>,
    pub text: Arc<str>,
    pub placement: Affine,
}

#[derive(Debug, Clone, PartialEq, Eq, Hash)]
pub enum Content {
    Svg(Arc<[u8]>),
    Raster(Arc<[u8]>),
    Pdf { bytes: Arc<[u8]>, page: usize },
}

#[derive(Debug, Clone, PartialEq)]
pub enum Element {
    Stroke { path: BezPath, pen: Pen, linetype: Option<Linetype> },
    Fill { region: Region, color: Color },
    Hatch { region: Region, pattern: Pattern, placement: Affine, pen: Pen },
    Text { text: Text, color: Color },
    Glyphs { run: GlyphRun, color: Color },
    Placed { content: Content, placement: Affine },
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum LineworkError {
    #[error(transparent)]
    Sweep(#[from] Error),
    #[error("element {index} has a path the sweep refuses")]
    Element { index: usize, source: Error },
}

// --- [OPERATIONS] ----------------------------------------------------------------------

#[must_use]
pub fn transform(element: Element, affine: Affine) -> Element {
    match element {
        Element::Stroke { path, pen, linetype } => Element::Stroke { path: affine * path, pen, linetype },
        Element::Fill { region, color } => Element::Fill { region: region.transform(affine), color },
        Element::Hatch { region, pattern: pattern @ Pattern { families: _, target: Target::Model }, placement, pen } =>
            Element::Hatch { region: region.transform(affine), pattern, placement: affine * placement, pen },
        Element::Hatch { region, pattern: pattern @ Pattern { families: _, target: Target::Drafting }, placement, pen } =>
            Element::Hatch { region: region.transform(affine), pattern, placement: affine * placement * Affine::scale(affine.determinant().abs().sqrt().recip()), pen },
        Element::Text { text, color } => Element::Text { text: Text { placement: affine * text.placement, ..text }, color },
        Element::Glyphs { run, color } => Element::Glyphs { run: GlyphRun { placement: affine * run.placement, ..run }, color },
        Element::Placed { content, placement } => Element::Placed { content, placement: affine * placement },
    }
}

fn swept<W>(paths: Vec<(&BezPath, W::Tag)>, tolerance: Tolerance, inside: impl Fn(&W) -> bool) -> Result<Region, LineworkError>
where W: WindingNumber {
    paths.iter().try_for_each(|(path, _)| finite(path))?;
    Ok(Region(Topology::<W>::from_paths(paths, tolerance.into_inner()).map_err(Error::from)?.contours(inside).into_contours().collect()))
}

fn finite(path: &BezPath) -> Result<(), Error> {
    match (path.is_nan(), path.is_finite()) {
        (true, _) => Err(Error::NaN),
        (false, false) => Err(Error::Infinity),
        (false, true) => Ok(()),
    }
}

fn extent(region: &Region) -> Option<Rect> { region.iter().map(|contour| contour.path.bounding_box()).reduce(|left, right| left.union(right)) }

fn coord(point: Point) -> Coord<f64> { Coord::from(<(f64, f64)>::from(point)) }

fn flattened(path: &BezPath, tolerance: Tolerance) -> Vec<LineString<f64>> {
    let mut lines = Vec::new();
    flatten(path.iter(), tolerance.into_inner(), |element| match element {
        PathEl::MoveTo(point) => lines.push(LineString::new(vec![coord(point)])),
        PathEl::LineTo(point) | PathEl::QuadTo(_, point) | PathEl::CurveTo(_, _, point) => lines.last_mut().into_iter().for_each(|line| line.0.push(coord(point))),
        PathEl::ClosePath => lines.last_mut().into_iter().for_each(LineString::close),
    });
    lines
}

fn path(lines: impl IntoIterator<Item = LineString<f64>>) -> BezPath {
    lines
        .into_iter()
        .flat_map(|LineString(coords)| {
            let closed = coords.len() > 2 && coords.first() == coords.last();
            let kept = coords.len().strict_sub(usize::from(closed));
            let mut points = coords.into_iter().take(kept).map(|coord| Point::from(coord.x_y()));
            iter::chain(points.next().map(PathEl::MoveTo), points.map(PathEl::LineTo)).chain(closed.then_some(PathEl::ClosePath))
        })
        .collect()
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub mod cleanup;
pub mod hatches;
pub mod pages;
pub mod regions;
pub mod strokes;
