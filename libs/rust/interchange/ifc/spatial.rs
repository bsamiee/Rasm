use core::{iter, num::NonZero};

use indexmap::IndexMap;
use nonempty_collections::{NESet, NEVec};
use petgraph::graphmap::DiGraphMap;
use serde::{Deserialize, Serialize, de::IntoDeserializer as _};
use serde_with::{DisplayFromStr, de::DeserializeAsWrap};
use strum::VariantArray as _;
use uom::si::f64::{Angle, Length};
use url::Url;
use validated::Validated;

use crate::{
    Composed, IfcError, Model, Part, Path,
    schemas::{Attribute, attribute, record},
};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Debug, Clone, Copy, PartialEq, Eq, Serialize, Deserialize)]
#[serde(rename_all = "UPPERCASE")]
pub enum Flow {
    Source,
    Sink,
    SourceAndSink,
    NotDefined,
}

impl Attribute for Flow {
    const ID: &'static str = "bsi::ifc::system::flowdirection";
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, strum::EnumString)]
pub enum Kind {
    Project,
    Site,
    Facility,
    Building,
    Bridge,
    MarineFacility,
    Railway,
    Road,
    FacilityPartCommon,
    BuildingStorey,
    BridgePart,
    MarinePart,
    RailwayPart,
    RoadPart,
    Space,
    SpatialZone,
    ExternalSpatialElement,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, strum::IntoStaticStr, strum::VariantArray)]
pub enum Link {
    #[strum(serialize = "bsi::ifc::system::connectsto")]
    ConnectsTo,
    #[strum(serialize = "bsi::ifc::system::partofsystem")]
    PartOfSystem,
    #[strum(serialize = "bsi::ifc::system::servicesfacility")]
    ServicesFacility,
}

// --- [MODELS] --------------------------------------------------------------------------

#[derive(Debug, Clone, PartialEq, Eq, Serialize, Deserialize)]
pub struct Class {
    pub code: Box<str>,
    pub uri: Url,
}

impl Attribute for Class {
    const ID: &'static str = "bsi::ifc::class";
}

#[derive(Debug, Clone, PartialEq, Eq, Serialize, Deserialize)]
#[serde(transparent)]
pub struct Material(pub Class);

impl Attribute for Material {
    const ID: &'static str = "bsi::ifc::material";
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum Relation<'model> {
    Aggregates,
    Contains,
    Bounds { boundary: &'model Path },
    Link(Link),
}

#[derive(Debug, Clone, Copy, PartialEq, Deserialize)]
#[serde(untagged, deny_unknown_fields, expecting = "latitude and longitude or eastings and northings with an optional height, or a height alone")]
pub enum Coordinates {
    Geographic { latitude: Angle, longitude: Angle, height: Option<Length> },
    Projected { eastings: Length, northings: Length, height: Option<Length> },
    Vertical { height: Length },
}

#[derive(Deserialize)]
struct Target {
    #[serde(rename = "ref")]
    reference: Path,
}

#[derive(Deserialize)]
struct SpaceBoundary {
    #[serde(rename = "relatedelement")]
    related_element: Target,
    #[serde(rename = "relatingspace")]
    relating_space: Target,
}

impl Attribute for SpaceBoundary {
    const ID: &'static str = "bsi::ifc::spaceBoundary";
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [CLASSIFICATION]

pub fn kind(node: &Composed) -> Result<Option<Kind>, IfcError> {
    attribute::<Class>(node).map(|class| class.and_then(|Class { code, uri: _ }| code.trim_end_matches(|letter: char| letter.is_ascii_uppercase() || letter == '_').strip_prefix("Ifc")?.parse().ok()))
}

// --- [RELATIONS]

pub fn relations(model: &Model) -> Result<DiGraphMap<&Path, NESet<Relation<'_>>>, NEVec<IfcError>> {
    let resolved = move |Target { reference }: Target| model.get(&reference).map(|target| &target.node).ok_or(IfcError::Missing { path: reference, part: Part::Node });
    let classified = model.nodes().filter(|node| node.attributes.contains_key(Class::ID)).map(|node| kind(node).map(|spatial| (&node.node, (node, spatial)))).collect::<Validated<IndexMap<_, _>, _>>();
    let stated = model
        .nodes()
        .flat_map(|node| {
            let bounded = attribute::<SpaceBoundary>(node).map_or_else(Validated::fail, |boundary| {
                boundary
                    .into_iter()
                    .map(|SpaceBoundary { related_element, relating_space }| {
                        Validated::from(resolved(related_element)).map2(Validated::from(resolved(relating_space)), |element, space| (element, space, Relation::Bounds { boundary: &node.node }))
                    })
                    .collect::<Validated<Vec<_>, _>>()
            });
            let linked = Link::VARIANTS.iter().map(move |&link| {
                record::<Vec<Target>>(node, link.into()).map_or_else(Validated::fail, |targets| {
                    targets.into_iter().flatten().map(|target| resolved(target).map(|to| (&node.node, to, Relation::Link(link)))).collect::<Validated<Vec<_>, _>>()
                })
            });
            iter::once(bounded).chain(linked)
        })
        .collect::<Validated<Vec<Vec<_>>, _>>();
    classified
        .map2(stated, |classified, stated| {
            let decomposed = classified.values().flat_map(|&(node, parent)| node.children.values().map(move |child| (node, parent, child))).filter_map(|(node, parent, child)| {
                match (parent, classified.get(&child.node)) {
                    (_, None) => None,
                    (Some(_), Some(&(_, None))) => Some((&node.node, &child.node, Relation::Contains)),
                    (None, Some(_)) | (Some(_), Some(&(_, Some(_)))) => Some((&node.node, &child.node, Relation::Aggregates)),
                }
            });
            let placed = classified.keys().fold(DiGraphMap::<_, NESet<_>>::new(), |mut graph, &path| {
                graph.add_node(path);
                graph
            });
            decomposed.chain(stated.into_iter().flatten()).fold(placed, |mut graph, (from, to, relation)| {
                if let Some(relations) = graph.edge_weight_mut(from, to) {
                    relations.insert(relation);
                } else {
                    graph.add_edge(from, to, NESet::new(relation));
                }
                graph
            })
        })
        .ok()
}

// --- [GEOREFERENCE]

pub fn georeference(node: &Composed) -> Result<IndexMap<NonZero<u32>, Coordinates>, NEVec<IfcError>> {
    node.attributes
        .keys()
        .filter_map(|id| id.strip_prefix("epsg").map(|digits| (id, digits)))
        .filter_map(|(id, digits)| {
            record::<Coordinates>(node, id).transpose().map(|decoded| {
                let epsg = serde_path_to_error::deserialize::<_, DeserializeAsWrap<NonZero<u32>, DisplayFromStr>>(digits.into_deserializer())
                    .map(DeserializeAsWrap::into_inner)
                    .map_err(|source| IfcError::Record { node: node.node.clone(), attribute: id.clone(), source });
                Validated::from(epsg).map2(Validated::from(decoded), |code, coordinates| (code, coordinates))
            })
        })
        .collect::<Validated<IndexMap<_, _>, _>>()
        .ok()
}
