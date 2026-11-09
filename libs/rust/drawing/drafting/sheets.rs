use core::{iter, num::NonZero};
use std::{
    collections::{BTreeMap, hash_map::OccupiedError},
    sync::Arc,
};

use enum_map::{EnumMap, enum_map};
use imagesize::{ImageSize, blob_size};
use indexmap::IndexMap;
use itertools::Itertools as _;
use kurbo::{Affine, Point, Rect, Shape, Size};
use nonempty_collections::NEVec;
use rasm_drawing::{
    Content, Element, Opacity, Placement, Unit, Visibility,
    pages::{self, Destination, Link, Page},
};
use uom::si::{f64::LinearNumberDensity, ratio::ratio};
use validated::Validated;

use crate::{DraftingError, Lookup, Style, Subject, text::Typesetter, validated, within};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone)]
pub struct Sheet {
    pub size: Size,
    pub style: Arc<Style>,
    pub viewports: IndexMap<Arc<str>, Viewport>,
    pub layers: BTreeMap<Arc<str>, Layer> = BTreeMap::new(),
    pub links: Vec<Link> = Vec::new(),
    pub fields: BTreeMap<Arc<str>, Arc<str>> = BTreeMap::new(),
}

#[derive(Debug, Clone, derive_more::AsRef)]
pub struct Set {
    #[as_ref]
    layers: IndexMap<Arc<str>, EnumMap<Usage, Visibility>>,
    #[as_ref]
    sheets: IndexMap<Arc<str>, Sheet>,
    targets: references::Targets,
}

impl Set {
    pub fn new(layers: IndexMap<Arc<str>, EnumMap<Usage, Visibility>>, sheets: IndexMap<Arc<str>, Sheet>) -> Result<Self, NEVec<DraftingError>> {
        let mut targets = references::Targets::new();
        let duplicates = sheets
            .iter()
            .enumerate()
            .flat_map(|(index, (key, sheet))| sheet.viewports.iter().enumerate().map(move |(position, (name, viewport))| (index, key, position, name, viewport)))
            .filter_map(|(index, key, position, name, viewport)| {
                targets
                    .try_insert(
                        Arc::clone(name),
                        (
                            enum_map! {
                                Mention::Number => Arc::from(position.strict_add(1).to_string()),
                                Mention::Sheet => Arc::clone(key),
                            },
                            Destination::Page { index, area: viewport.boundary.as_ref().map(Shape::bounding_box) },
                        ),
                    )
                    .err()
                    .map(|OccupiedError { entry, key: name, .. }| DraftingError::Duplicate { name, sheets: [Arc::clone(&entry.get().0[Mention::Sheet]), Arc::clone(key)] })
            })
            .map(Err::<(), _>)
            .collect::<Validated<(), _>>();
        let containers = sheets
            .iter()
            .map(|(key, sheet)| {
                within(
                    &Subject::Named { lookup: Lookup::Sheet, name: Arc::clone(key) },
                    iter::once(layers::unresolved(&layers, &targets, &sheet.layers))
                        .chain(
                            sheet
                                .viewports
                                .iter()
                                .map(|(name, viewport)| within(&Subject::Named { lookup: Lookup::Viewport, name: Arc::clone(name) }, layers::unresolved(&layers, &targets, &viewport.layers).ok())),
                        )
                        .collect::<Validated<(), _>>()
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
    Density {
        origin: Point,
        density: LinearNumberDensity,
    },
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn raster(bytes: Arc<[u8]>, fit: Fit, unit: Unit, opacity: Opacity) -> Result<Element, DraftingError> {
    let size @ ImageSize { width, height } = blob_size(&bytes)?;
    let count = |pixels: usize| u32::try_from(pixels).ok().and_then(NonZero::new).map(|counted| f64::from(counted.get()));
    let (across, down) = count(width).zip(count(height)).ok_or(DraftingError::Raster(size))?;
    let extent = match fit {
        Fit::Contain(frame) => frame.abs().inscribed_rect_with_aspect_ratio(across / down),
        Fit::Density { origin, density } => {
            (density.is_normal() && density.is_sign_positive()).ok_or(DraftingError::Density(density))?;
            let per = (*unit * density).get::<ratio>();
            Rect::from_origin_size(origin, (across / per, down / per))
        }
    };
    Ok(Element::Placed { content: Content::Raster(bytes), placement: Placement::try_new(Affine::map_unit_square(extent))?, opacity })
}

pub fn compose(typesetter: &mut Typesetter, set: Set) -> Result<Vec<Page>, NEVec<DraftingError>> {
    let Set { layers: table, sheets, targets } = set;
    sheets
        .into_iter()
        .map(|(key, Sheet { size, style, viewports: placed, layers: content, links, fields })| {
            let paper = validated(layers::drawn(typesetter, &style, Placement::default(), &table, &targets, content));
            let viewed = placed
                .into_iter()
                .map(|(name, viewport)| within(&Subject::Named { lookup: Lookup::Viewport, name }, viewports::drawn(typesetter, &style, &table, &targets, viewport)))
                .collect::<Validated<Vec<_>, _>>();
            let page = paper.map2(viewed, |layers::Drawing { model: linework, paper: annotation, links: derived }, views| {
                let (flat, framed, linked) = views.into_iter().collect::<(Vec<_>, Vec<_>, Vec<_>)>();
                let mut merged = framed.into_iter().flatten().chain(linework).chain(annotation).map(|pages::Layer { name, print: _, view: _, elements }| (name, elements)).into_group_map();
                Page {
                    size,
                    unit: style.unit,
                    viewports: flat,
                    layers: table
                        .iter()
                        .filter_map(|(name, states)| {
                            merged.remove(name).map(|groups| pages::Layer {
                                name: Arc::clone(name),
                                print: states[Usage::Print],
                                view: states[Usage::View],
                                elements: groups.into_iter().flatten().collect(),
                            })
                        })
                        .collect(),
                    links: iter::chain(links, linked.into_iter().flatten()).chain(derived).collect(),
                    fields,
                }
            });
            within(&Subject::Named { lookup: Lookup::Sheet, name: key }, page.ok())
        })
        .collect::<Validated<Vec<_>, _>>()
        .ok()
}

// --- [COMPOSITION] ---------------------------------------------------------------------

mod layers;
mod references;
mod viewports;

// --- [EXPORTS] -------------------------------------------------------------------------

pub use layers::{Layer, Usage};
pub use references::{Mention, Reference};
pub use viewports::Viewport;
