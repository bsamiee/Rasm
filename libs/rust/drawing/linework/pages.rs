use std::{collections::BTreeMap, num::NonZero, sync::Arc};

use kurbo::{Affine, Rect, Size};

use crate::{Element, Region, Visibility};

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
    pub boundary: Region,
    pub placement: Affine,
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

#[must_use]
pub fn affine(viewport: &Viewport) -> Affine { viewport.placement * Affine::scale(f64::from(viewport.scale.paper.get()) / f64::from(viewport.scale.model.get())) }

pub fn paper(viewport: Viewport) -> impl Iterator<Item = Layer> {
    let model = affine(&viewport);
    let Viewport { boundary: _, placement: _, scale: _, layers } = viewport;
    layers.into_iter().map(move |Layer { name, print, view, elements }| Layer { name, print, view, elements: elements.into_iter().map(|element| crate::transform(element, model)).collect() })
}
