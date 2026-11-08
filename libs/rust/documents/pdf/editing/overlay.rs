use core::iter;
use std::collections::{BTreeMap, BTreeSet};

use color::{OpaqueColor, Srgb};
use hayro_syntax::object::dict::keys::{COLORSPACE, CONTENTS, N, RESOURCES};
use krilla::{
    SerializeSettings,
    blend::BlendMode,
    configure::{ConfigurationBuilder, PdfVersion},
    geom::Size,
    page::PageSettings,
    pdf::PdfDocument,
};
use lopdf::{
    Dictionary, Object, ObjectId,
    content::{Content, Operation},
};
use nonempty_collections::NEVec;
use strum::VariantArray as _;
use validated::Validated;

use crate::{Document, PdfError, Subject};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
enum Mixing {
    Additive,
    Subtractive,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, strum::VariantArray)]
enum Painting {
    Fill,
    Stroke,
}

// --- [MODELS] --------------------------------------------------------------------------

enum Painted {
    Page(ObjectId),
    Form(ObjectId),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn overlay(earlier: &Document, later: &Document, pairs: impl IntoIterator<Item = (usize, usize)>, colors: [OpaqueColor<Srgb>; 2]) -> Result<Document, PdfError> {
    let sized = |document: &Document, index: usize| {
        Validated::from(
            crate::page(document, index)
                .and_then(|sheet| {
                    let (width, height) = sheet.render_dimensions();
                    crate::extent(kurbo::Size::new(f64::from(width), f64::from(height)))
                })
                .map_err(|error| (Subject::Page(index), error)),
        )
    };
    let sheets = pairs
        .into_iter()
        .map(|(before, after)| sized(earlier, before).map2(sized(later, after), |first, second| ((before, first), (after, second))))
        .collect::<Validated<Vec<_>, _>>()
        .ok()
        .map_err(PdfError::Refused)?;
    let located = |part: usize| move |error: PdfError| (Subject::Part(part), error);
    let [first, second] = colors;
    let (before, after) = Validated::from(recolored(earlier, sheets.iter().map(|&((page, _), _)| page), first).map_err(located(0)))
        .map2(Validated::from(recolored(later, sheets.iter().map(|&(_, (page, _))| page), second).map_err(located(1))), |before, after| (before, after))
        .ok()
        .map_err(PdfError::Refused)?;
    let version = if earlier.version().max(later.version()) == hayro_syntax::PdfVersion::Pdf20 { PdfVersion::Pdf20 } else { PdfVersion::default() };
    let configuration = ConfigurationBuilder::new().with_version(version).finish().map_err(PdfError::Configuration)?;
    let mut canvas = krilla::Document::new_with(SerializeSettings { configuration, enable_tagging: false, ..SerializeSettings::default() });
    for ((earlier_page, earlier_size), (later_page, later_size)) in sheets {
        let paper = Size::from_wh(earlier_size.width().max(later_size.width()), earlier_size.height().max(later_size.height())).unwrap_or(later_size);
        let mut sheet = canvas.start_page_with(PageSettings::new(paper));
        let mut surface = sheet.surface();
        surface.draw_pdf_page(&before, earlier_size, earlier_page);
        surface.push_blend_mode(BlendMode::Multiply);
        surface.draw_pdf_page(&after, later_size, later_page);
        surface.pop();
        surface.finish();
        sheet.finish();
    }
    Document::from_plain_bytes(canvas.finish()?)
}

fn recolored<I>(document: &Document, pages: I, color: OpaqueColor<Srgb>) -> Result<PdfDocument, PdfError>
where I: IntoIterator<Item = usize> {
    fn spaces<'held, I>(structure: &lopdf::Document, resources: I) -> Result<BTreeMap<Vec<u8>, Option<Mixing>>, lopdf::Error>
    where I: IntoIterator<Item = &'held Dictionary> {
        resources
            .into_iter()
            .filter(|held| held.has(COLORSPACE))
            .map(|held| structure.get_dict_in_dict(held, COLORSPACE))
            .try_collect::<Vec<_>>()?
            .into_iter()
            .flat_map(Dictionary::iter)
            .map(|(name, space)| mixing(structure, space).map(|mixing| (name.clone(), mixing)))
            .collect()
    }
    fn mixing(structure: &lopdf::Document, space: &Object) -> Result<Option<Mixing>, lopdf::Error> {
        const fn channels(components: i64) -> Option<Mixing> {
            match components {
                1 | 3 => Some(Mixing::Additive),
                4 => Some(Mixing::Subtractive),
                _ => None,
            }
        }
        match structure.dereference(space)?.1 {
            Object::Name(name) => Ok(device(name)),
            Object::Array([Object::Name(b"ICCBased"), profile]) => structure.dereference(profile)?.1.as_stream()?.dict.get(N)?.as_i64().map(channels),
            Object::Array([Object::Name(b"CalGray" | b"CalRGB"), ..]) => Ok(Some(Mixing::Additive)),
            Object::Array([Object::Name(b"Separation" | b"DeviceN"), ..]) => Ok(Some(Mixing::Subtractive)),
            _ => Ok(None),
        }
    }
    let mut structure = crate::structure(document)?;
    let sheets = structure.page_iter().collect::<Vec<_>>();
    let selected = pages.into_iter().collect::<BTreeSet<_>>().into_iter().map(|index| sheets.get(index).copied().ok_or(PdfError::Page { index, count: sheets.len() })).try_collect::<Vec<_>>()?;
    let revision = color.components;
    let held = selected
        .iter()
        .map(|&sheet| {
            let (own, inherited) = structure.get_page_resources(sheet)?;
            inherited.into_iter().rev().map(|id| structure.get_dictionary(id)).chain(own.map(Ok)).try_collect::<Vec<_>>().map(|resources| (sheet, resources))
        })
        .try_collect::<Vec<_>>()?;
    let found = held.iter().flat_map(|(_, resources)| resources).try_fold(BTreeSet::new(), |found, resources| super::forms(&structure, resources, found))?;
    let painted = held
        .iter()
        .map(|(sheet, resources)| {
            structure
                .get_dictionary(*sheet)?
                .has(CONTENTS)
                .then(|| {
                    spaces(&structure, resources.iter().copied()).and_then(|colorspaces| {
                        structure.get_and_decode_page_content(*sheet).and_then(|content| {
                            Content {
                                operations: iter::chain(Painting::VARIANTS.iter().map(|&painting| painted(painting, revision)), repainted(content.operations, &colorspaces, revision))
                                    .collect::<Vec<_>>(),
                            }
                            .encode()
                        })
                    })
                })
                .transpose()
                .map(|content| content.map(|content| (Painted::Page(*sheet), content)))
        })
        .filter_map(Result::transpose)
        .chain(found.into_iter().map(|form| {
            let stream = structure.get_object(form)?.as_stream()?;
            let resources = crate::nested(&structure, &stream.dict, &[RESOURCES])?;
            spaces(&structure, resources)
                .and_then(|colorspaces| {
                    stream
                        .decompressed_content()
                        .and_then(|bytes| Content::decode(&bytes))
                        .and_then(|content| Content { operations: repainted(content.operations, &colorspaces, revision).collect::<Vec<_>>() }.encode())
                })
                .map(|content| (Painted::Form(form), content))
        }))
        .try_collect::<Vec<_>>()?;
    for (target, content) in painted {
        match target {
            Painted::Page(sheet) => structure.change_page_content(sheet, content)?,
            Painted::Form(form) => structure.get_object_mut(form)?.as_stream_mut()?.set_plain_content(content),
        }
    }
    crate::saved(structure).and_then(|recolored| crate::embedded(&recolored))
}

const fn device(name: &[u8]) -> Option<Mixing> {
    match name {
        b"DeviceGray" | b"DeviceRGB" | b"G" | b"RGB" => Some(Mixing::Additive),
        b"DeviceCMYK" | b"CMYK" => Some(Mixing::Subtractive),
        _ => None,
    }
}

fn repainted(operations: Vec<Operation>, spaces: &BTreeMap<Vec<u8>, Option<Mixing>>, revision: [f32; 3]) -> impl Iterator<Item = Operation> {
    const PAPER: [f32; 3] = [1.0; 3];
    fn bare(operands: &[Object], mixing: Option<Mixing>) -> bool {
        !operands.is_empty()
            && operands.iter().all(|operand| match (mixing, operand) {
                (Some(Mixing::Additive), Object::Integer(tint)) => *tint >= 1,
                (Some(Mixing::Additive), Object::Real(tint)) => *tint >= 1.0,
                (Some(Mixing::Subtractive), Object::Integer(tint)) => *tint <= 0,
                (Some(Mixing::Subtractive), Object::Real(tint)) => *tint <= 0.0,
                (Some(Mixing::Additive | Mixing::Subtractive) | None, _) => false,
            })
    }
    let named = move |operands: &[Object]| match operands {
        [Object::Name(name)] => device(name).or_else(|| spaces.get(name).copied().flatten()),
        _ => None,
    };
    operations.into_iter().scan(NEVec::new([Some(Mixing::Additive); 2]), move |states, operation| {
        let [fill, stroke] = *states.last();
        let selected = match operation.operator {
            "q" => {
                states.push([fill, stroke]);
                None
            }
            "Q" => {
                states.pop();
                None
            }
            "g" | "rg" => Some((Painting::Fill, Some(Mixing::Additive))),
            "G" | "RG" => Some((Painting::Stroke, Some(Mixing::Additive))),
            "k" => Some((Painting::Fill, Some(Mixing::Subtractive))),
            "K" => Some((Painting::Stroke, Some(Mixing::Subtractive))),
            "cs" => Some((Painting::Fill, named(&operation.operands))),
            "CS" => Some((Painting::Stroke, named(&operation.operands))),
            "sc" | "scn" => Some((Painting::Fill, fill)),
            "SC" | "SCN" => Some((Painting::Stroke, stroke)),
            _ => None,
        };
        Some(match selected {
            Some((painting, mixing)) => {
                let [fill_state, stroke_state] = states.last_mut();
                *(match painting {
                    Painting::Fill => fill_state,
                    Painting::Stroke => stroke_state,
                }) = mixing;
                painted(painting, if bare(&operation.operands, mixing) { PAPER } else { revision })
            }
            None => operation,
        })
    })
}

fn painted(painting: Painting, color: [f32; 3]) -> Operation {
    let operator = match painting {
        Painting::Fill => "rg",
        Painting::Stroke => "RG",
    };
    Operation::new(operator, color.map(Object::from).to_vec())
}
