use core::num::NonZero;
use std::{collections::BTreeMap, sync::Arc};

use kurbo::{Affine, Rect, Size};
use nonempty_collections::{NEVec, nev};
use validated::Validated;

use crate::{Element, LineworkError, Placement, Region, Side, Visibility};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub struct Scale {
    pub paper: NonZero<u32>,
    pub model: NonZero<u32>,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Layer {
    pub name: Arc<str>,
    pub print: Visibility = Visibility::Visible,
    pub view: Visibility = Visibility::Visible,
    pub elements: Vec<Element>,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Viewport {
    pub boundary: Option<Region>,
    pub placement: Placement,
    pub scale: Scale,
    pub layers: Vec<Layer>,
}

#[derive(Debug, Clone, PartialEq)]
pub enum Destination {
    Uri(Arc<str>),
    Page { index: usize, area: Option<Rect> },
}

#[derive(Debug, Clone, PartialEq)]
pub struct Link {
    pub area: Rect,
    pub destination: Destination,
}

#[derive(Debug, Clone, PartialEq)]
pub struct Page {
    pub size: Size,
    pub viewports: Vec<Viewport> = Vec::new(),
    pub layers: Vec<Layer> = Vec::new(),
    pub links: Vec<Link> = Vec::new(),
    pub fields: BTreeMap<Arc<str>, Arc<str>> = BTreeMap::new(),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn affine(viewport: &Viewport) -> Result<Placement, LineworkError> {
    Placement::try_new(*viewport.placement * Affine::scale(f64::from(viewport.scale.paper.get()) / f64::from(viewport.scale.model.get())))
}

pub fn paper(viewport: Viewport) -> Result<Vec<Layer>, NEVec<LineworkError>> {
    let model = affine(&viewport).map_err(|error| nev![error])?;
    let Viewport { boundary, placement: _, scale: _, layers } = viewport;
    layers
        .into_iter()
        .map(|Layer { name, print, view, elements }| {
            crate::transform(elements, model).map_err(|errors| LineworkError::Layer { name: Arc::clone(&name), errors }).map(|placed| Layer {
                name,
                print,
                view,
                elements: match boundary.clone() {
                    Some(region) => vec![Element::Clip { region, keep: Side::Inside, elements: placed }],
                    None => placed,
                },
            })
        })
        .collect::<Validated<Vec<_>, _>>()
        .ok()
}
