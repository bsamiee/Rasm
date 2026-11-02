use core::{f64::consts::FRAC_PI_2, iter, num::NonZero};
use std::sync::Arc;

use color::{OpaqueColor, Srgb};
use colorgrad::Gradient;
use itertools::Itertools as _;
use kurbo::{Affine, BezPath, Line, Point, Rect, Shape as _, Size, Vec2};
use linesweeper::FillRule;
use nonempty_collections::{IntoNonEmptyIterator as _, NEVec, NonEmptyIterator as _, nev};
use rasm_linework::{Anchor, Baseline, Color, Element, Linetype, Opacity, OpacityError, Pattern, Pen, Placement, Region, Target, transform};
use validated::Validated;

use crate::{
    AnnotationError, Graphics, Style,
    notation::{self, Decimal},
    stroked,
    symbols::{self, Definition, Feature, Instance},
    text::{Label, Typesetter, labelled},
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
    pub swatch: Swatch,
    pub label: Label,
}

#[derive(Clone, derive_more::Debug)]
pub enum Legend {
    Key {
        at: Point,
        entries: NEVec<Entry>,
        swatch: Size,
        columns: NonZero<u16>,
    },
    Ramp {
        at: Point,
        #[debug(skip)]
        gradient: Arc<dyn Gradient + Send + Sync>,
        bands: NonZero<u16>,
        ticks: Vec<f64>,
        decimal: Decimal,
        extent: Size,
    },
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub(crate) fn draw(typesetter: &mut Typesetter, style: &Style, placement: Placement, legend: &Legend) -> Result<Graphics, NEVec<AnnotationError>> {
    let gap = style.gap.into_inner();
    let tolerance = style.tolerance.into_inner();
    let across = Vec2::new(1.0, 0.0);
    let filled = |shape: BezPath| Validated::from(Region::new(&shape, FillRule::NonZero, style.tolerance).map_err(AnnotationError::from));
    let (elements, clearances) = match legend {
        Legend::Key { at, entries, swatch, columns } => {
            (swatch.is_finite() && swatch.width > 0.0 && swatch.height >= 0.0).ok_or_else(|| nev![AnnotationError::Degenerate])?;
            let origin = *placement * *at;
            let paper = Placement::try_new(Affine::IDENTITY).map_err(|error| nev![AnnotationError::from(error)])?;
            let lettering = entries.iter().map(|entry| labelled(typesetter, style, &entry.label, Point::ORIGIN, across, Anchor::Start, Baseline::Middle)).collect::<Vec<_>>();
            let sizes = lettering.iter().map(|set| set.as_ref().map_or(Size::ZERO, |(_, mask)| mask.bounding_box().size())).collect::<Vec<_>>();
            let depth = entries.len().get().div_ceil(NonZero::<usize>::from(*columns).get());
            let lefts = sizes
                .chunks(depth)
                .scan(origin.x, |left, column| {
                    let start = *left;
                    *left += swatch.width + gap + column.iter().map(|size| size.width).fold(0.0, f64::max);
                    Some(start)
                })
                .flat_map(|left| iter::repeat_n(left, depth));
            let middles = sizes.chunks(depth).flat_map(|column| {
                column.iter().scan(origin.y, move |top, size| {
                    let height = swatch.height.max(gap.mul_add(-2.0, size.height));
                    let middle = height.mul_add(0.5, *top);
                    *top += height + gap;
                    Some(middle)
                })
            });
            let (drawn, outlines): (Vec<Vec<Element>>, Vec<BezPath>) = iter::zip(iter::zip(entries, lettering), iter::zip(lefts, middles))
                .map(|((Entry { swatch: sample, label: _ }, set), (left, middle))| {
                    let rect = Rect::from_center_size((swatch.width.mul_add(0.5, left), middle), *swatch);
                    let clearance = rect.inflate(gap, gap).to_path(tolerance);
                    let boxed = || Validated::from(stroked(rect.to_path(tolerance), &style.pen, None));
                    let shown = match sample {
                        Swatch::Line { pen, linetype } =>
                            Validated::from(stroked(Line::new((rect.x0, middle), (rect.x1, middle)).to_path(tolerance), pen, linetype.clone())).map(|stroke| (vec![stroke], clearance)),
                        Swatch::Fill { color, opacity } =>
                            filled(rect.to_path(tolerance)).map2(boxed(), |region, outline| (vec![Element::Fill { region, color: color.clone(), opacity: *opacity }, outline], clearance)),
                        Swatch::Hatch { pattern, pen } => filled(rect.to_path(tolerance)).map2(boxed(), |region, outline| {
                            let frame = match pattern.target {
                                Target::Model => placement,
                                Target::Drafting => paper,
                            };
                            (vec![Element::Hatch { region, pattern: pattern.clone(), placement: frame, pen: pen.clone() }, outline], clearance)
                        }),
                        Swatch::Symbol(definition) => symbols::draw(typesetter, style, paper, &Instance { definition: Arc::clone(definition), at: rect.center(), .. })
                            .map_or_else(Validated::Fail, |Graphics { elements, mask }| Validated::Good((elements, iter::chain(clearance, mask.path_elements(tolerance)).collect()))),
                    };
                    let placed = set.and_then(|(glyphs, mask)| {
                        let shift = Affine::translate((left + swatch.width + gap, middle));
                        Placement::try_new(shift)
                            .map_err(|error| nev![error])
                            .and_then(|moved| transform(glyphs, moved))
                            .map(|moved| (moved, shift * mask.path_elements(tolerance).collect::<BezPath>()))
                            .map_err(|errors| errors.into_nonempty_iter().map(AnnotationError::from).collect::<NEVec<_>>())
                    });
                    shown.map2(placed.map_or_else(Validated::Fail, Validated::Good), |(swatched, swatch_outline), (lettered, label_outline)| {
                        (iter::chain(swatched, lettered).collect::<Vec<_>>(), iter::chain(swatch_outline, label_outline).collect::<BezPath>())
                    })
                })
                .collect::<Validated<Vec<_>, _>>()
                .ok()?
                .into_iter()
                .unzip();
            (drawn.into_iter().flatten().collect::<Vec<_>>(), outlines.into_iter().flatten().collect::<BezPath>())
        }
        Legend::Ramp { at, gradient, bands, ticks, decimal, extent } => {
            let origin = *placement * *at;
            let (start, end) = gradient.domain();
            let (low, high) = iter::chain([f64::from(start), f64::from(end)], ticks.iter().copied()).minmax_by(f64::total_cmp).into_option().ok_or_else(|| nev![AnnotationError::Degenerate])?;
            let span = high - low;
            (span.is_finite() && span > 0.0 && extent.is_finite() && extent.width > 0.0 && extent.height > 0.0).ok_or_else(|| nev![AnnotationError::Degenerate])?;
            let (frame, length, thickness, anchor, baseline) = if extent.width >= extent.height {
                (Affine::translate(origin.to_vec2()), extent.width, extent.height, Anchor::Middle, Baseline::Top)
            } else {
                (Affine::rotate(-FRAC_PI_2).then_translate(Vec2::new(origin.x, origin.y + extent.height)), extent.height, extent.width, Anchor::Start, Baseline::Middle)
            };
            let along = |value: f64| length * (value - low) / span;
            let count = bands.get();
            let edges = (0..=count)
                .map(|band| length * f64::from(band) / f64::from(count))
                .chain(ticks.iter().map(|&value| along(value)))
                .sorted_by(f64::total_cmp)
                .dedup_by(|kept, next| next - kept <= tolerance)
                .collect::<Vec<_>>();
            let fills = edges
                .array_windows()
                .map(|&[near, far]| {
                    let colorgrad::Color { r, g, b, a: alpha } = gradient.at(span.mul_add(near.midpoint(far) / length, low).cast::<f32>()).clamp();
                    filled(frame * Rect::new(near, 0.0, far, thickness).to_path(tolerance)).map2(
                        Validated::from(
                            Opacity::try_new(f64::from(alpha))
                                .map_err(|(OpacityError::FiniteViolated | OpacityError::GreaterOrEqualViolated | OpacityError::LessOrEqualViolated)| AnnotationError::Unsupported(Feature::Opacity)),
                        ),
                        |region, opacity| Element::Fill { region, color: Color::Srgb(OpaqueColor::<Srgb>::new([r, g, b])), opacity },
                    )
                })
                .collect::<Validated<Vec<_>, _>>();
            let positions = ticks.iter().map(|&value| (value, along(value))).collect::<Vec<_>>();
            let outline = Rect::new(0.0, 0.0, length, thickness)
                .path_elements(tolerance)
                .chain(positions.iter().flat_map(|&(_, position)| Line::new((position, thickness), (position, thickness + gap)).path_elements(tolerance)))
                .collect::<BezPath>();
            let labels = positions
                .iter()
                .map(|&(value, position)| {
                    labelled(typesetter, style, &Label { content: notation::decimal(decimal, value).into(), .. }, frame * Point::new(position, gap.mul_add(2.0, thickness)), across, anchor, baseline)
                        .map_or_else(Validated::Fail, Validated::Good)
                })
                .collect::<Validated<Vec<_>, _>>();
            let bar = frame * Rect::new(0.0, 0.0, length, thickness + gap).to_path(tolerance);
            fills
                .map3(Validated::from(stroked(frame * outline, &style.pen, None)), labels, |colored, stroke, lettering| {
                    let (glyphs, masks): (Vec<_>, Vec<_>) = lettering.into_iter().unzip();
                    (iter::chain(colored, [stroke]).chain(glyphs.into_iter().flatten()).collect(), iter::chain(bar, masks.iter().flat_map(|mask| mask.path_elements(tolerance))).collect())
                })
                .ok()?
        }
    };
    Region::new(&clearances, FillRule::NonZero, style.tolerance).map(|mask| Graphics { elements, mask }).map_err(|error| nev![AnnotationError::from(error)])
}
