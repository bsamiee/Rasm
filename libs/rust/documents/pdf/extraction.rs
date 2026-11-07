use core::iter;
use std::{collections::BTreeMap, sync::Arc};

use color::OpaqueColor;
use image::DynamicImage;
use kurbo::{Affine, BezPath, Cap, Join, Point, Rect, Shape as _, Size};
use linesweeper::FillRule;
use nonempty_collections::{IntoNonEmptyIterator as _, NonEmptyIterator as _};
use pdf_oxide::{
    PdfDocument,
    elements::{LineCap, LineJoin, PathContent, PathOperation},
    geometry, layout,
    ocr::{OcrEngine, OcrOutput, OcrSpan},
};
use rasm_drawing::{Color, Element, LineworkError, MiterLimit, MiterLimitError, Path, Pen, Region, Tolerance, Visibility, Width, pages::Layer};
use uom::si::linear_number_density::per_millimeter;
use validated::Validated;

use crate::{
    Document, PdfError, Subject,
    layers::{self, Group},
    proofs::{self, Raster},
};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq)]
pub struct Paths {
    pub size: Size,
    pub elements: Vec<Element>,
    pub layers: Vec<Layer>,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn extract<T, E, P>(document: &Document, pages: P, mut read: impl FnMut(&PdfDocument, usize) -> Result<T, E>) -> Result<Vec<T>, PdfError>
where
    P: IntoIterator<Item = usize>,
    PdfError: From<E>,
{
    let engine = crate::engine(document)?;
    pages
        .into_iter()
        .map(|index| crate::page(document, index).and_then(|_| read(&engine, index).map_err(PdfError::from)).map_err(|error| (Subject::Page(index), error)))
        .collect::<Validated<Vec<_>, _>>()
        .ok()
        .map_err(PdfError::Refused)
}

pub fn paths<P>(document: &Document, pages: P, area: Option<Rect>, tolerance: Tolerance) -> Result<Vec<Paths>, PdfError>
where P: IntoIterator<Item = usize> {
    let paint = |layout::Color { r, g, b }: layout::Color| Color::Srgb(OpaqueColor::new([r, g, b]));
    let painted = |path: &PathContent, frame: Affine, scale: f64| {
        let PathContent { bbox: _, operations, stroke_color, fill_color, stroke_width, line_cap, line_join, dash_pattern: _, matrix: _, reading_order: _, artifact_type: _, layer: _ } = path;
        let user = |x: f32, y: f32| Point::new(f64::from(x), f64::from(y));
        let outline = Some(
            frame
                * operations.iter().skip_while(|operation| !matches!(operation, PathOperation::MoveTo(..) | PathOperation::Rectangle(..))).fold(BezPath::new(), |mut outline, &operation| {
                    match operation {
                        PathOperation::MoveTo(x, y) => outline.move_to(user(x, y)),
                        PathOperation::LineTo(x, y) => outline.line_to(user(x, y)),
                        PathOperation::CurveTo(x1, y1, x2, y2, x3, y3) => outline.curve_to(user(x1, y1), user(x2, y2), user(x3, y3)),
                        PathOperation::Rectangle(x, y, width, height) => {
                            let (left, bottom) = (f64::from(x), f64::from(y));
                            outline.extend(Rect::new(left, bottom, left + f64::from(width), bottom + f64::from(height)).path_elements(0.0));
                        }
                        PathOperation::ClosePath => outline.close_path(),
                    }
                    outline
                }),
        )
        .filter(|outline| !outline.elements().is_empty());
        let filled = outline
            .as_ref()
            .zip(fill_color.or_else(|| stroke_color.is_none().then(layout::Color::black)))
            .map(|(outline, color)| Validated::from(Region::new(outline, FillRule::NonZero, tolerance).map(|region| Element::Fill { region, color: paint(color), .. }).map_err(PdfError::from)));
        let stroked = outline.zip(*stroke_color).map(|(outline, color)| {
            let width = Width::try_new(f64::from(*stroke_width) * scale).map_err(PdfError::from);
            let centerline = Path::try_new(outline).map_err(|error| PdfError::from(LineworkError::from(error)));
            Validated::from(width).map2(Validated::from(centerline), |width, centerline| Element::Stroke {
                path: centerline,
                pen: Pen {
                    width,
                    color: paint(color),
                    cap: match line_cap {
                        LineCap::Butt => Cap::Butt,
                        LineCap::Round => Cap::Round,
                        LineCap::Square => Cap::Square,
                    },
                    join: match line_join {
                        LineJoin::Miter => Join::Miter,
                        LineJoin::Round => Join::Round,
                        LineJoin::Bevel => Join::Bevel,
                    },
                    miter_limit: const {
                        match MiterLimit::try_new(10.0) {
                            Ok(limit) => limit,
                            Err(MiterLimitError::FiniteViolated | MiterLimitError::GreaterOrEqualViolated) => panic!("the initial PDF miter limit is finite and at least one"),
                        }
                    },
                    ..
                },
                linetype: None,
            })
        });
        [filled, stroked].into_iter().flatten()
    };
    let groups = layers::groups(document)?;
    extract(document, pages, |engine, index| {
        let page = crate::page(document, index)?;
        let (frame, size) = crate::frame(page);
        let scale = frame.determinant().abs().sqrt();
        let extracted = match area {
            Some(area) => {
                let Rect { x0, y0, x1, y1 } = frame.inverse().transform_rect_bbox(area);
                engine.extract_paths_in_rect(index, geometry::Rect::from_points(x0.cast::<f32>(), y0.cast::<f32>(), x1.cast::<f32>(), y1.cast::<f32>()))
            }
            None => engine.extract_paths(index),
        }?;
        let drawn = |layer: &Arc<str>, members: &[&PathContent]| -> Validated<Vec<Element>, (Subject, PdfError)> {
            members
                .iter()
                .enumerate()
                .flat_map(|(element, &path)| {
                    let located = move |error| (Subject::Element { page: index, layer: Arc::clone(layer), index: element }, error);
                    painted(path, frame, scale).map(move |outcome| outcome.map_err(|errors| errors.into_nonempty_iter().map(located).collect()))
                })
                .collect()
        };
        let (outside, mut layered) = extracted.iter().fold((Vec::new(), BTreeMap::<&str, Vec<&PathContent>>::new()), |(mut outside, mut layered), path| {
            match &path.layer {
                Some(name) => layered.entry(name.as_str()).or_default().push(path),
                None => outside.push(path),
            }
            (outside, layered)
        });
        let declared = groups.iter().filter_map(|(_, group)| layered.remove(&*group.name).map(|members| (group.clone(), members))).collect::<Vec<_>>();
        let undeclared = layered.into_iter().map(|(name, members)| (Group { name: Arc::from(name), print: Visibility::Visible, view: Visibility::Visible }, members));
        let grouped = iter::chain(declared, undeclared)
            .map(|(Group { name, print, view }, members)| drawn(&name, &members).map(|elements| Layer { name, print, view, elements }))
            .collect::<Validated<Vec<_>, _>>();
        drawn(&Arc::default(), &outside).map2(grouped, |elements, layers| Paths { size, elements, layers }).ok().map_err(PdfError::Refused)
    })
}

pub fn recognize(document: &Document, index: usize, engine: &OcrEngine, raster: &Raster, area: Option<Rect>) -> Result<OcrOutput, PdfError> {
    let image = DynamicImage::ImageRgba8(proofs::render(document, index, raster)?);
    let sheet = Rect::new(0.0, 0.0, f64::from(image.width()), f64::from(image.height()));
    let (origin, output) = match area.map(|area| area.abs().scale_from_origin(raster.resolution.get::<per_millimeter>()).expand().intersect(sheet)) {
        None => (Point::ORIGIN, engine.ocr_image(&image)?),
        Some(window) if window.is_zero_area() => (window.origin(), OcrOutput { spans: Vec::new(), total_confidence: 0.0 }),
        Some(window) => {
            let [left, top, across, down] = [window.x0, window.y0, window.width(), window.height()].map(f64::to_int_saturating::<u32>);
            (window.origin(), engine.ocr_image(&image.crop_imm(left, top, across, down))?)
        }
    };
    let [left, top] = [origin.x, origin.y].map(f64::cast::<f32>);
    Ok(OcrOutput { spans: output.spans.into_iter().map(|span| OcrSpan { polygon: span.polygon.map(|[x, y]| [x + left, y + top]), ..span }).collect(), ..output })
}
