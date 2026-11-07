use core::iter;
use std::{collections::BTreeMap, sync::Arc};

use enum_map::EnumMap;
use indexmap::IndexMap;
use kurbo::Affine;
use linesweeper::BinaryOp;
use nonempty_collections::{IntoIteratorExt as _, IntoNonEmptyIterator as _, NEVec, NonEmptyIterator as _};
use rasm_drawing::{
    Element, Placement, Region, Side, Tolerance, Visibility, cleanup,
    pages::{self, Link},
    regions,
};
use validated::Validated;

use super::references;
use crate::{Annotation, DraftingError, Graphics, Lookup, Style, Subject, bounds, draw, text::Typesetter, within};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, enum_map::Enum)]
pub enum Usage {
    Print,
    View,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone)]
pub struct Layer {
    pub elements: Vec<Element> = Vec::new(),
    pub annotations: Vec<Annotation> = Vec::new(),
    pub references: Vec<references::Reference> = Vec::new(),
}

pub(super) struct Drawing {
    pub(super) model: Vec<pages::Layer>,
    pub(super) paper: Vec<pages::Layer>,
    pub(super) links: Vec<Link>,
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub(crate) fn unresolved(table: &IndexMap<Arc<str>, EnumMap<Usage, Visibility>>, targets: &references::Targets, layers: &BTreeMap<Arc<str>, Layer>) -> Validated<(), DraftingError> {
    layers
        .iter()
        .flat_map(|(name, Layer { elements: _, annotations: _, references })| {
            (!table.contains_key(name)).then(|| DraftingError::Unknown { lookup: Lookup::Layer, name: Arc::clone(name) }).into_iter().chain(
                references.iter().filter(|reference| !targets.contains_key(&reference.view)).map(|reference| DraftingError::Unknown { lookup: Lookup::Viewport, name: Arc::clone(&reference.view) }),
            )
        })
        .map(Err::<(), _>)
        .collect()
}

pub(super) fn drawn(
    typesetter: &mut Typesetter,
    style: &Style,
    placement: Placement,
    table: &IndexMap<Arc<str>, EnumMap<Usage, Visibility>>,
    targets: &references::Targets,
    mut layers: BTreeMap<Arc<str>, Layer>,
) -> Result<Drawing, NEVec<DraftingError>> {
    let marked = table
        .iter()
        .filter_map(|(name, states)| layers.remove(name).map(|layer| (name, states, layer)))
        .map(|(name, states, layer)| {
            let (symbols, destinations) = layer
                .references
                .into_iter()
                .map(|reference| {
                    let target = &targets[&reference.view];
                    references::resolved(reference, target)
                })
                .unzip::<_, _, Vec<_>, Vec<_>>();
            within(&Subject::Named { lookup: Lookup::Layer, name: Arc::clone(name) }, draw(typesetter, style, placement, iter::chain(&layer.annotations, &symbols))).map(
                |crate::Drawing { graphics, mask }| {
                    let links = graphics
                        .iter()
                        .skip(layer.annotations.len())
                        .zip(destinations)
                        .map(|(graphics, destination)| Link { area: bounds(graphics, Affine::IDENTITY), destination })
                        .collect::<Vec<_>>();
                    (name, states, layer.elements, graphics.into_iter().flat_map(|Graphics { elements, masks: _ }| elements).collect::<Vec<_>>(), links, mask)
                },
            )
        })
        .collect::<Validated<Vec<_>, _>>()
        .ok()?;
    let trim = marked
        .iter()
        .map(|(.., mask)| mask)
        .filter(|mask| !mask.is_empty())
        .try_into_nonempty_iter()
        .map(|masks| -> Result<_, DraftingError> {
            Ok((
                regions::boolean(&Region::default(), masks, BinaryOp::Union, style.tolerance)?.transform(Placement::try_new(placement.inverse())?)?,
                Tolerance::try_new(style.tolerance.into_inner() / placement.determinant().abs().sqrt())?,
            ))
        })
        .transpose()?;
    marked
        .into_iter()
        .map(|(name, states, model, paper, links, _)| {
            within(
                &Subject::Named { lookup: Lookup::Layer, name: Arc::clone(name) },
                match &trim {
                    Some((region, tolerance)) => cleanup::clip(model, region, Side::Outside, *tolerance).map_err(|errors| errors.into_nonempty_iter().map(DraftingError::from).collect()),
                    None => Ok(model),
                },
            )
            .map(|trimmed| {
                let [linework, annotation] =
                    [trimmed, paper].map(|elements| (!elements.is_empty()).then(|| pages::Layer { name: Arc::clone(name), print: states[Usage::Print], view: states[Usage::View], elements }));
                (linework, annotation, links)
            })
        })
        .collect::<Validated<(Vec<_>, Vec<_>, Vec<Vec<_>>), _>>()
        .ok()
        .map(|(model, paper, links)| Drawing { model: model.into_iter().flatten().collect(), paper: paper.into_iter().flatten().collect(), links: links.into_iter().flatten().collect() })
}
