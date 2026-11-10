use core::iter;
use core::num::NonZero;
use std::collections::hash_map::OccupiedError;
use std::collections::{BTreeMap, HashMap};
use std::sync::Arc;

use enum_map::{EnumMap, enum_map};
use imagesize::{ImageSize, blob_size};
use indexmap::IndexMap;
use itertools::Itertools as _;
use kurbo::{Affine, Point, Rect, Shape, Size};
use linesweeper::BinaryOp;
use nonempty_collections::{IntoIteratorExt as _, IntoNonEmptyIterator as _, NEVec, NonEmptyIterator as _};
use rasm_drawing::pages::{self, Destination, Link, Page};
use rasm_drawing::{Content, Element, Opacity, Placement, Region, Side, Tolerance, Unit, Visibility, cleanup, regions};
use uom::si::f64::LinearNumberDensity;
use uom::si::ratio::ratio;
use validated::Validated;

use crate::symbols::Instance;
use crate::text::Typesetter;
use crate::{Annotation, DraftingError, Drawing, Graphics, Lookup, Style, Subject, bounds, draw, validated, within};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, enum_map::Enum)]
pub enum Usage {
    Print,
    View,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, enum_map::Enum)]
pub enum Mention {
    Number,
    Sheet,
}

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
    targets: HashMap<Arc<str>, (EnumMap<Mention, Arc<str>>, Destination)>,
}

impl Set {
    pub fn new(layers: IndexMap<Arc<str>, EnumMap<Usage, Visibility>>, sheets: IndexMap<Arc<str>, Sheet>) -> Result<Self, NEVec<DraftingError>> {
        let mut targets = HashMap::new();
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
        let unresolved = |content: &BTreeMap<Arc<str>, Layer>| {
            content
                .iter()
                .flat_map(|(name, Layer { elements: _, annotations: _, references })| {
                    (!layers.contains_key(name)).then(|| DraftingError::Unknown { lookup: Lookup::Layer, name: Arc::clone(name) }).into_iter().chain(
                        references
                            .iter()
                            .filter(|reference| !targets.contains_key(&reference.view))
                            .map(|reference| DraftingError::Unknown { lookup: Lookup::Viewport, name: Arc::clone(&reference.view) }),
                    )
                })
                .map(Err::<(), _>)
                .collect::<Validated<(), _>>()
        };
        let containers = sheets
            .iter()
            .map(|(key, sheet)| {
                within(
                    &Subject::Named { lookup: Lookup::Sheet, name: Arc::clone(key) },
                    iter::once(unresolved(&sheet.layers))
                        .chain(sheet.viewports.iter().map(|(name, viewport)| within(&Subject::Named { lookup: Lookup::Viewport, name: Arc::clone(name) }, unresolved(&viewport.layers).ok())))
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
    Density { origin: Point, density: LinearNumberDensity },
}

#[derive(Debug, Clone)]
pub struct Viewport {
    pub placement: Placement,
    pub scale: pages::Scale,
    pub boundary: Option<Region> = None,
    pub layers: BTreeMap<Arc<str>, Layer> = BTreeMap::new(),
}

#[derive(Debug, Clone)]
pub struct Layer {
    pub elements: Vec<Element> = Vec::new(),
    pub annotations: Vec<Annotation> = Vec::new(),
    pub references: Vec<Reference> = Vec::new(),
}

#[derive(Debug, Clone, PartialEq)]
pub struct Reference {
    pub view: Arc<str>,
    pub marker: Instance,
    pub mentions: EnumMap<Mention, Option<Arc<str>>>,
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
    let mut drawn = |style: &Style, viewport: Option<&pages::Viewport>, mut layers: BTreeMap<Arc<str>, Layer>| -> Result<_, NEVec<DraftingError>> {
        let placement = viewport.map_or(Ok(Placement::default()), pages::affine).map_err(DraftingError::from)?;
        let boundary = viewport.and_then(|flat| flat.boundary.as_ref());
        let frame = boundary.map(Shape::bounding_box);
        let marked = table
            .iter()
            .filter_map(|(name, states)| layers.remove(name).map(|layer| (name, states, layer)))
            .map(|(name, states, Layer { elements, annotations, references })| {
                let (symbols, destinations) = references
                    .into_iter()
                    .map(|Reference { view, mut marker, mentions }| {
                        let (texts, destination) = &targets[&view];
                        marker.slots.extend(mentions.into_iter().filter_map(|(mention, slot)| slot.map(|slot| (slot, Arc::clone(&texts[mention])))));
                        (Annotation::Symbol(marker), destination.clone())
                    })
                    .unzip::<_, _, Vec<_>, Vec<_>>();
                within(&Subject::Named { lookup: Lookup::Layer, name: Arc::clone(name) }, draw(typesetter, style, placement, iter::chain(&annotations, &symbols))).map(|Drawing { graphics, mask }| {
                    let links =
                        graphics.iter().skip(annotations.len()).zip(destinations).map(|(graphics, destination)| Link { area: bounds(graphics, Affine::IDENTITY), destination }).collect::<Vec<_>>();
                    (name, states, elements, graphics.into_iter().flat_map(|Graphics { elements, masks: _ }| elements).collect::<Vec<_>>(), links, mask)
                })
            })
            .collect::<Validated<Vec<_>, _>>()
            .ok()?;
        let trim = marked
            .iter()
            .map(|(.., mask)| mask)
            .filter(|mask| !mask.contours().is_empty())
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
                    let paper = match boundary {
                        Some(region) if !paper.is_empty() => vec![Element::Clip { region: region.clone(), keep: Side::Inside, elements: paper.into() }],
                        Some(_) | None => paper,
                    };
                    let [linework, annotation] =
                        [trimmed, paper].map(|elements| (!elements.is_empty()).then(|| pages::Layer { name: Arc::clone(name), print: states[Usage::Print], view: states[Usage::View], elements }));
                    (linework, annotation, links)
                })
            })
            .collect::<Validated<(Vec<_>, Vec<_>, Vec<Vec<_>>), _>>()
            .ok()
            .map(|(model, paper, links)| {
                (
                    model.into_iter().flatten().collect::<Vec<_>>(),
                    paper.into_iter().flatten().collect::<Vec<_>>(),
                    links
                        .into_iter()
                        .flatten()
                        .filter_map(|Link { area, destination }| {
                            frame.map_or(Some(area), |frame| Some(area.intersect(frame)).filter(|clipped| !clipped.is_zero_area())).map(|area| Link { area, destination })
                        })
                        .collect::<Vec<_>>(),
                )
            })
    };
    sheets
        .into_iter()
        .map(|(key, Sheet { size, style, viewports: placed, layers: content, links, fields })| {
            let paper = validated(drawn(&style, None, content));
            let viewed = placed
                .into_iter()
                .map(|(name, Viewport { placement, scale, boundary, layers: content })| {
                    let flat = pages::Viewport { boundary, placement, scale, layers: Vec::new() };
                    within(
                        &Subject::Named { lookup: Lookup::Viewport, name },
                        drawn(&style, Some(&flat), content).map(|(linework, framed, linked)| (pages::Viewport { layers: linework, ..flat }, framed, linked)),
                    )
                })
                .collect::<Validated<Vec<_>, _>>();
            let page = paper.map2(viewed, |(linework, annotation, derived), views| {
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
