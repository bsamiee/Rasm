use std::{collections::BTreeMap, sync::Arc};

use enum_map::EnumMap;
use indexmap::IndexMap;
use kurbo::Affine;
use linesweeper::BinaryOp;
use nonempty_collections::{IntoNonEmptyIterator as _, NEVec, NonEmptyIterator as _};
use rasm_annotation::{Annotation, Drawing, Graphics, Style, bounds, draw, text::Typesetter};
use rasm_linework::{
    Element, Placement, Region, Side, Tolerance, Visibility, cleanup,
    pages::{self, Link},
    regions,
};
use validated::Validated;

use crate::{Lookup, SheetsError, references, within};

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

// --- [OPERATIONS] ----------------------------------------------------------------------

pub(crate) fn unresolved(table: &IndexMap<Arc<str>, EnumMap<Usage, Visibility>>, targets: &references::Targets, layers: &BTreeMap<Arc<str>, Layer>) -> Validated<(), SheetsError> {
    layers
        .iter()
        .flat_map(|(name, Layer { elements: _, annotations: _, references })| {
            (!table.contains_key(name)).then(|| SheetsError::Unresolved { lookup: Lookup::Layer, name: Arc::clone(name) }).into_iter().chain(
                references.iter().filter(|reference| !targets.contains_key(&reference.view)).map(|reference| SheetsError::Unresolved { lookup: Lookup::Viewport, name: Arc::clone(&reference.view) }),
            )
        })
        .map(Err::<(), _>)
        .collect()
}

pub(crate) fn drawn(
    typesetter: &mut Typesetter,
    style: &Style,
    placement: Placement,
    table: &IndexMap<Arc<str>, EnumMap<Usage, Visibility>>,
    targets: &references::Targets,
    mut layers: BTreeMap<Arc<str>, Layer>,
) -> Result<(Vec<pages::Layer>, Vec<pages::Layer>, Vec<Link>), NEVec<SheetsError>> {
    fn lifted<T, E>(result: Result<T, NEVec<E>>) -> Result<T, NEVec<SheetsError>>
    where SheetsError: From<E> {
        result.map_err(|errors| errors.into_nonempty_iter().map(SheetsError::from).collect())
    }
    let marked = table
        .iter()
        .filter_map(|(name, states)| layers.remove(name).map(|layer| (name, states, layer)))
        .map(|(name, states, Layer { elements, annotations, references })| {
            let (symbols, destinations) = references
                .into_iter()
                .map(|reference| {
                    let target = &targets[&reference.view];
                    references::resolved(reference, target)
                })
                .unzip::<_, _, Vec<_>, Vec<_>>();
            within(Lookup::Layer, name, lifted(draw(typesetter, style, placement, annotations.iter().chain(&symbols)))).map(|Drawing { graphics, mask }| {
                let links = graphics
                    .iter()
                    .skip(annotations.len())
                    .zip(destinations)
                    .map(|(graphics, destination)| Link { area: bounds(graphics, Affine::IDENTITY), destination })
                    .collect::<Vec<_>>();
                (name, states, elements, graphics.into_iter().flat_map(|Graphics { elements, masks: _ }| elements).collect::<Vec<_>>(), links, mask)
            })
        })
        .collect::<Validated<Vec<_>, _>>()
        .ok()?;
    let model = |masks: &[&Region]| -> Result<(Region, Tolerance), SheetsError> {
        Ok((
            regions::boolean(&Region::default(), masks.iter().copied(), BinaryOp::Union, style.tolerance)?.transform(Placement::try_new(placement.inverse())?)?,
            Tolerance::try_new(style.tolerance.into_inner() / placement.determinant().abs().sqrt())?,
        ))
    };
    let trim = match marked.iter().map(|(.., mask)| mask).filter(|mask| !mask.is_empty()).collect::<Vec<_>>().as_slice() {
        [] => None,
        masks => Some(model(masks)?),
    };
    marked
        .into_iter()
        .map(|(name, states, elements, marks, links, _)| {
            let clipped = match &trim {
                Some((region, tolerance)) => lifted(cleanup::clip(elements, region, Side::Outside, *tolerance)),
                None => Ok(elements),
            };
            within(Lookup::Layer, name, clipped).map(|elements| {
                let layer = |elements: Vec<Element>| (!elements.is_empty()).then(|| pages::Layer { name: Arc::clone(name), print: states[Usage::Print], view: states[Usage::View], elements });
                (layer(elements), layer(marks), links)
            })
        })
        .collect::<Validated<(Vec<_>, Vec<_>, Vec<Vec<_>>), _>>()
        .ok()
        .map(|(linework, marks, links)| (linework.into_iter().flatten().collect(), marks.into_iter().flatten().collect(), links.into_iter().flatten().collect()))
}
