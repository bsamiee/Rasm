use core::{f64::consts::FRAC_PI_2, iter, num::NonZero};
use std::sync::Arc;

use color::{AlphaColor, Srgb};
use colorgrad::Gradient;
use itertools::Itertools as _;
use kurbo::{Affine, Line, Point, Rect, Shape as _, Size, Vec2};
use linesweeper::FillRule;
use nonempty_collections::NEVec;
use rasm_drawing::{Anchor, Baseline, Color, Element, Linetype, Opacity, Pattern, Pen, Placement, Region, Target};

use crate::{
    DraftingError, Geometry, Graphics, bounds, colored, edges, filled, graduated, indexed, joined,
    notation::{self, Decimal, Sign},
    stroked,
    symbols::{self, Definition, Instance},
    text::{Label, Typesetter, labelled},
    transformed,
};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq)]
pub enum Swatch {
    Line {
        pen: Pen,
        linetype: Option<Linetype> = None,
    },
    Fill {
        color: Color,
        opacity: Opacity = Opacity::OPAQUE,
    },
    Hatch {
        pattern: Pattern,
        pen: Pen,
    },
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

pub(crate) fn draw(typesetter: &mut Typesetter, style: &crate::Style, placement: Placement, legend: &Legend) -> Result<Graphics, NEVec<DraftingError>> {
    let origin = *placement * legend.at;
    let accuracy = style.tolerance.into_inner();
    let gap = style.gap.into_inner();
    match &legend.layout {
        Layout::Key(Key { entries, swatch, columns }) => {
            (swatch.is_finite() && swatch.width > 0.0 && swatch.height >= 0.0).ok_or(DraftingError::Geometry(Geometry::Extent))?;
            let labels = entries.iter().map(|entry| labelled(typesetter, style, &entry.label, Point::ORIGIN, Vec2::new(1.0, 0.0), Anchor::Start, Baseline::Middle)).collect::<Vec<_>>();
            let sizes = labels.iter().map(|set| set.as_ref().map_or(Size::ZERO, |graphics| bounds(graphics, Affine::IDENTITY).size())).collect::<Vec<_>>();
            let depth = entries.len().get().div_ceil(NonZero::<usize>::from(*columns).get());
            let lefts = edges(sizes.chunks(depth).map(|column| swatch.width + gap + column.iter().map(|size| size.width).fold(0.0, f64::max))).flat_map(|left| iter::repeat_n(left + origin.x, depth));
            let middles = sizes.chunks(depth).flat_map(|column| {
                column.iter().scan(0.0, move |top, size| {
                    let height = swatch.height.max(gap.mul_add(-2.0, size.height));
                    let middle = height.mul_add(0.5, origin.y + *top);
                    *top += height + gap;
                    Some(middle)
                })
            });
            let rects = iter::zip(lefts, middles).map(|(left, middle)| Rect::from_center_size((swatch.width.mul_add(0.5, left), middle), *swatch));
            let mut entered = |Entry { swatches, outline, label: _ }: &Entry, set: Result<Graphics, NEVec<DraftingError>>, rect: Rect| {
                let clearance =
                    Region::new(rect.inflate(gap, gap), FillRule::NonZero, style.tolerance).map(|region| Graphics { masks: vec![region], .. }).map_err(|error| NEVec::from(DraftingError::from(error)));
                let samples = swatches.iter().map(|sample| swatched(typesetter, style, placement, sample, rect));
                let boxed = outline.as_ref().map(|pen| joined([stroked(rect.to_path(accuracy), pen, None)], []));
                let text = set.and_then(|graphics| transformed(graphics, Affine::translate((rect.x1 + gap, rect.center().y))));
                joined([], iter::once(clearance).chain(samples).chain(boxed).chain(iter::once(text)))
            };
            indexed(iter::zip(entries.iter(), labels).zip(rects).map(|((entry, set), rect)| entered(entry, set, rect)))
        }
        Layout::Ramp(Ramp { gradient, bands, ticks, decimal, extent }) => {
            let [low, high] = <[_; 2]>::from(gradient.domain()).map(f64::from);
            let span = high - low;
            (span.is_finite() && span > 0.0).ok_or(DraftingError::Geometry(Geometry::Domain))?;
            (extent.is_finite() && extent.width > 0.0 && extent.height > 0.0).ok_or(DraftingError::Geometry(Geometry::Extent))?;
            let (basis, length, thickness, anchor, baseline) = if extent.width >= extent.height {
                (Affine::translate(origin.to_vec2()), extent.width, extent.height, Anchor::Middle, Baseline::Top)
            } else {
                (Affine::rotate(-FRAC_PI_2).then_translate(Vec2::new(origin.x, origin.y + extent.height)), extent.height, extent.width, Anchor::Start, Baseline::Middle)
            };
            let count = bands.get();
            let fills = (0..=count).map(|band| length * f64::from(band) / f64::from(count)).tuple_windows().map(|(near, far)| {
                colored(AlphaColor::<Srgb>::new(gradient.at(span.mul_add(near.midpoint(far) / length, low).cast::<f32>()).clamp().to_array()))
                    .and_then(|(color, opacity)| filled(basis * Rect::new(near, 0.0, far, thickness).to_path(accuracy), FillRule::NonZero, color, opacity, style.tolerance))
            });
            let along = |value: f64| length * (value - low) / span;
            joined(
                fills,
                [graduated(
                    typesetter,
                    style,
                    basis,
                    Size::new(length, thickness),
                    ticks.iter().map(|&value| (along(value), Some(notation::decimal(decimal, value, Sign::Negative)))),
                    (Vec2::new(1.0, 0.0), anchor, baseline),
                )],
            )
        }
    }
}

pub(crate) fn swatched(typesetter: &mut Typesetter, style: &crate::Style, placement: Placement, sample: &Swatch, rect: Rect) -> Result<Graphics, NEVec<DraftingError>> {
    let accuracy = style.tolerance.into_inner();
    let single = |element: Element| Graphics { elements: vec![element], .. };
    match sample {
        Swatch::Line { pen, linetype } => {
            let middle = rect.center().y;
            stroked(Line::new((rect.x0, middle), (rect.x1, middle)).to_path(accuracy), pen, linetype.clone()).map(single).map_err(NEVec::from)
        }
        Swatch::Fill { color, opacity } => filled(rect, FillRule::NonZero, color.clone(), *opacity, style.tolerance).map(single).map_err(NEVec::from),
        Swatch::Hatch { pattern, pen } => Region::new(rect, FillRule::NonZero, style.tolerance)
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
            .map_err(|error| DraftingError::from(error).into()),
        Swatch::Symbol(definition) => {
            let extent = definition.extent;
            let scale = (rect.width() / extent.width()).min(rect.height() / extent.height());
            symbols::draw(typesetter, style, Placement::default(), &Instance { definition: Arc::clone(definition), at: rect.center() - extent.center().to_vec2() * scale, scale, .. })
        }
    }
}
