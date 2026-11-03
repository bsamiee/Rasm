use core::borrow::Borrow;
use std::sync::Arc;

use hayro_syntax::object::{self, Array, Dict, Name, dict::keys};
use kurbo::{Affine, Point, Rect, Shape, Vec2};
use lopdf::{Dictionary, Object, StringFormat, decode_text_string, dictionary, text_string};
use rasm_linework::pages::{self, Page, Scale, Viewport};
use uom::{
    si::{
        Unit as _,
        area::square_millimeter,
        f64::Length,
        length::{inch, millimeter},
    },
    str::ParseQuantityError,
};

use crate::{Document, POINTS_PER_INCH, PdfError};

// --- [CONSTANTS] -----------------------------------------------------------------------

const RECTILINEAR: &[u8] = b"RL";

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq)]
pub struct Measure {
    pub area: Rect,
    pub model: Affine,
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
                .map(|unit| (unit * factor).get::<millimeter>())
                .map_err(|(ParseQuantityError::NoSeparator | ParseQuantityError::ValueParseError | ParseQuantityError::UnknownUnit)| PdfError::Unit(label))
        }))
    };
    let page = crate::page(document, index)?;
    let frame = crate::frame(page);
    page.raw()
        .get::<Array<'_>>(keys::VP)
        .into_iter()
        .flat_map(|viewports| viewports.iter::<Dict<'_>>())
        .filter_map(|viewport| {
            let measure = viewport.get::<Dict<'_>>(keys::MEASURE).filter(|measure| measure.get::<Name<'_>>(keys::SUBTYPE).is_none_or(|subtype| *subtype == *RECTILINEAR))?;
            let bounds = viewport.get::<[f64; 4]>(keys::BBOX)?;
            let horizontal = axis(measure.get::<Array<'_>>(keys::X)?)?;
            let vertical = measure.get::<Array<'_>>(keys::Y).map_or(Some(None), |formats| axis(formats).map(Some))?;
            Some((viewport, measure, bounds, horizontal, vertical))
        })
        .map(|(viewport, measure, [x0, y0, x1, y1], horizontal, vertical)| {
            let horizontal = horizontal?;
            let origin = measure.get::<[f64; 2]>(keys::O).map_or(Point::new(x0, y0), |[x, y]| Point::new(x, y));
            Ok::<_, PdfError>(Measure {
                area: frame.transform_rect_bbox(Rect::new(x0, y0, x1, y1)),
                model: Affine::scale_non_uniform((x1 - x0).signum() * horizontal, (y1 - y0).signum() * vertical.transpose()?.unwrap_or(horizontal)).pre_translate(-origin.to_vec2()) * frame.inverse(),
                name: viewport.get::<object::String<'_>>(keys::NAME).map(text).transpose()?,
                ratio: measure.get::<object::String<'_>>(keys::R).map(text).transpose()?,
            })
        })
        .try_collect::<Vec<_>>()
}

#[must_use]
pub fn locate(measures: &[Measure], point: Point) -> Option<&Measure> { measures.iter().rfind(|measure| measure.area.contains(point)) }

pub(crate) fn write<I>(structure: &mut lopdf::Document, pages: I) -> Result<(), PdfError>
where I: IntoIterator<Item: Borrow<Page>> {
    let unit = millimeter::abbreviation();
    let formats = |label: &str, factor: f64| Object::from(vec![Object::from(dictionary! { keys::U => text_string(label), keys::C => factor })]);
    let numbers = |values: &[f64]| Object::from(values.iter().copied().map(Object::from).collect::<Vec<_>>());
    let ordered = |sign: f64, low: f64, high: f64| if sign < 0.0 { [high, low] } else { [low, high] };
    structure.page_iter().collect::<Vec<_>>().into_iter().zip(pages).try_for_each(|(id, page)| {
        let Page { size, viewports, layers: _, links: _, fields: _ } = page.borrow();
        let user = Affine::FLIP_Y.then_translate(Vec2::new(0.0, size.height)).then_scale(Length::new::<inch>(POINTS_PER_INCH.recip()).get::<millimeter>().recip());
        let entries = viewports
            .iter()
            .map(|viewport| {
                let Viewport { boundary, placement: _, scale: Scale { paper, model }, layers: _ } = viewport;
                let placed = user * *pages::affine(viewport)?;
                let [a, b, c, d, _, _] = placed.inverse().as_coeffs();
                let (horizontal, vertical) = (a.hypot(b), c.hypot(d));
                let Rect { x0, y0, x1, y1 } = user.transform_rect_bbox(boundary.as_ref().map_or(size.to_rect(), Shape::bounding_box));
                let ([left, right], [bottom, top]) = (ordered(a, x0, x1), ordered(d, y0, y1));
                let origin = placed * Point::ORIGIN;
                let distinct = horizontal.total_cmp(&vertical).is_ne().then(|| [(keys::Y, formats(unit, vertical)), (keys::CYX, Object::from(1.0))]);
                Ok::<_, PdfError>(Object::from(dictionary! {
                    keys::BBOX => numbers(&[left, bottom, right, top]),
                    keys::MEASURE => [
                        (keys::SUBTYPE, Object::Name(RECTILINEAR.to_vec())),
                        (keys::R, text_string(&format!("{paper} {unit} = {model} {unit}"))),
                        (keys::X, formats(unit, horizontal)),
                        (keys::D, formats(unit, 1.0)),
                        (keys::A, formats(square_millimeter::abbreviation(), 1.0)),
                        (keys::O, numbers(&[origin.x, origin.y])),
                    ]
                    .into_iter()
                    .chain(distinct.into_iter().flatten())
                    .collect::<Dictionary>(),
                }))
            })
            .try_collect::<Vec<_>>()?;
        if entries.is_empty() { Ok(()) } else { structure.get_dictionary_mut(id).map(|sheet| sheet.set(keys::VP, entries)).map_err(PdfError::from) }
    })
}
