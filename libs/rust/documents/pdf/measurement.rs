use core::{borrow::Borrow, iter};
use std::sync::Arc;

use hayro_syntax::object::{self, Array, Dict, Name, dict::keys};
use kurbo::{Affine, Point, Rect, Shape, Vec2};
use lopdf::{Dictionary, Object, StringFormat, decode_text_string, dictionary, text_string};
use nonempty_collections::{IntoNonEmptyIterator as _, NonEmptyIterator as _};
use rasm_drawing::pages::{self, Page, Scale, Viewport};
use uom::{
    si::{
        Unit as _,
        area::square_meter,
        f64::Length,
        length::{meter, point_computer},
        ratio,
    },
    str::ParseQuantityError,
};
use validated::Validated;

use crate::{Document, PdfError, Refusal, Subject};

// --- [CONSTANTS] -----------------------------------------------------------------------

const RECTILINEAR: &[u8] = b"RL";

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq)]
pub struct Measure {
    pub area: Rect,
    pub model: Affine,
    pub unit: Length,
    pub name: Option<Arc<str>>,
    pub ratio: Option<Arc<str>>,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn measures(document: &Document, index: usize) -> Result<Vec<Measure>, PdfError> {
    let text = |string: object::String<'_>| decode_text_string(&Object::String(string.to_vec(), StringFormat::Literal)).map(Arc::<str>::from).map_err(PdfError::from);
    let axis = |formats: Array<'_>| {
        let format = formats.iter::<Dict<'_>>().next()?;
        let factor = format.get::<f64>(keys::C).filter(|factor| factor.is_normal())?;
        Some(text(format.get::<object::String<'_>>(keys::U)?).and_then(|label| {
            format!("1 {label}")
                .parse::<Length>()
                .map(|unit| (factor, unit))
                .map_err(|(ParseQuantityError::NoSeparator | ParseQuantityError::ValueParseError | ParseQuantityError::UnknownUnit)| PdfError::Named { name: label, refusal: Refusal::Unit })
        }))
    };
    let page = crate::page(document, index)?;
    let (frame, _) = crate::frame(page);
    page.raw()
        .get::<Array<'_>>(keys::VP)
        .into_iter()
        .flat_map(|viewports| viewports.iter::<Dict<'_>>())
        .filter_map(|viewport| {
            let measure = viewport.get::<Dict<'_>>(keys::MEASURE).filter(|measure| measure.get::<Name<'_>>(keys::SUBTYPE).is_none_or(|subtype| *subtype == *RECTILINEAR))?;
            let ((bounds, horizontal), vertical) = viewport
                .get::<[f64; 4]>(keys::BBOX)
                .zip(measure.get::<Array<'_>>(keys::X).and_then(axis))
                .zip(measure.get::<Array<'_>>(keys::Y).map_or(Some(None), |formats| axis(formats).map(Some)))?;
            Some((viewport, measure, bounds, horizontal, vertical))
        })
        .map(|(viewport, measure, [x0, y0, x1, y1], horizontal, vertical)| {
            let origin = measure.get::<[f64; 2]>(keys::O).map_or_else(|| Point::new(x0, y0), |[x, y]| Point::new(x, y));
            Validated::from(horizontal)
                .map2(Validated::from(vertical.transpose()), |(horizontal, unit), vertical| {
                    let (factor, scale) = vertical.unwrap_or((horizontal, unit));
                    (horizontal, factor * (scale / unit).get::<ratio::ratio>(), unit)
                })
                .map3(
                    Validated::from(viewport.get::<object::String<'_>>(keys::NAME).map(text).transpose()),
                    Validated::from(measure.get::<object::String<'_>>(keys::R).map(text).transpose()),
                    |(horizontal, vertical, unit), name, ratio| Measure {
                        area: frame.transform_rect_bbox(Rect::new(x0, y0, x1, y1)),
                        model: Affine::scale_non_uniform((x1 - x0).signum() * horizontal, (y1 - y0).signum() * vertical).pre_translate(-origin.to_vec2()) * frame.inverse(),
                        unit,
                        name,
                        ratio,
                    },
                )
                .ok()
                .map_err(|errors| PdfError::Refused(errors.into_nonempty_iter().map(|error| (Subject::Page(index), error)).collect()))
        })
        .try_collect::<Vec<_>>()
}

#[must_use]
pub fn locate(measures: &[Measure], point: Point) -> Option<&Measure> { measures.iter().rfind(|measure| measure.area.contains(point)) }

pub(crate) fn write<I>(structure: &mut lopdf::Document, pages: I) -> Result<(), PdfError>
where I: IntoIterator<Item: Borrow<Page>> {
    let unit = meter::abbreviation();
    let formats = |label: &str, factor: f64| Object::from(vec![Object::from(dictionary! { keys::U => text_string(label), keys::C => factor })]);
    let numbers = |values: &[f64]| Object::from(values.iter().copied().map(Object::from).collect::<Vec<_>>());
    let ordered = |sign: f64, low: f64, high: f64| if sign < 0.0 { [high, low] } else { [low, high] };
    let sheets = structure.page_iter().collect::<Vec<_>>();
    iter::zip(sheets, pages).try_for_each(|(id, page)| {
        let Page { size, unit: length, viewports, layers: _, links: _, fields: _ } = page.borrow();
        let (user, meters) = (Affine::FLIP_Y.then_translate(Vec2::new(0.0, size.height)).then_scale(length.get::<point_computer>()), length.get::<meter>());
        let entries = viewports
            .iter()
            .map(|viewport| {
                let Viewport { boundary, placement: _, scale: Scale { paper, model }, layers: _ } = viewport;
                let placed = user * *pages::affine(viewport)?;
                let [xx, yx, xy, yy, _, _] = placed.inverse().as_coeffs();
                let (horizontal, vertical) = (xx.hypot(yx) * meters, xy.hypot(yy) * meters);
                let Rect { x0, y0, x1, y1 } = user.transform_rect_bbox(boundary.as_ref().map_or_else(|| size.to_rect(), Shape::bounding_box));
                let ([left, right], [bottom, top]) = (ordered(xx, x0, x1), ordered(yy, y0, y1));
                let origin = placed * Point::ORIGIN;
                let distinct = horizontal.total_cmp(&vertical).is_ne().then(|| [(keys::Y, formats(unit, vertical)), (keys::CYX, Object::from(1.0))]);
                Ok::<_, PdfError>(Object::from(dictionary! {
                    keys::BBOX => numbers(&[left, bottom, right, top]),
                    keys::MEASURE => iter::chain(
                        [
                            (keys::SUBTYPE, Object::Name(RECTILINEAR.to_vec())),
                            (keys::R, text_string(&format!("{paper}:{model}"))),
                            (keys::X, formats(unit, horizontal)),
                            (keys::D, formats(unit, 1.0)),
                            (keys::A, formats(square_meter::abbreviation(), 1.0)),
                            (keys::O, numbers(&[origin.x, origin.y])),
                        ],
                        distinct.into_iter().flatten(),
                    )
                    .collect::<Dictionary>(),
                }))
            })
            .try_collect::<Vec<_>>()?;
        if entries.is_empty() { Ok(()) } else { structure.get_dictionary_mut(id).map(|sheet| sheet.set(keys::VP, entries)).map_err(PdfError::from) }
    })
}
