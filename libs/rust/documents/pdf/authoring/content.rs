use core::iter;
use std::{
    collections::{HashMap, hash_map::Entry},
    sync::Arc,
};

use krilla::{
    Data,
    color::{
        self, cmyk, rgb,
        separation::{self, SeparationColorant, SeparationSpace},
    },
    geom::{self, PathBuilder, Transform},
    image::Image,
    num::NormalizedF32,
    paint::{Fill, FillRule, LineCap, LineJoin, Paint, Stroke, StrokeDash},
    pdf::PdfDocument,
    surface::Surface,
    text::{Font, GlyphId, KrillaGlyph, Tag},
};
use krilla_svg::{SurfaceExt as _, SvgSettings};
use kurbo::{Affine, Cap, Join, PathEl, Rect, Shape as _, Size};
use nonempty_collections::{IntoNonEmptyIterator as _, NEVec, NonEmptyIterator as _, nev};
use rasm_linework::{Color, Content, Element, GlyphRun, Opacity, Pen, Placement, Region, Side, hatches, strokes};
use uom::si::{f64::Length, length::millimeter, ratio::ratio};
use usvg::Tree;
use validated::Validated;

use super::{
    Options,
    rasters::{self, Raster},
    tagging::Leaves,
};
use crate::{Document, PdfError};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(PartialEq, Eq, Hash)]
struct Face {
    id: u64,
    index: u32,
    coordinates: Vec<(Tag, i32)>,
}

pub(crate) struct Drawing<'options> {
    options: &'options Options,
    trees: HashMap<usize, Tree>,
    rasters: HashMap<usize, Raster>,
    images: HashMap<(usize, (u32, u32)), Image>,
    documents: HashMap<usize, (PdfDocument, usize)>,
    faces: HashMap<Face, Font>,
}

impl<'options> Drawing<'options> {
    pub(crate) fn new(options: &'options Options) -> Self { Self { options, trees: HashMap::new(), rasters: HashMap::new(), images: HashMap::new(), documents: HashMap::new(), faces: HashMap::new() } }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub(crate) fn draw(surface: &mut Surface<'_>, elements: &[Element], sheet: Rect, drawing: &mut Drawing<'_>, leaves: &mut Leaves) -> Validated<(), (usize, PdfError)> {
    fn path<I>(elements: I) -> Option<geom::Path>
    where I: IntoIterator<Item = PathEl> {
        let coordinate = |value: f64| value.cast::<f32>();
        elements
            .into_iter()
            .fold(PathBuilder::new(), |mut builder, element| {
                match element {
                    PathEl::MoveTo(point) => builder.move_to(coordinate(point.x), coordinate(point.y)),
                    PathEl::LineTo(point) => builder.line_to(coordinate(point.x), coordinate(point.y)),
                    PathEl::QuadTo(control, point) => builder.quad_to(coordinate(control.x), coordinate(control.y), coordinate(point.x), coordinate(point.y)),
                    PathEl::CurveTo(first, second, point) =>
                        builder.cubic_to(coordinate(first.x), coordinate(first.y), coordinate(second.x), coordinate(second.y), coordinate(point.x), coordinate(point.y)),
                    PathEl::ClosePath => builder.close(),
                }
                builder
            })
            .finish()
    }
    fn stroked<I>(surface: &mut Surface<'_>, elements: I, pen: &Pen, dash: Option<StrokeDash>)
    where I: IntoIterator<Item = PathEl> {
        if let Some(outline) = path(elements) {
            surface.set_fill(None);
            surface.set_stroke(Some(Stroke {
                paint: paint(&pen.color),
                width: pen.width.into_inner().cast::<f32>(),
                miter_limit: pen.miter_limit.into_inner().cast::<f32>(),
                line_cap: match pen.cap {
                    Cap::Butt => LineCap::Butt,
                    Cap::Round => LineCap::Round,
                    Cap::Square => LineCap::Square,
                },
                line_join: match pen.join {
                    Join::Miter => LineJoin::Miter,
                    Join::Round => LineJoin::Round,
                    Join::Bevel => LineJoin::Bevel,
                },
                opacity: normalized(pen.opacity),
                dash,
            }));
            surface.draw_path(&outline);
        }
    }
    let tolerance = drawing.options.tolerance;
    let outlined = |region: &Region| path(region.path_elements(tolerance.into_inner()));
    let drawn = elements
        .iter()
        .enumerate()
        .map(|(index, element)| {
            let located = |drawn: Result<(), PdfError>| Validated::from(drawn.map_err(|error| (index, error)));
            match element {
                Element::Stroke { path: centerline, pen, linetype } => {
                    leaves.graphic(surface);
                    match linetype {
                        None => stroked(surface, centerline.iter(), pen, None),
                        Some(dashed) => strokes::phases(centerline, dashed, tolerance).into_iter().for_each(|(piece, offset, dashes)| {
                            let period = dashes.iter().sum::<f64>();
                            let dash = StrokeDash { array: dashes.iter().map(|length| length.cast::<f32>()).collect(), offset: offset.rem_euclid(period).cast::<f32>() };
                            stroked(surface, piece.iter().copied(), pen, Some(dash));
                        }),
                    }
                    Validated::Good(())
                }
                Element::Fill { region, color, opacity } => {
                    if let Some(outline) = outlined(region) {
                        leaves.graphic(surface);
                        surface.set_stroke(None);
                        surface.set_fill(Some(fill(color, *opacity, FillRule::EvenOdd)));
                        surface.draw_path(&outline);
                    }
                    Validated::Good(())
                }
                Element::Hatch { region, pattern, placement, pen } => match hatches::expand(region, pattern, *placement, tolerance) {
                    Ok(lines) => {
                        if let Some(outline) = outlined(region) {
                            leaves.graphic(surface);
                            surface.push_clip_path(&outline, &FillRule::EvenOdd);
                            stroked(surface, lines.iter(), pen, None);
                            surface.pop();
                        }
                        Validated::Good(())
                    }
                    Err(errors) => Validated::Fail(errors.into_nonempty_iter().map(|error| (index, PdfError::from(error))).collect()),
                },
                Element::Text { text, color: _, opacity: _ } => located(Err(PdfError::Unshaped(Arc::clone(&text.content)))),
                Element::Glyphs { run, color, opacity } => located(glyphs(surface, run, color, *opacity, drawing, leaves)),
                Element::Placed { content, placement, opacity } => placed(surface, content, *placement, *opacity, drawing, leaves)
                    .map_or_else(|errors| Validated::Fail(errors.into_nonempty_iter().map(|error| (index, error)).collect()), Validated::Good),
                Element::Clip { region, keep, elements: grouped } => {
                    let outline = match keep {
                        Side::Inside => outlined(region),
                        Side::Outside => path(iter::chain(sheet.path_elements(tolerance.into_inner()), region.path_elements(tolerance.into_inner()))),
                    };
                    match outline {
                        Some(mask) => {
                            leaves.close(surface);
                            surface.push_clip_path(&mask, &FillRule::EvenOdd);
                            let drawn = draw(surface, grouped, sheet, drawing, leaves);
                            surface.pop();
                            drawn.map_err(|errors| errors.into_nonempty_iter().map(|(_, error)| (index, error)).collect())
                        }
                        None => Validated::Good(()),
                    }
                }
            }
        })
        .collect();
    leaves.close(surface);
    drawn
}

fn transform(affine: Affine) -> Transform {
    let [xx, xy, yx, yy, tx, ty] = affine.as_coeffs().map(f64::cast::<f32>);
    Transform::from_row(xx, xy, yx, yy, tx, ty)
}

fn glyphs(surface: &mut Surface<'_>, run: &GlyphRun, color: &Color, opacity: Opacity, drawing: &mut Drawing<'_>, leaves: &mut Leaves) -> Result<(), PdfError> {
    const FIXED_ONE: f64 = 65536.0;
    let variations = run.variations.iter().map(|variation| (Tag::new(&variation.tag.to_bytes()), variation.value)).collect::<Vec<_>>();
    let coordinates = variations.iter().map(|&(tag, value)| (tag, (f64::from(value) * FIXED_ONE).round().to_int_saturating::<i32>())).collect();
    let font = match drawing.faces.entry(Face { id: run.font.data.id(), index: run.font.index, coordinates }) {
        Entry::Occupied(held) => held.get().clone(),
        Entry::Vacant(vacant) => vacant
            .insert(
                Font::new_variable(Data::from(run.font.data.clone().into_raw_parts().0), run.font.index, &variations)
                    .ok_or_else(|| PdfError::Font { family: Arc::clone(&run.family), index: run.font.index })?,
            )
            .clone(),
    };
    let size = run.size.into_inner();
    let normalize = |measure: f64| (measure / size).cast::<f32>();
    let positioned = run
        .glyphs
        .iter()
        .map(|glyph| {
            KrillaGlyph::new(GlyphId::new(glyph.id), normalize(glyph.advance.x), normalize(glyph.offset.x), normalize(-glyph.offset.y), normalize(-glyph.advance.y), glyph.cluster.into(), None)
        })
        .collect::<Vec<_>>();
    surface.set_stroke(None);
    surface.set_fill(Some(fill(color, opacity, FillRule::NonZero)));
    leaves.run(surface, |marked| {
        marked.push_transform(&transform(*run.placement));
        marked.draw_glyphs(geom::Point::from_xy(0.0, 0.0), &positioned, font, &run.text, size.cast::<f32>(), false);
        marked.pop();
    });
    Ok(())
}

fn placed(surface: &mut Surface<'_>, content: &Content, placement: Placement, opacity: Opacity, drawing: &mut Drawing<'_>, leaves: &mut Leaves) -> Result<(), NEVec<PdfError>> {
    fn enclosed<F>(surface: &mut Surface<'_>, placement: Placement, opacity: Opacity, leaves: &mut Leaves, draw: F)
    where F: FnOnce(&mut Surface<'_>) {
        leaves.graphic(surface);
        surface.push_opacity(normalized(opacity));
        surface.push_transform(&transform(*placement));
        draw(surface);
        surface.pop();
        surface.pop();
    }
    let address = |bytes: &Arc<[u8]>| Arc::as_ptr(bytes).cast::<u8>().addr();
    let unit = crate::extent(Size::new(1.0, 1.0)).map_err(|error| nev![error])?;
    match content {
        Content::Svg(bytes) => {
            let fonts = &drawing.options.fonts;
            let tree = match drawing.trees.entry(address(bytes)) {
                Entry::Occupied(held) => held.into_mut(),
                Entry::Vacant(vacant) => vacant.insert(Tree::from_data(bytes, &usvg::Options { fontdb: Arc::clone(fonts), ..usvg::Options::default() }).map_err(|error| nev![error.into()])?),
            };
            let resolution = drawing.options.resolution;
            enclosed(surface, placement, opacity, leaves, |marked| {
                let filter_scale = resolution.map_or_else(
                    || SvgSettings::default().filter_scale,
                    |density| {
                        let ctm = marked.cur_transform();
                        let per_millimeter = (Length::new::<millimeter>(1.0) * *density).get::<ratio>();
                        let paper = placement.determinant().abs().sqrt();
                        let stream = f64::from(ctm.sx().mul_add(ctm.sy(), -(ctm.kx() * ctm.ky()))).abs().sqrt();
                        (per_millimeter * paper / stream).cast::<f32>()
                    },
                );
                marked.draw_svg(tree, unit, SvgSettings { filter_scale, ..SvgSettings::default() });
            });
        }
        Content::Raster(bytes) => {
            let key = address(bytes);
            let raster = match drawing.rasters.entry(key) {
                Entry::Occupied(held) => held.into_mut(),
                Entry::Vacant(vacant) => vacant.insert(rasters::raster(bytes).map_err(|error| nev![error])?),
            };
            let [xx, xy, yx, yy, _, _] = placement.as_coeffs();
            let target = rasters::target(raster, Size::new(xx.hypot(xy), yx.hypot(yy)), drawing.options.resolution);
            let image = match drawing.images.entry((key, target)) {
                Entry::Occupied(held) => held.get().clone(),
                Entry::Vacant(vacant) => vacant.insert(rasters::image(raster, target)?).clone(),
            };
            enclosed(surface, placement, opacity, leaves, |marked| marked.draw_image(image, unit));
        }
        Content::Pdf { bytes, page } => {
            let (source, count) = match drawing.documents.entry(address(bytes)) {
                Entry::Occupied(held) => held.into_mut(),
                Entry::Vacant(vacant) => {
                    let document = Document::new(bytes.to_vec(), None).map_err(|error| nev![error])?;
                    vacant.insert((crate::embedded(&document).map_err(|error| nev![error])?, document.pages().len()))
                }
            };
            (*page < *count).ok_or_else(|| nev![PdfError::Page { index: *page, count: *count }])?;
            enclosed(surface, placement, opacity, leaves, |marked| marked.draw_pdf_page(source, unit, *page));
        }
    }
    Ok(())
}

fn fill(color: &Color, opacity: Opacity, rule: FillRule) -> Fill { Fill { paint: paint(color), opacity: normalized(opacity), rule } }

fn paint(color: &Color) -> Paint {
    let inks = |ink: &rasm_linework::Cmyk| cmyk::Color::new(ink.cyan, ink.magenta, ink.yellow, ink.black);
    match color {
        Color::Srgb(value) => {
            let [red, green, blue, _] = value.to_rgba8().to_u8_array();
            rgb::Color::new(red, green, blue).into()
        }
        Color::Cmyk(ink) => inks(ink).into(),
        Color::Spot { name, tint, alternate } => color::Color::from(separation::Color::new(*tint, SeparationSpace::new(SeparationColorant::Custom(name.to_string()), inks(alternate).into()))).into(),
    }
}

fn normalized(opacity: Opacity) -> NormalizedF32 { NormalizedF32::new(opacity.into_inner().cast::<f32>()).unwrap_or(NormalizedF32::ONE) }
