use std::{collections::BTreeMap, sync::Arc};

use enum_map::EnumMap;
use indexmap::IndexMap;
use kurbo::Shape as _;
use nonempty_collections::NEVec;
use rasm_annotation::{Style, text::Typesetter};
use rasm_linework::{
    Element, Placement, Region, Side, Visibility,
    pages::{self, Link},
};

use crate::{SheetsError, layers, references};

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone)]
pub struct Viewport {
    pub placement: Placement,
    pub scale: pages::Scale,
    pub boundary: Option<Region> = None,
    pub layers: BTreeMap<Arc<str>, layers::Layer> = BTreeMap::new(),
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub(crate) fn drawn(
    typesetter: &mut Typesetter,
    style: &Style,
    table: &IndexMap<Arc<str>, EnumMap<layers::Usage, Visibility>>,
    targets: &references::Targets,
    viewport: Viewport,
) -> Result<(pages::Viewport, Vec<pages::Layer>, Vec<Link>), NEVec<SheetsError>> {
    let Viewport { placement, scale, boundary, layers: content } = viewport;
    let flat = pages::Viewport { boundary, placement, scale, layers: Vec::new() };
    let (linework, annotation, links) = layers::drawn(typesetter, style, pages::affine(&flat).map_err(SheetsError::from)?, table, targets, content)?;
    let (framed, linked) = match &flat.boundary {
        Some(region) => {
            let frame = region.bounding_box();
            (
                annotation
                    .into_iter()
                    .map(|pages::Layer { name, print, view, elements }| pages::Layer { name, print, view, elements: vec![Element::Clip { region: region.clone(), keep: Side::Inside, elements }] })
                    .collect(),
                links.into_iter().map(|Link { area, destination }| Link { area: area.intersect(frame), destination }).filter(|Link { area, destination: _ }| !area.is_zero_area()).collect(),
            )
        }
        None => (annotation, links),
    };
    Ok((pages::Viewport { layers: linework, ..flat }, framed, linked))
}
