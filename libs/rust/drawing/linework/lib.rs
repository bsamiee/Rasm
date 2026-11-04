#![feature(cmp_minmax, const_result_trait_fn, const_trait_impl, default_field_values, impl_trait_in_assoc_type)]

use core::{f64::consts::PI, iter, range::Range};
use std::sync::Arc;

use color::{OpaqueColor, Srgb};
use geo_types::{Coord, LineString, MultiPolygon, Polygon};
use itertools::Itertools as _;
use kurbo::{Affine, BezPath, Cap, Join, ParamCurveDeriv as _, PathEl, PathSeg, Point, Rect, Shape, Vec2, flatten};
use linebender_resource_handle::FontData;
use linesweeper::{
    Error, FillRule,
    topology::{Contour, ContourIdx, Topology, WindingNumber},
};
use nonempty_collections::{IntoNonEmptyIterator as _, NEVec, NonEmptyIterator as _};
use parlance::{FontFamily, FontFeature, FontVariation};
use stacksafe::StackSafe;
use validated::Validated;

// --- [TYPES] ---------------------------------------------------------------------------

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

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, PartialOrd, Ord, enum_map::Enum)]
pub enum EdgeKind {
    Crease,
    Silhouette,
    Boundary,
    Cut,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, PartialOrd, Ord, enum_map::Enum)]
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

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, derive_more::Display)]
pub enum Dash {
    #[display("dash array")]
    Array,
    #[display("dash offset")]
    Offset,
}

// --- [MODELS] --------------------------------------------------------------------------

#[nutype::nutype(validate(finite, greater = 0.0), derive(Debug, Clone, Copy, PartialEq, PartialOrd))]
pub struct Tolerance(f64);

#[nutype::nutype(validate(finite, greater_or_equal = 0.0), derive(Debug, Clone, Copy, PartialEq, PartialOrd))]
pub struct Width(f64);

#[nutype::nutype(const_fn, validate(finite, greater_or_equal = 1.0), derive(Debug, Clone, Copy, PartialEq, PartialOrd))]
pub struct MiterLimit(f64);

impl MiterLimit {
    pub const DEFAULT: Self = Self::try_new(4.0_f64).ok().unwrap();
}

#[nutype::nutype(const_fn, validate(finite, greater_or_equal = 0.0, less_or_equal = 1.0), derive(Debug, Clone, Copy, PartialEq, PartialOrd))]
pub struct Opacity(f64);

impl Opacity {
    pub const OPAQUE: Self = Self::try_new(1.0_f64).ok().unwrap();
}

#[nutype::nutype(validate(finite, greater = 0.0), derive(Debug, Clone, Copy, PartialEq, PartialOrd))]
pub struct FontSize(f64);

#[nutype::nutype(const_fn, validate(finite, greater_or_equal = 1.0, less_or_equal = 1000.0), derive(Debug, Clone, Copy, PartialEq, PartialOrd))]
pub struct FontWeight(f32);

impl FontWeight {
    pub const NORMAL: Self = Self::try_new(parlance::FontWeight::NORMAL.value()).ok().unwrap();
}

#[nutype::nutype(const_fn, validate(finite, greater = 0.0), derive(Debug, Clone, Copy, PartialEq, PartialOrd))]
pub struct FontWidth(f32);

impl FontWidth {
    pub const NORMAL: Self = Self::try_new(parlance::FontWidth::NORMAL.ratio()).ok().unwrap();
}

#[nutype::nutype(validate(finite, greater_or_equal = -90.0, less_or_equal = 90.0), derive(Debug, Clone, Copy, PartialEq, PartialOrd))]
pub struct Slant(f32);

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum FontStyle {
    Normal,
    Italic,
    Oblique { angle: Option<Slant> },
}

impl From<FontStyle> for parlance::FontStyle {
    fn from(value: FontStyle) -> Self {
        match value {
            FontStyle::Normal => Self::Normal,
            FontStyle::Italic => Self::Italic,
            FontStyle::Oblique { angle } => Self::Oblique(angle.map(Slant::into_inner)),
        }
    }
}

impl From<FontWeight> for parlance::FontWeight {
    fn from(value: FontWeight) -> Self { Self::new(value.into_inner()) }
}

impl From<FontWidth> for parlance::FontWidth {
    fn from(value: FontWidth) -> Self { Self::from_ratio(value.into_inner()) }
}

#[nutype::nutype(validate(finite), derive(Debug, Clone, Copy, PartialEq, PartialOrd))]
pub struct DashOffset(f64);

#[nutype::nutype(validate(finite, greater_or_equal = 0.0, less_or_equal = PI), derive(Debug, Clone, Copy, PartialEq, PartialOrd))]
pub struct Deflection(f64);

#[nutype::nutype(validate(with = invertible, error = LineworkError), default = Affine::IDENTITY, derive(Debug, Clone, Copy, PartialEq, Deref, Default))]
pub struct Placement(Affine);

#[nutype::nutype(validate(with = finite, error = Error), derive(Debug, Clone, PartialEq, Deref))]
pub struct Path(BezPath);

#[nutype::nutype(
    sanitize(with = |pattern: NEVec<f64>| if pattern.len().get().is_multiple_of(2) { pattern } else { pattern.nonempty_iter().chain(pattern.iter()).copied().collect() }),
    validate(predicate = |pattern: &NEVec<f64>| {
        let period = pattern.iter().sum::<f64>();
        pattern.iter().all(|dash| *dash >= 0.0_f64) && period.is_finite() && period > 0.0_f64
    }),
    derive(Debug, Clone, PartialEq, Deref)
)]
pub struct Dashes(NEVec<f64>);

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Alignment {
    Start { dash_offset: DashOffset },
    Center,
    Corners { angle: Deflection },
}

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
    pub opacity: Opacity = Opacity::OPAQUE,
    pub cap: Cap = Cap::Round,
    pub join: Join = Join::Round,
    pub miter_limit: MiterLimit = MiterLimit::DEFAULT,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Linetype {
    pub dashes: Dashes,
    pub alignment: Alignment,
    pub target: Target,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Descriptor {
    pub angle: f64,
    pub origin: Point,
    pub delta: Vec2,
    pub dashes: Option<Dashes>,
}

#[nutype::nutype(
    validate(predicate = |descriptor: &Descriptor| descriptor.angle.is_finite() && descriptor.origin.is_finite() && descriptor.delta.is_finite() && descriptor.delta.y != 0.0_f64),
    derive(Debug, Clone, PartialEq, Deref)
)]
pub struct Family(Descriptor);

#[derive(Debug, Clone, PartialEq)]
pub struct Pattern {
    pub families: NEVec<Family>,
    pub target: Target,
}

#[derive(Debug, Clone, Default, PartialEq, derive_more::Deref)]
#[deref(forward)]
pub struct Region(Arc<[Contour]>);

impl Region {
    pub fn new(shape: impl Shape, fill_rule: FillRule, tolerance: Tolerance) -> Result<Self, LineworkError> {
        let closed = shape
            .into_path(tolerance.into_inner())
            .subpaths()
            .flat_map(|elements| iter::chain(elements.iter().copied(), (!matches!(elements.last(), Some(PathEl::ClosePath))).then_some(PathEl::ClosePath)))
            .collect::<BezPath>();
        finite(&closed)?;
        swept::<i32>(iter::once((&closed, ())), tolerance, |&winding| fill_rule.is_inside(winding))
    }

    pub fn transform(&self, affine: Placement) -> Result<Self, LineworkError> {
        let mirrored = affine.determinant().is_sign_negative();
        let moved = self
            .iter()
            .map(|Contour { path, parent, outer }| {
                let moved = *affine * path;
                Contour { path: if mirrored { moved.reverse_subpaths() } else { moved }, parent: *parent, outer: *outer }
            })
            .collect::<Arc<[_]>>();
        moved.iter().try_for_each(|contour| finite(&contour.path))?;
        Ok(Self(moved))
    }
}

impl Shape for Region {
    type PathElementsIter<'iter> = impl Iterator<Item = PathEl> + 'iter;

    fn path_elements(&self, _tolerance: f64) -> Self::PathElementsIter<'_> { self.iter().flat_map(|contour| contour.path.iter()) }

    fn area(&self) -> f64 { self.iter().map(|contour| contour.path.area()).sum() }

    fn perimeter(&self, accuracy: f64) -> f64 { self.iter().map(|contour| contour.path.perimeter(accuracy)).sum() }

    fn winding(&self, pt: Point) -> i32 { self.iter().map(|contour| contour.path.winding(pt)).sum() }

    fn bounding_box(&self) -> Rect { self.iter().map(|contour| contour.path.bounding_box()).reduce(|left, right| left.union(right)).unwrap_or_default() }

    fn as_rect(&self) -> Option<Rect> {
        match self.0.as_ref() {
            [Contour { path, .. }] => {
                let bounds = path.bounding_box();
                path.segments()
                    .all(|segment| match segment {
                        #[expect(clippy::float_cmp, reason = "Shape::as_rect requires exact axis alignment with native signed-zero equality")]
                        PathSeg::Line(kurbo::Line { p0, p1 }) => (p0.x == p1.x && [bounds.x0, bounds.x1].contains(&p0.x)) || (p0.y == p1.y && [bounds.y0, bounds.y1].contains(&p0.y)),
                        PathSeg::Quad(_) | PathSeg::Cubic(_) => false,
                    })
                    .then_some(bounds)
            }
            [] | [_, _, ..] => None,
        }
    }
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, PartialOrd, Ord, enum_map::Enum)]
pub struct Class {
    pub kind: EdgeKind,
    pub visibility: Visibility,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Edge {
    pub path: Path,
    pub class: Class,
    pub depth: f64,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Text {
    pub content: Arc<str>,
    pub family: FontFamily<'static>,
    pub size: FontSize,
    pub placement: Placement,
    pub weight: FontWeight = FontWeight::NORMAL,
    pub width: FontWidth = FontWidth::NORMAL,
    pub style: FontStyle = FontStyle::Normal,
    pub anchor: Anchor = Anchor::Start,
    pub baseline: Baseline = Baseline::Alphabetic,
    pub max_advance: Option<Width> = None,
    pub line_height: Option<Width> = None,
    pub features: Vec<FontFeature> = Vec::new(),
    pub spans: Vec<Span> = Vec::new(),
    pub target: Target = Target::Model,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Span {
    pub range: Range<usize>,
    pub weight: Option<FontWeight> = None,
    pub style: Option<FontStyle> = None,
    pub features: Vec<FontFeature> = Vec::new(),
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Glyph {
    pub id: u32,
    pub cluster: Range<usize>,
    pub advance: Vec2,
    pub offset: Vec2,
}

#[derive(Debug, Clone, PartialEq)]
pub struct GlyphRun {
    pub font: FontData,
    pub family: Arc<str>,
    pub size: FontSize,
    pub variations: Arc<[FontVariation]>,
    pub glyphs: Vec<Glyph>,
    pub text: Arc<str>,
    pub placement: Placement,
    pub target: Target,
}

#[derive(Debug, Clone, PartialEq, Eq, Hash)]
pub enum Content {
    Svg(Arc<[u8]>),
    Raster(Arc<[u8]>),
    Pdf { bytes: Arc<[u8]>, page: usize },
}

#[derive(Debug, Clone, PartialEq)]
pub enum Element {
    Stroke { path: Path, pen: Pen, linetype: Option<Linetype> },
    Fill { region: Region, color: Color, opacity: Opacity = Opacity::OPAQUE },
    Hatch { region: Region, pattern: Pattern, placement: Placement, pen: Pen },
    Text { text: Text, color: Color, opacity: Opacity = Opacity::OPAQUE },
    Glyphs { run: GlyphRun, color: Color, opacity: Opacity = Opacity::OPAQUE },
    Placed { content: Content, placement: Placement, opacity: Opacity = Opacity::OPAQUE },
    Clip { region: Region, keep: Side, elements: StackSafe<Vec<Self>> },
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum LineworkError {
    #[error(transparent)]
    Sweep(#[from] Error),
    #[error("affine [{}] is singular or not finite", .0.as_coeffs().iter().format(", "))]
    Placement(Affine),
    #[error("{0} leaves its range once scaled")]
    Scaled(Dash),
    #[error("hatch family {family} places its rows {spacing} apart on paper, under the tolerance")]
    Spacing { family: usize, spacing: f64 },
    #[error("element {index}: {}", errors.iter().format("; "))]
    Element { index: usize, errors: NEVec<StackSafe<Self>> },
    #[error("layer {name}: {}", errors.iter().format("; "))]
    Layer { name: Arc<str>, errors: NEVec<StackSafe<Self>> },
}

// --- [OPERATIONS] ----------------------------------------------------------------------

#[stacksafe::stacksafe]
pub fn transform(elements: Vec<Element>, affine: Placement) -> Result<Vec<Element>, NEVec<LineworkError>> {
    let scale = affine.determinant().abs().sqrt();
    let placed = |placement: Placement, target: Target| {
        Validated::from(Placement::try_new(match target {
            Target::Model => *affine * *placement,
            Target::Drafting => *affine * *placement * Affine::scale(scale.recip()),
        }))
    };
    let lengthened = |linetype: Linetype| match linetype.target {
        Target::Drafting => Validated::Good(linetype),
        Target::Model => scaled(linetype, scale),
    };
    let moved = |region: &Region| Validated::from(region.transform(affine));
    elements
        .into_iter()
        .enumerate()
        .map(|(index, element)| {
            match element {
                Element::Stroke { path, pen, linetype } => Validated::from({
                    let mut path = path.into_inner();
                    path.apply_affine(*affine);
                    Path::try_new(path).map_err(LineworkError::from)
                })
                .map2(linetype.map_or(Validated::Good(None), |linetype| lengthened(linetype).map(Some)), |path, linetype| Element::Stroke { path, pen, linetype }),
                Element::Fill { region, color, opacity } => moved(&region).map(|region| Element::Fill { region, color, opacity }),
                Element::Hatch { region, pattern, placement, pen } => moved(&region).map2(placed(placement, pattern.target), |region, placement| Element::Hatch { region, pattern, placement, pen }),
                Element::Text { text, color, opacity } => placed(text.placement, text.target).map(|placement| Element::Text { text: Text { placement, ..text }, color, opacity }),
                Element::Glyphs { run, color, opacity } => placed(run.placement, run.target).map(|placement| Element::Glyphs { run: GlyphRun { placement, ..run }, color, opacity }),
                Element::Placed { content, placement, opacity } => placed(placement, Target::Model).map(|placement| Element::Placed { content, placement, opacity }),
                Element::Clip { region, keep, elements: grouped } =>
                    moved(&region).map2(transform(grouped.into_inner(), affine).map_or_else(Validated::Fail, Validated::Good), |region, inner| Element::Clip { region, keep, elements: inner.into() }),
            }
            .ok()
            .map_err(|errors| LineworkError::Element { index, errors: errors.into_nonempty_iter().map(StackSafe::new).collect() })
        })
        .collect::<Validated<Vec<_>, _>>()
        .ok()
}

#[stacksafe::stacksafe]
pub fn rebase(elements: Vec<Element>, viewport: Placement) -> Result<Vec<Element>, NEVec<LineworkError>> {
    let scale = viewport.determinant().abs().sqrt().recip();
    let placed = |placement: Placement, target: Target| match target {
        Target::Model => Ok(placement),
        Target::Drafting => Placement::try_new(*placement * Affine::scale(scale)),
    };
    elements
        .into_iter()
        .enumerate()
        .map(|(index, element)| {
            match element {
                Element::Stroke { path, pen, linetype } => match linetype {
                    None => Validated::Good(None),
                    Some(linetype @ Linetype { dashes: _, alignment: _, target: Target::Model }) => Validated::Good(Some(linetype)),
                    Some(linetype @ Linetype { dashes: _, alignment: _, target: Target::Drafting }) => scaled(Linetype { target: Target::Model, ..linetype }, scale).map(Some),
                }
                .map(|linetype| Element::Stroke { path, pen, linetype }),
                Element::Text { text, color, opacity } =>
                    Validated::from(placed(text.placement, text.target)).map(|placement| Element::Text { text: Text { placement, target: Target::Model, ..text }, color, opacity }),
                Element::Glyphs { run, color, opacity } =>
                    Validated::from(placed(run.placement, run.target)).map(|placement| Element::Glyphs { run: GlyphRun { placement, target: Target::Model, ..run }, color, opacity }),
                Element::Hatch { region, pattern, placement, pen } =>
                    Validated::from(placed(placement, pattern.target)).map(|placement| Element::Hatch { region, pattern: Pattern { target: Target::Model, ..pattern }, placement, pen }),
                Element::Clip { region, keep, elements: grouped } =>
                    rebase(grouped.into_inner(), viewport).map_or_else(Validated::Fail, Validated::Good).map(|grouped| Element::Clip { region, keep, elements: grouped.into() }),
                element @ (Element::Fill { .. } | Element::Placed { .. }) => Validated::Good(element),
            }
            .ok()
            .map_err(|errors| LineworkError::Element { index, errors: errors.into_nonempty_iter().map(StackSafe::new).collect() })
        })
        .collect::<Validated<Vec<_>, _>>()
        .ok()
}

fn scaled(Linetype { dashes, alignment, target }: Linetype, scale: f64) -> Validated<Linetype, LineworkError> {
    Validated::from(Dashes::try_new(dashes.nonempty_iter().map(|dash| dash * scale).collect()).map_err(|DashesError::PredicateViolated| LineworkError::Scaled(Dash::Array))).map2(
        Validated::from(
            match alignment {
                Alignment::Start { dash_offset } => DashOffset::try_new(dash_offset.into_inner() * scale).map(|dash_offset| Alignment::Start { dash_offset }),
                Alignment::Center | Alignment::Corners { angle: _ } => Ok(alignment),
            }
            .map_err(|DashOffsetError::FiniteViolated| LineworkError::Scaled(Dash::Offset)),
        ),
        |dashes, alignment| Linetype { dashes, alignment, target },
    )
}

fn invertible(affine: &Affine) -> Result<(), LineworkError> { (affine.is_finite() && affine.determinant().is_normal()).ok_or(LineworkError::Placement(*affine)) }

fn swept<'path, W>(paths: impl IntoIterator<Item = (&'path BezPath, W::Tag), IntoIter: Clone>, tolerance: Tolerance, inside: impl Fn(&W) -> bool) -> Result<Region, LineworkError>
where W: WindingNumber {
    let paths = paths.into_iter();
    let bounds = paths.clone().map(|(path, _)| path.bounding_box()).reduce(|left, right| left.union(right)).unwrap_or_default();
    let magnitude = bounds.min_x().min(bounds.min_y()).abs().max(bounds.max_x().max(bounds.max_y()).abs());
    Ok(Region(Topology::<W>::from_paths(paths, tolerance.into_inner().max(magnitude * (f64::EPSILON * 64.0_f64))).map_err(Error::from)?.contours(inside).into_contours().collect()))
}

fn finite(path: &BezPath) -> Result<(), Error> {
    match path {
        path if path.is_finite() => Ok(()),
        path if path.is_nan() => Err(Error::NaN),
        _ => Err(Error::Infinity),
    }
}

fn tangents(segment: &PathSeg) -> Option<(Vec2, Vec2)> {
    let derivative = segment.to_cubic().deriv();
    let tangent = |controls: [Point; 3]| controls.into_iter().map(Point::to_vec2).find(|direction| *direction != Vec2::ZERO);
    tangent([derivative.p0, derivative.p1, derivative.p2]).zip(tangent([derivative.p2, derivative.p1, derivative.p0]))
}

fn deflection(incoming: Vec2, outgoing: Vec2) -> f64 { incoming.cross(outgoing).atan2(incoming.dot(outgoing)).abs() }

fn coord(point: Point) -> Coord<f64> { Coord::from(<(f64, f64)>::from(point)) }

fn flattened(elements: impl IntoIterator<Item = PathEl>, tolerance: Tolerance) -> Vec<LineString<f64>> {
    let mut lines = Vec::new();
    flatten(elements, tolerance.into_inner(), |element| match element {
        PathEl::MoveTo(point) => lines.push(LineString::new(vec![coord(point)])),
        PathEl::LineTo(point) | PathEl::QuadTo(_, point) | PathEl::CurveTo(_, _, point) => lines.last_mut().into_iter().for_each(|line| line.0.push(coord(point))),
        PathEl::ClosePath => lines.last_mut().into_iter().for_each(LineString::close),
    });
    lines
}

fn polygons(region: &Region, tolerance: Tolerance) -> MultiPolygon<f64> {
    let ring = |path: &BezPath| flattened(path.iter(), tolerance).into_iter().flat_map(|line| line.0).collect::<LineString<f64>>();
    let holes = region.iter().fold(vec![Vec::new(); region.len()], |mut holes, contour| match contour {
        Contour { path, parent: Some(ContourIdx(parent)), outer: false } => {
            holes.get_mut(*parent).into_iter().for_each(|group| group.push(ring(path)));
            holes
        }
        Contour { path: _, parent: _, outer: _ } => holes,
    });
    region.iter().zip(holes).filter_map(|(contour, holes)| contour.outer.then(|| Polygon::new(ring(&contour.path), holes))).collect()
}

fn path(lines: impl IntoIterator<Item = LineString<f64>>) -> BezPath {
    lines
        .into_iter()
        .flat_map(|line| {
            let closed = line.0.len() > 2 && line.is_closed();
            let LineString(coords) = line;
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
