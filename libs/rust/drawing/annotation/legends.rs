use core::{f64::consts::FRAC_PI_2, iter, num::NonZero};
use std::sync::Arc;

use color::{AlphaColor, Srgb};
use colorgrad::Gradient;
use kurbo::{Affine, Line, Point, Rect, Shape as _, Size, Vec2};
use linesweeper::FillRule;
use nonempty_collections::NEVec;
use rasm_linework::{Anchor, Baseline, Color, Element, Linetype, Opacity, Pattern, Pen, Placement, Region, Target};
use validated::Validated;

use crate::{
    AnnotationError, Geometry, Graphics, bounds, colored, edges, filled, graduated, indexed,
    notation::{self, Decimal, Sign},
    stroked,
    symbols::{self, Definition, Instance},
    text::{Label, Typesetter, labelled},
    transformed, validated,
};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq)]
pub enum Swatch {
    Line { pen: Pen, linetype: Option<Linetype> = None },
    Fill { color: Color, opacity: Opacity = Opacity::OPAQUE },
    Hatch { pattern: Pattern, pen: Pen },
    Symbol(Arc<Definition>),
}

#[derive(Debug, Clone, PartialEq)]
pub struct Entry {
    pub swatches: NEVec<Swatch>,
    pub outline: Option<Pen> = None,
    pub label: Label,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Key {
    pub entries: NEVec<Entry>,
    pub swatch: Size,
    pub columns: NonZero<u16>,
}

#[derive(Clone, derive_more::Debug)]
pub struct Ramp {
    #[debug(skip)]
    pub gradient: Arc<dyn Gradient + Send + Sync>,
    pub bands: NonZero<u16>,
    pub ticks: Vec<f64>,
    pub decimal: Decimal,
    pub extent: Size,
}

#[derive(Debug, Clone)]
pub enum Layout {
    Key(Key),
    Ramp(Ramp),
}

#[derive(Debug, Clone)]
pub struct Legend {
    pub at: Point,
    pub layout: Layout,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub(crate) fn draw(typesetter: &mut Typesetter, style: &crate::Style, placement: Placement, legend: &Legend) -> Result<Graphics, NEVec<AnnotationError>> {
    let origin = *placement * legend.at;
    match &legend.layout {
        Layout::Key(key) => keyed(typesetter, style, placement, origin, key),
        Layout::Ramp(ramp) => ramped(typesetter, style, origin, ramp),
    }
}

fn keyed(typesetter: &mut Typesetter, style: &crate::Style, placement: Placement, origin: Point, key: &Key) -> Result<Graphics, NEVec<AnnotationError>> {
    let Key { entries, swatch, columns } = key;
    let gap = style.gap.into_inner();
    (swatch.is_finite() && swatch.width > 0.0 && swatch.height >= 0.0).ok_or(AnnotationError::Geometry(Geometry::Extent))?;
    let lettered = entries.iter().map(|entry| labelled(typesetter, style, &entry.label, Point::ORIGIN, Vec2::new(1.0, 0.0), Anchor::Start, Baseline::Middle)).collect::<Vec<_>>();
    let sizes = lettered.iter().map(|set| set.as_ref().map_or(Size::ZERO, |graphics| bounds(graphics).size())).collect::<Vec<_>>();
    let depth = entries.len().get().div_ceil(NonZero::<usize>::from(*columns).get());
    let lefts = edges(sizes.chunks(depth).map(|column| swatch.width + gap + column.iter().map(|size| size.width).fold(0.0, f64::max))).flat_map(|left| iter::repeat_n(left + origin.x, depth));
    let middles = sizes.chunks(depth).flat_map(|column| {
        let heights = column.iter().map(|size| swatch.height.max(gap.mul_add(-2.0, size.height))).collect::<Vec<_>>();
        let tops = edges(heights.iter().map(|height| height + gap)).collect::<Vec<_>>();
        iter::zip(tops, heights).map(|(top, height)| height.mul_add(0.5, origin.y + top))
    });
    let rects = iter::zip(lefts, middles).map(|(left, middle)| Rect::from_center_size((swatch.width.mul_add(0.5, left), middle), *swatch)).collect::<Vec<_>>();
    indexed(iter::zip(entries.iter(), lettered).zip(rects).map(|((entry, set), rect)| entered(typesetter, style, placement, entry, set, rect))).map(Graphics::from_iter)
}

fn entered(typesetter: &mut Typesetter, style: &crate::Style, placement: Placement, entry: &Entry, set: Result<Graphics, NEVec<AnnotationError>>, rect: Rect) -> Result<Graphics, NEVec<AnnotationError>> {
    let Entry { swatches, outline, label: _ } = entry;
    let accuracy = style.tolerance.into_inner();
    let gap = style.gap.into_inner();
    let clearance = Region::new(&rect.inflate(gap, gap).to_path(accuracy), FillRule::NonZero, style.tolerance).map_err(AnnotationError::from);
    let samples = swatches.iter().map(|sample| validated(swatched(typesetter, style, placement, sample, rect))).collect::<Validated<Vec<_>, _>>();
    let boxed = outline.as_ref().map(|pen| stroked(rect.to_path(accuracy), pen, None)).transpose();
    let text = set.and_then(|graphics| transformed(graphics, Affine::translate((rect.x1 + gap, rect.center().y))));
    Validated::from(clearance)
        .map4(samples, Validated::from(boxed), validated(text), |clearance, layers, boundary, moved| {
            iter::once(Graphics { elements: Vec::new(), masks: vec![clearance] }).chain(layers).chain(iter::once(Graphics { elements: boundary.into_iter().collect(), masks: Vec::new() })).chain(iter::once(moved)).collect()
        })
        .ok()
}

pub(crate) fn swatched(typesetter: &mut Typesetter, style: &crate::Style, placement: Placement, sample: &Swatch, rect: Rect) -> Result<Graphics, NEVec<AnnotationError>> {
    let accuracy = style.tolerance.into_inner();
    let single = |element: Element| Graphics { elements: vec![element], masks: Vec::new() };
    match sample {
        Swatch::Line { pen, linetype } => {
            let middle = rect.center().y;
            stroked(Line::new((rect.x0, middle), (rect.x1, middle)).to_path(accuracy), pen, linetype.clone()).map(single).map_err(NEVec::from)
        }
        Swatch::Fill { color, opacity } => filled(&rect.to_path(accuracy), FillRule::NonZero, color.clone(), *opacity, style.tolerance).map(single).map_err(NEVec::from),
        Swatch::Hatch { pattern, pen } => Region::new(&rect.to_path(accuracy), FillRule::NonZero, style.tolerance)
            .map(|region| {
                single(Element::Hatch {
                    region,
                    pattern: pattern.clone(),
                    placement: match pattern.target {
                        Target::Model => placement,
                        Target::Drafting => Placement::default(),
                    },
                    pen: pen.clone(),
                })
            })
            .map_err(|error| AnnotationError::from(error).into()),
        Swatch::Symbol(definition) => {
            let extent = definition.extent;
            let scale = (rect.width() / extent.width()).min(rect.height() / extent.height());
            symbols::draw(typesetter, style, Placement::default(), &Instance { definition: Arc::clone(definition), at: rect.center() - extent.center().to_vec2() * scale, scale, .. })
        }
    }
}

fn ramped(typesetter: &mut Typesetter, style: &crate::Style, origin: Point, ramp: &Ramp) -> Result<Graphics, NEVec<AnnotationError>> {
    let Ramp { gradient, bands, ticks, decimal, extent } = ramp;
    let accuracy = style.tolerance.into_inner();
    let (start, end) = gradient.domain();
    let [low, high] = [start, end].map(f64::from);
    let span = high - low;
    (span.is_finite() && span > 0.0).ok_or(AnnotationError::Geometry(Geometry::Domain))?;
    (extent.is_finite() && extent.width > 0.0 && extent.height > 0.0).ok_or(AnnotationError::Geometry(Geometry::Extent))?;
    let (basis, length, thickness, anchor, baseline) = if extent.width >= extent.height {
        (Affine::translate(origin.to_vec2()), extent.width, extent.height, Anchor::Middle, Baseline::Top)
    } else {
        (Affine::rotate(-FRAC_PI_2).then_translate(Vec2::new(origin.x, origin.y + extent.height)), extent.height, extent.width, Anchor::Start, Baseline::Middle)
    };
    let count = bands.get();
    let positions = (0..=count).map(|band| length * f64::from(band) / f64::from(count)).collect::<Vec<_>>();
    let fills = positions
        .array_windows()
        .map(|&[near, far]| {
            let colorgrad::Color { r, g, b, a } = gradient.at(span.mul_add(near.midpoint(far) / length, low).cast::<f32>()).clamp();
            colored(AlphaColor::<Srgb>::new([r, g, b, a])).and_then(|(color, opacity)| filled(&(basis * Rect::new(near, 0.0, far, thickness).to_path(accuracy)), FillRule::NonZero, color, opacity, style.tolerance))
        })
        .collect::<Validated<Vec<_>, _>>();
    let along = |value: f64| length * (value - low) / span;
    fills
        .map2(
            validated(graduated(typesetter, style, basis, Size::new(length, thickness), ticks.iter().map(|&value| (along(value), Some(notation::decimal(decimal, value, Sign::Negative)))), (Vec2::new(1.0, 0.0), anchor, baseline))),
            |bands, scale| [Graphics { elements: bands, masks: Vec::new() }, scale].into_iter().collect(),
        )
        .ok()
}
