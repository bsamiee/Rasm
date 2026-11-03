#![feature(default_field_values)]

use core::num::NonZero;
use std::{
    collections::{BTreeMap, HashMap, hash_map::Entry},
    sync::Arc,
};

use enum_map::{EnumMap, enum_map};
use imagesize::{ImageError, ImageSize, blob_size};
use indexmap::IndexMap;
use kurbo::{Affine, Point, Rect, Shape, Size};
use nonempty_collections::{IntoNonEmptyIterator as _, NEVec, NonEmptyIterator as _, nev};
use rasm_annotation::{AnnotationError, Style, text::Typesetter};
use rasm_linework::{
    Content, Element, LineworkError, Opacity, Placement, ToleranceError, Visibility,
    pages::{self, Destination, Link, Page},
};
use uom::si::{f64::LinearNumberDensity, linear_number_density::per_millimeter};
use validated::Validated;

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, derive_more::Display)]
#[display(rename_all = "lowercase")]
pub enum Lookup {
    Sheet,
    Viewport,
    Layer,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone)]
pub struct Sheet {
    pub size: Size,
    pub style: Arc<Style>,
    pub viewports: IndexMap<Arc<str>, viewports::Viewport>,
    pub layers: BTreeMap<Arc<str>, layers::Layer> = BTreeMap::new(),
    pub links: Vec<Link> = Vec::new(),
    pub fields: BTreeMap<Arc<str>, Arc<str>> = BTreeMap::new(),
}

#[derive(Debug, Clone, derive_more::AsRef)]
pub struct Set {
    #[as_ref]
    layers: IndexMap<Arc<str>, EnumMap<layers::Usage, Visibility>>,
    #[as_ref]
    sheets: IndexMap<Arc<str>, Sheet>,
    targets: references::Targets,
}

impl Set {
    pub fn new(layers: IndexMap<Arc<str>, EnumMap<layers::Usage, Visibility>>, sheets: IndexMap<Arc<str>, Sheet>) -> Result<Self, NEVec<SheetsError>> {
        let mut targets = references::Targets::new();
        let duplicates = sheets
            .iter()
            .enumerate()
            .flat_map(|(index, (key, sheet))| sheet.viewports.iter().enumerate().map(move |(position, (name, viewport))| (index, key, position, name, viewport)))
            .map(|(index, key, position, name, viewport)| match targets.entry(Arc::clone(name)) {
                Entry::Vacant(vacant) => {
                    vacant.insert((
                        enum_map! {
                            references::Mention::Number => Arc::from(position.strict_add(1).to_string()),
                            references::Mention::Sheet => Arc::clone(key),
                        },
                        Destination::Page { index, area: viewport.boundary.as_ref().map(Shape::bounding_box) },
                    ));
                    Ok(())
                }
                Entry::Occupied(held) => Err(SheetsError::Duplicate { name: Arc::clone(name), sheets: [Arc::clone(&held.get().0[references::Mention::Sheet]), Arc::clone(key)] }),
            })
            .collect::<Validated<(), _>>();
        let containers = sheets
            .iter()
            .map(|(key, sheet)| {
                within(
                    Lookup::Sheet,
                    key,
                    layers::unresolved(&layers, &targets, &sheet.layers)
                        .map2(
                            sheet.viewports.iter().map(|(name, viewport)| within(Lookup::Viewport, name, layers::unresolved(&layers, &targets, &viewport.layers).ok())).collect::<Validated<(), _>>(),
                            |(), ()| (),
                        )
                        .ok(),
                )
            })
            .collect::<Validated<(), _>>();
        duplicates.map2(containers, |(), ()| Self { layers, sheets, targets }).ok()
    }
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Fit {
    Contain(Rect),
    Density { origin: Point, density: LinearNumberDensity },
}

// --- [ERRORS] --------------------------------------------------------------------------

#[derive(Debug, thiserror::Error)]
pub enum SheetsError {
    #[error(transparent)]
    Linework(#[from] LineworkError),
    #[error(transparent)]
    Annotation(#[from] AnnotationError),
    #[error(transparent)]
    Tolerance(#[from] ToleranceError),
    #[error(transparent)]
    Image(#[from] ImageError),
    #[error("raster of {width} by {height} pixels spans no placeable extent")]
    Raster { width: usize, height: usize },
    #[error("scan density {0:?} is not positive")]
    Density(LinearNumberDensity),
    #[error("no {lookup} named {name}")]
    Unresolved { lookup: Lookup, name: Arc<str> },
    #[error("viewport {name} sits on sheets {} and {}", .sheets[0], .sheets[1])]
    Duplicate { name: Arc<str>, sheets: [Arc<str>; 2] },
    #[error("{lookup} {name}")]
    Within { lookup: Lookup, name: Arc<str>, source: Box<Self> },
}

impl From<SheetsError> for NEVec<SheetsError> {
    fn from(value: SheetsError) -> Self { nev![value] }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn raster(bytes: Arc<[u8]>, fit: Fit, opacity: Opacity) -> Result<Element, SheetsError> {
    let ImageSize { width, height } = blob_size(&bytes)?;
    let count = |pixels: usize| u32::try_from(pixels).ok().and_then(NonZero::new).map(|counted| f64::from(counted.get()));
    let (across, down) = count(width).zip(count(height)).ok_or(SheetsError::Raster { width, height })?;
    let extent = match fit {
        Fit::Contain(frame) => frame.abs().inscribed_rect_with_aspect_ratio(across / down),
        Fit::Density { origin, density } => {
            (density.is_normal() && density.is_sign_positive()).ok_or(SheetsError::Density(density))?;
            let per = density.get::<per_millimeter>();
            Rect::from_origin_size(origin, (across / per, down / per))
        }
    };
    Ok(Element::Placed { content: Content::Raster(bytes), placement: Placement::try_new(Affine::map_unit_square(extent))?, opacity })
}

pub fn compose(typesetter: &mut Typesetter, set: Set) -> Result<Vec<Page>, NEVec<SheetsError>> {
    let Set { layers: table, sheets, targets } = set;
    sheets
        .into_iter()
        .map(|(key, Sheet { size, style, viewports: placed, layers: content, links, fields })| {
            let paper = layers::drawn(typesetter, &style, Placement::default(), &table, &targets, content).map_or_else(Validated::Fail, Validated::Good);
            let viewed = placed.into_iter().map(|(name, viewport)| within(Lookup::Viewport, &name, viewports::drawn(typesetter, &style, &table, &targets, viewport))).collect::<Validated<Vec<_>, _>>();
            let page = paper.map2(viewed, |(linework, annotation, derived), views| {
                let (flat, framed, linked) = views.into_iter().collect::<(Vec<_>, Vec<_>, Vec<_>)>();
                let mut merged = framed.into_iter().flatten().chain(linework).chain(annotation).fold(HashMap::<Arc<str>, Vec<Element>>::new(), |mut merged, pages::Layer { name, print: _, view: _, elements }| {
                    merged.entry(name).or_default().extend(elements);
                    merged
                });
                Page {
                    size,
                    viewports: flat,
                    layers: table
                        .iter()
                        .filter_map(|(name, states)| merged.remove(name).map(|elements| pages::Layer { name: Arc::clone(name), print: states[layers::Usage::Print], view: states[layers::Usage::View], elements }))
                        .collect(),
                    links: links.into_iter().chain(linked.into_iter().flatten()).chain(derived).collect(),
                    fields,
                }
            });
            within(Lookup::Sheet, &key, page.ok())
        })
        .collect::<Validated<Vec<_>, _>>()
        .ok()
}

pub(crate) fn within<T>(lookup: Lookup, name: &Arc<str>, result: Result<T, NEVec<SheetsError>>) -> Validated<T, SheetsError> {
    result.map_or_else(
        |errors| Validated::Fail(errors.into_nonempty_iter().map(|source| SheetsError::Within { lookup, name: Arc::clone(name), source: Box::new(source) }).collect()),
        Validated::Good,
    )
}

// --- [COMPOSITION] ---------------------------------------------------------------------

pub mod layers;
pub mod references;
pub mod viewports;
