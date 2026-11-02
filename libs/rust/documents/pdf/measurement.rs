use core::borrow::Borrow;

use hayro::hayro_interpret::RectExt as _;
use hayro_syntax::object::{self, Array, Dict, Name, dict::keys};
use kurbo::{Affine, Rect, Shape, Vec2};
use lopdf::{Object, StringFormat, decode_text_string, dictionary, text_string};
use rasm_linework::pages::{Page, Scale, Viewport};
use uom::si::{
    Unit as _,
    area::square_millimeter,
    f64::{Length, Ratio},
    length::{self, centimeter, foot, inch, kilometer, meter, mile, millimeter, yard},
};

use crate::{Document, POINTS_PER_INCH, PdfError};

// --- [CONSTANTS] -----------------------------------------------------------------------

const RECTILINEAR: &[u8] = b"RL";

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Measure {
    pub area: Rect,
    pub scale: Ratio,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn measures(document: &Document, index: usize) -> Result<Vec<Measure>, PdfError> {
    fn labeled<N>() -> (&'static str, Length)
    where N: length::Conversion<f64> {
        (N::abbreviation(), Length::new::<N>(1.0))
    }
    let units = [labeled::<millimeter>(), labeled::<centimeter>(), labeled::<meter>(), labeled::<kilometer>(), labeled::<inch>(), labeled::<foot>(), labeled::<yard>(), labeled::<mile>()];
    let page = crate::page(document, index)?;
    let frame = crate::frame(page);
    page.raw()
        .get::<Array<'_>>(keys::VP)
        .into_iter()
        .flat_map(|viewports| viewports.iter::<Dict<'_>>())
        .filter_map(|viewport| {
            let measure = viewport.get::<Dict<'_>>(keys::MEASURE).filter(|measure| measure.get::<Name<'_>>(keys::SUBTYPE).is_none_or(|subtype| *subtype == *RECTILINEAR))?;
            let format = measure.get::<Array<'_>>(keys::X)?.iter::<Dict<'_>>().next()?;
            Some((viewport.get::<object::Rect>(keys::BBOX)?, format.get::<object::String<'_>>(keys::U)?, format.get::<f64>(keys::C)?))
        })
        .map(|(bounds, label, factor)| {
            decode_text_string(&Object::String(label.to_vec(), StringFormat::Literal)).map_err(PdfError::from).and_then(|text| {
                units
                    .iter()
                    .find(|&&(abbreviation, _)| abbreviation == text)
                    .map(|&(_, unit)| Measure { area: frame.transform_rect_bbox(bounds.to_kurbo()), scale: unit * factor / point() })
                    .ok_or_else(|| PdfError::Unit(text.into()))
            })
        })
        .try_collect::<Vec<_>>()
}

pub(crate) fn write<I>(structure: &mut lopdf::Document, pages: I) -> Result<(), PdfError>
where I: IntoIterator<Item: Borrow<Page>> {
    let unit = millimeter::abbreviation();
    let formats = |label: &str, factor: f64| Object::from(vec![Object::from(dictionary! { keys::U => text_string(label), keys::C => factor })]);
    structure
        .page_iter()
        .collect::<Vec<_>>()
        .into_iter()
        .zip(pages)
        .try_for_each(|(id, page)| {
            let Page { size, viewports, layers: _, links: _, fields: _ } = page.borrow();
            let user = Affine::FLIP_Y.then_translate(Vec2::new(0.0, size.height)).then_scale(point().get::<millimeter>().recip());
            let entries = viewports
                .iter()
                .map(|Viewport { boundary, placement: _, scale: Scale { paper, model }, layers: _ }| {
                    let bounds = user.transform_rect_bbox(boundary.as_ref().map_or(size.to_rect(), Shape::bounding_box));
                    Object::from(dictionary! {
                        keys::BBOX => Vec::from([bounds.x0, bounds.y0, bounds.x1, bounds.y1].map(Object::from)),
                        keys::MEASURE => dictionary! {
                            keys::SUBTYPE => Object::Name(RECTILINEAR.to_vec()),
                            keys::R => text_string(&format!("{paper} {unit} = {model} {unit}")),
                            keys::X => formats(unit, (point() * f64::from(model.get()) / f64::from(paper.get())).get::<millimeter>()),
                            keys::D => formats(unit, 1.0),
                            keys::A => formats(square_millimeter::abbreviation(), 1.0),
                        },
                    })
                })
                .collect::<Vec<_>>();
            if entries.is_empty() { Ok(()) } else { structure.get_dictionary_mut(id).map(|sheet| sheet.set(keys::VP, entries)) }
        })
        .map_err(PdfError::from)
}

fn point() -> Length { Length::new::<inch>(POINTS_PER_INCH.recip()) }
